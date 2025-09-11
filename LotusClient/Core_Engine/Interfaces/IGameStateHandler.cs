using LotusCore.BaseClasses;
using LotusCore.Modules.GameStateHandler.BaseClasses;

namespace LotusCore.Interfaces
{
    public interface IGameStateHandler
    {
        public enum DisplayedSkinPartsFlags
        {
            CAPE_ENABLED = 0x01,
            JACKET_ENABLED = 0x02,
            LEFT_SLEEVE_ENABLED = 0x04,
            RIGHT_SLEEVE_ENABLED = 0x08,
            LEFT_PANTS_LEG_ENABLED = 0x10,
            RIGHT_PANTS_LEG_ENABLED = 0x20,
            HAT_ENABLED = 0x40,
        }

        public enum MainHand
        {
            LEFT = 0,
            RIGHT = 1,
        }

        public struct VideoSettings
        {
            public enum GraphicsSettings
            {
                FAST,
                FANCY,
                FABULOUS,
            }

            public enum CloudSettings
            {
                OFF,
                FAST,
                FANCY,
            }

            public enum ParticleStatus
            {
                ALL = 0,
                DECREASED = 1,
                MINIMAL = 2,
            }

            public byte _BiomeBlend = 5;

            public GraphicsSettings _Graphics = GraphicsSettings.FABULOUS;

            public byte _RenderDistance = 24;

            public byte _SimulationDistance = 12;

            public bool _SmoothLighting = true;

            public bool _VSync = true;

            public byte _GUIScale = 3;

            public CloudSettings _Clouds = CloudSettings.FANCY;

            public byte _Brightness = 100;

            public ParticleStatus _ParticleStatus = ParticleStatus.ALL;

            public byte _MipmapLevels = 4;

            public bool _EntityShadows = true;

            public bool _ViewBobbing = false;

            public VideoSettings() { }
        }

        public struct SkinCustomization
        {
            public byte _DisplayedSkinParts { private set; get; } =
                (byte)DisplayedSkinPartsFlags.CAPE_ENABLED
                | (byte)DisplayedSkinPartsFlags.JACKET_ENABLED
                | (byte)DisplayedSkinPartsFlags.LEFT_PANTS_LEG_ENABLED
                | (byte)DisplayedSkinPartsFlags.RIGHT_PANTS_LEG_ENABLED
                | (byte)DisplayedSkinPartsFlags.LEFT_SLEEVE_ENABLED
                | (byte)DisplayedSkinPartsFlags.RIGHT_SLEEVE_ENABLED
                | (byte)DisplayedSkinPartsFlags.HAT_ENABLED;

            public bool IsSkinPartDisplayed(DisplayedSkinPartsFlags flag)
            {
                return (_DisplayedSkinParts & (byte)flag) > 0;
            }

            public void SetSkinPartDisplayed(DisplayedSkinPartsFlags flag, bool display)
            {
                if (display)
                {
                    _DisplayedSkinParts |= (byte)flag;
                }
                else
                {
                    _DisplayedSkinParts &= (byte)(~flag);
                }
            }

            public MainHand _MainHand = MainHand.RIGHT;

            public SkinCustomization() { }
        }

        public struct ChatSettings
        {
            public enum ChatMode
            {
                ENABLED = 0,
                COMMANDS_ONLY = 1,
                HIDDEN = 2,
            }

            public ChatMode _ChatShown = ChatMode.ENABLED;

            public bool _Colors = true;

            public bool _WebLinks = true;

            public bool _PromptOnLinks = true;

            public byte _ChatTextOpacity = 100;

            public byte _TextBackgroundOpacity = 50;

            public byte _ChatTextSize = 100;

            public byte _LineSpacing = 0;

            public float _ChatDelay = 0;

            public int _Width = 320;

            public int _FocusedHeight = 180;

            public int _UnfocusedHeight = 90;

            public ChatSettings() { }
        }

        public struct ClientSettings
        {
            public byte _FOV = 70;

            public SkinCustomization _SkinCustomization;

            public VideoSettings _VideoSettings;

            public ChatSettings _ChatSettings;

            public bool _AllowServerListings = true;

            public ClientSettings() { }
        }

        public static ClientSettings _Settings { get; protected set; }

        //==========================================
        //===========CONFIGURATION METHODS==========
        //==========================================
        public ServerCookie? GetServerCookie(Identifier key);
        public void AddServerCookie(ServerCookie cookie);

        public RegistryData? GetServerRegistryData(Identifier ID);
        public void UpdateServerRegistryData(
            RegistryData registryData,
            bool overwrite = true,
            bool replace = false
        );

        public ResourcePack? GetServerResourcePack(MinecraftUUID ID);
        public void AddServerResourcePack(ResourcePack resourcePack);

        public ServerTag? GetServerTag(Identifier Registry, Identifier TagName);
        public void AddServerTag(Identifier Registry, Identifier TagName, List<int> Entries);

        public Identifier? GetServerFeatureFlag(Identifier FeatureFlag);
        public void AddServerFeatureFlag(Identifier FeatureFlag);

        public void ProcessTransfer();

        public void ProcessGameStateReset();

        public void SetLastKeepAliveTime(DateTime lastPacketTime);

        public DateTime GetLastKeepAliveTime();

        //==========================================
        //===============PLAY METHODS===============
        //==========================================
    }
}
