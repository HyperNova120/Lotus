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
                        Task.Run(async () =>
                            {
                                await ProcessPacket(sender, args);
                            })
                            .GetAwaiter()
                            .GetResult();
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
                        Logging.LogDebug("Disconnect Packet Received");
                        await internals.HandleLoginDisconnect(packet);
                        break;
                    case 0x01:
                        Logging.LogDebug("Encryption Request Packet Received");
                        await internals.HandleEncryptionRequest(packet);
                        break;
                    case 0x02:
                        Logging.LogDebug("Login Success Packet Received");
                        await internals.HandleLoginSuccess(packet);
                        break;
                    case 0x03:

                        Logging.LogDebug("Compression Packet Received");
                        internals.HandleSetCompression(packet);
                        break;
                    default:
                        Logging.LogError(
                            $"LoginHandler State 0x{packet.protocol_id:X} Not Implemented"
                        );
                        Core_Engine
                            .GetModule<Networking.Networking>("Networking")!
                            .DisconnectFromServer();
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.LogError($"LoginHandler; ProcessPacket ERROR: {e}");
            }
        }

        public async Task LoginToServer(string serverIp, ushort port = 25565)
        {
            Networking.Networking networking = Core_Engine.GetModule<Networking.Networking>(
                "Networking"
            )!;
            MojangLogin.MojangLogin mojangLogin = Core_Engine.GetModule<MojangLogin.MojangLogin>(
                "MojangLogin"
            )!;

            if (mojangLogin.userProfile == null)
            {
                Console.WriteLine("You are not signed into a Minecraft account");
                return;
            }
            try
            {
                if (
                    networking.connectionState == ConnectionState.NONE
                    || networking.connectionState == ConnectionState.STATUS
                )
                {
                    networking.connectionState = ConnectionState.LOGIN;
                    networking.ConnectToServer(serverIp, port);
                    networking.SendPacket(
                        new HandshakePacket(serverIp, HandshakePacket.Intent.Login, port)
                    );
                    Logging.LogDebug(
                        $"ServerLogin; LoginToServer; username:{mojangLogin.userProfile!.name}; uuid:{new Guid(mojangLogin.userProfile!.id)}"
                    );
                    networking.SendPacket(
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
                networking.DisconnectFromServer();
                networking.connectionState = ConnectionState.NONE;
            }
        }
    }
}
