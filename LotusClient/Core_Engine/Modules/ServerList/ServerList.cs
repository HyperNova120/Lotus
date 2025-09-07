using System.Net;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Networking;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Packets.ServerBound.Handshake;
using LotusCore.Modules.Networking.Packets.ServerBound.Status;
using LotusCore.Modules.ServerList.Commands;
using LotusCore.Utils;
using LotusCore.Utils;
using LotusCore.Utils.MinecraftPaths;
using LotusCore.Utils.NBTInternals.Tags;
using Silk.NET.OpenGL;

namespace LotusCore.Modules.ServerList
{
    public class ServerList : IModuleBase
    {
        private NBT _ServerListDat;
        private Networking.Networking _networkingModule;

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("list", new ListCommand(_ServerListDat));
        }

        public void RegisterEvents(Action<string> RegisterEvent)
        {
            RegisterEvent.Invoke("ServerListIP_Request");
        }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke(
                "STATUS_Packet_Received",
                new EngineEventHandler(ProcessPacket)
            );
            SubscribeToEvent.Invoke(
                "ServerListIP_Request",
                new EngineEventHandler(
                    (sender, serverName) =>
                    {
                        string[] ipPort = GetServerIPFromName(
                                ((ServerListIPRequestEventArgs)serverName)._serverName
                            )
                            .Split(":");
                        if (ipPort.Length == 0)
                        {
                            //no ip
                            return new ServerListServerIPResult() { _ip = "" };
                        }
                        else if (ipPort.Length == 1)
                        {
                            //only ip
                            return new ServerListServerIPResult() { _ip = ipPort[0] };
                        }
                        else
                        {
                            //ip and port
                            return new ServerListServerIPResult()
                            {
                                _ip = ipPort[0],
                                _port = ipPort[1],
                            };
                        }
                    }
                )
            );
        }

        public ServerList()
        {
            _networkingModule = Core_Engine.GetModule<Networking.Networking>("Networking")!;
            _ServerListDat = new();
            _ServerListDat.ReadFromBytes(File.ReadAllBytes(MinecraftPathsStruct._ServerData));
            //Logging.LogDebug(_ServerListDat.GetNBTAsString());
            _ = PingServerlist();
        }

        private async Task PingServerlist()
        {
            await Task.Delay(100);
            var servers = _ServerListDat.TryGetTag<TAG_List>("servers");
            if (servers == null)
            {
                return;
            }
            foreach (TAG_Compound tmp in servers._Contained_Tags.OfType<TAG_Compound>())
            {
                /* TAG_String? name = (TAG_String?)tmp.TryGetTag("name");
                Logging.LogDebug(name.Value); */
                TAG_Byte? isHidden = (TAG_Byte?)tmp.TryGetTag("hidden");
                if (isHidden != null && isHidden.Value == 0x01)
                {
                    continue;
                }
                _ = HandshakeServer(tmp);
            }
        }

        private async Task<bool> HandshakeServer(TAG_Compound tmp)
        {
            TAG_String? ip = (TAG_String?)tmp.TryGetTag("ip");
            TAG_String? serverName = (TAG_String?)tmp.TryGetTag("name");
            if (ip == null)
            {
                return false;
            }
            IPAddress remoteHost;
            try
            {
                remoteHost = (await Dns.GetHostAddressesAsync(ip.Value))[0];
            }
            catch (Exception e)
            {
                //no such host
                return false;
            }
            //Logging.LogDebug($"ServerName:{serverName!.Value} IP:{remoteHost.ToString()}");
            ServerConnection? connection = _networkingModule.GetServerConnection(remoteHost);
            if (connection == null)
            {
                if (!_networkingModule.ConnectToServer(remoteHost.ToString()))
                {
                    //connection refused
                    return false;
                }
                connection = _networkingModule.GetServerConnection(remoteHost)!;
            }
            connection._ServerListEntry = tmp;
            connection._ConnectionState = Networking.Networking.ConnectionState.STATUS;
            _networkingModule.SendPacket(
                remoteHost,
                new HandshakePacket(ip.Value, HandshakePacket.Intent.Status, 25565)
                {
                    _Protocol_ID = 0x00,
                }
            );
            SendStatusRequest(remoteHost);
            return true;
        }

        public EngineEventResult? ProcessPacket(object? sender, IEngineEventArgs args)
        {
            PacketReceivedEventArgs eventArgs = (PacketReceivedEventArgs)args;
            var packet = eventArgs._Packet;
            //Logging.LogDebug($"StatusHandler State 0x{packet._Protocol_ID:X}");
            switch (packet._Protocol_ID)
            {
                case 0x00:
                    HandleStatusResponse(packet);
                    break;
                case 0x01:
                    HandlePingResponse(packet);
                    break;
                default:
                    Logging.LogError(
                        $"StatusHandler State 0x{packet._Protocol_ID:X} Not Implemented"
                    );
                    Core_Engine
                        .GetModule<Networking.Networking>("Networking")!
                        .DisconnectFromServer(eventArgs._RemoteHost);
                    break;
            }
            return null;
        }

        private void HandlePingResponse(MinecraftServerPacket packet)
        {
            try
            {
                long value = NetworkLong.DecodeBytes(packet._Data);
                ServerConnection connection = _networkingModule.GetServerConnection(
                    packet._RemoteHost
                )!;
                connection._LastPingLength = (
                    DateTime.UtcNow - connection._LastPingTime
                ).TotalMilliseconds;

                connection._ServerListEntry.WriteTag<TAG_Double>(
                    new TAG_Double() { _Name = "ping", Value = connection._LastPingLength }
                );
                /* Logging.LogDebug(
                    $"Response: {value} Ping:{(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - value}m"
                ); */
            }
            catch (Exception e)
            {
                Logging.LogDebug(e.ToString());
            }
            _networkingModule.DisconnectFromServer(packet._RemoteHost);
        }

        private void HandleStatusResponse(MinecraftServerPacket packet)
        {
            (string value, int size) = StringN.DecodeBytes(packet._Data);
            //Logging.LogDebug($"Response Size: {size}\n{value.Replace("\r", "").Replace("\n", "")}");
            ServerConnection connection = _networkingModule.GetServerConnection(
                packet._RemoteHost
            )!;
            connection._ServerListEntry.WriteTag<TAG_String>(
                new TAG_String()
                {
                    _Name = "serverlist_info",
                    Value = value.Replace("\r", "").Replace("\n", ""),
                }
            );
            SendPingRequest(packet._RemoteHost);
        }

        private void SendPingRequest(IPAddress remoteHost)
        {
            ServerConnection connection = _networkingModule.GetServerConnection(remoteHost)!;
            var connectionState = connection._ConnectionState;
            if (connectionState == Networking.Networking.ConnectionState.STATUS)
            {
                connection._LastPingTime = DateTime.UtcNow;
                _networkingModule.SendPacket(connection._RemoteHost, new StatusPingRequestPacket());
            }
        }

        private void SendStatusRequest(IPAddress remoteHost)
        {
            ServerConnection connection = _networkingModule.GetServerConnection(remoteHost)!;
            var connectionState = connection._ConnectionState;
            if (connectionState == Networking.Networking.ConnectionState.STATUS)
            {
                _networkingModule.SendPacket(connection._RemoteHost, new EmptyPacket(0x00));
            }
        }

        private string GetServerIPFromName(string name)
        {
            TAG_List? servers = _ServerListDat.TryGetTag<TAG_List>("servers");
            if (servers == null)
            {
                return "";
            }
            foreach (var cur in servers._Contained_Tags)
            {
                TAG_Compound curTag = (TAG_Compound)cur;
                if (((TAG_String)curTag.TryGetTag("name")!).Value == name)
                {
                    return ((TAG_String)curTag.TryGetTag("ip")!).Value;
                }
            }
            
            return "";
        }
    }
}
