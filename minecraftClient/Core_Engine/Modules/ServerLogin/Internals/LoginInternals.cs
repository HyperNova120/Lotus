using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Core_Engine.Modules.Networking.Internals;
using Core_Engine.Modules.Networking.Models;
using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.Networking.Packets.ClientBound.Login;
using Core_Engine.Modules.Networking.Packets.ClientBound.Login.Internals;
using Core_Engine.Modules.Networking.Packets.ServerBound.Handshake;
using Core_Engine.Modules.Networking.Packets.ServerBound.Login;
using Core_Engine.Modules.Networking.Types;
using static Core_Engine.Modules.Networking.Networking;

namespace Core_Engine.Modules.ServerLogin.Internals
{
    public class ServerLoginInternals
    {
        private enum RegisteredEventIdentifiers
        {
            SERVERLOGIN_loginSuccessful,
        }

        public void HandleLoginDisconnect(MinecraftServerPacket packet)
        {
            Logging.LogInfo(
                $"Client disconnected during login, Reason:{Encoding.UTF8.GetString(packet.data)}"
            );
            Core_Engine.GetModule<Networking.Networking>("Networking")!.DisconnectFromServer();
        }

        public async Task HandleEncryptionRequest(MinecraftServerPacket packet)
        {
            (string serverID, int serverIDBytes) = StringN.DecodeBytes(packet.data);

            byte[] remainingBytes = packet.data[serverIDBytes..];
            (byte[] PublicKey, int PublicKeyBytes) = PrefixedArray.DecodeBytes(remainingBytes);

            remainingBytes = remainingBytes[PublicKeyBytes..];
            (byte[] VerifyToken, int VerifyTokenBytes) = PrefixedArray.DecodeBytes(remainingBytes);

            remainingBytes = remainingBytes[VerifyTokenBytes..];
            bool ShouldAuth = remainingBytes[0] == 0x01;

            try
            {
                string hash = Core_Engine
                    .GetModule<Networking.Networking>("Networking")!
                    .encryption.GenerateMinecraftAuthenticationHash(serverID, PublicKey);
                if (ShouldAuth)
                {
                    await AuthenticateWithMinecraftServer(hash);
                }
                //Logging.LogDebug("return encryption packet");

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportSubjectPublicKeyInfo(PublicKey, out _);

                    EncryptionResponsePacket encryptionResponsePacket =
                        new EncryptionResponsePacket(
                            rsa.Encrypt(
                                Core_Engine
                                    .GetModule<Networking.Networking>("Networking")!
                                    .encryption.SharedSecret,
                                false
                            ),
                            rsa.Encrypt(VerifyToken, false)
                        );
                    Core_Engine
                        .GetModule<Networking.Networking>("Networking")!
                        .SendPacket(encryptionResponsePacket);
                }
                //Logging.LogDebug("Set Encryption True");
                MinecraftPacketHandler.IsEncryptionEnabled = true;
            }
            catch (Exception e)
            {
                Logging.LogError("HandleEncryptionRequest:" + e.ToString());
            }
        }

        private async Task<bool> AuthenticateWithMinecraftServer(string serverHash)
        {
            MinecraftServerAuthModel authModel = new MinecraftServerAuthModel();
            MojangLogin.MojangLogin mojangLogin = Core_Engine.GetModule<MojangLogin.MojangLogin>(
                "MojangLogin"
            )!;
            authModel.accessToken = mojangLogin.MinecraftAuth!.access_token;
            authModel.selectedProfile = mojangLogin.userProfile!.id.Replace("-", "");
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

        public void HandleSetCompression(MinecraftServerPacket packet)
        {
            (int CompresionThreshold, _) = VarInt_VarLong.DecodeVarInt(packet.data);
            //Logging.LogDebug("CompresionThreshold:" + CompresionThreshold);
            MinecraftPacketHandler.CompresionThreshold = CompresionThreshold;
            MinecraftPacketHandler.IsCompressionEnabled = true;
        }

        public void HandleLoginSuccess(MinecraftServerPacket packet)
        {
            LoginSuccessPacket loginSuccessPacket = new();
            loginSuccessPacket.decodeFromBytes(packet.data);
            /* Logging.LogInfo(
                $"Login Success: {packet.data.Length} bytes; UUID:{loginSuccessPacket.uuid}; username:{loginSuccessPacket.Username}"
            ); */
            Logging.LogInfo("Successfully Joined Server!");
            /* foreach (LoginSuccessPacketElement element in loginSuccessPacket.elements)
            {
                Logging.LogDebug(
                    $"\tS1:{element.s1}; S2:{element.s2}; optional S3:{(element.optionalS3 ?? "")}"
                );
            } */
            Core_Engine
                .GetModule<Networking.Networking>("Networking")!
                .SendPacket(new EmptyPacket(0x03));

            Core_Engine.InvokeEvent(
                nameof(RegisteredEventIdentifiers.SERVERLOGIN_loginSuccessful),
                new()
            );
        }
    }
}
