using System;
using System.Collections;
using System.Net;
using Silk.NET.OpenGL;

namespace LotusCore.Modules.LotusNetty.Packets
{
    public abstract class MinecraftPacket
    {
        public int _protocol_ID { get; set; } = 0x00;

        public abstract byte[] GetBytes();

        public MinecraftPacket() { }
    }

    public class MinecraftServerPacket
    {
        public int _protocol_ID = 0x00;
        public byte[] _data;
        public Guid _remoteHostID;

        public MinecraftServerPacket(Guid remoteHostID, int protocol_id, byte[] data)
        {
            this._data = data;
            this._protocol_ID = protocol_id;
            this._remoteHostID = remoteHostID;
        }
    }

    public enum PacketBoundTo
    {
        Server,
        Client,
    }
}
