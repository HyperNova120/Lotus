using System.Net;
using System.Text;
using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;
using Core_Engine.EngineEventArgs;
using Core_Engine.Interfaces;
using Core_Engine.Modules.GameStateHandler;
using Core_Engine.Modules.GameStateHandler.BaseClasses;
using Core_Engine.Modules.Networking.Internals;
using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.Networking.Packets.ClientBound.Configuration;
using Core_Engine.Modules.Networking.Packets.ServerBound.Configuration;
using Core_Engine.Utils;
using static Core_Engine.Modules.Networking.Networking;

namespace Core_Engine.Modules.ServerConfig.Internals;

public class ConfigurationInternals
{
    private readonly IGameStateHandler _GameStateHandler;
    private readonly Networking.Networking _NetworkingManager;

    public ConfigurationInternals()
    {
        _GameStateHandler = Core_Engine.GetModule<IGameStateHandler>("GameStateHandler")!;
        _NetworkingManager = Core_Engine.GetModule<Networking.Networking>("Networking")!;
    }

    public void HandleStoreCookie(MinecraftServerPacket minecraftPacket)
    {
        try
        {
            Logging.LogDebug("StoreCookie");
            StoreCookiePacket storeCookiePacket = new();
            storeCookiePacket.DecodeFromBytes(minecraftPacket._Data);
            _GameStateHandler.AddServerCookie(storeCookiePacket._ServerCookie!);
        }
        catch (Exception e)
        {
            Logging.LogError(e.ToString());
        }
    }

    internal void HandleClientboundKnownPacks(MinecraftServerPacket packet)
    {
        ConfigClientboundKnownPacks configClientboundKnownPacks = new();
        configClientboundKnownPacks.DecodeFromBytes(packet._Data);
        foreach (var pack in configClientboundKnownPacks._KnownPacks)
        {
            Logging.LogDebug($"Namespace:{pack.Namespace}; ID:{pack.ID}; Version:{pack.Version}");
        }

        ServerboundKnownPacksPacket serverboundKnownPacksPacket = new();

        _NetworkingManager.SendPacket(packet._RemoteHost, serverboundKnownPacksPacket);
    }

    internal void HandleRegistryData(MinecraftServerPacket packet)
    {
        Identifier RegistryID = new();
        int offset = RegistryID.GetFromBytes(packet._Data);

        (int arraySize, int numBytesRead) = PrefixedArray.GetSizeOfArray(packet._Data[offset..]);
        offset += numBytesRead;

        RegistryData registry = new() { _RegistryNameSpace = RegistryID };
        Logging.LogDebug(RegistryID.GetString());

        try
        {
            for (int i = 0; i < arraySize; i++)
            {
                Identifier EntryID = new();
                int EntryIDBytes = EntryID.GetFromBytes(packet._Data[offset..]);
                offset += EntryIDBytes;

                RegistryEntry registryEntry = new() { ID = EntryID };
                Logging.LogDebug("\t" + EntryID.GetString());

                (bool isPresent, int numberBytesRead) = PrefixedOptional.DecodeBytes(
                    packet._Data[offset..]
                );
                offset += numberBytesRead;
                if (isPresent)
                {
                    NBT EntryData = new();
                    int EntryDataBytes = EntryData.ReadFromBytes(packet._Data[offset..], true);
                    offset += EntryDataBytes;
                    registryEntry.Data = EntryData;
                    Logging.LogDebug("\n" + EntryData.GetNBTAsString(2));
                }

                registry._Entries.Add(registryEntry);
            }
        }
        catch (Exception e)
        {
            Logging.LogError("REGISTRY DATA: " + e.ToString());
        }
        _GameStateHandler.AddServerRegistryData(registry);
    }

    internal void HandleTransfer(MinecraftServerPacket minecraftPacket)
    {
        try
        {
            Logging.LogDebug("Transfer");
            _GameStateHandler.ProcessTransfer();
            ConfigTransferPacket configTransferPacket = new();
            configTransferPacket.DecodeFromBytes(minecraftPacket._Data);
            Core_Engine.signalInteractiveHoldTransfer(
                Core_Engine.State.Configuration,
                Core_Engine.State.JoiningServer
            );
            _NetworkingManager.DisconnectFromServer(minecraftPacket._RemoteHost);
            _ = Core_Engine.HandleCommand(
                "join",
                [configTransferPacket._Host, configTransferPacket._Port.ToString()]
            );
        }
        catch (Exception e)
        {
            Logging.LogError(e.ToString());
        }
    }

    public void SendServerboundPluginMessage(IPAddress remoteHost)
    {
        Logging.LogDebug("Sending Plugin Message");
        PluginMessagePacket pluginMessagePacket = new()
        {
            _Channel = new("minecraft:brand"),
            _Data = StringN.GetBytes("lotus"),
        };
        _NetworkingManager.SendPacket(remoteHost, pluginMessagePacket);
    }

