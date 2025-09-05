namespace LotusCore.Utils.MinecraftPaths;

public struct MinecraftPathsStruct
{
    public static readonly string _MinecraftFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        ".minecraft"
    );

    public static readonly string _ServerData = Path.Combine(_MinecraftFolderPath, "servers.dat");
}
