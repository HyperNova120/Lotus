using System.Globalization;
using System.Text;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.Utils;

namespace LotusCore.Modules.GameStateHandler.BaseClasses
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
            int offset = 0;
            _UUID.DecodeBytes(inputBytes, ref offset);
            _URL = StringN.DecodeBytes(inputBytes, ref offset);
            _Hash = StringN.DecodeBytes(inputBytes, ref offset);
            _Forced = inputBytes[offset++] == 1;
            bool isPresent = PrefixedOptional.DecodeBytes(inputBytes, ref offset);
            if (isPresent)
            {
                _TextComponent = new(true);
                offset += _TextComponent.ReadFromBytes(inputBytes[offset..], true);
            }
            return offset;
        }
    }
}
