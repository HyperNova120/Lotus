using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ServerBound.Play.Chat;

public class AcknowledgeMessagePacket : MinecraftPacket
{
    int _messageCount = 0;

    public AcknowledgeMessagePacket(int messageCount)
    {
        _protocol_ID = 0x05;
        _messageCount = messageCount;
    }

    public override byte[] GetBytes()
    {
        return VarInt_VarLong.EncodeInt(_messageCount);
    }
}
