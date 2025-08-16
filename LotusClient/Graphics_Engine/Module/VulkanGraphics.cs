using System.Runtime.InteropServices;
using Core_Engine;
using Core_Engine.Interfaces;
using Graphics_Engine.Commands;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg.Sig;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Graphics_Engine
{
    public unsafe class VulkanGraphics : IGraphicsModule
    {
        private int WIDTH = 1920;
        private int HEIGHT = 1080;
        public Thread? _GraphicsThread;

        private static IWindow? _Window;

        private static Vk? _Vulkan;
        private Instance _Instance;

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("graphics", new StartGraphicsCommand());
        }

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent) { }

        public void StartGraphics()
        {
            if (InitWindow())
            {
                InitVulkan();
                MainLoop();
            }
            CleanUp();
        }

        private void MainLoop()
        {
            _Window!.Run();
        }

        private void CleanUp()
        {
            _Vulkan?.DestroyInstance(_Instance, null);
            _Vulkan?.Dispose();

            _Window?.Dispose();
        }

        private bool InitWindow()
        {
            var options = WindowOptions.DefaultVulkan with
            {
                Size = new Vector2D<int>(WIDTH, HEIGHT),
                Title = "Vulkan",
            };

            _Window = Window.Create(options);
            _Window.Initialize();

            if (_Window.VkSurface is null)
            {
                Logging.LogError("Windowing Platform Doesn't Support Vulkan.");
                return false;
            }
            return true;
        }

        private void InitVulkan()
        {
            _Vulkan = Vk.GetApi();

            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Lotus Client"),
                ApplicationVersion = new Version32(1, 0, 0),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("No Engine"),
                EngineVersion = new Version32(1, 0, 0),
                ApiVersion = Vk.Version13,
            };

            InstanceCreateInfo createInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
            };

            var glfwExtensions = _Window!.VkSurface!.GetRequiredExtensions(
                out var glfwExtensionCount
            );

            createInfo.EnabledExtensionCount = glfwExtensionCount;
            createInfo.PpEnabledExtensionNames = glfwExtensions;
            createInfo.EnabledLayerCount = 0;

            if (_Vulkan.CreateInstance(in createInfo, null, out _Instance) != Result.Success)
            {
                Logging.LogError("Failed to Create Graphics Instance");
            }

            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
        }
    }
}
