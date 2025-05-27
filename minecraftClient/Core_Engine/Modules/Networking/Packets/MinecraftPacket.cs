using System;
using System.Collections;
using Silk.NET.OpenGL;

namespace Core_Engine.Modules.Networking.Packets
{    public abstract class MinecraftPacket
    {
        public int protocol_id = 0x00;

        public abstract byte[] GetBytes();
    }

    public class MinecraftServerPacket
    {
        public int protocol_id = 0x00;
        public byte[] data;

        public MinecraftServerPacket(int protocol_id, byte[] data)
        {
            this.data = data;
            this.protocol_id = protocol_id;
        }
    }

    
    public enum PacketBoundTo
    {
        Server,
        Client,
    }
}
