using System.Net;
using LotusCore.BaseClasses.Types;
using LotusCore.Modules.Networking.Packets;

namespace LotusCore.Modules.Networking.Internals
{
    public class MinecraftPacketHandler
    {
        public bool _IsCompressionEnabled = false;
        public bool _IsEncryptionEnabled = false;
        public int _CompresionThreshold;

        public void Init()
        {
            _IsCompressionEnabled = false;
            _IsEncryptionEnabled = false;
            _CompresionThreshold = default;
        }

        public byte[] CreatePacket(ServerConnection connection, MinecraftPacket data)
        {
            if (!_IsCompressionEnabled || _CompresionThreshold < 0)
            {
                byte[] packet_id = VarInt_VarLong.EncodeInt(data._protocol_ID);
                byte[] packet_data = data.GetBytes();
                byte[] packet_length = VarInt_VarLong.EncodeInt(
                    packet_id.Length + packet_data.Length
                );
                byte[] packetBytes = [.. packet_length, .. packet_id, .. packet_data];
                if (_IsEncryptionEnabled)
                {
                    packetBytes = connection._encryption.EncryptData(packetBytes);
                }
                return packetBytes;
            }
            else
            {
                byte[] packet_id = VarInt_VarLong.EncodeInt(data._protocol_ID);
                byte[] packet_data = data.GetBytes();
                if (packet_id.Length + packet_data.Length < _CompresionThreshold)
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

                    if (_IsEncryptionEnabled)
                    {
                        packetBytes = connection._encryption.EncryptData(packetBytes);
                    }
                    return packetBytes;
                }
                else
                {
                    //compress
                    Logging.LogDebug($"Compression Required For Packet");
                    byte[] data_length = VarInt_VarLong.EncodeInt(
                        packet_id.Length + packet_data.Length
                    );
                    byte[] compressed_Section = ZlibCompressionHandler.Compress(
                        [.. packet_id, .. packet_data]
                    );
                    byte[] packet_length = VarInt_VarLong.EncodeInt(
                        data_length.Length + compressed_Section.Length
                    );
                    byte[] packetBytes = [.. packet_length, .. data_length, .. compressed_Section];

                    if (_IsEncryptionEnabled)
                    {
                        packetBytes = connection._encryption.EncryptData(packetBytes);
                    }
                    return packetBytes;
                }
            }
        }

        public (MinecraftServerPacket? firstpacket, byte[] remainingBytes) DecodePacket(
            Guid remoteHostID,
            byte[] bytes
        )
        {
            if (_IsCompressionEnabled)
            {
                int packetLengthnumBytes = 0;
                int packetLength = VarInt_VarLong.DecodeVarInt(bytes, ref packetLengthnumBytes);

                if (packetLength > (bytes.Length - packetLengthnumBytes))
                {
                    return (null, bytes);
                }
                bytes = bytes[packetLengthnumBytes..];
                int dataLengthNumBytes = 0;
                int dataLength = VarInt_VarLong.DecodeVarInt(bytes, ref dataLengthNumBytes);
                bytes = bytes[dataLengthNumBytes..];
                int remainingBytesInPacket = packetLength - dataLengthNumBytes;

                byte[] packetBytes = bytes[..remainingBytesInPacket];
                bytes = bytes[remainingBytesInPacket..];

                if (dataLength != 0)
                {
                    //data is compressed
                    packetBytes = ZlibCompressionHandler.Decompress(packetBytes);
                    if (packetBytes.Length != dataLength)
                    {
                        Logging.LogError(
                            $"MinecraftPacketHandler CompressionEnabled 2; DecodePacket ERROR: Size Mismatch, DataLength:{dataLength}, Decompressed Packet Length:{packetBytes.Length}"
                        );
                        return (null, []);
                    }
                }

                int packetIDNumBytes = 0;
                int packetID = VarInt_VarLong.DecodeVarInt(packetBytes, ref packetIDNumBytes);
                packetBytes = packetBytes[packetIDNumBytes..];
                return (new MinecraftServerPacket(remoteHostID, packetID, packetBytes), bytes);
            }
            else
            {
                //only data length
                int totalLength = bytes.Length;
                int numDataBytes = 0;
                int dataLength = VarInt_VarLong.DecodeVarInt(bytes, ref numDataBytes);

                if (dataLength > bytes.Length)
                {
                    return (null, bytes);
                }
                bytes = bytes[numDataBytes..];

                byte[] packetBytes = bytes[..dataLength];
                bytes = bytes[dataLength..];

                int packetIDNumBytes = 0;
                int packetID = VarInt_VarLong.DecodeVarInt(packetBytes, ref packetIDNumBytes);
                packetBytes = packetBytes[packetIDNumBytes..];

                return (new(remoteHostID, packetID, packetBytes), bytes);
            }
        }
    }
}
