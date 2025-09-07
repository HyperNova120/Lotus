namespace LotusCore.Modules.Networking.Internals;

public static class ProtocolVersionUtils
{
    public enum ProtocolVersion
    {
        V1_21_8 = 772,
        V1_21_7 = 772,
        V1_21_6 = 771,
        V1_21_5 = 770,
        V1_21_4 = 769,
        V1_21_3 = 768,
        V1_21_2 = 768,
        V1_21_1 = 767,
        V1_21 = 767,
        V1_20_5 = 766,
        V1_20_4 = 765,
        V1_20_3 = 765,
        V1_20_2 = 764,
        V1_20_1 = 763,
        V1_20 = 763,
        V1_19_4 = 762,
        V1_19_3 = 761,
        V1_19_2 = 760,
        V1_19_1 = 760,
        V1_19 = 759,
        V1_18_2 = 758,
        V1_18_1 = 757,
        V1_18 = 757,
        V1_17_1 = 756,
        V1_17 = 755,
        V1_16_5 = 754,
        V1_16_4 = 754,
        V1_16_3 = 753,
        V1_16_2 = 751,
        V1_16_1 = 736,
        V1_16 = 735,
        V1_15_2 = 578,
        V1_15_1 = 575,
        V1_15 = 573,
        V1_14_4 = 498,
        V1_14_3 = 490,
        V1_14_2 = 485,
        V1_14_1 = 480,
        V1_14 = 477,
        V1_13_2 = 404,
        V1_13_1 = 401,
        V1_13 = 393,
        V1_12_2 = 340,
        V1_12_1 = 338,
        V1_12 = 335,
        V1_11_2 = 316,
        V1_11 = 315,
        V1_10_2 = 210,
        V1_10 = 210,
        V1_9_4 = 110,
        V1_9_2 = 109,
        V1_9 = 107,
        V1_8_9 = 47,
        V1_8 = 47,
        V1_7_10 = 5,
        V1_7_2 = 4,
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
            case ProtocolVersion.V1_21_4:
                return "1.21.4";
            case ProtocolVersion.V1_21_3:
                return "1.21.3";
            case ProtocolVersion.V1_21_1:
                return "1.21.1";
            case ProtocolVersion.V1_20_5:
                return "1.20.5";
            case ProtocolVersion.V1_20_4:
                return "1.20.4";
            case ProtocolVersion.V1_20_2:
                return "1.20.2";
            case ProtocolVersion.V1_20_1:
                return "1.20.1";
            case ProtocolVersion.V1_19_4:
                return "1.19.4";
            case ProtocolVersion.V1_19_3:
                return "1.19.3";
            case ProtocolVersion.V1_19_2:
                return "1.19.2";
            case ProtocolVersion.V1_19:
                return "1.19";
            case ProtocolVersion.V1_18_2:
                return "1.18.2";
            case ProtocolVersion.V1_18_1:
                return "1.18.1";
            case ProtocolVersion.V1_17_1:
                return "1.17.1";
            case ProtocolVersion.V1_17:
                return "1.17";
            case ProtocolVersion.V1_16_5:
                return "1.16.5";
            case ProtocolVersion.V1_16_3:
                return "1.16.3";
            case ProtocolVersion.V1_16_2:
                return "1.16.2";
            case ProtocolVersion.V1_16_1:
                return "1.16.1";
            case ProtocolVersion.V1_16:
                return "1.16";
            case ProtocolVersion.V1_15_2:
                return "1.15.2";
            case ProtocolVersion.V1_15_1:
                return "1.15.1";
            case ProtocolVersion.V1_15:
                return "1.15";
            case ProtocolVersion.V1_14_4:
                return "1.14.4";
            case ProtocolVersion.V1_14_3:
                return "1.14.3";
            case ProtocolVersion.V1_14_2:
                return "1.14.2";
            case ProtocolVersion.V1_14_1:
                return "1.14.1";
            case ProtocolVersion.V1_14:
                return "1.14";
            case ProtocolVersion.V1_13_2:
                return "1.13.2";
            case ProtocolVersion.V1_13_1:
                return "1.13.1";
            case ProtocolVersion.V1_13:
                return "1.13";
            case ProtocolVersion.V1_12_2:
                return "1.12.2";
            case ProtocolVersion.V1_12_1:
                return "1.12.1";
            case ProtocolVersion.V1_12:
                return "1.12";
            case ProtocolVersion.V1_11_2:
                return "1.11.2";
            case ProtocolVersion.V1_11:
                return "1.11";
            case ProtocolVersion.V1_10_2:
                return "1.10.2";
            case ProtocolVersion.V1_9_4:
                return "1.9.4";
            case ProtocolVersion.V1_9_2:
                return "1.9.2";
            case ProtocolVersion.V1_9:
                return "1.9";
            case ProtocolVersion.V1_8_9:
                return "1.8.9";
            case ProtocolVersion.V1_7_10:
                return "1.7.10";
            case ProtocolVersion.V1_7_2:
                return "1.7.2";
            default:
                return "Unknown";
        }
    }
}
