using System.Net;
using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;
using Core_Engine.Interfaces;
using Core_Engine.Modules.GameStateHandler;
using Core_Engine.Modules.GameStateHandler.BaseClasses;
using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.Networking.Packets.ClientBound.Configuration;
using Core_Engine.Utils;

namespace Core_Engine.Modules.ServerConfig.Internals;

public class ConfigurationInternals
{
    private readonly IGameStateHandler gameStateHandler;
    private readonly Networking.Networking NetworkingManager;

    public ConfigurationInternals()
    {
        gameStateHandler = Core_Engine.GetModule<IGameStateHandler>("GameStateHandler")!;
        NetworkingManager = Core_Engine.GetModule<Networking.Networking>("Networking")!;
    }

    public void StoreCookie(MinecraftServerPacket minecraftPacket)
    {
        StoreCookiePacket storeCookiePacket = new();
        storeCookiePacket.DecodeFromBytes(minecraftPacket.data);
        gameStateHandler.AddServerCookie(storeCookiePacket.serverCookie!);
    }

    internal void ClientboundKnownPacks(MinecraftServerPacket packet)
    {
        throw new NotImplementedException();
    }

    internal void FinishConfiguration(MinecraftServerPacket packet)
    {
        try
        {
            Logging.LogDebug("FinishConfiguration; Showing REGISTRIES");
            foreach (
                var item in ((GameStateHandler.GameStateHandler)gameStateHandler).serverRegistryData
            )
            {
                Logging.LogDebug(item.Key.GetString());
                foreach (var item2 in item.Value.Entries)
                {
                    string value = (item2.Data != null) ? item2.Data.GetNBTAsString(3) : "NULL";
                    Logging.LogDebug($"\tID:{item2.ID.GetString()} VALUE:\n{value}");
                }
            }
        }
        catch (Exception e)
        {
            Logging.LogError(e.ToString(), false);
        }
    }

    internal void RegistryData(MinecraftServerPacket packet)
    {
        Identifier RegistryID = new();
        int offset = RegistryID.GetFromBytes(packet.data);

        Logging.LogDebug($"\t\tRegistryID: {RegistryID.GetString()}");

        (int arraySize, int numBytesRead) = PrefixedArray.GetSizeOfArray(packet.data[offset..]);
        offset += numBytesRead;

        RegistryData registryData = new() { ID = RegistryID };
        for (int i = 0; i < arraySize; i++)
        {
            RegistryEntry registryEntry = new();

            Identifier ID = new();
            offset += ID.GetFromBytes(packet.data[offset..]);
            registryEntry.ID = ID;
            Logging.LogDebug("\t\tEntry: " + ID.GetString());

            (bool isPresent, int numberBytesRead) = PrefixedOptional.DecodeBytes(
                packet.data[offset..]
            );
            offset += numberBytesRead;

            if (isPresent)
            {
                NBT Data = new(true);
                try
                {
                    Logging.LogDebug($"NBT Present");
                    offset += Data.ReadFromBytes(packet.data[offset..], true);
                    registryEntry.Data = Data;
                    Logging.LogDebug("\tNBT DATA:\n" + Data.GetNBTAsString(2));
                }
                catch (Exception e)
                {
                    Logging.LogError(e.ToString(), false);
                    Logging.LogError($"DATA: {Data.GetNBTAsString()}", false);
                }
            }
            else
            {
                Logging.LogDebug(
                    $"NBT NOT PRESENT, offset:{offset} dataLength:{packet.data.Length}"
                );
            }

            Logging.LogDebug($"offset:{offset} dataLength:{packet.data.Length}");

            registryData.Entries.Add(registryEntry);
        }
        gameStateHandler.AddServerRegistryData(registryData);
    }

    internal void Transfer(MinecraftServerPacket minecraftPacket)
    {
        gameStateHandler.ProcessTransfer();
        ConfigTransferPacket configTransferPacket = new();
        configTransferPacket.DecodeFromBytes(minecraftPacket.data);
        Core_Engine.signalInteractiveTransferHold(
            Core_Engine.State.Configuration,
            Core_Engine.State.JoiningServer
        );
        NetworkingManager.DisconnectFromServer(minecraftPacket.remoteHost);
        _ = Core_Engine.HandleCommand(
            "join",
            [configTransferPacket.host, configTransferPacket.port.ToString()]
        );
    }
}
