using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.Modules.Chat.Types;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Utils;

namespace LotusCore.Modules.Chat.Internals;

public class ChatMessageDecoder
{
    public void DecodePlayerChatMessageHeader(
        PlayerChatMessage playerChatMessage,
        MinecraftServerPacket packet,
        ref int offset
    )
    {
        playerChatMessage._header._globalIndex = VarInt_VarLong.DecodeVarInt(
            packet._data,
            ref offset
        );

        MinecraftUUID SenderUUID = new();
        SenderUUID.DecodeBytes(packet._data, ref offset);
        playerChatMessage._header._sender = SenderUUID;

        playerChatMessage._header._index = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);

        bool isPresent = PrefixedOptional.DecodeBytes(packet._data, ref offset);
        if (isPresent)
        {
            playerChatMessage._header._messageSignatureBytes = packet._data[offset..(offset + 256)];
            offset += 256;
        }
    }

    public void DecodePlayerChatMessageBody(
        PlayerChatMessage playerChatMessage,
        MinecraftServerPacket packet,
        ref int offset
    )
    {
        playerChatMessage._body._message = StringN.DecodeBytes(packet._data, ref offset);
        //Logging.LogInfo($"<Unknown User> {Message}");

        playerChatMessage._body._timestamp = NetworkLong.DecodeBytes(packet._data, ref offset);

        playerChatMessage._body._salt = NetworkLong.DecodeBytes(packet._data, ref offset);
        int arraySize = PrefixedArray.GetSizeOfArray(packet._data, ref offset);

        for (int i = 0; i < arraySize; i++)
        {
            int MessageID = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);
            if (MessageID == 0)
            {
                byte[] Sig = packet._data[offset..(offset + 256)];
                offset += 256;
            }
        }
    }

    public void DecodePlayerChatMessageOther(
        PlayerChatMessage playerChatMessage,
        MinecraftServerPacket packet,
        ref int offset
    )
    {
        bool isUnsignedContentPresent = PrefixedOptional.DecodeBytes(packet._data, ref offset);
        NBT UnsignedContent = new();
        if (isUnsignedContentPresent)
        {
            offset += UnsignedContent.ReadFromBytes(packet._data[offset..]);
        }
        playerChatMessage._other._unsignedContent = UnsignedContent;

        playerChatMessage._other._filterType = (ChatFilterType)
            VarInt_VarLong.DecodeVarInt(packet._data, ref offset);
        if (playerChatMessage._other._filterType == ChatFilterType.PARTIALLY_FILTERED)
        {
            playerChatMessage._other._filterTypeBits = NetworkBitset.DecodeBytes(
                packet._data,
                ref offset
            );
        }
    }

    public void DecodePlayerChatMessageChatFormatting(
        PlayerChatMessage playerChatMessage,
        MinecraftServerPacket packet,
        ref int offset
    )
    {
        int IDorChatType = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);
        if (IDorChatType != 0)
        {
            //ID
            Logging.LogDebug($"ITS AN ID: {IDorChatType}");
        }
        else
        {
            Logging.LogDebug($"ITS NOT AN ID");
            playerChatMessage._chatFormatting._chatType = ChatType.DecodeBytes(
                packet._data,
                ref offset
            );
            //TODO use Chat type decoration for both Chat portion and Narration portion.
        }

        playerChatMessage._chatFormatting._senderName = new();

        offset += playerChatMessage._chatFormatting._senderName.ReadFromBytes(
            packet._data[offset..],
            networkBytes: true
        );

        if (PrefixedOptional.DecodeBytes(packet._data, ref offset))
        {
            playerChatMessage._chatFormatting._targetName = new();

            offset += playerChatMessage._chatFormatting._targetName.ReadFromBytes(
                packet._data[offset..],
                networkBytes: true
            );
        }
        else
        {
            playerChatMessage._chatFormatting._targetName = null;
        }
    }
}
