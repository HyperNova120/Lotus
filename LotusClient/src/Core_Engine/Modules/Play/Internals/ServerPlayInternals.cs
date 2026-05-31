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
using LotusCore.Modules.LotusNetty.Internals;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Play;
using LotusCore.Utils;

namespace LotusCore.Modules.ServerPlay.Internals;

public class ServerPlayInternals
{
    private IServerChatModule _serverChat;

    private INetworkModule _networking;

    public ServerPlayInternals(IServerChatModule serverChat, INetworkModule networking)
    {
        _serverChat = serverChat;
        _networking = networking;
    }

    public void HandlePlayerChatMessage(MinecraftServerPacket packet)
    {
        _serverChat.ReceivePlayerChatMessagePacket(packet, packet._remoteHostID);
    }

    internal void HandleKeepAlive(MinecraftServerPacket packet)
    {
        Logging.LogDebug(
            $"\tKeepAlive:0x{string.Concat(packet._data.Select(b => b.ToString("X2")))}"
        );
        int offset = 0;
        KeepAlivePacket KAP = new(0x1B, NetworkLong.DecodeBytes(packet._data, ref offset));
        _networking.SendPacket(packet._remoteHostID, KAP);
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
