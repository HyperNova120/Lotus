using System.Security.Cryptography;
using System.Text;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.Modules.Chat.Types;

namespace LotusCore.Modules.Chat.Internals;

public class ChatMessageCreator
{
    public ChatMessage? GenerateSignedChatMessage(
        Guid remoteHostID,
        string msg,
        ServerChatSession session
    )
    {
        if (msg.Length > 256)
        {
            Logging.LogError($"GenerateChatMessage: Message too Long! msg.Length:{msg.Length}");
            return null; // msg too long
        }
        var rng = RandomNumberGenerator.Create();
        byte[] saltBytes = new byte[8];
        rng.GetBytes(saltBytes);
        FixedBitSet acknowledgedFixedBitSet = new(20);
        for (int i = 0; i < session._rollingWindow.Count; i++)
        {
            acknowledgedFixedBitSet[19 - i] = session._rollingWindow.ElementAt(i)._ack;
            /* if (acknowledgedFixedBitSet[i])
            {
                session._rollingWindow.ElementAt(i).SetAckFalse();
            } */
        }

        ChatMessage chatMessage = new()
        {
            _message = msg,
            _timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            _salt = BitConverter.ToInt64(saltBytes),
            _messageCount = session._numberMessagesSeenSinceLastSentMessage,
            _acknowledged = acknowledgedFixedBitSet,
            _rollingWindow = session._rollingWindow.ToArray(),
        };

        chatMessage._signature = GenerateChatMessageSignature(
            session._userUUID,
            session._sessionUUID,
            session._currentSentMessageIndex,
            chatMessage._salt,
            chatMessage._timestamp / 1000,
            msg,
            session._rollingWindow,
            session._rsa
        );

        Console.WriteLine(
            $"_rollingWindow.Count:{session._rollingWindow.Count} _numberMessagesSeenSinceLastSentMessage:{session._numberMessagesSeenSinceLastSentMessage}"
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
        IEnumerable<RollingWindowEntry> previousMessageSignatures,
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
            .. BitConverter.GetBytes(previousMessageSignatures.Count()).Reverse(),
        ];
        Console.WriteLine($"previousMessageSignaturesCount:{previousMessageSignatures.Count()}");
        foreach (RollingWindowEntry previousMessageSignature in previousMessageSignatures)
        {
            if (previousMessageSignature._sig.Length != 256)
            {
                throw new Exception("PANIC: previousMessageSignature.Length NOT 256");
            }
            Console.WriteLine("Added previousMessageSignature");
            sigBytes.AddRange(previousMessageSignature._sig);
        }

        byte[] hash = SHA256.HashData(sigBytes.ToArray());

        return rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
