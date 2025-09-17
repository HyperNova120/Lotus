namespace LotusCore.Utils.MinecraftPaths;

public struct MinecraftPathsStruct
{
    public static readonly string _MinecraftFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        ".minecraft"
    );
    public static readonly string _LotusData = Path.Combine(_MinecraftFolderPath, "lotus");

    public static readonly string _ServerData = Path.Combine(_MinecraftFolderPath, "servers.dat");
    public static readonly string _Assets = Path.Combine(_MinecraftFolderPath, "assets");
    public static readonly string _Skins = Path.Combine(_MinecraftFolderPath, "skins");
    public static readonly string _LotusCache = Path.Combine(_LotusData, "lotus_cache.bin");
    public static readonly string _LotusCacheConf = Path.Combine(_LotusData, "lotus_cacheConf.bin");

    public static void InitRequiredFolderStructure()
    {
        if (!Directory.Exists(_LotusData))
        {
            Directory.CreateDirectory(_LotusData);
        }
    }
}

public static class MojangAPIEndpoints
{
    public static string _PlayerConfigEndpoints = "https://api.minecraftservices.com";
}
