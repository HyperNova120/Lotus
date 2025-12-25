using System.Net;
using System.Text;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEventArgs;
using LotusCore.Interfaces;
using LotusCore.Modules.GameStateHandlerModule;
using LotusCore.Modules.GameStateHandlerModule.BaseClasses;
using LotusCore.Modules.GameStateHandlerModule.Types;
using LotusCore.Modules.LotusNetty;
using LotusCore.Modules.LotusNetty.Internals;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Modules.LotusNetty.Packets.ClientBound.Configuration;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Configuration;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Status;
using LotusCore.Modules.LotusNetty.Types;
using LotusCore.Utils;
using Microsoft.AspNetCore.Mvc;
using static LotusCore.Modules.LotusNetty.Networking;

namespace LotusCore.Modules.ServerConfig.Internals;

public class ConfigurationInternals
{
    private INetworkModule _networkModule;

    private IGameStateHandlerModule _gamestateHandler;

    private IServerPlayHandlerModule _serverPlayHandler;

    ICoreModule? _coreModule;

    public ConfigurationInternals(
        ICoreModule coreModule,
        INetworkModule networkModule,
        IGameStateHandlerModule gamestateHandler,
        IServerPlayHandlerModule serverPlayHandler
    )
    {
        _coreModule = coreModule;
        _networkModule = networkModule;
        _gamestateHandler = gamestateHandler;
        _serverPlayHandler = serverPlayHandler;
    }

    public void HandleStoreCookie(MinecraftServerPacket minecraftPacket)
    {
        try
        {
            Logging.LogDebug("StoreCookie");
            StoreCookiePacket storeCookiePacket = new();
            storeCookiePacket.DecodeFromBytes(minecraftPacket._data);
            _gamestateHandler.AddServerCookie(storeCookiePacket._ServerCookie!);
        }
        catch (Exception e)
        {
            Logging.LogError(e.ToString());
        }
    }

    internal void HandleClientboundKnownPacks(MinecraftServerPacket packet)
    {
        ConfigClientboundKnownPacks configClientboundKnownPacks = new();
        configClientboundKnownPacks.DecodeFromBytes(packet._data);
        foreach (var pack in configClientboundKnownPacks._KnownPacks)
        {
            Logging.LogDebug($"Namespace:{pack.Namespace}; ID:{pack.ID}; Version:{pack.Version}");
        }

        ServerboundKnownPacksPacket serverboundKnownPacksPacket = new();
        _networkModule.SendPacket(packet._remoteHostID, serverboundKnownPacksPacket);
    }

    internal void HandleRegistryData(MinecraftServerPacket packet)
    {
        Identifier RegistryID = new();
        int offset = 0;
        RegistryID.GetFromBytes(packet._data, ref offset);

        int arraySize = PrefixedArray.GetSizeOfArray(packet._data, ref offset);

        RegistryData registry = new() { _RegistryNameSpace = RegistryID };
        bool shouldLogInfo = false;

        if (RegistryID.GetString() == "minecraft:enchantment")
        {
            //shouldLogInfo = true;
        }

        if (shouldLogInfo)
            Logging.LogDebug(RegistryID.GetString());

        try
        {
            for (int i = 0; i < arraySize; i++)
            {
                Identifier EntryID = new();
                EntryID.GetFromBytes(packet._data, ref offset);

                RegistryEntry registryEntry = new() { ID = EntryID };
                if (shouldLogInfo)
                    Logging.LogDebug("\t" + EntryID.GetString());

                bool isPresent = PrefixedOptional.DecodeBytes(packet._data, ref offset);
                if (isPresent)
                {
                    NBT EntryData = new();
                    int EntryDataBytes = EntryData.ReadFromBytes(packet._data[offset..], true);
                    offset += EntryDataBytes;
                    registryEntry.Data = EntryData;
                    if (shouldLogInfo)
                        Logging.LogDebug("\n" + EntryData.GetNBTAsString(2));
                }

                registry._Entries.Add(registryEntry);
            }
        }
        catch (Exception e)
        {
            Logging.LogError("REGISTRY DATA: " + e.ToString());
        }
        _gamestateHandler.UpdateServerRegistryData(registry);
    }

