using System.Diagnostics.Eventing.Reader;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Chat.Types;
using LotusCore.Modules.GameStateHandlerModule.Types;
using LotusCore.Modules.MojangLogin.Models;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Packets.ServerBound.Play;
using LotusCore.Modules.Networking.Packets.ServerBound.Play.Chat;
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

public class ServerChat : IModuleBase
{
    Dictionary<Guid, ServerChatSession> _serverChatSessions = new();

    public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

    public void RegisterEvents(Action<string> RegisterEvent)
    {
        RegisterEvent.Invoke("CHAT_StartChatSession");
        RegisterEvent.Invoke("CHAT_DecodePlayerChatMessagePacket");
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
        SubscribeToEvent.Invoke(
            "CHAT_DecodePlayerChatMessagePacket",
            new EngineEventHandler(
                (sender, args) =>
                {
                    ReceivePlayerChatMessagePacket((PacketReceivedEventArgs)args!);
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
        await Task.Delay(5000);
        msg = GenerateSignedChatMessage(remoteHostID, "Hello World 2!")!;
        Core_Engine.InvokeEvent("NETWORKING_SendPacket", new SendPacketArgs(remoteHostID, msg));
    }

    private ServerChatSession CreateServerChatSession(Guid remoteHostID)
    {
        var minecraftProfile = Core_Engine
            .InvokeEvent<MinecraftProfileResult>("GAMESTATE_GetUserProfile")!
            ._minecraftProfile!;
        ServerChatSession returner = new()
        {
            _userUUID = new MinecraftUUID(minecraftProfile.id),
            _sessionUUID = MinecraftUUID.CreateVersion4(),
            _mojangKeyPair = Core_Engine
                .InvokeEvent<MojangKeyPairResult>("GAMESTATE_GetMojangKeyPair")!
                ._mojangKeyPair!,
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
            session._userUUID,
            session._sessionUUID,
            session._currentSentMessageIndex,
            chatMessage._salt,
            chatMessage._timestamp / 1000,
            msg,
            session._previousMessageSignatures.Count,
            session._previousMessageSignatures,
            session._rsa
        );

        ++session._currentSentMessageIndex;
        session._numberMessagesSeenSinceLastSentMessage = 0;

        return chatMessage;
    }

    private byte[] GenerateChatMessageSignature(
        MinecraftUUID userUUID,
        MinecraftUUID sessionUUID,
        int messageIndex,
        long salt,
        long timestamp, //as seconds since unix epoch
        string msg,
        int previousMessageSignaturesCount,
        IEnumerable<byte[]> previousMessageSignatures,
        RSA rsa
    )
    {
        //timestamp = timestamp / 1000; // convert to seconds
        byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
        List<byte> sigBytes =
        [
            .. BitConverter.GetBytes(1).Reverse(),
            .. userUUID.GetBytes(),
            .. sessionUUID.GetBytes(),
            .. BitConverter.GetBytes(messageIndex).Reverse(),
            .. BitConverter.GetBytes(salt).Reverse(),
            .. BitConverter.GetBytes(timestamp).Reverse(),
            .. BitConverter.GetBytes(msgBytes.Length).Reverse(),
            .. msgBytes,
            .. BitConverter.GetBytes(previousMessageSignaturesCount).Reverse(),
        ];
        foreach (byte[] previousMessageSignature in previousMessageSignatures)
        {
            if (previousMessageSignature.Length != 256)
            {
                throw new Exception("PANIC: previousMessageSignature.Length NOT 256");
            }
            sigBytes.AddRange(previousMessageSignature);
        }

        byte[] hash = SHA256.HashData(sigBytes.ToArray());

        return rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    public void ReceivePlayerChatMessagePacket(PacketReceivedEventArgs args)
    {
        PlayerChatMessage playerChatMessage = new();
        int offset = 0;
        //Header
        DecodePlayerChatMessageHeader(playerChatMessage, args._packet, ref offset);

        //Body
        DecodePlayerChatMessageBody(playerChatMessage, args._packet, ref offset);

        //Other
        DecodePlayerChatMessageOther(playerChatMessage, args._packet, ref offset);

        //Chat Formatting
        DecodePlayerChatMessageChatFormatting(playerChatMessage, args._packet, ref offset);

        Logging.LogDebug(playerChatMessage._chatFormatting._senderName.ToString());
        Logging.LogInfo(
            $"<{playerChatMessage._chatFormatting._senderName.TryGetTag<TAG_String>("text")!.Value}> {playerChatMessage._body._message}"
        );

        //update session signed messages
        var session = _serverChatSessions[args._remoteHostID];
        if (
            playerChatMessage._header._messageSignatureBytes != null
            && !_serverChatSessions[args._remoteHostID]
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

    private void DecodePlayerChatMessageHeader(
        PlayerChatMessage playerChatMessage,
        MinecraftServerPacket packet,
        ref int offset
    )
    {
        playerChatMessage._header._globalIndex = VarInt_VarLong.DecodeVarInt(
            packet._data,
            ref offset
        );

        MinecraftUUID SenderUUID = new();
        SenderUUID.DecodeBytes(packet._data, ref offset);
        playerChatMessage._header._sender = SenderUUID;

        playerChatMessage._header._index = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);

        bool isPresent = PrefixedOptional.DecodeBytes(packet._data, ref offset);
        if (isPresent)
        {
            playerChatMessage._header._messageSignatureBytes = packet._data[offset..(offset + 256)];
            offset += 256;
        }
    }

    private void DecodePlayerChatMessageBody(
        PlayerChatMessage playerChatMessage,
        MinecraftServerPacket packet,
        ref int offset
    )
    {
        playerChatMessage._body._message = StringN.DecodeBytes(packet._data, ref offset);
        //Logging.LogInfo($"<Unknown User> {Message}");

        playerChatMessage._body._timestamp = NetworkLong.DecodeBytes(packet._data, ref offset);

        playerChatMessage._body._salt = NetworkLong.DecodeBytes(packet._data, ref offset);
        int arraySize = PrefixedArray.GetSizeOfArray(packet._data, ref offset);

        for (int i = 0; i < arraySize; i++)
        {
            int MessageID = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);
            if (MessageID == 0)
            {
                byte[] Sig = packet._data[offset..(offset + 256)];
                offset += 256;
            }
        }
    }

    private void DecodePlayerChatMessageOther(
        PlayerChatMessage playerChatMessage,
        MinecraftServerPacket packet,
        ref int offset
    )
    {
        bool isUnsignedContentPresent = PrefixedOptional.DecodeBytes(packet._data, ref offset);
        NBT UnsignedContent = new();
        if (isUnsignedContentPresent)
        {
            offset += UnsignedContent.ReadFromBytes(packet._data[offset..]);
        }
        playerChatMessage._other._unsignedContent = UnsignedContent;

        playerChatMessage._other._filterType = (ChatFilterType)
            VarInt_VarLong.DecodeVarInt(packet._data, ref offset);
        if (playerChatMessage._other._filterType == ChatFilterType.PARTIALLY_FILTERED)
        {
            playerChatMessage._other._filterTypeBits = NetworkBitset.DecodeBytes(
                packet._data,
                ref offset
            );
        }
    }

    private void DecodePlayerChatMessageChatFormatting(
        PlayerChatMessage playerChatMessage,
        MinecraftServerPacket packet,
        ref int offset
    )
    {
        int IDorChatType = VarInt_VarLong.DecodeVarInt(packet._data, ref offset);
        if (IDorChatType != 0)
        {
            //ID
            Logging.LogDebug($"ITS AN ID: {IDorChatType}");
        }
        else
        {
            Logging.LogDebug($"ITS NOT AN ID");
            playerChatMessage._chatFormatting._chatType = ChatType.DecodeBytes(
                packet._data,
                ref offset
            );
            //TODO use Chat type decoration for both Chat portion and Narration portion.
        }

        playerChatMessage._chatFormatting._senderName = new();

        offset += playerChatMessage._chatFormatting._senderName.ReadFromBytes(
            packet._data[offset..],
            networkBytes: true
        );

        if (PrefixedOptional.DecodeBytes(packet._data, ref offset))
        {
            playerChatMessage._chatFormatting._targetName = new();

            offset += playerChatMessage._chatFormatting._targetName.ReadFromBytes(
                packet._data[offset..],
                networkBytes: true
            );
        }
        else
        {
            playerChatMessage._chatFormatting._targetName = null;
        }
    }
}
