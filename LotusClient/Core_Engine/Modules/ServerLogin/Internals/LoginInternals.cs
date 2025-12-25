using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEventArgs;
using LotusCore.Interfaces;
using LotusCore.Modules.GameStateHandlerModule;
using LotusCore.Modules.GameStateHandlerModule.Types;
using LotusCore.Modules.LotusNetty;
using LotusCore.Modules.LotusNetty.Internals;
using LotusCore.Modules.LotusNetty.Models;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Modules.LotusNetty.Packets.ClientBound.Login;
using LotusCore.Modules.LotusNetty.Packets.ClientBound.Login.Internals;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Configuration;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Handshake;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Login;
using LotusCore.Modules.LotusNetty.Types;
using LotusCore.Modules.MojangLogin.Types;
using LotusCore.Utils;
using static LotusCore.Modules.LotusNetty.Networking;

namespace LotusCore.Modules.ServerLogin.Internals
{
    public class ServerLoginInternals
    {
        private enum RegisteredEventIdentifiers
        {
            SERVERLOGIN_loginSuccessful,
            CONFIG_Start_Config_Process,
        }

        private INetworkModule _networking;

        private IMojangLoginModule _mojangLogin;

        private IGameStateHandlerModule _gameStateHandler;

        ICoreModule? _coreModule;

        public ServerLoginInternals(
            ICoreModule coreModule,
            INetworkModule networking,
            IMojangLoginModule mojangLogin,
            IGameStateHandlerModule gameStateHandler
        )
        {
            _coreModule = coreModule;
            _networking = networking;
            _mojangLogin = mojangLogin;
            _gameStateHandler = gameStateHandler;
        }

        public void HandleLoginDisconnect(MinecraftServerPacket packet)
        {
            Logging.LogInfo(
                $"Client disconnected during login, Reason:{Encoding.UTF8.GetString(packet._data)}"
            );
            NBT test = new(true);
            test.ReadFromBytes(packet._data, true);
            Console.WriteLine(test.GetNBTAsString());
            _networking.DisconnectFromServer(packet._remoteHostID);
            //Core_Engine.CurrentState = Core_Engine.State.Interactive;
            _coreModule!.SignalInteractiveFree(Core_Engine.State.JoiningServer);
        }

        public async Task HandleEncryptionRequest(MinecraftServerPacket packet)
        {
            int offset = 0;
            string serverID = StringN.DecodeBytes(packet._data, ref offset);

            byte[] remainingBytes = packet._data[offset..];
            (byte[] PublicKey, int PublicKeyBytes) = PrefixedArray.DecodeBytes(remainingBytes);

            remainingBytes = remainingBytes[PublicKeyBytes..];
            (byte[] VerifyToken, int VerifyTokenBytes) = PrefixedArray.DecodeBytes(remainingBytes);

            remainingBytes = remainingBytes[VerifyTokenBytes..];
            bool ShouldAuth = remainingBytes[0] == 0x01;

            try
            {
                string hash = _networking
                    .GetServerConnection(packet._remoteHostID)!
                    ._encryption.GenerateMinecraftAuthenticationHash(serverID, PublicKey);
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
                                _networking
                                    .GetServerConnection(packet._remoteHostID)!
                                    ._encryption._SharedSecret,
                                false
                            ),
                            rsa.Encrypt(VerifyToken, false)
                        );

                    _networking.SendPacket(packet._remoteHostID, encryptionResponsePacket);
                }
                //Logging.LogDebug("Set Encryption True");
                _networking
                    .GetServerConnection(packet._remoteHostID)!
                    ._minecraftPacketHandler._IsEncryptionEnabled = true;
            }
            catch (Exception e)
            {
                Logging.LogError("HandleEncryptionRequest:" + e.ToString());
            }
        }

        private async Task<bool> AuthenticateWithMinecraftServer(string serverHash)
        {
            MinecraftServerAuthModel authModel = new MinecraftServerAuthModel();
            authModel.accessToken = _mojangLogin.GetMinecraftAuth()!.access_token;

            authModel.selectedProfile = _mojangLogin.GetUserProfile()!.id.Replace("-", "");
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
            int offset = 0;
            int CompresionThreshold = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);
            //Logging.LogDebug("CompresionThreshold:" + CompresionThreshold);
            ServerConnection connection = _networking.GetServerConnection(packet._remoteHostID)!;
            connection._minecraftPacketHandler._CompresionThreshold = CompresionThreshold;
            connection._minecraftPacketHandler._IsCompressionEnabled = true;
        }

        public void HandleLoginSuccess(MinecraftServerPacket packet)
        {
            LoginSuccessPacket loginSuccessPacket = new();
            loginSuccessPacket.DecodeFromBytes(packet._data);
            /* Logging.LogInfo(
                $"Login Success: {packet.data.Length} bytes; UUID:{loginSuccessPacket.uuid}; username:{loginSuccessPacket.Username}"
            ); */

            //Logging.LogDebug($"HandleLoginSuccess: test1:{loginSuccessPacket._uuid._UUID:X}");
            MinecraftUUID test = new();
            int offset = 0;
            test.DecodeBytes(packet._data, ref offset);
            //Logging.LogDebug($"HandleLoginSuccess: test2:{test._UUID:X}");

            Logging.LogInfo("Successfully Joined Server!");

            _networking.SetIsClientConnectedToPrimaryServer(true);
            /* foreach (LoginSuccessPacketElement element in loginSuccessPacket.elements)
            {
                Logging.LogDebug(
                    $"\tS1:{element.s1}; S2:{element.s2}; optional S3:{(element.optionalS3 ?? "")}"
                );
            } */
            /* NetworkModuleCache.GetServerConnection(packet.remoteHost)!.connectionStatF =
                ConnectionState.CONFIGURATION; */

            _coreModule!.SignalInteractiveHoldTransfer(
                Core_Engine.State.JoiningServer,
                Core_Engine.State.Configuration
            );

            _networking.SendPacket(packet._remoteHostID, new EmptyPacket(0x03));
            _networking.LoginSuccessful(packet._remoteHostID);
            _coreModule!.InvokeEvent(
                nameof(RegisteredEventIdentifiers.CONFIG_Start_Config_Process),
                new ConnectionEventArgs(packet._remoteHostID)
            );
        }

        internal void HandlePluginRequest(MinecraftServerPacket packet)
        {
            int offset = 0;
            int value = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);
            Identifier channel = new();
            channel.GetFromBytes(packet._data, ref offset);

            PluginMessageReceivedEventArgs args = new(
                packet._remoteHostID,
                ConnectionState.LOGIN,
                channel,
                packet._data[offset..],
                value
            );

            _coreModule!.InvokeEvent("PLUGIN_Packet_Received", args);
        }

        internal void HandleCookieRequest(MinecraftServerPacket packet)
        {
            Identifier key = new();
            int offset = 0;
            key.GetFromBytes(packet._data, ref offset);
            var cookie = _gameStateHandler.GetServerCookie(key);

            CookieResponsepacket cookieResponsepacket = new()
            {
                _protocol_ID = 0x04,
                _Key = key,
                _Payload = cookie?._Payload ?? [],
            };

            _networking.SendPacket(packet._remoteHostID, cookieResponsepacket);
        }
    }
}
