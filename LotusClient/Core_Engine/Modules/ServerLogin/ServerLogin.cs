using System.Net;
using Core_Engine.EngineEventArgs;
using Core_Engine.Interfaces;
using Core_Engine.Modules.MojangLogin.Commands;
using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.Networking.Packets.ServerBound.Handshake;
using Core_Engine.Modules.Networking.Packets.ServerBound.Login;
using Core_Engine.Modules.ServerLogin.Commands;
using Core_Engine.Modules.ServerLogin.Internals;
using static Core_Engine.Modules.Networking.Networking;

namespace Core_Engine.Modules.ServerLogin
{
    public class LoginHandler : IModuleBase
    {
        private readonly ServerLoginInternals internals = new();

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("join", new JoinCommand());
        }

        public void RegisterEvents(Action<string> RegisterEvent)
        {
            RegisterEvent.Invoke("SERVERLOGIN_loginSuccessful");
        }

        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke(
                "LOGIN_Packet_Received",
                new EventHandler(
                    (sender, args) =>
                    {
                        _ = ProcessPacket(sender, args);
                    }
                )
            );
        }

        public async Task ProcessPacket(object? sender, EventArgs args)
        {
            try
            {
                PacketReceivedEventArgs eventArgs = (PacketReceivedEventArgs)args;
                MinecraftServerPacket packet = eventArgs.packet;
                switch (packet.protocol_id)
                {
                    case 0x00:
                        //Logging.LogDebug("Disconnect Packet Received");
                        internals.HandleLoginDisconnect(packet);
                        break;
                    case 0x01:
                        //Logging.LogDebug("Encryption Request Packet Received");
                        await internals.HandleEncryptionRequest(packet);
                        break;
                    case 0x02:
                        //Logging.LogDebug("Login Success Packet Received");
                        internals.HandleLoginSuccess(packet);
                        break;
                    case 0x03:

                        //Logging.LogDebug("Compression Packet Received");
                        internals.HandleSetCompression(packet);
                        break;
                    default:
                        Logging.LogError(
                            $"LoginHandler State 0x{packet.protocol_id:X} Not Implemented"
                        );
                        Core_Engine
                            .GetModule<Networking.Networking>("Networking")!
                            .DisconnectFromServer(eventArgs.remoteHost);
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.LogError($"LoginHandler; ProcessPacket ERROR: {e}");
                if (Core_Engine.CurrentState == Core_Engine.State.Waiting)
                {
                    Core_Engine.CurrentState = Core_Engine.State.Interactive;
                }
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

            if (mojangLogin.userProfile == null)
            {
                Console.WriteLine("You are not signed into a Minecraft account");
                if (Core_Engine.CurrentState == Core_Engine.State.Waiting)
                {
                    Core_Engine.CurrentState = Core_Engine.State.Interactive;
                }
                return;
            }
            try
            {
                if (networking.GetServerConnection(remoteHost) == null)
                {
                    networking.ConnectToServer(remoteHost.ToString(), port);
                    networking.GetServerConnection(remoteHost)!.connectionState =
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
                            mojangLogin.userProfile!.name,
                            new Guid(mojangLogin.userProfile!.id)
                        )
                    );
                }
            }
            catch (Exception e)
            {
                Logging.LogError($"LoginToServer Failed: {e.ToString()}");
                networking.DisconnectFromServer(remoteHost);
            }
        }
    }
}
