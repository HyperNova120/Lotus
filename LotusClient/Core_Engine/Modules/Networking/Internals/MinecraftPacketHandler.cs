using System.Net;
using Core_Engine.Modules.Networking.Packets;
using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Internals
{
    public class MinecraftPacketHandler
    {
        public bool IsCompressionEnabled = false;
        public bool IsEncryptionEnabled = false;
        public int CompresionThreshold;

        public void Init()
        {
            IsCompressionEnabled = false;
            IsEncryptionEnabled = false;
            CompresionThreshold = default;
        }

        public byte[] CreatePacket(ServerConnection connection, MinecraftPacket data)
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
                    packetBytes = connection.encryption.EncryptData(packetBytes);
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
                        packetBytes = connection.encryption.EncryptData(packetBytes);
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
                        packetBytes = connection.encryption.EncryptData(packetBytes);
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
            if (IsCompressionEnabled)
            {
                //has packet length
                //Logging.LogDebug("Decoding Compressed Packet");

                (int packetLength, int packetLengthnumBytes) = VarInt_VarLong.DecodeVarInt(bytes);
                bytes = bytes[packetLengthnumBytes..];

                if (packetLength > bytes.Length)
                {
                    Logging.LogError(
                        $"MinecraftPacketHandler; DecodePacket ERROR: Size Mismatch, PacketLength:{packetLength}, RemainingBytes:{bytes.Length}"
                    );
                    return (null, []);
                }
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
                            $"MinecraftPacketHandler; DecodePacket ERROR: Size Mismatch, DataLength:{dataLength}, Decompressed Packet Length:{packetBytes.Length}"
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
