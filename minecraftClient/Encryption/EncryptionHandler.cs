using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

public static class EncryptionHandler
{
    public static byte[] SharedSecret { get; private set; }

    private static IBufferedCipher? EncryptionCipher;
    private static IBufferedCipher? DecryptionCipher;

    public static string GenerateMinecraftAuthenticationHash(
        string serverID,
        byte[] serverPublicKey
    )
    {
        GenerateSharedSecret();

        SHA1 sha1 = SHA1.Create();

        sha1.TransformBlock(Encoding.ASCII.GetBytes(serverID), 0, serverID.Length, null, 0);
        sha1.TransformBlock(SharedSecret, 0, SharedSecret.Length, null, 0);
        sha1.TransformFinalBlock(serverPublicKey, 0, serverPublicKey.Length);
        return MinecraftHexDigest(sha1.Hash!.Reverse().ToArray());
    }

    public static string MinecraftHexDigest(byte[] bytes)
    {
        var bitInt = new BigInteger(bytes);
        string hex;
        if (bitInt < 0)
        {
            // toss in a negative sign if the interpreted number is negative
            hex = "-" + (-bitInt).ToString("x").TrimStart('0');
        }
        else
        {
            hex = bitInt.ToString("x").TrimStart('0');
        }

        Logging.LogDebug($"MinecraftHexDigest:{bitInt.ToString()} HEX:{hex}");
        return hex;
    }

    /* public static String MinecraftShaDigest(String input)
    {
        var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
        // Reverse the bytes since BigInteger uses little endian
        Array.Reverse(hash);

        BigInteger b = new BigInteger(hash);
        // very annoyingly, BigInteger in C# tries to be smart and puts in
        // a leading 0 when formatting as a hex number to allow roundtripping
        // of negative numbers, thus we have to trim it off.
        if (b < 0)
        {
            // toss in a negative sign if the interpreted number is negative
            return "-" + (-b).ToString("x").TrimStart('0');
        }
        else
        {
            return b.ToString("x").TrimStart('0');
        }
    } */

    private static void GenerateSharedSecret()
    {
        SharedSecret = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(SharedSecret);
        }
    }

    public static byte[] EncryptData(byte[] data)
    {
        if (SharedSecret == null)
        {
            return [];
        }
        if (EncryptionCipher == null)
        {
            InitCiphers();
        }

        return EncryptionCipher!.DoFinal(data);
    }

    private static void InitCiphers()
    {
        EncryptionCipher = CipherUtilities.GetCipher("AES/CFB8/NoPadding");
        EncryptionCipher.Init(
            true,
            new ParametersWithIV(new KeyParameter(SharedSecret), SharedSecret)
        );

        DecryptionCipher = CipherUtilities.GetCipher("AES/CFB8/NoPadding");
        DecryptionCipher.Init(
            false,
            new ParametersWithIV(new KeyParameter(SharedSecret), SharedSecret)
        );
    }

    public static byte[] DecryptData(byte[] data)
    {
        if (SharedSecret == null)
        {
            return [];
        }
        if (DecryptionCipher == null)
        {
            InitCiphers();
        }

        return DecryptionCipher!.DoFinal(data);
    }
}
