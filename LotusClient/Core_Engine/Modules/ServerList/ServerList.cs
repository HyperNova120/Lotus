using LotusCore.BaseClasses.Types;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.LotusNetty;
using LotusCore.Modules.LotusNetty.Internals;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Handshake;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Status;
using LotusCore.Modules.ServerList.Commands;
using LotusCore.Utils;
using LotusCore.Utils.MinecraftPaths;
using LotusCore.Utils.NBTInternals.Tags;

namespace LotusCore.Modules.ServerList
{
    public class ServerList : IServerListModule, IModuleBase, IPacketHandler
    {
        private NBT _ServerListDat;

        private INetworkModule _networking;

        public ServerList()
        {
            _ServerListDat = new();
            _ServerListDat.ReadFromBytes(File.ReadAllBytes(MinecraftPathsStruct._ServerData));
            //Logging.LogDebug(_ServerListDat.GetNBTAsString());
            _ = PingServerlist();
        }

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("list", new ListCommand(_ServerListDat));
        }

        public void RegisterEvents(Action<string> RegisterEvent)
        {
            //RegisterEvent.Invoke("ServerListIP_Request");
        }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
        {
            /* SubscribeToEvent.Invoke(
                "STATUS_Packet_Received",
                new EngineEventHandler(ProcessPacket)
            ); */
        }

        public (string ip, string port) ServerListIPRequest(string serverName)
        {
            string[] ipPort = GetServerIPFromName(serverName).Split(":");
            if (ipPort.Length == 0)
            {
                //no ip
                return ("", "");
            }
            else if (ipPort.Length == 1)
            {
                //only ip
                return (ipPort[0], "");
            }
            else
            {
                //ip and port
                return (ipPort[0], ipPort[1]);
            }
        }

        public void LinkModules()
        {
            _networking = Core_Engine.GetModule<INetworkModule>()!;
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
            if (ip == null)
            {
                return false;
            }
            ip.Value = ip.Value.ToLower();
            (string? serverIP, int? port) = await ServerDNSLookup.GetServerDNSRecordAsync(ip.Value);
            if (serverIP == null)
            {
                //no such host
                return false;
            }
            TAG_String? serverName = (TAG_String?)tmp.TryGetTag("name");

            //Guid? remoteHostID = _networkingModule.ConnectToServer(serverIP, port ?? 25565);

            Guid? remoteHostID = _networking.ConnectToServer(serverIP, port ?? 25565);

            if (remoteHostID == null)
            {
                //connection refused
                return false;
            }

            ServerConnection connection = _networking.GetServerConnection(remoteHostID.Value)!;

            connection._serverListInfo._ServerListEntry = tmp;
            connection._connectionState = ConnectionState.STATUS;
            _networking.SendPacket(
                connection._id,
                new HandshakePacket(ip.Value, HandshakePacket.Intent.Status, 25565)
                {
                    _protocol_ID = 0x00,
                    _NextState = (int)HandshakePacket.Intent.Status,
                }
            );

            SendStatusRequest(remoteHostID.Value);
            return true;
        }

        [Obsolete]
        public EngineEventResult? ProcessPacket(object? sender, IEngineEventArgs args)
        {
            PacketReceivedEventArgs eventArgs = (PacketReceivedEventArgs)args;
            var packet = eventArgs._packet;
            //Logging.LogDebug($"StatusHandler State 0x{packet._Protocol_ID:X}");
            switch (packet._protocol_ID)
            {
                case 0x00:
                    HandleStatusResponse(packet);
                    break;
                case 0x01:
                    HandlePingResponse(packet);
                    break;
                default:
                    Logging.LogError(
                        $"StatusHandler State 0x{packet._protocol_ID:X} Not Implemented"
                    );

                    _networking.DisconnectFromServer(eventArgs._remoteHostID);

                    break;
            }
            return null;
        }

        public async Task ProcessPacket(MinecraftServerPacket packet)
        {
            //Logging.LogDebug($"StatusHandler State 0x{packet._Protocol_ID:X}");
            switch (packet._protocol_ID)
            {
                case 0x00:
                    HandleStatusResponse(packet);
                    break;
                case 0x01:
                    HandlePingResponse(packet);
                    break;
                default:
                    Logging.LogError(
                        $"StatusHandler State 0x{packet._protocol_ID:X} Not Implemented"
                    );

                    _networking.DisconnectFromServer(packet._remoteHostID);

                    break;
            }
            return;
        }

        private void HandlePingResponse(MinecraftServerPacket packet)
        {
            try
            {
                int offset = 0;
                long value = NetworkLong.DecodeBytes(packet._data, ref offset);
                ServerConnection connection = _networking.GetServerConnection(
                    packet._remoteHostID
                )!;
                connection._serverListInfo._LastPingLength = (
                    DateTime.UtcNow - connection._serverListInfo._LastPingTime
                ).TotalMilliseconds;

                connection._serverListInfo._ServerListEntry!.WriteTag<TAG_Double>(
                    new TAG_Double()
                    {
                        _Name = "ping",
                        Value = connection._serverListInfo._LastPingLength,
                    }
                );
                /* Logging.LogDebug(
                    $"Response: {value} Ping:{(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - value}m"
                ); */
            }
            catch (Exception e)
            {
                Logging.LogDebug(e.ToString());
            }

            _networking.DisconnectFromServer(packet._remoteHostID);
        }

        private void HandleStatusResponse(MinecraftServerPacket packet)
        {
            int offset = 0;
            string value = StringN.DecodeBytes(packet._data, ref offset);
            //Logging.LogDebug($"Response Size: {size}\n{value.Replace("\r", "").Replace("\n", "")}");

            ServerConnection connection = _networking.GetServerConnection(packet._remoteHostID)!;
            connection._serverListInfo._ServerListEntry!.WriteTag(
                new TAG_String()
                {
                    _Name = "serverlist_info",
                    Value = value.Replace("\r", "").Replace("\n", ""),
                }
            );
            SendPingRequest(packet._remoteHostID);
        }

        private void SendPingRequest(Guid remoteHostID)
        {
            ServerConnection connection = _networking.GetServerConnection(remoteHostID)!;
            var connectionState = connection._connectionState;
            if (connectionState == ConnectionState.STATUS)
            {
                connection._serverListInfo._LastPingTime = DateTime.UtcNow;
                _networking.SendPacket(connection._id, new StatusPingRequestPacket());
            }
        }

        private void SendStatusRequest(Guid remoteHostID)
        {
            ServerConnection connection = _networking.GetServerConnection(remoteHostID)!;
            var connectionState = connection._connectionState;
            if (connectionState == ConnectionState.STATUS)
            {
                //_networkingModule.SendPacket(connection._id, new EmptyPacket(0x00));

                _networking.SendPacket(connection._id, new EmptyPacket(0x00));
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
