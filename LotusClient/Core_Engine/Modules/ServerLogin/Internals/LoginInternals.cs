using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEventArgs;
using LotusCore.Interfaces;
using LotusCore.Modules.GameStateHandlerModule;
using LotusCore.Modules.GameStateHandlerModule.Types;
using LotusCore.Modules.MojangLogin.Types;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Models;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Packets.ClientBound.Login;
using LotusCore.Modules.Networking.Packets.ClientBound.Login.Internals;
using LotusCore.Modules.Networking.Packets.ServerBound.Configuration;
using LotusCore.Modules.Networking.Packets.ServerBound.Handshake;
using LotusCore.Modules.Networking.Packets.ServerBound.Login;
using LotusCore.Modules.Networking.Types;
using LotusCore.Utils;
using static LotusCore.Modules.Networking.Networking;

namespace LotusCore.Modules.ServerLogin.Internals
{
    public class ServerLoginInternals
    {
        private enum RegisteredEventIdentifiers
        {
            SERVERLOGIN_loginSuccessful,
            CONFIG_Start_Config_Process,
        }

        public ServerLoginInternals() { }

        public void HandleLoginDisconnect(MinecraftServerPacket packet)
        {
            Logging.LogInfo(
                $"Client disconnected during login, Reason:{Encoding.UTF8.GetString(packet._data)}"
            );
            NBT test = new(true);
            test.ReadFromBytes(packet._data, true);
            Console.WriteLine(test.GetNBTAsString());
            Core_Engine.InvokeEvent(
                "NETWORKING_DisconnectFromServer",
                new GuidEngineArgs(packet._remoteHostID)
            );
            //Core_Engine.CurrentState = Core_Engine.State.Interactive;
            Core_Engine.SignalInteractiveFree(Core_Engine.State.JoiningServer);
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
                string hash = Core_Engine
                    .InvokeEvent<ServerConnectionResult>(
                        "NETWORKING_GetServerConnection",
                        new GuidEngineArgs(packet._remoteHostID)
                    )!
                    ._serverConnection!._encryption.GenerateMinecraftAuthenticationHash(
                        serverID,
                        PublicKey
                    );
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
                                    .InvokeEvent<ServerConnectionResult>(
                                        "NETWORKING_GetServerConnection",
                                        new GuidEngineArgs(packet._remoteHostID)
                                    )!
                                    ._serverConnection!._encryption._SharedSecret,
                                false
                            ),
                            rsa.Encrypt(VerifyToken, false)
                        );

                    Core_Engine.InvokeEvent(
                        "NETWORKING_SendPacket",
                        new SendPacketArgs(packet._remoteHostID, encryptionResponsePacket)
                    );
                }
                //Logging.LogDebug("Set Encryption True");
                Core_Engine
                    .InvokeEvent<ServerConnectionResult>(
                        "NETWORKING_GetServerConnection",
                        new GuidEngineArgs(packet._remoteHostID)
                    )!
                    ._serverConnection!._minecraftPacketHandler._IsEncryptionEnabled = true;
            }
            catch (Exception e)
            {
                Logging.LogError("HandleEncryptionRequest:" + e.ToString());
            }
        }

        private async Task<bool> AuthenticateWithMinecraftServer(string serverHash)
        {
            MinecraftServerAuthModel authModel = new MinecraftServerAuthModel();
            authModel.accessToken = Core_Engine
                .InvokeEvent<MinecraftAuthResult>("MOJANGLOGIN_GetMinecraftAuth", null)!
                ._minecraftAuth!.access_token;

            authModel.selectedProfile = Core_Engine
                .InvokeEvent<UserProfileResult>("MOJANGLOGIN_GetUserProfile", null)!
                ._userProfile!.id.Replace("-", "");
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
            ServerConnection connection = Core_Engine
                .InvokeEvent<ServerConnectionResult>(
                    "NETWORKING_GetServerConnection",
                    new GuidEngineArgs(packet._remoteHostID)
                )!
                ._serverConnection!;
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
            Logging.LogInfo("Successfully Joined Server!");

            Core_Engine.InvokeEvent(
                "NETWORKING_SetIsClientConnectedToPrimaryServer",
                new BoolEngineArgs(true)
            );
            /* foreach (LoginSuccessPacketElement element in loginSuccessPacket.elements)
            {
                Logging.LogDebug(
                    $"\tS1:{element.s1}; S2:{element.s2}; optional S3:{(element.optionalS3 ?? "")}"
                );
            } */
            /* NetworkModuleCache.GetServerConnection(packet.remoteHost)!.connectionStatF =
                ConnectionState.CONFIGURATION; */

            Core_Engine.signalInteractiveHoldTransfer(
                Core_Engine.State.JoiningServer,
                Core_Engine.State.Configuration
            );

            Core_Engine.InvokeEvent(
                "NETWORKING_SendPacket",
                new SendPacketArgs(packet._remoteHostID, new EmptyPacket(0x03))
            );
            Core_Engine.InvokeEvent(
                nameof(RegisteredEventIdentifiers.SERVERLOGIN_loginSuccessful),
                new ConnectionEventArgs(packet._remoteHostID)
            );
            Core_Engine.InvokeEvent(
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

            Core_Engine.InvokeEvent("PLUGIN_Packet_Received", args);
        }

        internal void HandleCookieRequest(MinecraftServerPacket packet)
        {
            Identifier key = new();
            int offset = 0;
            key.GetFromBytes(packet._data, ref offset);
            var cookie = Core_Engine
                .InvokeEvent<ServerCookieResult>(
                    "GAMESTATE_GetServerCookie",
                    new IdentifierEngineArgs(key)
                )!
                ._serverCookie;

            CookieResponsepacket cookieResponsepacket = new()
            {
                _protocol_ID = 0x04,
                _Key = key,
                _Payload = cookie?._Payload ?? [],
            };

            Core_Engine.InvokeEvent(
                "NETWORKING_SendPacket",
                new SendPacketArgs(packet._remoteHostID, cookieResponsepacket)
            );
        }
    }
}
