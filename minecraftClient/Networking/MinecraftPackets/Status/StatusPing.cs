namespace MinecraftNetworking.Packets
{
    public class StatusPingRequestPacket : MinecraftPacket
    {
        public override byte[] GetBytes()
        {
            return [.. BitConverter.GetBytes(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond)];
        }

        public StatusPingRequestPacket()
        {
            protocol_id = 0x01;
        }
    }
}
