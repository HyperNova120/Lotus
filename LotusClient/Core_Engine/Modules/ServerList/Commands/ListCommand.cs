using LotusCore.Interfaces;
using LotusCore.Utils;
using LotusCore.Utils.NBTInternals.Tags;

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
            foreach (var tmp in serverList._Contained_Tags)
            {
                TAG_Compound tmpCompound = (TAG_Compound)tmp;
                TAG_Byte? isHidden = (TAG_Byte?)tmpCompound.TryGetTag("hidden");
                if (isHidden != null && isHidden.Value == 1)
                {
                    continue;
                }
                Console.WriteLine(((TAG_String)tmpCompound.TryGetTag("name")!).Value);
            }
            Core_Engine.SignalInteractiveFree(Core_Engine.State.ServerList);
        }
    }
}
