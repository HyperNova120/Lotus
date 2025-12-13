using System.Collections;
using LotusCore.BaseClasses.Types;
using LotusCore.Modules.LotusNetty.Packets;

namespace LotusCore.Modules.Chat.Types;

public class ChatMessage : MinecraftPacket
{
    public ChatMessage()
    {
        _protocol_ID = 0x08;
    }

    public string? _message;

    public long _timestamp;

    public long _salt;

    public byte[]? _signature;

    public int _messageCount;

    public FixedBitSet? _acknowledged;

    public RollingWindowEntry[]? _rollingWindow;

    public override byte[] GetBytes()
    {
        byte[] data =
        [
            .. StringN.GetBytes(_message!),
            .. NetworkLong.GetBytes(_timestamp),
            .. NetworkLong.GetBytes(_salt),
            .. PrefixedOptional.GetBytes(_signature!),
            .. VarInt_VarLong.EncodeInt(_messageCount),
            .. _acknowledged!.GetBytes(),
        ];

        return [.. data, CreateChecksum()];
    }

    private byte CreateChecksum()
    {
        /* byte checksum = 0;

        foreach (byte b in data)
        {
            checksum += b;
        }
        return (byte)(checksum & 0xFF); */
        /* byte checksum = 0;
        foreach (byte b in data)
        {
            checksum ^= b; // XOR each byte
        }
        return checksum; */

        int acc = 1;
        foreach (RollingWindowEntry entry in _rollingWindow!)
        {
            acc = 31 * acc + entry.checksum();
        }
        byte result = (byte)acc;
        return (byte)((result == 0) ? 1 : result);
    }
}
