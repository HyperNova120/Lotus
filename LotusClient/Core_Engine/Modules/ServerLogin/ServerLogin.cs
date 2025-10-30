using System.Net;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.LotusNetty;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Handshake;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Login;
using LotusCore.Modules.LotusNetty.Types;
using LotusCore.Modules.MojangLogin.Commands;
using LotusCore.Modules.MojangLogin.Models;
using LotusCore.Modules.MojangLogin.Types;
using LotusCore.Modules.ServerLogin.Commands;
using LotusCore.Modules.ServerLogin.Internals;
using LotusCore.Utils;
using static LotusCore.Modules.LotusNetty.Networking;

namespace LotusCore.Modules.ServerLogin
{
    public class LoginHandler : IModuleBase
    {
        private ServerLoginInternals _internals;

        private INetworkModule _networkModule;

        private IMojangLoginModule _mojangLoginModule;

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

        public void LinkModules()
        {
            _networkModule = Core_Engine.GetModule<INetworkModule>("Networking")!;
            _mojangLoginModule = Core_Engine.GetModule<IMojangLoginModule>("MojangLogin")!;
            _internals = new(
                _networkModule,
                _mojangLoginModule,
                Core_Engine.GetModule<IGameStateHandlerModule>("GameStateHandler")!
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
                        _internals.HandleLoginDisconnect(packet);
                        break;
                    case 0x01:
                        await _internals.HandleEncryptionRequest(packet);
                        break;
                    case 0x02:
                        _internals.HandleLoginSuccess(packet);
                        break;
                    case 0x03:
                        _internals.HandleSetCompression(packet);
                        break;
                    case 0x04:
                        _internals.HandlePluginRequest(packet);
                        break;
                    case 0x05:
                        _internals.HandleCookieRequest(packet);
                        break;
                    default:
                        Logging.LogError(
                            $"LoginHandler State 0x{packet._protocol_ID:X} Not Implemented"
                        );
                        _networkModule.DisconnectFromServer(eventArgs._remoteHostID);
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

            if (_mojangLoginModule.GetUserProfile() == null)
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
                Guid? conGuid = _networkModule.GetServerConnectionInState(
                    remoteHost,
                    [ConnectionState.PLAY, ConnectionState.CONFIGURATION, ConnectionState.LOGIN]
                );

                if (conGuid != null)
                {
                    _networkModule.DisconnectFromServer((Guid)conGuid);
                }

                conGuid = _networkModule.ConnectToServer(remoteHost.ToString(), port);

                if (conGuid == null)
                {
                    Logging.LogInfo("Unable to connect to server");
                    Core_Engine.SignalInteractiveFree(Core_Engine.State.JoiningServer);
                    return;
                }

                _networkModule.GetServerConnection((Guid)conGuid)!._connectionState =
                    ConnectionState.LOGIN;

                _networkModule.SendPacket(
                    (Guid)conGuid,
                    new HandshakePacket(serverIp, HandshakePacket.Intent.Login, port)
                    {
                        _NextState = isTransfer
                            ? (int)HandshakePacket.Intent.Transfer
                            : (int)HandshakePacket.Intent.Login,
                    }
                );

                MinecraftProfile userProfile = _mojangLoginModule.GetUserProfile()!;

                _networkModule.SendPacket(
                    (Guid)conGuid,
                    new LoginStartPacket(userProfile.name, new Guid(userProfile.id))
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
