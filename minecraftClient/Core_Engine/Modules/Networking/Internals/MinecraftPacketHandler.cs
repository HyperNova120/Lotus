using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.Networking.Types;

namespace Core_Engine.Modules.Networking.Internals
{
    public static class MinecraftPacketHandler
    {
        public static bool IsCompressionEnabled = false;
        public static bool IsEncryptionEnabled = false;
        public static int CompresionThreshold;

        public static void Init()
        {
            IsCompressionEnabled = false;
            IsEncryptionEnabled = false;
            CompresionThreshold = default;
        }

        public static byte[] CreatePacket(MinecraftPacket data)
        {
            if (!IsCompressionEnabled || CompresionThreshold < 0)
            {
                byte[] packet_id = VarInt_VarLong.EncodeInt(data.protocol_id);
                byte[] packet_data = data.GetBytes();
                byte[] packet_length = VarInt_VarLong.EncodeInt(
                    packet_id.Length + packet_data.Length
                );
                byte[] packetBytes = [.. packet_length, .. packet_id, .. packet_data];
                if (IsEncryptionEnabled)
                {
                    packetBytes = Core_Engine
                        .GetModule<Networking>("Networking")!
                        .encryption.EncryptData(packetBytes);
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
                        packetBytes = Core_Engine
                            .GetModule<Networking>("Networking")!
                            .encryption.EncryptData(packetBytes);
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
                        packetBytes = Core_Engine
                            .GetModule<Networking>("Networking")!
                            .encryption.EncryptData(packetBytes);
                    }
                    return packetBytes;
                }
            }
        }

        public static (MinecraftServerPacket? firstpacket, byte[] remainingBytes) DecodePacket(
            byte[] bytes
        )
        {
            if (IsCompressionEnabled)
            {
                //has packet length
                Logging.LogDebug("Decoding Compressed Packet");

                (int compressed_packet_length, int packet_length_numBytesRead) =
                    VarInt_VarLong.DecodeVarInt(bytes);

                if (compressed_packet_length > bytes.Length - packet_length_numBytesRead)
                {
                    Logging.LogError(
                        $"Received Packet Data Length mismatch with actual length; packet_length:{compressed_packet_length} remainingBytes.Length:{bytes.Length - packet_length_numBytesRead}"
                    );
                    return (null, []);
                }

                byte[] packetData = new ArraySegment<byte>(
                    bytes,
                    packet_length_numBytesRead,
                    compressed_packet_length
                ).ToArray();

                bytes = bytes.Skip(packet_length_numBytesRead + compressed_packet_length).ToArray();

                (int uncompressed_data_length, int data_length_numBytesRead) =
                    VarInt_VarLong.DecodeVarInt(packetData);
                packetData = packetData.Skip(data_length_numBytesRead).ToArray();

                if (uncompressed_data_length != 0)
                {
                    packetData = ZlibCompressionHandler.Decompress(packetData);
                    if (packetData.Length != uncompressed_data_length)
                    {
                        Logging.LogError(
                            $"DecodePacket; Compression error, given uncompressed_data_length:{uncompressed_data_length} actual length:{packetData.Length}"
                        );
                        return (null, []);
                    }
                }

                (int packet_id, int packet_id_numBytesRead) = VarInt_VarLong.DecodeVarInt(
                    packetData
                );
                packetData = packetData.Skip(packet_id_numBytesRead).ToArray();
                MinecraftServerPacket returner = new(packet_id, packetData);

                return (returner, bytes);
            }
            else
            {
                //only data length
                (int dataLength, int numDataBytes) = VarInt_VarLong.DecodeVarInt(bytes);
                ArraySegment<byte> packetBytes = new System.ArraySegment<byte>(
                    bytes,
                    numDataBytes,
                    dataLength
                );

                if (dataLength != packetBytes.Count)
                {
                    Logging.LogError(
                        $"Received Packet Data Length mismatch with actual length; dataLength:{dataLength} packetBytes.Count:{packetBytes.Count}"
                    );
                }
                else
                {
                    Logging.LogDebug($"Received Packet Data Length: {dataLength}");
                }

                (int packetId, int numPacketIDBytes) = VarInt_VarLong.DecodeVarInt(
                    packetBytes.ToArray()
                );

                MinecraftServerPacket returner = new(
                    packetId,
                    new ArraySegment<byte>(
                        packetBytes.ToArray(),
                        numPacketIDBytes,
                        packetBytes.ToArray().Length - numPacketIDBytes
                    ).ToArray()
                );
                return (returner, bytes.Skip(packetBytes.Count + numDataBytes).ToArray());
            }
        }
    }
}
