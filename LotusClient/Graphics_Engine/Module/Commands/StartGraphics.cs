using Core_Engine;
using Core_Engine.Interfaces;

namespace Graphics_Engine.Commands
{
    public class StartGraphicsCommand : ICommandBase
    {
        public string GetCommandDescription()
        {
            return "Starts the graphics engine and opens the graphics window";
        }

        public async Task ProcessCommand(string[] commandArgs)
        {
            VulkanGraphics VG = Core_Engine.Core_Engine.GetModule<VulkanGraphics>(
                "VulkanGraphics"
            )!;
            //Core_Engine.Core_Engine.CurrentState = Core_Engine.Core_Engine.State.Waiting;
            Core_Engine.Core_Engine.SignalInteractiveHold(Core_Engine.Core_Engine.State.Graphics);
            VG._GraphicsThread = new(new ThreadStart(VG.StartGraphics));
            VG._GraphicsThread.Start();
        }
    }
}
