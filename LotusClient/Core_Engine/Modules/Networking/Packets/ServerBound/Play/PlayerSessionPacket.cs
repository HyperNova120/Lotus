using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.Modules.ServerPlay;

namespace LotusCore.Modules.LotusNetty.Packets.ServerBound.Play;

public class PlayerSessionPacket : MinecraftPacket
{
    public PlayerSessionPacket()
    {
        _protocol_ID = (int)PlayPacketsServerbound.PLAYER_SESSION;
    }

    public MinecraftUUID? _UUID;

    public long _ExpiresAt;

    public byte[]? _PublicKey;

    public byte[]? _Signature;

    public override byte[] GetBytes()
    {
        return
        [
            .. _UUID!.GetBytes(),
            .. NetworkLong.GetBytes(_ExpiresAt),
            .. PrefixedArray.GetBytes(_PublicKey!),
            .. PrefixedArray.GetBytes(_Signature!),
        ];
    }
}
