using System.Diagnostics.Eventing.Reader;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Chat.Types;
using LotusCore.Modules.GameStateHandlerModule.Types;
using LotusCore.Modules.MojangLogin.Models;
using LotusCore.Modules.Networking.Packets.ServerBound.Play;
using LotusCore.Utils;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace LotusCore.Modules.Chat;

public class ServerChat : IModuleBase
{
    Dictionary<Guid, ServerChatSession> _serverChatSessions = new();

    public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

    public void RegisterEvents(Action<string> RegisterEvent)
    {
        RegisterEvent.Invoke("CHAT_StartChatSession");
    }

    public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
    {
        SubscribeToEvent.Invoke(
            "CHAT_StartChatSession",
            new EngineEventHandler(
                (sender, args) =>
                {
                    StartServerChatSessionAsync(((GuidEngineArgs)args!)._value);
                    return null;
                }
            )
        );
    }

    public async Task StartServerChatSessionAsync(Guid remoteHostID)
    {
        ServerChatSession session = CreateServerChatSession(remoteHostID);
        _serverChatSessions[remoteHostID] = session;

        PlayerSessionPacket playerSessionPacket = new()
        {
            _PublicKey = Convert.FromBase64String(
                session
                    ._mojangKeyPair.keyPair.publicKey.Replace("-----BEGIN RSA PUBLIC KEY-----", "")
                    .Replace("-----END RSA PUBLIC KEY-----", "")
            ),
            _Signature = Convert.FromBase64String(session._mojangKeyPair.publicKeySignatureV2),
            _ExpiresAt = DateTimeOffset
                .Parse(session._mojangKeyPair.expiresAt)
                .ToUnixTimeMilliseconds(),
            _UUID = session._sessionUUID,
        };

        Core_Engine.InvokeEvent(
            "NETWORKING_SendPacket",
            new SendPacketArgs(remoteHostID, playerSessionPacket)
        );

        await Task.Delay(1000);
        ChatMessage msg = GenerateSignedChatMessage(remoteHostID, "Hello World!")!;
        Core_Engine.InvokeEvent("NETWORKING_SendPacket", new SendPacketArgs(remoteHostID, msg));
    }

    private ServerChatSession CreateServerChatSession(Guid remoteHostID)
    {
        ServerChatSession returner = new()
        {
            _userUUID = new MinecraftUUID(
                Core_Engine
                    .InvokeEvent<MinecraftProfileResult>("GAMESTATE_GetUserProfile")!
                    ._minecraftProfile!.id
            ),
            _sessionUUID = MinecraftUUID.CreateVersion4(),
            _mojangKeyPair = Core_Engine
                .InvokeEvent<MojangKeyPairResult>("GAMESTATE_GetMojangKeyPair")!
                ._mojangKeyPair!,
            _remoteHostID = remoteHostID,
        };
        string pkcs8 = returner
            ._mojangKeyPair.keyPair.privateKey.Replace("\r\n", "")
            .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
            .Replace("-----END RSA PRIVATE KEY-----", "");
        returner._rsa = RSA.Create();
        returner._rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(pkcs8), out _);

        return returner;
    }

    public ChatMessage? GenerateSignedChatMessage(Guid remoteHostID, string msg)
    {
        if (msg.Length > 256)
        {
            Logging.LogError($"GenerateChatMessage: Message too Long! msg.Length:{msg.Length}");
            return null; // msg too long
        }
        var session = _serverChatSessions[remoteHostID];
        var rng = RandomNumberGenerator.Create();
        byte[] saltBytes = new byte[8];
        rng.GetBytes(saltBytes);
        FixedBitSet acknowledgedFixedBitSet = new(20);
        for (int i = 0; i < session._numberMessagesSeenSinceLastSentMessage; i++)
        {
            acknowledgedFixedBitSet[i] = true;
        }

        ChatMessage chatMessage = new()
        {
            _message = msg,
            _timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            _salt = BitConverter.ToInt64(saltBytes),
            _messageCount = session._numberMessagesSeenSinceLastSentMessage,
            _acknowledged = acknowledgedFixedBitSet,
        };

        chatMessage._signature = GenerateChatMessageSignature(
            remoteHostID,
            msg,
            chatMessage._salt,
            chatMessage._timestamp
        );

        ++session._currentSentMessageIndex;

        return chatMessage;
    }

    private byte[] GenerateChatMessageSignature(
        Guid remoteHostID,
        string msg,
        long salt,
        long timestamp
    )
    {
        timestamp = timestamp / 1000; // convert to seconds

        ServerChatSession session = _serverChatSessions[remoteHostID];
        byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
        List<byte> sigBytes =
        [
            .. BitConverter.GetBytes(1).Reverse(),
            .. session._userUUID.GetBytes(),
            .. session._sessionUUID.GetBytes(),
            .. BitConverter.GetBytes(session._currentSentMessageIndex).Reverse(),
            .. BitConverter.GetBytes(salt).Reverse(),
            .. BitConverter.GetBytes(timestamp).Reverse(),
            .. BitConverter.GetBytes(msgBytes.Length).Reverse(),
            .. msgBytes,
            .. BitConverter.GetBytes(session._previousMessageSignatures.Count).Reverse(),
        ];
        foreach (byte[] previousMessageSignature in session._previousMessageSignatures)
        {
            sigBytes.AddRange(previousMessageSignature);
        }

        byte[] hash = SHA256.HashData(sigBytes.ToArray());

        return session._rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    public void ReceiveChatMessage(Guid remoteHostID, PlayerChatMessage playerChatMessage)
    {
        var session = _serverChatSessions[remoteHostID];

        session._previousMessageSignatures.Append(playerChatMessage._header._messageSignatureBytes);
        if (session._previousMessageSignatures.Count > 20)
        {
            session._previousMessageSignatures.Dequeue();
        }
    }
}