    internal void HandleTransfer(MinecraftServerPacket minecraftPacket)
    {
        try
        {
            Logging.LogDebug("Transfer");
            _coreModule!.InvokeEvent("GAMESTATE_ProcessTransfer");
            ConfigTransferPacket configTransferPacket = new();
            configTransferPacket.DecodeFromBytes(minecraftPacket._data);
            _coreModule.SignalInteractiveHoldTransfer(
                Core_Engine.State.Configuration,
                Core_Engine.State.JoiningServer
            );
            _networkModule.DisconnectFromServer(minecraftPacket._remoteHostID);
            _ = _coreModule.HandleCommand(
                "join",
                [configTransferPacket._Host, "-t", configTransferPacket._Port.ToString()]
            );
        }
        catch (Exception e)
        {
            Logging.LogError(e.ToString());
        }
    }

    public void SendServerboundPluginMessage(Guid remoteHostID)
    {
        Logging.LogDebug("Sending Plugin Message");
        PluginMessagePacket pluginMessagePacket = new()
        {
            _Channel = new("minecraft:brand"),
            _Data = StringN.GetBytes("lotus"),
        };
        _networkModule.SendPacket(remoteHostID, pluginMessagePacket);
    }

    public void SendServerboundClientInformation(Guid remoteHostID)
    {
        Logging.LogDebug("Sending Client Information");
        ClientInformationPacket pluginMessagePacket = new()
        {
            _AllowServerListings = IGameStateHandlerModule._Settings._AllowServerListings,
            _ChatColors = IGameStateHandlerModule._Settings._ChatSettings._Colors,
            _ChatMode = (int)IGameStateHandlerModule._Settings._ChatSettings._ChatShown,
            _DisplayedSkinParts = IGameStateHandlerModule
                ._Settings
                ._SkinCustomization
                ._DisplayedSkinParts,
            _EnableTextFiltering = false, //hardcoded for now, add setting later
            _Locale = "en_US", //hardcoded, chage if more locales added later
            _MainHand = (int)IGameStateHandlerModule._Settings._SkinCustomization._MainHand,
            _ParticleStatus = (int)IGameStateHandlerModule._Settings._VideoSettings._ParticleStatus,
            _ViewDistance = IGameStateHandlerModule._Settings._VideoSettings._RenderDistance,
        };
        _networkModule.SendPacket(remoteHostID, pluginMessagePacket);
    }

    public void SendConfigBrandAndClientInfo(Guid remoteHostID)
    {
        Logging.LogDebug("Sending Plugin Message And Client Information");
        PluginMessagePacket pluginBrandMessagePacket = new()
        {
            _Channel = new("minecraft:brand"),
            _Data = StringN.GetBytes("lotus"),
        };
        ClientInformationPacket pluginInfoMessagePacket = new()
        {
            _AllowServerListings = IGameStateHandlerModule._Settings._AllowServerListings,
            _ChatColors = IGameStateHandlerModule._Settings._ChatSettings._Colors,
            _ChatMode = (int)IGameStateHandlerModule._Settings._ChatSettings._ChatShown,
            _DisplayedSkinParts = IGameStateHandlerModule
                ._Settings
                ._SkinCustomization
                ._DisplayedSkinParts,
            _EnableTextFiltering = false, //hardcoded for now, add setting later
            _Locale = "en_US", //hardcoded, chage if more locales added later
            _MainHand = (int)IGameStateHandlerModule._Settings._SkinCustomization._MainHand,
            _ParticleStatus = (int)IGameStateHandlerModule._Settings._VideoSettings._ParticleStatus,
            _ViewDistance = IGameStateHandlerModule._Settings._VideoSettings._RenderDistance,
        };
        _networkModule.SendPackets(
            remoteHostID,
            [pluginBrandMessagePacket, pluginInfoMessagePacket]
        );
    }

    internal void HandleCookieRequest(MinecraftServerPacket packet)
    {
        Identifier Key = new();
        int offset = 0;
        Key.GetFromBytes(packet._data, ref offset);

        var cookie = _gamestateHandler.GetServerCookie(Key);

        CookieResponsepacket cookieResponsepacket = new()
        {
            _protocol_ID = 0x00,
            _Key = Key,
            _Payload = cookie?._Payload ?? [],
        };

        _networkModule.SendPacket(packet._remoteHostID, cookieResponsepacket);
    }

