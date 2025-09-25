using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace LotusCore.Utils;

public static class MinecraftKeyFormatter
{
    public static byte[] ConvertPemToX509Bytes(string pem)
    {
        Logging.LogDebug($"ConvertPemToX509Bytes PEM:{pem} Len:{Encoding.UTF8.GetByteCount(pem)}");
        string base64 = Regex.Replace(
            pem,
            @"-----BEGIN RSA PUBLIC KEY-----|-----END RSA PUBLIC KEY-----|\s+",
            ""
        );
        byte[] bytes = Convert.FromBase64String(base64);
        using RSA rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(bytes, out _);

        Logging.LogDebug($"After len:{rsa.ExportSubjectPublicKeyInfo().Length}");
        return rsa.ExportSubjectPublicKeyInfo();
    }
}
