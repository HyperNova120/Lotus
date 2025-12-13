using System.Security.Cryptography;
using LotusCore.BaseClasses;
using LotusCore.Modules.GameStateHandlerModule.Models;

namespace LotusCore.Modules.Chat.Types;

public class ServerChatSession
{
    public Guid _remoteHostID;

    public MinecraftUUID _userUUID;

    public string _username;

    public MinecraftUUID _sessionUUID = MinecraftUUID.CreateVersion4();

    public Queue<RollingWindowEntry> _rollingWindow = new(); //head = oldest, end = newest

    public int _currentSentMessageIndex = 0;

    public int _numberMessagesSeenSinceLastSentMessage = 0;

    public MojangKeyPair _mojangKeyPair;

    public RSA _rsa;
}

public struct RollingWindowEntry
{
    public bool _ack;

    public bool _fromThisUser;
    public byte[] _sig;

    public RollingWindowEntry(bool ack, bool fromThisUser, byte[] sig)
    {
        _ack = ack;
        _sig = sig;
        _fromThisUser = fromThisUser;
    }

    public void SetAckFalse()
    {
        _ack = false;
    }

    public int checksum()
    {
        int result = 1;
        foreach (byte b in _sig)
        {
            result = 31 * result + b;
        }
        return result;
    }
}
