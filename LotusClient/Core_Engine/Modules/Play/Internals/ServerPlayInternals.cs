using System.Collections;
using System.Net;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEventArgs;
using LotusCore.Interfaces;
using LotusCore.Modules.Chat.Types;
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

    /* public void ServerboundPlayerSession(Guid remoteHostID)
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
    } */

    //SERVER TO CLIENT

    public void HandlePlayerChatMessage(MinecraftServerPacket packet)
    {
        Core_Engine.InvokeEvent(
            "CHAT_DecodePlayerChatMessagePacket",
            new PacketReceivedEventArgs(packet, packet._remoteHostID)
        );
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
}
