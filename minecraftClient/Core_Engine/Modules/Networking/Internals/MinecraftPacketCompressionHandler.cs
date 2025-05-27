using System;
using System.IO;
using Ionic.Zlib;

namespace Core_Engine.Modules.Networking.Internals
{
    public static class ZlibCompressionHandler
    {
        public static byte[] Compress(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (
                    ZlibStream zlibStream = new ZlibStream(memoryStream, CompressionMode.Compress)
                )
                {
                    zlibStream.Write(data, 0, data.Length);
                }
                return memoryStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] compressedData)
        {
            using (MemoryStream memoryStream = new MemoryStream(compressedData))
            {
                using (
                    ZlibStream zlibStream = new ZlibStream(memoryStream, CompressionMode.Decompress)
                )
                {
                    using (MemoryStream decompressedStream = new MemoryStream())
                    {
                        zlibStream.CopyTo(decompressedStream);
                        return decompressedStream.ToArray();
                    }
                }
            }
        }
    }
}
