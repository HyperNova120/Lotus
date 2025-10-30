using System.Diagnostics.Eventing.Reader;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Chat.Internals;
using LotusCore.Modules.Chat.Types;
using LotusCore.Modules.GameStateHandlerModule.Types;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Play;
using LotusCore.Modules.LotusNetty.Packets.ServerBound.Play.Chat;
using LotusCore.Modules.MojangLogin.Models;
using LotusCore.Utils;
using LotusCore.Utils.NBTInternals.Tags;
using Microsoft.Identity.Client.NativeInterop;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace LotusCore.Modules.Chat;

public class ServerChat : IServerChatModule
{
    private INetworkModule _networking;
    private IGameStateHandlerModule _gamestate;

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
                    ._mojangKeyPair.keyPair.publicKey.Replace("-----BEGIN RSA PUBLIC KEY-----", "")
                    .Replace("-----END RSA PUBLIC KEY-----", "")
            ),
            _Signature = Convert.FromBase64String(session._mojangKeyPair.publicKeySignatureV2),
            _ExpiresAt = DateTimeOffset
                .Parse(session._mojangKeyPair.expiresAt)
                .ToUnixTimeMilliseconds(),
            _UUID = session._sessionUUID,
        };

        _networking.SendPacket(remoteHostID, playerSessionPacket);
        _ = SendTestMessages(remoteHostID);
    }

    public void LinkModules()
    {
        _networking = Core_Engine.GetModule<INetworkModule>("Networking")!;
        _gamestate = Core_Engine.GetModule<IGameStateHandlerModule>("GameStateHandler")!;
    }

    public async Task SendTestMessages(Guid remoteHostID)
    {
        await Task.Delay(1000);
        SendChatMessage(remoteHostID, "Hello World 1!");

        await Task.Delay(5000);
        SendChatMessage(remoteHostID, "Hello World 2!");

        await Task.Delay(10000);
        SendChatMessage(remoteHostID, "Hello World 3!");
    }

    private ServerChatSession CreateServerChatSession(Guid remoteHostID)
    {
        var minecraftProfile = _gamestate.GetUserProfile();
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

        Logging.LogDebug(playerChatMessage._chatFormatting._senderName.ToString());
        Logging.LogInfo(
            $"<{playerChatMessage._chatFormatting._senderName.TryGetTag<TAG_String>("text")!.Value}> {playerChatMessage._body._message}"
        );

        //update session signed messages
        var session = _serverChatSessions[remoteHostID];
        if (
            playerChatMessage._header._messageSignatureBytes != null
            && !_serverChatSessions[remoteHostID]
                ._userUUID.Equals(playerChatMessage._header._sender)
        )
        {
            ++session._numberMessagesSeenSinceLastSentMessage;
            session._previousMessageSignatures.Enqueue(
                playerChatMessage._header._messageSignatureBytes
            );
            if (session._previousMessageSignatures.Count > 20)
            {
                session._previousMessageSignatures.Dequeue();
                /* //send ack
                AcknowledgeMessagePacket acknowledgeMessagePacket = new(1);
                Core_Engine.InvokeEvent(
                    "NETWORKING_SendPacket",
                    new SendPacketArgs(args._remoteHostID, acknowledgeMessagePacket)
                ); */
            }
        }
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

        _networking.SendPacket(remoteHostID, msg);
    }

    public void SendChatCommand(Guid remoteHostID, string msg)
    {
        throw new NotImplementedException();
    }
}
