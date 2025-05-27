using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Core_Engine.Modules.Networking.Internals
{
    public class Encryption
    {
        public static byte[] SharedSecret { get; private set; }

        private static IBufferedCipher? EncryptionCipher;
        private static IBufferedCipher? DecryptionCipher;

        public byte[] EncryptData(byte[] data)
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

        public byte[] DecryptData(byte[] data)
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

        public string GenerateMinecraftAuthenticationHash(string serverID, byte[] serverPublicKey)
        {
            GenerateSharedSecret();

            SHA1 sha1 = SHA1.Create();

            sha1.TransformBlock(Encoding.ASCII.GetBytes(serverID), 0, serverID.Length, null, 0);
            sha1.TransformBlock(SharedSecret, 0, SharedSecret.Length, null, 0);
            sha1.TransformFinalBlock(serverPublicKey, 0, serverPublicKey.Length);
            return MinecraftHexDigest(sha1.Hash!.Reverse().ToArray());
        }

        private void InitCiphers()
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

        private void GenerateSharedSecret()
        {
            SharedSecret = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(SharedSecret);
            }
        }

        private string MinecraftHexDigest(byte[] bytes)
        {
            var bitInt = new BigInteger(bytes);
            if (bitInt < 0)
            {
                // toss in a negative sign if the interpreted number is negative
                return "-" + (-bitInt).ToString("x").TrimStart('0');
            }
            else
            {
                return bitInt.ToString("x").TrimStart('0');
            }
        }
    }
}
