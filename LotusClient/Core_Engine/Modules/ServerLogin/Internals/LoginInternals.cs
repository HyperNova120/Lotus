using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEventArgs;
using LotusCore.Interfaces;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Models;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Packets.ClientBound.Login;
using LotusCore.Modules.Networking.Packets.ClientBound.Login.Internals;
using LotusCore.Modules.Networking.Packets.ServerBound.Configuration;
using LotusCore.Modules.Networking.Packets.ServerBound.Handshake;
using LotusCore.Modules.Networking.Packets.ServerBound.Login;
using LotusCore.Utils;
using static LotusCore.Modules.Networking.Networking;

namespace LotusCore.Modules.ServerLogin.Internals
{
    public class ServerLoginInternals
    {
        private IGameStateHandler _GameStateHandler;
        private Networking.Networking _NetworkingManager;

        private enum RegisteredEventIdentifiers
        {
            SERVERLOGIN_loginSuccessful,
            CONFIG_Start_Config_Process,
        }

        public ServerLoginInternals()
        {
            _GameStateHandler = Core_Engine.GetModule<IGameStateHandler>("GameStateHandler")!;
            _NetworkingManager = Core_Engine.GetModule<Networking.Networking>("Networking")!;
        }

        public void HandleLoginDisconnect(MinecraftServerPacket packet)
        {
            Logging.LogInfo(
                $"Client disconnected during login, Reason:{Encoding.UTF8.GetString(packet._Data)}"
            );
            NBT test = new(true);
            test.ReadFromBytes(packet._Data, true);
            Console.WriteLine(test.GetNBTAsString());
            Core_Engine
                .GetModule<Networking.Networking>("Networking")!
                .DisconnectFromServer(packet._RemoteHost);
            //Core_Engine.CurrentState = Core_Engine.State.Interactive;
            Core_Engine.SignalInteractiveFree(Core_Engine.State.JoiningServer);
        }

        public async Task HandleEncryptionRequest(MinecraftServerPacket packet)
        {
            (string serverID, int serverIDBytes) = StringN.DecodeBytes(packet._Data);

            byte[] remainingBytes = packet._Data[serverIDBytes..];
            (byte[] PublicKey, int PublicKeyBytes) = PrefixedArray.DecodeBytes(remainingBytes);

            remainingBytes = remainingBytes[PublicKeyBytes..];
            (byte[] VerifyToken, int VerifyTokenBytes) = PrefixedArray.DecodeBytes(remainingBytes);

            remainingBytes = remainingBytes[VerifyTokenBytes..];
            bool ShouldAuth = remainingBytes[0] == 0x01;

            try
            {
                string hash = Core_Engine
                    .GetModule<Networking.Networking>("Networking")!
                    .GetServerConnection(packet._RemoteHost)!
                    ._Encryption.GenerateMinecraftAuthenticationHash(serverID, PublicKey);
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
                                    .GetServerConnection(packet._RemoteHost)!
                                    ._Encryption._SharedSecret,
                                false
                            ),
                            rsa.Encrypt(VerifyToken, false)
                        );
                    Core_Engine
                        .GetModule<Networking.Networking>("Networking")!
                        .SendPacket(packet._RemoteHost, encryptionResponsePacket);
                }
                //Logging.LogDebug("Set Encryption True");
                Core_Engine
                    .GetModule<Networking.Networking>("Networking")!
                    .GetServerConnection(packet._RemoteHost)!
                    ._MinecraftPacketHandler._IsEncryptionEnabled = true;
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
            authModel.accessToken = mojangLogin._MinecraftAuth!.access_token;
            authModel.selectedProfile = mojangLogin._UserProfile!.id.Replace("-", "");
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
            (int CompresionThreshold, _) = VarInt_VarLong.DecodeVarInt(packet._Data);
            //Logging.LogDebug("CompresionThreshold:" + CompresionThreshold);
            ServerConnection connection = Core_Engine
                .GetModule<Networking.Networking>("Networking")!
                .GetServerConnection(packet._RemoteHost)!;
            connection._MinecraftPacketHandler._CompresionThreshold = CompresionThreshold;
            connection._MinecraftPacketHandler._IsCompressionEnabled = true;
        }

        public void HandleLoginSuccess(MinecraftServerPacket packet)
        {
            LoginSuccessPacket loginSuccessPacket = new();
            loginSuccessPacket.DecodeFromBytes(packet._Data);
            /* Logging.LogInfo(
                $"Login Success: {packet.data.Length} bytes; UUID:{loginSuccessPacket.uuid}; username:{loginSuccessPacket.Username}"
            ); */
            Logging.LogInfo("Successfully Joined Server!");
            Core_Engine
                .GetModule<Networking.Networking>("Networking")!
                ._IsClientConnectedToPrimaryServer = true;
            /* foreach (LoginSuccessPacketElement element in loginSuccessPacket.elements)
            {
                Logging.LogDebug(
                    $"\tS1:{element.s1}; S2:{element.s2}; optional S3:{(element.optionalS3 ?? "")}"
                );
            } */
            Networking.Networking NetworkModuleCache = Core_Engine.GetModule<Networking.Networking>(
                "Networking"
            )!;
            /* NetworkModuleCache.GetServerConnection(packet.remoteHost)!.connectionState =
                ConnectionState.CONFIGURATION; */

            Core_Engine.signalInteractiveHoldTransfer(
                Core_Engine.State.JoiningServer,
                Core_Engine.State.Configuration
            );

            NetworkModuleCache.SendPacket(packet._RemoteHost, new EmptyPacket(0x03));
            Core_Engine.InvokeEvent(
                nameof(RegisteredEventIdentifiers.SERVERLOGIN_loginSuccessful),
                new ConnectionEventArgs(packet._RemoteHost)
            );
            Core_Engine.InvokeEvent(
                nameof(RegisteredEventIdentifiers.CONFIG_Start_Config_Process),
                new ConnectionEventArgs(packet._RemoteHost)
            );
        }

        internal void HandlePluginRequest(MinecraftServerPacket packet)
        {
            (int value, int offset) = VarInt_VarLong.DecodeVarInt(packet._Data);
            Identifier channel = new();
            offset += channel.GetFromBytes(packet._Data[offset..]);

            PluginMessageReceivedEventArgs args = new(
                packet._RemoteHost,
                ConnectionState.LOGIN,
                channel,
                packet._Data[offset..],
                value
            );

            Core_Engine.InvokeEvent("PLUGIN_Packet_Received", args);
        }

        internal void HandleCookieRequest(MinecraftServerPacket packet)
        {
            Identifier Key = new();
            Key.GetFromBytes(packet._Data);

            var cookie = _GameStateHandler.GetServerCookie(Key);

            CookieResponsepacket cookieResponsepacket = new()
            {
                _Protocol_ID = 0x04,
                _Key = Key,
                _Payload = cookie?._Payload ?? [],
            };

            _NetworkingManager.SendPacket(packet._RemoteHost, cookieResponsepacket);
        }
    }
}
