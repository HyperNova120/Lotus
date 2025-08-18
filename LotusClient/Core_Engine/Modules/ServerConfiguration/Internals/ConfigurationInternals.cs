using System.Net;
using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;
using Core_Engine.Interfaces;
using Core_Engine.Modules.GameStateHandler;
using Core_Engine.Modules.GameStateHandler.BaseClasses;
using Core_Engine.Modules.Networking.Internals;
using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.Networking.Packets.ClientBound.Configuration;
using Core_Engine.Modules.Networking.Packets.ServerBound.Configuration;
using Core_Engine.Utils;

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

    public void StoreCookie(MinecraftServerPacket minecraftPacket)
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

    internal void ClientboundKnownPacks(MinecraftServerPacket packet)
    {
        ConfigClientboundKnownPacks configClientboundKnownPacks = new();
        configClientboundKnownPacks.DecideFromBytes(packet._Data);

        //tmp for testing
        //NetworkingManager.SendPacket(packet.remoteHost, );
        throw new NotImplementedException();
    }

    internal void RegistryData(MinecraftServerPacket packet)
    {
        Identifier RegistryID = new();
        int offset = RegistryID.GetFromBytes(packet._Data);

        Logging.LogDebug($"\t\tRegistryID: {RegistryID.GetString()}");

        (int arraySize, int numBytesRead) = PrefixedArray.GetSizeOfArray(packet._Data[offset..]);
        offset += numBytesRead;

        RegistryData registryData = new() { _RegistryNameSpace = RegistryID };
        for (int i = 0; i < arraySize; i++)
        {
            if (offset == packet._Data.Length)
            {
                break;
            }
            RegistryEntry registryEntry = new();

            Identifier ID = new();
            Logging.LogDebug($"\t\t\toffset:{offset} packet.data Length:{packet._Data.Length}");
            offset += ID.GetFromBytes(packet._Data[offset..]);
            registryEntry.ID = ID;
            //Logging.LogDebug("\t\tEntry: " + ID.GetString());

            (bool isPresent, int numberBytesRead) = PrefixedOptional.DecodeBytes(
                packet._Data[offset..]
            );
            offset += numberBytesRead;

            if (isPresent)
            {
                NBT Data = new(true);
                try
                {
                    //Logging.LogDebug($"NBT Present");
                    offset += Data.ReadFromBytes(packet._Data[offset..], true);
                    registryEntry.Data = Data;
                    //Logging.LogDebug("\tNBT DATA:\n" + Data.GetNBTAsString(2));
                }
                catch (Exception e)
                {
                    Logging.LogError(e.ToString(), false);
                    //Logging.LogError($"DATA: {Data.GetNBTAsString()}", false);
                }
            }
            else
            {
                /* Logging.LogDebug(
                    $"NBT NOT PRESENT, offset:{offset} dataLength:{packet.data.Length}"
                ); */
            }

            //Logging.LogDebug($"offset:{offset} dataLength:{packet.data.Length}");

            registryData._Entries.Add(registryEntry);
        }
        _GameStateHandler.AddServerRegistryData(registryData);
    }

    internal void Transfer(MinecraftServerPacket minecraftPacket)
    {
        try
        {
            Logging.LogDebug("Transfer");
            _GameStateHandler.ProcessTransfer();
            ConfigTransferPacket configTransferPacket = new();
            configTransferPacket.DecodeFromBytes(minecraftPacket._Data);
            Core_Engine.signalInteractiveTransferHold(
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
            _DisplayedSkinParts = IGameStateHandler._Settings._SkinCustomization._DisplayedSkinParts,
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
            _DisplayedSkinParts = IGameStateHandler._Settings._SkinCustomization._DisplayedSkinParts,
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
}
