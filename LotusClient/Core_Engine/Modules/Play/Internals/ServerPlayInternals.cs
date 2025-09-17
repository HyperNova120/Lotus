using System.Collections;
using System.Net;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.Interfaces;
using LotusCore.Modules.GameStateHandlerModule;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Packets.ServerBound.Play;
using LotusCore.Utils;

namespace LotusCore.Modules.ServerPlay.Internals;

public class ServerPlayInternals
{
    private GameStateHandler _GameStateHandler;
    private Networking.Networking _NetworkingManager;

    public ServerPlayInternals()
    {
        _GameStateHandler = Core_Engine.GetModule<GameStateHandler>("GameStateHandler")!;
        _NetworkingManager = Core_Engine.GetModule<Networking.Networking>("Networking")!;
    }

    //CLIENT TO SERVER

    public void ServerboundPlayerSession(IPAddress remoteHost)
    {
        PlayerSessionPacket playerSessionPacket = new()
        {
            _PublicKey = MinecraftKeyFormatter.ConvertPemToX509Bytes(
                _GameStateHandler._MojangKeyPair.keyPair.publicKey
            ),
            _Signature = Convert.FromBase64String(
                _GameStateHandler._MojangKeyPair.publicKeySignatureV2
            ),
            _ExpiresAt = DateTimeOffset
                .Parse(_GameStateHandler._MojangKeyPair.expiresAt)
                .ToUnixTimeMilliseconds(),
            _UUID = new MinecraftUUID(_GameStateHandler._UserProfile.id),
        };
        _NetworkingManager.SendPacket(remoteHost, playerSessionPacket);
    }

    //SERVER TO CLIENT

    public void HandlePlayerChatMessage(MinecraftServerPacket packet)
    {
        //Header
        int offset = 0;
        int globalIndex = VarInt_VarLong.DecodeVarInt(packet._Data, ref offset);

        MinecraftUUID SenderUUID = new();
        SenderUUID.DecodeBytes(packet._Data, ref offset);

        int index = VarInt_VarLong.DecodeVarInt(packet._Data, ref offset);

        bool isPresent = PrefixedOptional.DecodeBytes(packet._Data, ref offset);
        byte[]? msgSig = null;
        if (isPresent)
        {
            msgSig = packet._Data[offset..(offset + 256)];
            offset += 256;
        }

        //Body
        string Message = StringN.DecodeBytes(packet._Data, ref offset);
        Logging.LogInfo($"<Unknown User> {Message}");

        long timestamp = NetworkLong.DecodeBytes(packet._Data, ref offset);

        long salt = NetworkLong.DecodeBytes(packet._Data, ref offset);
        int arraySize = PrefixedArray.GetSizeOfArray(packet._Data, ref offset);

        for (int i = 0; i < arraySize; i++)
        {
            int MessageID = VarInt_VarLong.DecodeVarInt(packet._Data, ref offset);
            if (MessageID == 0)
            {
                byte[] Sig = packet._Data[offset..(offset + 256)];
                offset += 256;
            }
        }

        //Other

        bool isUnsignedContentPresent = PrefixedOptional.DecodeBytes(packet._Data, ref offset);
        NBT UnsignedContent = new();
        if (isUnsignedContentPresent)
        {
            offset += UnsignedContent.ReadFromBytes(packet._Data[offset..]);
        }
        ChatFilterType FilterType = (ChatFilterType)
            VarInt_VarLong.DecodeVarInt(packet._Data, ref offset);
        BitArray? FilterTypeBits = null;
        if (FilterType == ChatFilterType.PARTIALLY_FILTERED)
        {
            FilterTypeBits = NetworkBitset.DecodeBytes(packet._Data, ref offset);
        }
    }

    internal void HandleSystemChatMessage(MinecraftServerPacket packet)
    {
        NBT textComponent = new();
        int offset = textComponent.ReadFromBytes(packet._Data, true);
        bool isOverlay = packet._Data[offset] == 0;
        Console.WriteLine(
            $"SystemChatMessage: isOverlay:{isOverlay} MSG:{textComponent.ToString()}"
        );
    }

    public enum ChatFilterType
    {
        PASS_THROUGH,
        FULLY_FILTERED,
        PARTIALLY_FILTERED,
    }
}
