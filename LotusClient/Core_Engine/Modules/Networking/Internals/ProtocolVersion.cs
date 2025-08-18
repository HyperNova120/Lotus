namespace Core_Engine.Modules.Networking.Internals;

public static class ProtocolVersionUtils
{
    public enum ProtocolVersion
    {
        V1_21_8 = 772,
        V1_21_7 = 772,
        V1_21_6 = 771,
        V1_21_5 = 770,
    }

    public static string GetProtocolVersionName(ProtocolVersion version)
    {
        switch (version)
        {
            case ProtocolVersion.V1_21_8:
                return "1.21.8";
            case ProtocolVersion.V1_21_6:
                return "1.21.6";
            case ProtocolVersion.V1_21_5:
                return "1.21.5";
            default:
                return "";
        }
    }
}