    internal void HandlePluginMessage(MinecraftServerPacket packet)
    {
        //(int value, int offset) = VarInt_VarLong.DecodeVarInt(packet._Data);
        int offset = 0;
        Identifier channel = new();
        channel.GetFromBytes(packet._data, ref offset);

        PluginMessageReceivedEventArgs args = new(
            packet._remoteHostID,
            ConnectionState.CONFIGURATION,
            channel,
            packet._data[offset..]
        );

        _coreModule!.InvokeEvent("PLUGIN_Packet_Received", args);
    }

    internal void HandleDisconnect(MinecraftServerPacket packet)
    {
        try
        {
            /* Logging.LogInfo(
                $"Client disconnected during Config, Reason:{Encoding.UTF8.GetString(packet._Data).Replace("\r", "").Replace("\n", "").Trim()}"
            ); */
            NBT test = new();
            test.ReadFromBytes(packet._data, true);
            Console.WriteLine("\n\n" + test.GetNBTAsString());
        }
        catch (Exception e)
        {
            Logging.LogError(e.ToString());
            throw;
        }

        _networkModule.DisconnectFromServer(packet._remoteHostID);
        //Core_Engine.CurrentState = Core_Engine.State.Interactive;
        _coreModule!.SignalInteractiveFree(Core_Engine.State.Configuration);
    }

    internal void HandleFinishConfiguration(MinecraftServerPacket packet)
    {
        Logging.LogInfo("Server Config Success!");
        _coreModule!.SignalInteractiveHoldTransfer(
            Core_Engine.State.Configuration,
            Core_Engine.State.Play
        );
        _networkModule.GetServerConnection(packet._remoteHostID)!._connectionState =
            ConnectionState.PLAY;

        _networkModule.SendPacket(packet._remoteHostID, new EmptyPacket(0x03));
        _serverPlayHandler.InitPlaySession(packet._remoteHostID);
    }

    internal void HandleKeepAlive(MinecraftServerPacket packet)
    {
        int offset = 0;
        KeepAlivePacket keepAlivePacket = new(
            0x04,
            NetworkLong.DecodeBytes(packet._data, ref offset)
        );

        _gamestateHandler.SetLastKeepAliveTime(DateTime.Now);
        _networkModule.SendPacket(packet._remoteHostID, keepAlivePacket);
    }

    internal void HandlePing(MinecraftServerPacket packet)
    {
        int offset = 0;
        PongPacket pongPacket = new(0x05, NetworkInt.DecodeBytes(packet._data, ref offset));
        _networkModule.SendPacket(packet._remoteHostID, pongPacket);
    }

    internal void HandleAddResourcePack(MinecraftServerPacket packet)
    {
        ResourcePack resourcePack = new();
        resourcePack.DecodeBytes(packet._data);
        _gamestateHandler.AddServerResourcePack(resourcePack);
    }

    internal void HandleRemoveResourcePack(MinecraftServerPacket packet)
    {
        throw new NotImplementedException();
    }

    internal void HandleFeatureFlags(MinecraftServerPacket packet)
    {
        //Logging.LogDebug("HandleFeatureFlags");
        int offset = 0;
        int arraySize = PrefixedArray.GetSizeOfArray(packet._data, ref offset);
        for (int i = 0; i < arraySize; i++)
        {
            Identifier tmp = new();
            tmp.GetFromBytes(packet._data, ref offset);
            //Logging.LogDebug("\t" + tmp.GetString());
        }
    }

    internal void HandleUpdateTags(MinecraftServerPacket packet)
    {
        int offset = 0;
        int arraySize = PrefixedArray.GetSizeOfArray(packet._data, ref offset);
        //Logging.LogDebug("HandleUpdateTags");
        for (int i = 0; i < arraySize; i++)
        {
            Identifier Registry = new();
            Registry.GetFromBytes(packet._data, ref offset);
            //Logging.LogDebug($"\tRegistry_Name: {Registry}");
            int TagsArraySize = PrefixedArray.GetSizeOfArray(packet._data, ref offset);
            for (int j = 0; j < TagsArraySize; j++)
            {
                Identifier TagName = new();
                TagName.GetFromBytes(packet._data, ref offset);
                //Logging.LogDebug($"\t\tTag_Name: {TagName}");

                int tagArraySize = PrefixedArray.GetSizeOfArray(packet._data, ref offset);
                List<int> values = new();
                for (int k = 0; k < tagArraySize; k++)
                {
                    int value = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);
                    values.Add(value);
                    //Logging.LogDebug($"\t\t\tTag_Value: {value}");
                }

                _gamestateHandler.AddServerTag(Registry, TagName, values);
            }
        }
    }
}
