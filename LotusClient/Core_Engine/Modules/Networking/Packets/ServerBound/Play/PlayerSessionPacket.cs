using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ServerBound.Play;

public class PlayerSessionPacket : MinecraftPacket
{
    public PlayerSessionPacket()
    {
        _Protocol_ID = 0x09;
    }

    public MinecraftUUID _UUID;

    public long _ExpiresAt;

    public byte[] _PublicKey;

    public byte[] _Signature;

    public override byte[] GetBytes()
    {
        return
        [
            .. _UUID.GetBytes(),
            .. NetworkLong.GetBytes(_ExpiresAt),
            .. PrefixedArray.GetBytes(_PublicKey),
            .. PrefixedArray.GetBytes(_Signature),
        ];
    }
}
