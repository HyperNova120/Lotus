using System.Numerics;
using Microsoft.Identity.Client;

namespace LotusCore.BaseClasses.Types
{
    public static class VarInt_VarLong
    {
        private static readonly int SEGMENT_BITS = 0x7F;
        private static readonly int CONTINUE_BIT = 0x80;

        public static byte[] EncodeInt(int value)
        {
            List<byte> bytes = new();
            while (true)
            {
                if ((value & ~SEGMENT_BITS) == 0)
                {
                    bytes.Add((byte)value);
                    break;
                }

                bytes.Add((byte)((value & SEGMENT_BITS) | CONTINUE_BIT));
                value >>>= 7;
            }
            return bytes.ToArray();
        }

        /// <summary>
        /// decodes input as varint
        /// </summary>
        /// <param name="input"></param>
        /// <returns>(value, number of bytes read)</returns>
        /// <exception cref="Exception"></exception>
        public static int DecodeVarInt(byte[] input, ref int offset)
        {
            int value = 0;
            int position = 0;
            while (true)
            {
                value |= (input[offset] & SEGMENT_BITS) << position;
                if ((input[offset] & CONTINUE_BIT) == 0)
                {
                    break;
                }
                position += 7;
                offset++;
                if (position >= 32)
                {
                    throw new Exception("VarInt is too big");
                }
            }
            ++offset;
            return value;
        }

        /// <summary>
        /// decodes input as varLong
        /// </summary>
        /// <param name="input"></param>
        /// <returns>(value, number of bytes read)</returns>
        /// <exception cref="Exception"></exception>
        public static long DecodeVarLong(byte[] input, ref int offset)
        {
            long value = 0;
            int position = 0;
            while (true)
            {
                value |= (long)(input[offset] & SEGMENT_BITS) << position;
                if ((input[offset] & CONTINUE_BIT) == 0)
                {
                    break;
                }
                position += 7;
                offset++;
                if (position >= 64)
                {
                    throw new Exception("VarLong is too big");
                }
            }
            ++offset;
            return value;
        }

        public static byte[] EncodeLong(long value)
        {
            List<byte> bytes = new();
            while (true)
            {
                if ((value & ~((long)SEGMENT_BITS)) == 0)
                {
                    bytes.Add((byte)value);
                    break;
                }

                bytes.Add((byte)((value & (long)SEGMENT_BITS) | (long)CONTINUE_BIT));
                value >>>= 7;
            }
            return bytes.ToArray();
        }
    }
}
