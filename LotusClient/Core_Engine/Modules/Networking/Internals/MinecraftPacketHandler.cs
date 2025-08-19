using System.Net;
using Core_Engine.BaseClasses.Types;
using Core_Engine.Modules.Networking.Packets;

namespace Core_Engine.Modules.Networking.Internals
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
                byte[] packet_id = VarInt_VarLong.EncodeInt(data._Protocol_ID);
                byte[] packet_data = data.GetBytes();
                byte[] packet_length = VarInt_VarLong.EncodeInt(
                    packet_id.Length + packet_data.Length
                );
                byte[] packetBytes = [.. packet_length, .. packet_id, .. packet_data];
                if (_IsEncryptionEnabled)
                {
                    packetBytes = connection._Encryption.EncryptData(packetBytes);
                }
                return packetBytes;
            }
            else
            {
                byte[] packet_id = VarInt_VarLong.EncodeInt(data._Protocol_ID);
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
                        packetBytes = connection._Encryption.EncryptData(packetBytes);
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

                    if (_IsEncryptionEnabled)
                    {
                        packetBytes = connection._Encryption.EncryptData(packetBytes);
                    }
                    return packetBytes;
                }
            }
        }

        public (MinecraftServerPacket? firstpacket, byte[] remainingBytes) DecodePacket(
            IPAddress remoteHost,
            byte[] bytes
        )
        {
            if (_IsCompressionEnabled)
            {
                //has packet length
                //Logging.LogDebug("Decoding Compressed Packet");

                (int packetLength, int packetLengthnumBytes) = VarInt_VarLong.DecodeVarInt(bytes);

                if (packetLength > (bytes.Length - packetLengthnumBytes))
                {
                    /* Logging.LogError(
                        $"MinecraftPacketHandler CompressionEnabled 1; DecodePacket ERROR: Size Mismatch, PacketLength:{packetLength}, RemainingBytes:{bytes.Length}"
                    ); */
                    return (null, bytes);
                }
                bytes = bytes[packetLengthnumBytes..];
                (int dataLength, int dataLengthNumBytes) = VarInt_VarLong.DecodeVarInt(bytes);
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

                (int packetID, int packetIDNumBytes) = VarInt_VarLong.DecodeVarInt(packetBytes);
                packetBytes = packetBytes[packetIDNumBytes..];
                return (new MinecraftServerPacket(remoteHost, packetID, packetBytes), bytes);
            }
            else
            {
                //only data length
                (int dataLength, int numDataBytes) = VarInt_VarLong.DecodeVarInt(bytes);
                bytes = bytes[numDataBytes..];

                if (dataLength > bytes.Length)
                {
                    Logging.LogError(
                        $"Received Packet Data Length mismatch with actual length; dataLength:{dataLength} bytes.Length:{bytes.Length}"
                    );
                    return (null, []);
                }
                else
                {
                    /* Logging.LogDebug(
                        $"Received Packet Data Length: {dataLength} Remaining Byte Length: {bytes.Length}"
                    ); */
                }

                byte[] packetBytes = bytes[..dataLength];
                bytes = bytes[dataLength..];

                (int packetID, int packetIDNumBytes) = VarInt_VarLong.DecodeVarInt(packetBytes);
                packetBytes = packetBytes[packetIDNumBytes..];

                return (new(remoteHost, packetID, packetBytes), bytes);
            }
        }
    }
}
