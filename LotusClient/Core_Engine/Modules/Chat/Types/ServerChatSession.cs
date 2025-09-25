using System.Security.Cryptography;
using LotusCore.BaseClasses;
using LotusCore.Modules.GameStateHandlerModule.Models;

namespace LotusCore.Modules.Chat.Types;

public class ServerChatSession
{
    public Guid _remoteHostID;

    public MinecraftUUID _userUUID;

    public MinecraftUUID _sessionUUID = MinecraftUUID.CreateVersion4();

    public Queue<byte[]> _previousMessageSignatures = new(); //head = oldest, end = newest

    public int _currentSentMessageIndex = 0;

    public int _numberMessagesSeenSinceLastSentMessage = 0;

    public MojangKeyPair _mojangKeyPair;

    public RSA? _rsa;
}
