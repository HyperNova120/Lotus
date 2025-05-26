using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Identity.Client.NativeInterop;
using MinecraftAuthModels;
using MinecraftNetworking.Connection;
using MinecraftNetworking.Packets;
using MinecraftNetworking.Types;

namespace MinecraftNetworking.StateHandlers
{
    public static class LoginHandler
    {
        private static byte[]? VerifyKey;

        public static async Task ProcessPacket(MinecraftServerPacket packet)
        {
            switch (packet.protocol_id)
            {
                case 0x00:
                    _ = HandleLoginDisconnect(packet);
                    break;
                case 0x01:
                    _ = HandleEncryptionRequest(packet);
                    break;
                case 0x02:
                    Logging.LogInfo("Login Success");
                    break;
                case 0x03:
                    _ = HandleSetCompression(packet);
                    break;
                default:
                    Logging.LogError(
                        $"LoginHandler State 0x{packet.protocol_id:X} Not Implemented"
                    );
                    break;
            }
        }

        private static async Task HandleSetCompression(MinecraftServerPacket packet)
        {
            (int CompresionThreshold, _) = VarInt_VarLong.DecodeVarInt(packet.data);
            Logging.LogDebug("CompresionThreshold:" + CompresionThreshold);
            MinecraftPacketHandler.CompresionThreshold = CompresionThreshold;
            MinecraftPacketHandler.IsCompressionEnabled = true;
        }

        private static async Task HandleLoginDisconnect(MinecraftServerPacket packet)
        {
            Logging.LogInfo(
                $"Client disconnected during login, Reason:{Encoding.UTF8.GetString(packet.data)}"
            );
        }

        private static async Task HandleEncryptionRequest(MinecraftServerPacket packet)
        {
            (string serverID, int serverIDBytes) = StringN.DecodeBytes(packet.data);

            byte[] remainingBytes = packet.data.Skip(serverIDBytes).ToArray();
            (byte[] PublicKey, int PublicKeyBytes) = PrefixedArray.DecodeBytes(remainingBytes);

            remainingBytes = remainingBytes.Skip(PublicKeyBytes).ToArray();
            (byte[] VerifyToken, int VerifyTokenBytes) = PrefixedArray.DecodeBytes(remainingBytes);

            remainingBytes = remainingBytes.Skip(VerifyTokenBytes).ToArray();
            bool ShouldAuth = remainingBytes[0] == 0x01;

            Logging.LogDebug(
                $"HandleEncryptionRequest: serverID:{serverID} Bytes_Read:{serverIDBytes}; PublicKey Length:{PublicKey.Length} Bytes_Read:{PublicKeyBytes}; VerifyToken Length:{VerifyToken.Length} Bytes_Read:{VerifyTokenBytes}; Should Auth:{ShouldAuth}"
            );
            Logging.LogDebug(
                $"\tTotal Bytes Read:{serverIDBytes + PublicKeyBytes + VerifyTokenBytes + 1}"
            );

            try
            {
                VerifyKey = VerifyToken;

                string hash = EncryptionHandler.GenerateMinecraftAuthenticationHash(
                    serverID,
                    PublicKey
                );
                if (ShouldAuth)
                {
                    await AuthenticateWithMinecraftServer(hash);
                }
                Logging.LogDebug("return encryption packet");

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    // Load the public key
                    /* string PemKey =
                        "-----BEGIN PUBLIC KEY-----\n"
                        + Convert.ToBase64String(
                            PublicKey,
                            Base64FormattingOptions.InsertLineBreaks
                        )
                        + "\n-----END PUBLIC KEY-----";
                    Logging.LogDebug($"|{PemKey}|"); */
                    rsa.ImportSubjectPublicKeyInfo(PublicKey, out _);

                    EncryptionResponsePacket encryptionResponsePacket =
                        new EncryptionResponsePacket(
                            rsa.Encrypt(EncryptionHandler.SharedSecret, false),
                            rsa.Encrypt(VerifyKey, false)
                        );
                    ConnectionHandler.SendPacket(encryptionResponsePacket);
                }
                Logging.LogDebug("Set Encryption True");
                MinecraftPacketHandler.IsEncryptionEnabled = true;
            }
            catch (Exception e)
            {
                Logging.LogError("HandleEncryptionRequest:" + e.ToString());
            }
        }

        public static async Task LoginToServer(string serverIp, ushort ip = 25565)
        {
            if (
                ConnectionHandler.connectionState == ConnectionState.NONE
                || ConnectionHandler.connectionState == ConnectionState.STATUS
            )
            {
                ConnectionHandler.connectionState = ConnectionState.LOGIN;

                ConnectionHandler.SendPacket(
                    new HandshakePacket(serverIp, HandshakePacket.Intent.Login, ip)
                );
                Logging.LogDebug(
                    $"LoginToServer; username:{MojangLogin.UserMinecraftProfile!.name}; uuid:{new Guid(MojangLogin.UserMinecraftProfile!.id)}"
                );
                ConnectionHandler.SendPacket(
                    new LoginStartPacket(
                        MojangLogin.UserMinecraftProfile!.name,
                        new Guid(MojangLogin.UserMinecraftProfile!.id)
                    )
                );
            }
        }

        private static async Task<bool> AuthenticateWithMinecraftServer(string serverHash)
        {
            MinecraftServerAuthModel authModel = new MinecraftServerAuthModel();
            authModel.accessToken = MojangLogin.MinecraftAuth!.access_token;
            authModel.selectedProfile = MojangLogin.UserMinecraftProfile!.id.Replace("-", "");
            authModel.serverId = serverHash;

            HttpRequestMessage msg = HttpHandler.CreateHttpRequestMessage(
                HttpMethod.Post,
                "https://sessionserver.mojang.com/session/minecraft/join",
                new StringContent(
                    JsonSerializer.Serialize(authModel),
                    Encoding.UTF8,
                    "application/json"
                )
            );
            HttpResponseMessage response = await HttpHandler.SendRequest(msg);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                //good auth
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                //bad auth
                Logging.LogError(
                    $"Minecraft server auth failed, Code:{response.StatusCode}, with reason: {await response.Content.ReadAsStringAsync()}"
                );
                return false;
            }
            Logging.LogError(
                $"Minecraft server auth failed, Code:{response.StatusCode}, with reason: {await response.Content.ReadAsStringAsync()}"
            );
            return false;
        }
    }
}
