using MinecraftNetworking.Compression;
using MinecraftNetworking.Types;

namespace MinecraftNetworking
{
    public static class MinecraftPacketHandler
    {
        public static bool IsCompressionEnabled = false;
        public static bool IsEncryptionEnabled = false;
        public static int CompresionThreshold;

        public static byte[] CreatePacket(MinecraftPacket data)
        {
            if (!IsCompressionEnabled)
            {
                byte[] packet_id = VarInt_VarLong.EncodeInt(data.protocol_id);
                byte[] packet_data = data.GetBytes();
                byte[] packet_length = VarInt_VarLong.EncodeInt(
                    packet_id.Length + packet_data.Length
                );
                byte[] packetBytes = [.. packet_length, .. packet_id, .. packet_data];
                if (IsEncryptionEnabled)
                {
                    packetBytes = EncryptionHandler.EncryptData(packetBytes);
                }
                return packetBytes;
            }
            else
            {
                byte[] packet_id = VarInt_VarLong.EncodeInt(data.protocol_id);
                byte[] packet_data = data.GetBytes();
                if (packet_id.Length + packet_data.Length < CompresionThreshold)
                {
                    //prepare uncompressed
                    byte[] data_length = VarInt_VarLong.EncodeInt(0);
                    byte[] packet_length = VarInt_VarLong.EncodeInt(
                        data_length.Length + packet_id.Length + packet_data.Length
                    );
                    byte[] packetBytes =
                    [
                        .. packet_length,
                        .. data_length,
                        .. packet_id,
                        .. packet_data,
                    ];

                    if (IsEncryptionEnabled)
                    {
                        packetBytes = EncryptionHandler.EncryptData(packetBytes);
                    }
                    return packetBytes;
                }
                else
                {
                    //compress
                    byte[] data_length = VarInt_VarLong.EncodeInt(
                        packet_id.Length + packet_data.Length
                    );
                    byte[] compressed_Section = ZlibCompressionHandler.Compress(
                        [.. packet_id, .. packet_data]
                    );
                    byte[] packet_length = VarInt_VarLong.EncodeInt(
                        data_length.Length + packet_id.Length + packet_data.Length
                    );
                    byte[] packetBytes = [.. packet_length, .. data_length, .. compressed_Section];

                    if (IsEncryptionEnabled)
                    {
                        packetBytes = EncryptionHandler.EncryptData(packetBytes);
                    }
                    return packetBytes;
                }
            }
        }

        public static MinecraftServerPacket DecodePacket(byte[] bytes)
        {
            if (IsEncryptionEnabled)
            {
                Logging.LogDebug("MinecraftPacketHandler DecodePacket Decrypt Packet");
                bytes = EncryptionHandler.DecryptData(bytes);
            }
            if (IsCompressionEnabled)
            {
                //has packet length

                (int packet_length, int packet_length_numBytesRead) = VarInt_VarLong.DecodeVarInt(
                    bytes
                );
                byte[] remainingBytes = bytes.Skip(packet_length_numBytesRead).ToArray();

                if (packet_length != remainingBytes.Length)
                {
                    Logging.LogError(
                        $"Received Packet Data Length mismatch with actual length; packet_length:{packet_length} remainingBytes.Length:{remainingBytes.Length}"
                    );
                }

                (int data_length, int data_length_numBytesRead) = VarInt_VarLong.DecodeVarInt(
                    remainingBytes
                );
                remainingBytes = remainingBytes.Skip(data_length_numBytesRead).ToArray();

                byte[] uncompressed_data = remainingBytes;
                if (data_length != 0)
                {
                    //compressed
                    uncompressed_data = ZlibCompressionHandler.Decompress(remainingBytes);
                    if (data_length != uncompressed_data.Length)
                    {
                        Logging.LogError(
                            $"Compression Length mismatch with actual length; data_length:{data_length} uncompressed_data.Length:{uncompressed_data.Length}"
                        );
                    }
                }

                (int packet_id, int packet_id_numBytesRead) = VarInt_VarLong.DecodeVarInt(
                    uncompressed_data
                );
                remainingBytes = uncompressed_data.Skip(packet_id_numBytesRead).ToArray();

                Logging.LogDebug(
                    $"compressed length:{bytes.Length}; packet_length:{packet_length}; data_length:{data_length}; uncompressed_data.Length:{uncompressed_data.Length}"
                );
                MinecraftServerPacket returner = new(packet_id, remainingBytes);
                return returner;
            }
            else
            {
                //only data length
                (int dataLength, int numDataBytes) = VarInt_VarLong.DecodeVarInt(bytes);
                ArraySegment<byte> remainingBytes = new System.ArraySegment<byte>(
                    bytes,
                    numDataBytes,
                    bytes.Length - numDataBytes
                );

                if (dataLength != remainingBytes.Count)
                {
                    Logging.LogError(
                        $"Received Packet Data Length mismatch with actual length; dataLength:{dataLength} remainingBytes.Count:{remainingBytes.Count}"
                    );
                }
                else
                {
                    Logging.LogDebug($"Received Packet Data Length: {dataLength}");
                }

                (int packetId, int numPacketIDBytes) = VarInt_VarLong.DecodeVarInt(
                    remainingBytes.ToArray()
                );

                MinecraftServerPacket returner = new(
                    packetId,
                    new ArraySegment<byte>(
                        remainingBytes.ToArray(),
                        numPacketIDBytes,
                        remainingBytes.ToArray().Length - numPacketIDBytes
                    ).ToArray()
                );
                return returner;
            }
        }
    }
}
