using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace LotusCore.Modules.Networking.Internals
{
    public class Encryption
    {
        public byte[] _SharedSecret { get; private set; }

        private IBufferedCipher? _EncryptionCipher;
        private IBufferedCipher? _DecryptionCipher;

        public Encryption()
        {
            //Logging.LogDebug($"\tEncryption Constructor");
            _SharedSecret = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(_SharedSecret);
            }
        }

        public byte[] EncryptData(byte[] data)
        {
            if (_SharedSecret == null)
            {
                return [];
            }
            if (_EncryptionCipher == null)
            {
                InitCiphers();
            }
            byte[] buffer = new byte[data.Length];
            _EncryptionCipher!.ProcessBytes(data, 0, data.Length, buffer, 0);
            return buffer;
        }

        public byte[] DecryptData(byte[] data)
        {
            if (_SharedSecret == null)
            {
                return [];
            }
            if (_DecryptionCipher == null)
            {
                InitCiphers();
            }
            byte[] buffer = new byte[data.Length];
            _DecryptionCipher!.ProcessBytes(data, 0, data.Length, buffer, 0);
            return buffer;
        }

        public string GenerateMinecraftAuthenticationHash(string serverID, byte[] serverPublicKey)
        {
            SHA1 sha1 = SHA1.Create();

            sha1.TransformBlock(Encoding.ASCII.GetBytes(serverID), 0, serverID.Length, null, 0);
            sha1.TransformBlock(_SharedSecret, 0, _SharedSecret.Length, null, 0);
            sha1.TransformFinalBlock(serverPublicKey, 0, serverPublicKey.Length);
            return MinecraftHexDigest(sha1.Hash!.Reverse().ToArray());
        }

        private void InitCiphers()
        {
            //Logging.LogDebug("\tInit Ciphers");
            _EncryptionCipher = CipherUtilities.GetCipher("AES/CFB8/NoPadding");
            _EncryptionCipher.Init(
                true,
                new ParametersWithIV(new KeyParameter(_SharedSecret), _SharedSecret)
            );

            _DecryptionCipher = CipherUtilities.GetCipher("AES/CFB8/NoPadding");
            _DecryptionCipher.Init(
                false,
                new ParametersWithIV(new KeyParameter(_SharedSecret), _SharedSecret)
            );
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
