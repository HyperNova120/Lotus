using LotusCore.BaseClasses.Types;
using LotusCore.Modules.ServerPlay;

namespace LotusCore.Modules.LotusNetty.Packets.ServerBound.Play.Chat;

public class AcknowledgeMessagePacket : MinecraftPacket
{
    int _messageCount = 0;

    public AcknowledgeMessagePacket(int messageCount)
    {
        _protocol_ID = (int)PlayPacketsServerbound.ACKNOWLEDGE_MESSAGE;
        _messageCount = messageCount;
    }

    public override byte[] GetBytes()
    {
        return VarInt_VarLong.EncodeInt(_messageCount);
    }
}
