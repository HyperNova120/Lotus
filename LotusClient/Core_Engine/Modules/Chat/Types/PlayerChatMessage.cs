using System.Collections;
using LotusCore.BaseClasses;
using LotusCore.Utils;

namespace LotusCore.Modules.Chat.Types;

public class PlayerChatMessage
{
    public ChatMessageHeader _header;

    public ChatMessageBody _body;

    public ChatMessageOther _other;

    public ChatMessageChatFormatting _chatFormatting;

    public PlayerChatMessage()
    {
        _header = new();
        _body = new();
        _other = new();
        _chatFormatting = new();
    }
}

public class ChatMessageHeader
{
    public int _globalIndex;

    public MinecraftUUID? _sender;

    public int _index;

    public byte[]? _messageSignatureBytes;
}

public class ChatMessageBody
{
    public string? _message; //maxlenght is 256

    public long _timestamp;

    public long _salt;
}

public class ChatMessageOther
{
    public NBT? _unsignedContent;

    public ChatFilterType _filterType;

    public BitArray? _filterTypeBits;
}

public enum ChatFilterType
{
    PASS_THROUGH,
    FULLY_FILTERED,
    PARTIALLY_FILTERED,
}

public class ChatMessageChatFormatting
{
    public int? _id;

    //add chattype
    public ChatType? _chatType;

    public NBT? _senderName;

    public NBT? _targetName;
}
