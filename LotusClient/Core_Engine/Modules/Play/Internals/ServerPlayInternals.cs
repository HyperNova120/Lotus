using System.Collections;
using System.Net;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.Interfaces;
using LotusCore.Modules.GameStateHandlerModule;
using LotusCore.Modules.GameStateHandlerModule.Models;
using LotusCore.Modules.GameStateHandlerModule.Types;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Packets.ServerBound.Play;
using LotusCore.Utils;

namespace LotusCore.Modules.ServerPlay.Internals;

public class ServerPlayInternals
{
    public ServerPlayInternals() { }

    //CLIENT TO SERVER

    public void ServerboundPlayerSession(Guid remoteHostID)
    {
        MojangKeyPair mojangKeyPair = Core_Engine
            .InvokeEvent<MojangKeyPairResult>("GAMESTATE_GetMojangKeyPair")!
            ._mojangKeyPair!;
        PlayerSessionPacket playerSessionPacket = new()
        {
            _PublicKey = MinecraftKeyFormatter.ConvertPemToX509Bytes(
                mojangKeyPair.keyPair.publicKey
            ),
            _Signature = Convert.FromBase64String(mojangKeyPair.publicKeySignatureV2),
            _ExpiresAt = DateTimeOffset.Parse(mojangKeyPair.expiresAt).ToUnixTimeMilliseconds(),
            _UUID = new MinecraftUUID(
                Core_Engine
                    .InvokeEvent<MinecraftProfileResult>("GAMESTATE_GetUserProfile")!
                    ._minecraftProfile!.id
            ),
        };
        Core_Engine.InvokeEvent(
            "NETWORKING_SendPacket",
            new SendPacketArgs(remoteHostID, playerSessionPacket)
        );
    }

    //SERVER TO CLIENT

    public void HandlePlayerChatMessage(MinecraftServerPacket packet)
    {
        //Header
        int offset = 0;
        int globalIndex = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);

        MinecraftUUID SenderUUID = new();
        SenderUUID.DecodeBytes(packet._data, ref offset);

        int index = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);

        bool isPresent = PrefixedOptional.DecodeBytes(packet._data, ref offset);
        byte[]? msgSig = null;
        if (isPresent)
        {
            msgSig = packet._data[offset..(offset + 256)];
            offset += 256;
        }

        //Body
        string Message = StringN.DecodeBytes(packet._data, ref offset);
        Logging.LogInfo($"<Unknown User> {Message}");

        long timestamp = NetworkLong.DecodeBytes(packet._data, ref offset);

        long salt = NetworkLong.DecodeBytes(packet._data, ref offset);
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

        //Other

        bool isUnsignedContentPresent = PrefixedOptional.DecodeBytes(packet._data, ref offset);
        NBT UnsignedContent = new();
        if (isUnsignedContentPresent)
        {
            offset += UnsignedContent.ReadFromBytes(packet._data[offset..]);
        }
        ChatFilterType FilterType = (ChatFilterType)
            VarInt_VarLong.DecodeVarInt(packet._data, ref offset);
        BitArray? FilterTypeBits = null;
        if (FilterType == ChatFilterType.PARTIALLY_FILTERED)
        {
            FilterTypeBits = NetworkBitset.DecodeBytes(packet._data, ref offset);
        }
    }

    internal void HandleSystemChatMessage(MinecraftServerPacket packet)
    {
        NBT textComponent = new();
        int offset = textComponent.ReadFromBytes(packet._data, true);
        bool isOverlay = packet._data[offset] == 0;
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