    public void SendServerboundClientInformation(IPAddress remoteHost)
    {
        Logging.LogDebug("Sending Client Information");
        ClientInformationPacket pluginMessagePacket = new()
        {
            _AllowServerListings = IGameStateHandler._Settings._AllowServerListings,
            _ChatColors = IGameStateHandler._Settings._ChatSettings._Colors,
            _ChatMode = (int)IGameStateHandler._Settings._ChatSettings._ChatShown,
            _DisplayedSkinParts = IGameStateHandler
                ._Settings
                ._SkinCustomization
                ._DisplayedSkinParts,
            _EnableTextFiltering = false, //hardcoded for now, add setting later
            _Locale = "en_US", //hardcoded, chage if more locales added later
            _MainHand = (int)IGameStateHandler._Settings._SkinCustomization._MainHand,
            _ParticleStatus = (int)IGameStateHandler._Settings._VideoSettings._ParticleStatus,
            _ViewDistance = IGameStateHandler._Settings._VideoSettings._RenderDistance,
        };
        _NetworkingManager.SendPacket(remoteHost, pluginMessagePacket);
    }

    public void SendConfigBrandAndClientInfo(IPAddress remoteHost)
    {
        Logging.LogDebug("Sending Plugin Message And Client Information");
        PluginMessagePacket pluginBrandMessagePacket = new()
        {
            _Channel = new("minecraft:brand"),
            _Data = StringN.GetBytes("lotus"),
        };
        ClientInformationPacket pluginInfoMessagePacket = new()
        {
            _AllowServerListings = IGameStateHandler._Settings._AllowServerListings,
            _ChatColors = IGameStateHandler._Settings._ChatSettings._Colors,
            _ChatMode = (int)IGameStateHandler._Settings._ChatSettings._ChatShown,
            _DisplayedSkinParts = IGameStateHandler
                ._Settings
                ._SkinCustomization
                ._DisplayedSkinParts,
            _EnableTextFiltering = false, //hardcoded for now, add setting later
            _Locale = "en_US", //hardcoded, chage if more locales added later
            _MainHand = (int)IGameStateHandler._Settings._SkinCustomization._MainHand,
            _ParticleStatus = (int)IGameStateHandler._Settings._VideoSettings._ParticleStatus,
            _ViewDistance = IGameStateHandler._Settings._VideoSettings._RenderDistance,
        };
        _NetworkingManager.SendPacket(
            remoteHost,
            [pluginBrandMessagePacket, pluginInfoMessagePacket]
        );
    }

    internal void HandleCookieRequest(MinecraftServerPacket packet)
    {
        Identifier Key = new();
        Key.GetFromBytes(packet._Data);

        var cookie = _GameStateHandler.GetServerCookie(Key);

        CookieResponsepacket cookieResponsepacket = new()
        {
            _Protocol_ID = 0x00,
            _Key = Key,
            _Payload = cookie?._Payload ?? [],
        };

        _NetworkingManager.SendPacket(packet._RemoteHost, cookieResponsepacket);
    }

    internal void HandlePluginMessage(MinecraftServerPacket packet)
    {
        (int value, int offset) = VarInt_VarLong.DecodeVarInt(packet._Data);
        Identifier channel = new();
        offset += channel.GetFromBytes(packet._Data[offset..]);

        PluginMessageReceivedEventArgs args = new(
            packet._RemoteHost,
            ConnectionState.CONFIGURATION,
            channel,
            packet._Data[offset..],
            value
        );

        Core_Engine.InvokeEvent("PLUGIN_Packet_Received", args);
    }

    internal void HandleDisconnect(MinecraftServerPacket packet)
    {
        try
        {
            /* Logging.LogInfo(
                $"Client disconnected during Config, Reason:{Encoding.UTF8.GetString(packet._Data).Replace("\r", "").Replace("\n", "").Trim()}"
            ); */
            NBT test = new();
            test.ReadFromBytes(packet._Data, true);
            Console.WriteLine("\n\n" + test.GetNBTAsString());
        }
        catch (Exception e)
        {
            Logging.LogError(e.ToString());
            throw;
        }
        Core_Engine
            .GetModule<Networking.Networking>("Networking")!
            .DisconnectFromServer(packet._RemoteHost);
        //Core_Engine.CurrentState = Core_Engine.State.Interactive;
        Core_Engine.SignalInteractiveFree(Core_Engine.State.Configuration);
    }

    internal void HandleFinishConfiguration(MinecraftServerPacket packet)
    {
        Logging.LogInfo("Server Config Success!");
        Core_Engine.signalInteractiveHoldTransfer(
            Core_Engine.State.Configuration,
            Core_Engine.State.Play
        );
        _NetworkingManager.GetServerConnection(packet._RemoteHost)!._ConnectionState =
            ConnectionState.PLAY;
        _NetworkingManager.SendPacket(packet._RemoteHost, new EmptyPacket(0x03));
    }

    internal void HandleKeepAlive(MinecraftServerPacket packet)
    {
        KeepAlivePacket keepAlivePacket = new(0x04, NetworkLong.DecodeBytes(packet._Data));
        _GameStateHandler.SetLastKeepAliveTime(DateTime.Now);
        _NetworkingManager.SendPacket(packet._RemoteHost, keepAlivePacket);
    }

    internal void HandlePing(MinecraftServerPacket packet)
    {
        PongPacket pongPacket = new(0x05, NetworkInt.DecodeBytes(packet._Data));
        _NetworkingManager.SendPacket(packet._RemoteHost, pongPacket);
    }

    internal void HandleAddResourcePack(MinecraftServerPacket packet)
    {
        ResourcePack resourcePack = new();
        resourcePack.DecodeBytes(packet._Data);
        _GameStateHandler.AddServerResourcePack(resourcePack);
    }

    internal void HandleRemoveResourcePack(MinecraftServerPacket packet)
    {
        throw new NotImplementedException();
    }
}
