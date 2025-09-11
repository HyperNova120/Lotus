using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using LotusCore.Utils.MinecraftPaths;

namespace LotusCore.Utils.CacheEncryption;

public static class CacheEncryption
{
    //probably not very good, but hopefully enough.
    //this isn't meant to really be anything more then a personal project atm.

    private static string _IntegrityCheckString = "LOTUS_CACHE::";

    private static byte[] GenerateSalt(int size = 32)
    {
        byte[] salt = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    private static byte[] DeriveLinuxKey(byte[] salt, int keySize = 32)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            File.ReadAllText("/etc/machine-id").Trim(),
            salt,
            100_000,
            HashAlgorithmName.SHA256
        );
        return pbkdf2.GetBytes(keySize);
    }

    private static byte[] GetSalt()
    {
        if (File.Exists(MinecraftPathsStruct._LotusCacheConf))
        {
            return File.ReadAllBytes(MinecraftPathsStruct._LotusCacheConf);
        }
        byte[] salt = GenerateSalt();
        File.WriteAllBytes(MinecraftPathsStruct._LotusCacheConf, salt);
        return salt;
    }

    public static byte[]? DecryptCache(byte[] encryptedCache)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ProtectedData.Unprotect(
                    encryptedCache,
                    null,
                    DataProtectionScope.CurrentUser
                );
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using var aes = Aes.Create();
                using var ms = new MemoryStream(encryptedCache);

                byte[] iv = new byte[aes.BlockSize / 8];
                ms.Read(iv, 0, iv.Length);
                aes.Key = DeriveLinuxKey(GetSalt());
                aes.IV = iv;

                using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var result = new MemoryStream();
                cs.CopyTo(result);

                byte[] decryptedCache = result.ToArray();

                if (
                    Encoding.UTF8.GetString(decryptedCache[.._IntegrityCheckString.Length])
                    != _IntegrityCheckString
                )
                {
                    //integrity check failed
                    return null;
                }

                return decryptedCache[_IntegrityCheckString.Length..];
            }
        }
        catch
        {
            return null; //Decryption failed
        }
        return encryptedCache; //no encryption implemented
    }

    public static byte[] EncryptCache(byte[] cache)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ProtectedData.Protect(cache, null, DataProtectionScope.CurrentUser);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                cache = [.. Encoding.UTF8.GetBytes(_IntegrityCheckString), .. cache];
                using var aes = Aes.Create();
                aes.Key = DeriveLinuxKey(GetSalt());
                aes.GenerateIV();

                using var memStream = new MemoryStream();
                memStream.Write(aes.IV, 0, aes.IV.Length);

                using var cryptoStream = new CryptoStream(
                    memStream,
                    aes.CreateEncryptor(),
                    CryptoStreamMode.Write
                );
                cryptoStream.Write(cache, 0, cache.Length);
                cryptoStream.FlushFinalBlock();
                return memStream.ToArray();
            }
        }
        catch
        {
            return cache; //encryption failed
        }
        return cache; //no encryption implemented
    }
}
