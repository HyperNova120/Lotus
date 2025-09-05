using System.Runtime.InteropServices;
using Graphics_Engine.Commands;
using LotusCore;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
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
        private int _WIDTH = 1920;
        private int _HEIGHT = 1080;
        public Thread? _graphicsThread;

        private static IWindow? _window;

        private static Vk? _vulkan;
        private Instance _instance;

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("graphics", new StartGraphicsCommand());
        }

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent) { }

        public void StartGraphicsThread()
        {
            if (_graphicsThread == null)
                return;

            _graphicsThread = new(new ThreadStart(StartGraphics));
            _graphicsThread.Start();
        }

        private void StartGraphics()
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
            _window!.Run();
        }

        private void CleanUp()
        {
            _vulkan?.DestroyInstance(_instance, null);
            _vulkan?.Dispose();

            _window?.Dispose();
        }

        private bool InitWindow()
        {
            var options = WindowOptions.DefaultVulkan with
            {
                Size = new Vector2D<int>(_WIDTH, _HEIGHT),
                Title = "Lotus Client",
            };

            _window = Window.Create(options);
            _window.Initialize();

            if (_window.VkSurface is null)
            {
                Logging.LogError("Windowing Platform Doesn't Support Vulkan.");
                return false;
            }
            return true;
        }

        private void InitVulkan()
        {
            _vulkan = Vk.GetApi();

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

            var glfwExtensions = _window!.VkSurface!.GetRequiredExtensions(
                out var glfwExtensionCount
            );

            createInfo.EnabledExtensionCount = glfwExtensionCount;
            createInfo.PpEnabledExtensionNames = glfwExtensions;
            createInfo.EnabledLayerCount = 0;

            if (_vulkan.CreateInstance(in createInfo, null, out _instance) != Result.Success)
            {
                Logging.LogError("Failed to Create Graphics Instance");
            }

            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
        }
    }
}
