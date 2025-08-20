using System.Globalization;
using System.Text;
using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;
using Core_Engine.Utils;

namespace Core_Engine.Modules.GameStateHandler.BaseClasses
{
    public class ResourcePack
    {
        public MinecraftUUID _UUID;
        public string _URL;
        public string _Hash;
        public bool _Forced;
        public NBT? _TextComponent = null;

        public int DecodeBytes(byte[] inputBytes)
        {
            int offset = _UUID.DecodeBytes(inputBytes);
            (_URL, int numBytes) = StringN.DecodeBytes(inputBytes[offset..]);
            offset += numBytes;
            (_Hash, int numBytes2) = StringN.DecodeBytes(inputBytes[offset..]);
            offset += numBytes2;
            _Forced = inputBytes[offset++] == 1;
            (bool isPresent, int numberBytesRead) = PrefixedOptional.DecodeBytes(
                inputBytes[offset..]
            );
            offset += numberBytesRead;
            if (isPresent)
            {
                _TextComponent = new(true);
                offset += _TextComponent.ReadFromBytes(inputBytes[offset..], true);
            }
            return offset;
        }
    }
}
