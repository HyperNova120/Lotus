namespace LotusCore.Utils.MinecraftPaths;

using System.Runtime.InteropServices;

public struct MinecraftPathsStruct
{
    public static readonly string _MinecraftFolderPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        ".minecraft"
    ) : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".minecraft");
    public static readonly string _LotusData = Path.Combine(_MinecraftFolderPath, "lotus");

    public static readonly string _ServerData = Path.Combine(_MinecraftFolderPath, "servers.dat");
    public static readonly string _Assets = Path.Combine(_MinecraftFolderPath, "assets");
    public static readonly string _Skins = Path.Combine(_MinecraftFolderPath, "skins");
    public static readonly string _LotusCache = Path.Combine(_LotusData, "lotus_cache.bin");
    public static readonly string _LotusCacheConf = Path.Combine(_LotusData, "lotus_cacheConf.bin");

    public static void InitRequiredFolderStructure()
    {
        EnsureDirExists(_LotusData);
        EnsureDirExists(_Assets);
        EnsureFileExists(_ServerData);
        EnsureFileExists(_LotusCache);
        EnsureFileExists(_LotusCacheConf);
    }

    private static void EnsureDirExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static void EnsureFileExists(string path)
    {
        if (!File.Exists(path))
        {
            File.Create(path);
        }
    }
}

public static class MojangAPIEndpoints
{
    public static string _PlayerConfigEndpoints = "https://api.minecraftservices.com";
}
