using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LotusCore.Utils.MinecraftPaths;

namespace LotusCore.Utils.SkinCapeTextures;

public static class SkinCapeTextureHandler
{
    /// <summary>
    /// retrieves the specified texture.
    /// use only for skin textures found at http://textures.minecraft.net/texture/
    /// </summary>
    /// <param name="url"></param>
    /// <returns>texture</returns>
    public static async Task<byte[]> GetSkinTexture(string url)
    {
        string urlHash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(url)));

        string skinFolderPath = Path.Combine(MinecraftPathsStruct._Skins, urlHash.Substring(0, 2));
        string skinFilePath = Path.Combine(skinFolderPath, urlHash);
        if (Directory.Exists(skinFolderPath) && File.Exists(skinFilePath))
        {
            return await File.ReadAllBytesAsync(skinFilePath);
        }
        Directory.CreateDirectory(skinFolderPath);
        //need to retrieve skin from mojang api
        byte[] skinBytes = await HttpHandler.DownloadSkinTexture(url);
        if (skinBytes.Length == 0)
        {
            //download failed
            return [];
        }
        await File.WriteAllBytesAsync(skinFilePath, skinBytes);
        return skinBytes;
    }

    /// <summary>
    /// retrieves the specified texture.
    /// use only for skin textures found at http://textures.minecraft.net/texture/
    /// </summary>
    /// <param name="url"></param>
    /// <returns>texture</returns>
    public static async Task<byte[]> GetCapeTexture(string url)
    {
        string urlHash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(url)));

        string capeFolderPath = Path.Combine(MinecraftPathsStruct._Skins, urlHash.Substring(0, 2));
        string capeFilePath = Path.Combine(capeFolderPath, urlHash);
        byte[] capeBytes = await HttpHandler.DownloadSkinTexture(url);
        if (capeBytes.Length == 0)
        {
            //download failed
            return [];
        }
        return capeBytes;
    }
}
