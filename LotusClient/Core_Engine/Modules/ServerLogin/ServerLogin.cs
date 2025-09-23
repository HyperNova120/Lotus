using System.Net;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.MojangLogin.Commands;
using LotusCore.Modules.MojangLogin.Models;
using LotusCore.Modules.MojangLogin.Types;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Packets.ServerBound.Handshake;
using LotusCore.Modules.Networking.Packets.ServerBound.Login;
using LotusCore.Modules.Networking.Types;
using LotusCore.Modules.ServerLogin.Commands;
using LotusCore.Modules.ServerLogin.Internals;
using LotusCore.Utils;
using static LotusCore.Modules.Networking.Networking;

namespace LotusCore.Modules.ServerLogin
{
    public class LoginHandler : IModuleBase
    {
        private readonly ServerLoginInternals internals = new();

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("join", new JoinCommand(this));
            RegisterCommand.Invoke("listjoin", new ListJoinCommand());
        }

        public void RegisterEvents(Action<string> RegisterEvent)
        {
            RegisterEvent.Invoke("SERVERLOGIN_loginSuccessful");
            RegisterEvent.Invoke("CONFIG_Start_Config_Process");
        }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke(
                "LOGIN_Packet_Received",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        _ = ProcessPacket(sender, args);
                        return null;
                    }
                )
            );
        }

        public async Task ProcessPacket(object? sender, IEngineEventArgs args)
        {
            try
            {
                PacketReceivedEventArgs eventArgs = (PacketReceivedEventArgs)args;
                MinecraftServerPacket packet = eventArgs._packet;
                switch (packet._protocol_ID)
                {
                    case 0x00:
                        internals.HandleLoginDisconnect(packet);
                        break;
                    case 0x01:
                        await internals.HandleEncryptionRequest(packet);
                        break;
                    case 0x02:
                        internals.HandleLoginSuccess(packet);
                        break;
                    case 0x03:
                        internals.HandleSetCompression(packet);
                        break;
                    case 0x04:
                        internals.HandlePluginRequest(packet);
                        break;
                    case 0x05:
                        internals.HandleCookieRequest(packet);
                        break;
                    default:
                        Logging.LogError(
                            $"LoginHandler State 0x{packet._protocol_ID:X} Not Implemented"
                        );

                        Core_Engine.InvokeEvent(
                            "NETWORKING_DisconnectFromServer",
                            new GuidEngineArgs(eventArgs._remoteHostID)
                        );
                        Core_Engine.SignalInteractiveFree(Core_Engine.State.JoiningServer);
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.LogError($"LoginHandler; ProcessPacket ERROR: {e}");
                /* if (Core_Engine.CurrentState == Core_Engine.State.Waiting)
                {
                    Core_Engine.CurrentState = Core_Engine.State.Interactive;
                } */
                Core_Engine.SignalInteractiveFree(Core_Engine.State.JoiningServer);
            }
        }

        public void LoginToServer(string serverIp, bool isTransfer, ushort port = 25565)
        {
            /* Networking.Networking networking = Core_Engine.GetModule<Networking.Networking>(
                "Networking"
            )!;
            MojangLogin.MojangLogin mojangLogin = Core_Engine.GetModule<MojangLogin.MojangLogin>(
                "MojangLogin"
            )!; */

            IPAddress remoteHost;

            (string? serverIP, int? srvPort) = ServerDNSLookup.GetServerDNSRecord(serverIp);
            if (serverIP == null)
            {
                Logging.LogInfo("Invalid Server IP");
                Core_Engine.SignalInteractiveFree(Core_Engine.State.JoiningServer);
                return;
            }
            remoteHost = IPAddress.Parse(serverIP);
            if (port == 25565)
            {
                //only use srv provided port if the default port is provided.
                //ensures users take priority over dns record
                port = (ushort)(srvPort ?? 25565);
            }

            if (
                Core_Engine
                    .InvokeEvent<UserProfileResult>("MOJANGLOGIN_GetUserProfile", null)!
                    ._userProfile == null
            )
            {
                Console.WriteLine("You are not signed into a Minecraft account");
                /* if (Core_Engine.CurrentState == Core_Engine.State.Waiting)
                {
                    Core_Engine.CurrentState = Core_Engine.State.Interactive;
                } */
                Core_Engine.SignalInteractiveFree(Core_Engine.State.JoiningServer);
                return;
            }
            try
            {
                Logging.LogDebug($"\tisTransfer:{isTransfer}");
                var conGuid = Core_Engine
                    .InvokeEvent<GuidResult>(
                        "NETWORKING_GetServerConnectionInState",
                        new GetServerConnectionInStateArgs(
                            remoteHost,
                            [
                                ConnectionState.PLAY,
                                ConnectionState.CONFIGURATION,
                                ConnectionState.LOGIN,
                            ]
                        )
                    )!
                    ._result;
                if (conGuid != null)
                {
                    //disconnect if already connected
                    Core_Engine.InvokeEvent(
                        "NETWORKING_DisconnectFromServer",
                        new GuidEngineArgs((Guid)conGuid)
                    );
                }

                conGuid = Core_Engine
                    .InvokeEvent<GuidResult>(
                        "NETWORKING_ConnectToServer",
                        new ConnectToServerArgs(remoteHost.ToString(), port)
                    )!
                    ._result;

                if (conGuid == null)
                {
                    Logging.LogInfo("Unable to connect to server");
                    Core_Engine.SignalInteractiveFree(Core_Engine.State.JoiningServer);
                    return;
                }

                Core_Engine
                    .InvokeEvent<ServerConnectionResult>(
                        "NETWORKING_GetServerConnection",
                        new GuidEngineArgs((Guid)conGuid)
                    )!
                    ._serverConnection!._connectionState = ConnectionState.LOGIN;

                Core_Engine.InvokeEvent(
                    "NETWORKING_SendPacket",
                    new SendPacketArgs(
                        (Guid)conGuid,
                        new HandshakePacket(serverIp, HandshakePacket.Intent.Login, port)
                        {
                            _NextState = isTransfer
                                ? (int)HandshakePacket.Intent.Transfer
                                : (int)HandshakePacket.Intent.Login,
                        }
                    )
                );

                MinecraftProfile userProfile = Core_Engine
                    .InvokeEvent<UserProfileResult>("MOJANGLOGIN_GetUserProfile", null)!
                    ._userProfile!;

                Core_Engine.InvokeEvent(
                    "NETWORKING_SendPacket",
                    new SendPacketArgs(
                        (Guid)conGuid,
                        new LoginStartPacket(userProfile.name, new Guid(userProfile.id))
                    )
                );
            }
            catch (Exception e)
            {
                Logging.LogError($"LoginToServer Failed: {e.ToString()}");
                //networking.DisconnectFromServer(remoteHost);
                Core_Engine.SignalInteractiveFree(Core_Engine.State.JoiningServer);
            }
        }
    }
}
