using LotusCore.BaseClasses.Types;

namespace LotusCore.BaseClasses.PluginChannels.DataTypes;


struct EntityPath
{
    public bool Reaches_Target;

    public int Current_node_index;

    public Position Target_Position;

    public PathNode[] Nodes;
    public PathNode[] Target_Nodes;
    public PathNode[] Traversed_Nodes;
    public PathNode[] Remaining_Nodes;
}