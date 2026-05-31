using LotusCore.BaseClasses.Types;
using LotusCore.Interfaces;
using LotusCore.Utils;

namespace LotusCore.Modules.Chat.Types;

public class ChatType : INetworkData<ChatType>
{
    public ChatDecoration _chat = new();

    public ChatDecoration _narration = new();

    public static ChatType DecodeBytes(byte[] data, ref int offset)
    {
        ChatType returner = new();
        returner._chat = ChatDecoration.DecodeBytes(data, ref offset);
        returner._narration = ChatDecoration.DecodeBytes(data, ref offset);
        return returner;
    }

    public static byte[] GetBytes(ChatType data)
    {
        return
        [
            .. ChatDecoration.GetBytes(data._chat),
            .. ChatDecoration.GetBytes(data._narration),
        ];
    }
}

public class ChatDecoration : INetworkData<ChatDecoration>
{
    public string _translationKey = "";

    public List<int> _parameters = new();

    public NBT _style = new();

    public static ChatDecoration DecodeBytes(byte[] data, ref int offset)
    {
        ChatDecoration returner = new();
        returner._translationKey = StringN.DecodeBytes(data, ref offset);
        for (int i = 0; i < PrefixedArray.GetSizeOfArray(data, ref offset); i++)
        {
            returner._parameters.Add(VarInt_VarLong.DecodeVarInt(data, ref offset));
        }
        returner._style = new(true);
        offset += returner._style.ReadFromBytes(data[offset..], true);
        return returner;
    }

    public static byte[] GetBytes(ChatDecoration data)
    {
        List<byte> returner = [.. StringN.GetBytes(data._translationKey)];
        foreach (int i in data._parameters)
        {
            returner.AddRange(VarInt_VarLong.EncodeInt(i));
        }
        returner.AddRange(data._style.GetBytes());

        return returner.ToArray();
    }
}
