namespace Core_Engine.BaseClasses.PluginChannels.DataTypes;

struct PathNode
{
    public  int X,
        Y,
        Z;

    public  float Distance_From_Origin,
        Cost,
        Heap_Weight;

    public  bool Has_Been_Visited;

    public  NodeTypeEnum NodeType;

    public enum NodeTypeEnum
    {
        Blocked,
        Open,
        Walkable,
        Walkable_Door,
        Trapdoor,
        Powder_Snow,
        Danger_Powder_Snow,
        Fence,
        Lava,
        Water,
        Water_Border,
        Rail,
        Unpassable_Rail,
        Danger_Fire,
        Damage_Fire,
        Danger_Other,
        Damage_Other,
        Open_Door,
        Closed_Wooden_Door,
        Closed_Iron_Door,
        Breach_Water,
        Leaves,
        Sticky_Honey,
        Cocoa,
        Damage_Cautious,
        Danger_Trapdoor,
    }
}
