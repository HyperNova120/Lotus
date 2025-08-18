using System;
using System.Collections;
using System.Net;
using Silk.NET.OpenGL;

namespace Core_Engine.Modules.Networking.Packets
{
    public abstract class MinecraftPacket
    {
        public int _Protocol_ID { get; protected set; } = 0x00;

        public abstract byte[] GetBytes();
    }

    public class MinecraftServerPacket
    {
        public int _Protocol_ID = 0x00;
        public byte[] _Data;
        public IPAddress _RemoteHost;

        public MinecraftServerPacket(IPAddress remoteHost, int protocol_id, byte[] data)
        {
            this._Data = data;
            this._Protocol_ID = protocol_id;
            this._RemoteHost = remoteHost;
        }
    }

    public enum PacketBoundTo
    {
        Server,
        Client,
    }
}
