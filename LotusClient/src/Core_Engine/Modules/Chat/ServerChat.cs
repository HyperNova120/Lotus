using System.Security.Cryptography;
using LotusCore.BaseClasses;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Chat.Internals;
using LotusCore.Modules.Chat.Types;
using LotusCore.Modules.GameStateHandlerModule;
using LotusCore.Modules.LotusNetty;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Play;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Play.Chat;
using LotusCore.Utils.NBTInternals.Tags;

namespace LotusCore.Modules.Chat;

public class ServerChat : IServerChatModule
{
    private INetworkModule? _networking;
    private IGameStateHandlerModule? _gamestate;

    private ChatMessageCreator _chatMessageCreator = new();
    private ChatMessageDecoder _chatMessageDecoder = new();

    Dictionary<Guid, ServerChatSession> _serverChatSessions = new();

    public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

    public void RegisterEvents(Action<string> RegisterEvent) { }

    public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent) { }

    public void StartChatSession(Guid remoteHostID)
    {
        ServerChatSession session = CreateServerChatSession(remoteHostID);
        _serverChatSessions[remoteHostID] = session;

        PlayerSessionPacket playerSessionPacket = new()
        {
            _PublicKey = Convert.FromBase64String(
                session
                    ._mojangKeyPair!.keyPair.publicKey.Replace("-----BEGIN RSA PUBLIC KEY-----", "")
                    .Replace("-----END RSA PUBLIC KEY-----", "")
            ),
            _Signature = Convert.FromBase64String(session._mojangKeyPair.publicKeySignatureV2),
            _ExpiresAt = DateTimeOffset
                .Parse(session._mojangKeyPair.expiresAt)
                .ToUnixTimeMilliseconds(),
            _UUID = session._sessionUUID,
        };

        _networking!.SendPacket(remoteHostID, playerSessionPacket);
        _ = SendTestMessages(remoteHostID);
    }

    public void LinkModules(ICoreModule coreModule)
    {
        _networking = coreModule.GetModule<INetworkModule>()!;
        _gamestate = coreModule.GetModule<IGameStateHandlerModule>()!;
    }

    public async Task SendTestMessages(Guid remoteHostID)
    {
        await Task.Delay(1000);
        SendChatMessage(remoteHostID, "Hello World 1!");

        await Task.Delay(5000);
        SendChatMessage(remoteHostID, "Hello World 2!");

        await Task.Delay(10000);
        SendChatMessage(remoteHostID, "Hello World 3!");

        await Task.Delay(15000);
        SendChatMessage(remoteHostID, "Hello World 4!");
    }

    private ServerChatSession CreateServerChatSession(Guid remoteHostID)
    {
        var minecraftProfile = _gamestate!.GetUserProfile();
        ServerChatSession returner = new()
        {
            _userUUID = new MinecraftUUID(minecraftProfile.id),
            _sessionUUID = MinecraftUUID.CreateVersion4(),
            _mojangKeyPair = _gamestate.GetMojangKeyPair(),
            _remoteHostID = remoteHostID,
            _username = minecraftProfile.name,
        };
        string pkcs8 = returner
            ._mojangKeyPair.keyPair.privateKey.Replace("\r\n", "")
            .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
            .Replace("-----END RSA PRIVATE KEY-----", "");
        returner._rsa = RSA.Create();
        returner._rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(pkcs8), out _);

        return returner;
    }

    public void ReceivePlayerChatMessagePacket(MinecraftServerPacket packet, Guid remoteHostID)
    {
        PlayerChatMessage playerChatMessage = new();
        int offset = 0;
        //Header
        _chatMessageDecoder.DecodePlayerChatMessageHeader(playerChatMessage, packet, ref offset);

        //Body
        _chatMessageDecoder.DecodePlayerChatMessageBody(playerChatMessage, packet, ref offset);

        //Other
        _chatMessageDecoder.DecodePlayerChatMessageOther(playerChatMessage, packet, ref offset);

        //Chat Formatting
        _chatMessageDecoder.DecodePlayerChatMessageChatFormatting(
            playerChatMessage,
            packet,
            ref offset
        );

        ProcessChatMessage(playerChatMessage, remoteHostID);
    }

    public void SendChatMessage(Guid remoteHostID, string msgConent)
    {
        ChatMessage? msg = _chatMessageCreator.GenerateSignedChatMessage(
            remoteHostID,
            msgConent,
            _serverChatSessions[remoteHostID]
        );
        if (msg == null)
        {
            return;
        }

        _networking!.SendPacket(remoteHostID, msg);
    }

    public void SendChatCommand(Guid remoteHostID, string msg)
    {
        throw new NotImplementedException();
    }

    private void ProcessChatMessage(PlayerChatMessage playerChatMessage, Guid remoteHostID)
    {
        Logging.LogDebug(playerChatMessage._chatFormatting._senderName!.ToString());
        Logging.LogInfo(
            $"<{playerChatMessage._chatFormatting._senderName.TryGetTag<TAG_String>("text")!.Value}> {playerChatMessage._body._message}"
        );

        ServerChatSession session = _serverChatSessions[remoteHostID];

        AddToCachedMessages(playerChatMessage, session);

        //update session signed messages
        if (playerChatMessage._header._messageSignatureBytes != null)
        {
            AddToRollingWindow(playerChatMessage, remoteHostID, session);
        }
    }

    private void AddToCachedMessages(PlayerChatMessage playerChatMessage, ServerChatSession session)
    {
        session._previousMessages.Enqueue(playerChatMessage);
        if (session._previousMessages.Count > ServerChatSession.MAX_STORED_MESSAGES)
        {
            session._previousMessages.Dequeue();
        }
    }

    private void AddToRollingWindow(
        PlayerChatMessage playerChatMessage,
        Guid remoteHostID,
        ServerChatSession session
    )
    {
        bool isEcho = playerChatMessage._header._sender!.Equals(session._userUUID);
        //Logging.LogDebug("Add msg to Rolling Window");
        session._rollingWindow.Enqueue(
            new RollingWindowEntry(true, isEcho, playerChatMessage._header._messageSignatureBytes!)
        );

        ++session._numberMessagesSeenSinceLastSentMessage;
        if (session._rollingWindow.Count > ServerChatSession.MAX_ROLLING_WINDOW_SIZE)
        {
            //keep rolling window at max size
            session._rollingWindow.Dequeue();
        }

        if (
            session._numberMessagesSeenSinceLastSentMessage
            > ServerChatSession.MAX_UNACKED_ROLLING_WINDOW_SIZE
        )
        {
            //send ack to clear excess server cache
            int numToAck =
                session._numberMessagesSeenSinceLastSentMessage
                - ServerChatSession.MAX_ROLLING_WINDOW_SIZE;
            AcknowledgeMessagePacket acknowledgeMessagePacket = new(numToAck);
            _networking!.SendPacket(remoteHostID, acknowledgeMessagePacket);
            session._numberMessagesSeenSinceLastSentMessage -= numToAck;
        }
    }
}
