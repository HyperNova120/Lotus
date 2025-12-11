using System.Net;
using System.Net.Sockets;
using LotusCore.EngineEventArgs;
using LotusCore.Interfaces;
using LotusCore.Modules.LotusNetty.Packets;

namespace LotusCore.Modules.LotusNetty.Internals;

public class ConnectionHandler
{
    public IPacketHandler? _loginPacketHandler;
    public IPacketHandler? _configPacketHandler;
    public IPacketHandler? _playPacketHandler;

    //==========================================
    //            Connection States
    //==========================================

    private Dictionary<Guid, ServerConnection> _connections = new();

    public bool _isClientConnectedToPrimaryServer = false;

    private Guid? _primaryClientServerConnection = null;

    public ServerConnection? GetServerConnection(Guid connectionID)
    {
        if (_connections.ContainsKey(connectionID))
        {
            return _connections[connectionID];
        }
        return null;
    }

    public bool IsConnectionPrimary(Guid connectionID)
    {
        return _isClientConnectedToPrimaryServer && _primaryClientServerConnection == connectionID;
    }

    public void SetPrimaryConnection(Guid? connectionID)
    {
        if (connectionID == null)
        {
            _isClientConnectedToPrimaryServer = false;
            _primaryClientServerConnection = null;
            return;
        }
        _isClientConnectedToPrimaryServer = true;
        _primaryClientServerConnection = connectionID;
    }

    public void AddServerConnection(Guid id, ServerConnection connection)
    {
        _connections[id] = connection;
    }

    public void RemoveServerConnection(Guid id)
    {
        _connections.Remove(id);
    }

    public bool IsClientConnectedToPrimaryServer()
    {
        return _isClientConnectedToPrimaryServer;
    }

    public Guid? GetConnectionInState(
        IPAddress connectionHost,
        IEnumerable<ConnectionState> connectionStates
    )
    {
        foreach (var con in _connections.Values)
        {
            if (
                con._connectionInfo._remoteHost == connectionHost
                && connectionStates.Contains(con._connectionState)
            )
            {
                return con._id;
            }
        }
        return null;
    }

    //==========================================
    //               Sockets
    //==========================================

    public Guid? ConnectToServer(string ip, int port = 25565)
    {
        Guid id = Guid.CreateVersion7();
        ServerConnection serverConnection = new(ip, port, id);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        serverConnection._tcpSocket = new Socket(
            endPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp
        );
        serverConnection._tcpSocket.NoDelay = true;
        try
        {
            serverConnection._tcpSocket.Connect(endPoint);
        }
        catch (Exception e)
        {
            //Logging.LogError($"ConnectToServer:\n{e}");
            return null;
        }
        AddServerConnection(id, serverConnection);
        Logging.LogDebug($"Successfully Connected to Server: {ip}:{port}");
        _ = HandleConnection(serverConnection, serverConnection._tcpSocket);
        return id;
    }

    public bool DisconnectFromServer(Guid remoteHostID)
    {
        ServerConnection? connection = GetServerConnection(remoteHostID);
        if (connection == null)
        {
            return false;
        }
        Logging.LogInfo("Disconnected from Server:" + remoteHostID);
        if (IsConnectionPrimary(remoteHostID))
        {
            SetPrimaryConnection(null);
        }
        if (connection!._tcpSocket != null)
        {
            connection!._tcpSocket!.Disconnect(false);
            connection!._tcpSocket!.Close();
            connection!._tcpSocket = null;
        }
        RemoveServerConnection(remoteHostID);
        return true;
    }

    private async Task HandleConnection(ServerConnection serverConnection, Socket socket)
    {
        byte[] buffer = new byte[2048];
        int readN = 0;
        while (socket.Connected)
        {
            readN = await socket.ReceiveAsync(buffer);
            if (!ProcessReceive(serverConnection, buffer, readN))
            {
                if (IsConnectionPrimary(serverConnection._id))
                {
                    Core_Engine.SignalInteractiveResetServerHolds();
                }
                return;
            }
            readN = 0;
        }

        DisconnectFromServer(serverConnection._id);
    }

    private bool ProcessReceive(ServerConnection serverConnection, byte[] buffer, int readN)
    {
        if (readN == 0)
        {
            Logging.LogInfo("Connection Closed by Remote Host");
            DisconnectFromServer(serverConnection._id);
            return false;
        }

        byte[] data = ProcessPacketEncryption(buffer[..readN], serverConnection);
        serverConnection._packetInfo._incompletePacketBytesBuffer.AddRange(data);
        ProcessPacketData(serverConnection);
        return true;
    }

    private byte[] ProcessPacketEncryption(byte[] data, ServerConnection serverConnection)
    {
        if (!serverConnection._minecraftPacketHandler._IsEncryptionEnabled)
        {
            return data;
        }
        return serverConnection._encryption.DecryptData(data);
    }

    private bool ProcessPacketData(ServerConnection serverConnection)
    {
        bool firstRun = true;
        while (serverConnection._packetInfo._incompletePacketBytesBuffer.Count != 0)
        {
            if (!firstRun)
            {
                Logging.LogDebug($"ProcessPacketData: Multi Packet Receive");
            }
            (var packet, var remainingbuffer) =
                serverConnection._minecraftPacketHandler.DecodePacket(
                    serverConnection._id,
                    serverConnection._packetInfo._incompletePacketBytesBuffer.ToArray()
                );
            serverConnection._packetInfo._incompletePacketBytesBuffer.Clear();
            serverConnection._packetInfo._incompletePacketBytesBuffer.AddRange(remainingbuffer);
            if (packet == null)
            {
                //Logging.LogInfo("Bad Packet");
                return false;
            }
            ProcessPacketEvent(serverConnection, packet);
        }
        return true;
    }

    private void ProcessPacketEvent(ServerConnection connection, MinecraftServerPacket packet)
    {
        /* Logging.LogDebug(
            $"\tProcessing Packet 0x{packet._Protocol_ID:X} in State: {connection._ConnectionState.ToString()}"
        ); */
        try
        {
            switch (connection._connectionState)
            {
                case ConnectionState.STATUS:
                    Core_Engine.InvokeEvent(
                        "STATUS_Packet_Received",
                        new PacketReceivedEventArgs(packet, connection._id)
                    );
                    break;
                case ConnectionState.LOGIN:
                    _loginPacketHandler!.ProcessPacket(packet);
                    break;
                case ConnectionState.CONFIGURATION:
                    _configPacketHandler!.ProcessPacket(packet);
                    break;
                case ConnectionState.PLAY:
                    _playPacketHandler!.ProcessPacket(packet);
                    break;
                default:
                    Logging.LogError(
                        $"ReceiveConnections State {connection._connectionState} Not Implemented"
                    );
                    DisconnectFromServer(connection._id);
                    if (IsConnectionPrimary(connection._id))
                    {
                        Core_Engine.SignalInteractiveResetServerHolds();
                    }
                    Core_Engine.SignalInteractiveResetServerHolds();
                    return;
            }
        }
        catch
        {
            DisconnectFromServer(connection._id);
            if (IsConnectionPrimary(connection._id))
            {
                Core_Engine.SignalInteractiveResetServerHolds();
            }
            return;
        }
    }
}
