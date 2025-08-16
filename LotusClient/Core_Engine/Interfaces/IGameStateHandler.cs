using Core_Engine.BaseClasses;
using Core_Engine.Modules.GameStateHandler.BaseClasses;

namespace Core_Engine.Interfaces
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

            public byte biomeBlend = 5;

            public GraphicsSettings graphics = GraphicsSettings.FABULOUS;

            public byte renderDistance = 24;

            public byte simulationDistance = 12;

            public bool smoothLighting = true;

            public bool VSync = true;

            public byte GUIScale = 3;

            public CloudSettings clouds = CloudSettings.FANCY;

            public byte brightness = 100;

            public ParticleStatus particleStatus = ParticleStatus.ALL;

            public byte mipmapLevels = 4;

            public bool entityShadows = true;

            public bool viewBobbing = false;

            public VideoSettings() { }
        }

        public struct SkinCustomization
        {
            private byte displayedSkinParts =
                (byte)DisplayedSkinPartsFlags.CAPE_ENABLED
                | (byte)DisplayedSkinPartsFlags.JACKET_ENABLED
                | (byte)DisplayedSkinPartsFlags.LEFT_PANTS_LEG_ENABLED
                | (byte)DisplayedSkinPartsFlags.RIGHT_PANTS_LEG_ENABLED
                | (byte)DisplayedSkinPartsFlags.LEFT_SLEEVE_ENABLED
                | (byte)DisplayedSkinPartsFlags.RIGHT_SLEEVE_ENABLED
                | (byte)DisplayedSkinPartsFlags.HAT_ENABLED;

            public bool IsSkinPartDisplayed(DisplayedSkinPartsFlags flag)
            {
                return (displayedSkinParts & (byte)flag) > 0;
            }

            public void SetSkinPartDisplayed(DisplayedSkinPartsFlags flag, bool display)
            {
                if (display)
                {
                    displayedSkinParts |= (byte)flag;
                }
                else
                {
                    displayedSkinParts &= (byte)(~flag);
                }
            }

            public MainHand mainHand = MainHand.RIGHT;

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

            public ChatMode chatShown = ChatMode.ENABLED;

            public bool colors = true;

            public bool webLinks = true;

            public bool promptOnLinks = true;

            public byte chatTextOpacity = 100;

            public byte textBackgroundOpacity = 50;

            public byte chatTextSize = 100;

            public byte lineSpacing = 0;

            public float chatDelay = 0;

            public int width = 320;

            public int focusedHeight = 180;

            public int unfocusedHeight = 90;

            public ChatSettings() { }
        }

        public struct ClientSettings
        {
            public byte FOV = 70;

            public SkinCustomization skinCustomization;

            public VideoSettings videoSettings;

            public ChatSettings chatSettings;

            public bool allowServerListings = true;

            public ClientSettings() { }
        }

        public static ClientSettings settings;

        //==========================================
        //===========CONFIGURATION METHODS==========
        //==========================================
        public ServerCookie? GetServerCookie(Identifier key);
        public void AddServerCookie(ServerCookie cookie);

        public RegistryData? GetServerRegistryData(Identifier ID);
        public void AddServerRegistryData(RegistryData registryData);

        public ResourcePack? GetServerResourcePack(MinecraftUUID ID);
        public void AddServerResourcePack(ResourcePack resourcePack);

        public ServerTag? GetServerTag(Identifier Registry, Identifier TagName);
        public void AddServerTag(Identifier Registry, Identifier TagName);

        public Identifier? GetServerFeatureFlag(Identifier FeatureFlag);
        public void AddServerFeatureFlag(Identifier FeatureFlag);

        public void ProcessTransfer();

        public void ProcessGameStateReset();

        //==========================================
        //===============PLAY METHODS===============
        //==========================================
    }
}
