using System.Text.Json;
using LotusCore.Interfaces;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.ServerList.Models;
using LotusCore.Utils;
using LotusCore.Utils;
using LotusCore.Utils.NBTInternals.Tags;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LotusCore.Modules.ServerList.Commands
{
    public class ListCommand : ICommandBase
    {
        NBT _serverList;

        public ListCommand(NBT serverList)
        {
            _serverList = serverList;
        }

        public string GetCommandDescription()
        {
            return "Lists all servers in the server lsit";
        }

        public string GetCommandCorrectUsage()
        {
            return "Correct usage: 'list'";
        }

        public async Task ProcessCommand(string[] commandArgs)
        {
            if (commandArgs.Length != 0)
            {
                Console.WriteLine(GetCommandCorrectUsage());
                return;
            }
            Core_Engine.SignalInteractiveHold(Core_Engine.State.ServerList);
            TAG_List? serverList = _serverList.TryGetTag<TAG_List>("servers");
            if (serverList == null)
            {
                Core_Engine.SignalInteractiveFree(Core_Engine.State.ServerList);
                return;
            }
            List<string[]> rows = new();
            foreach (var tmp in serverList._Contained_Tags)
            {
                TAG_Compound tmpCompound = (TAG_Compound)tmp;
                TAG_Byte? isHidden = (TAG_Byte?)tmpCompound.TryGetTag("hidden");
                if (isHidden != null && isHidden.Value == 1)
                {
                    continue;
                }

                TAG_String? infoTag = (TAG_String?)tmpCompound.TryGetTag("serverlist_info");
                TAG_Double? pingTag = (TAG_Double?)tmpCompound.TryGetTag("ping");

                string infoStr = (infoTag == null) ? "{}" : infoTag.Value;
                string pingStr = (pingTag == null) ? "N/A" : pingTag.Value.ToString();
                string nameStr = ((TAG_String)tmpCompound.TryGetTag("name")!).Value;

                JsonDocument? statusInfo = (infoStr == "{}") ? null : JsonDocument.Parse(infoStr);
                ServerStatusModelPlayers? players =
                    (statusInfo == null)
                        ? null
                        : JsonSerializer.Deserialize<ServerStatusModelPlayers>(
                            statusInfo.RootElement.GetProperty("players")
                        )!;

                int version =
                    (statusInfo == null)
                        ? 0
                        : statusInfo
                            .RootElement.GetProperty("version")
                            .GetProperty("protocol")
                            .GetInt32()!;

                string versionText =
                    (statusInfo == null)
                        ? "N/A"
                        : ProtocolVersionUtils.GetProtocolVersionName(
                            (ProtocolVersionUtils.ProtocolVersion)version
                        );

                string playersOnlineText =
                    (statusInfo != null) ? $"{players.online}/{players.max}" : "N/A";

                rows.Add(
                    [
                        nameStr,
                        playersOnlineText,
                        pingStr + ((pingStr == "N/A") ? "" : "ms"),
                        versionText,
                    ]
                );
            }
            Console.WriteLine(
                StringTableBuilder.CreateTable(
                    40,
                    ["Server Name", "Players Online", "Ping", "Version"],
                    rows.ToArray()
                )
            );
            Core_Engine.SignalInteractiveFree(Core_Engine.State.ServerList);
        }
    }
}
