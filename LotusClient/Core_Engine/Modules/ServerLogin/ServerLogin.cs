using System.Net;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.MojangLogin.Commands;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Packets.ServerBound.Handshake;
using LotusCore.Modules.Networking.Packets.ServerBound.Login;
using LotusCore.Modules.ServerLogin.Commands;
using LotusCore.Modules.ServerLogin.Internals;
using static LotusCore.Modules.Networking.Networking;

namespace LotusCore.Modules.ServerLogin
{
    public class LoginHandler : IModuleBase
    {
        private readonly ServerLoginInternals internals = new();

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("directJoin", new DirectJoinCommand());
            RegisterCommand.Invoke("Join", new JoinCommand());
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
                MinecraftServerPacket packet = eventArgs._Packet;
                switch (packet._Protocol_ID)
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
                            $"LoginHandler State 0x{packet._Protocol_ID:X} Not Implemented"
                        );
                        Core_Engine
                            .GetModule<Networking.Networking>("Networking")!
                            .DisconnectFromServer(eventArgs._RemoteHost);
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

        public void LoginToServer(string serverIp, ushort port = 25565)
        {
            Networking.Networking networking = Core_Engine.GetModule<Networking.Networking>(
                "Networking"
            )!;
            MojangLogin.MojangLogin mojangLogin = Core_Engine.GetModule<MojangLogin.MojangLogin>(
                "MojangLogin"
            )!;

            IPAddress remoteHost = Dns.GetHostAddresses(serverIp)[0];

            if (mojangLogin._UserProfile == null)
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
                if (networking.GetServerConnection(remoteHost) == null)
                {
                    networking.ConnectToServer(remoteHost.ToString(), port);
                    networking.GetServerConnection(remoteHost)!._ConnectionState =
                        ConnectionState.LOGIN;
                    networking.SendPacket(
                        remoteHost,
                        new HandshakePacket(serverIp, HandshakePacket.Intent.Login, port)
                    );
                    /* Logging.LogDebug(
                        $"ServerLogin; LoginToServer; username:{mojangLogin.userProfile!.name}; uuid:{new Guid(mojangLogin.userProfile!.id)}"
                    ); */
                    networking.SendPacket(
                        remoteHost,
                        new LoginStartPacket(
                            mojangLogin._UserProfile!.name,
                            new Guid(mojangLogin._UserProfile!.id)
                        )
                    );
                }
            }
            catch (Exception e)
            {
                Logging.LogError($"LoginToServer Failed: {e.ToString()}");
                networking.DisconnectFromServer(remoteHost);
                Core_Engine.SignalInteractiveFree(Core_Engine.State.JoiningServer);
            }
        }
    }
}
