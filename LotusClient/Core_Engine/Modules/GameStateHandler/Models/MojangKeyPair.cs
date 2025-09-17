namespace LotusCore.Modules.GameStateHandlerModule.Models;

public class MojangKeyPair
{
    public KeyPair keyPair { get; set; }

    public string publicKeySignature { get; set; }
    public string publicKeySignatureV2 { get; set; }
    public string expiresAt { get; set; }
    public string refreshedAfter { get; set; }
}

public class KeyPair
{
    public string privateKey { get; set; }
    public string publicKey { get; set; }
}
