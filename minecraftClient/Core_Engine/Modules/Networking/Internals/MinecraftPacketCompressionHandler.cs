using System;
using System.IO;
using System.IO.Compression;
namespace Core_Engine.Modules.Networking.Internals
{
    public static class ZlibCompressionHandler
    {
        public static byte[] Compress(byte[] data)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (
                        ZLibStream zlibStream = new ZLibStream(
                            memoryStream,
                            CompressionMode.Compress
                        )
                    )
                    {
                        zlibStream.Write(data, 0, data.Length);
                    }
                    return memoryStream.ToArray();
                }
            }
            catch (Exception e)
            {
                Logging.LogError($"ZlibCompressionHandler; Compress ERROR:{e}");
                return [];
            }
        }

        public static byte[] Decompress(byte[] compressedData)
        {
            using (MemoryStream memoryStream = new MemoryStream(compressedData))
            {
                try
                {
                    using (
                        ZLibStream zlibStream = new ZLibStream(
                            memoryStream,
                            CompressionMode.Decompress
                        )
                    )
                    {
                        using (MemoryStream decompressedStream = new MemoryStream())
                        {
                            zlibStream.CopyTo(decompressedStream);
                            return decompressedStream.ToArray();
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.LogError($"ZlibCompressionHandler; Decompress ERROR:{e}");
                    return [];
                }
            }
        }
    }
}
