using LotusCore;
using LotusCore.Interfaces;

namespace Graphics_Engine.Commands
{
    public class StartGraphicsCommand : ICommandBase
    {
        private VulkanGraphics _vulkanGraphics;

        public StartGraphicsCommand(VulkanGraphics vulkanGraphics)
        {
            this._vulkanGraphics = vulkanGraphics;
        }

        public string GetCommandDescription()
        {
            return "Starts the graphics engine and opens the graphics window";
        }

        public string GetCommandCorrectUsage()
        {
            return "Correct usage: 'graphics'";
        }

        public async Task ProcessCommand(string[] commandArgs)
        {
            //Core_Engine.Core_Engine.CurrentState = Core_Engine.Core_Engine.State.Waiting;
            LotusCore.Core_Engine.SignalInteractiveHold(LotusCore.Core_Engine.State.Graphics);
            _vulkanGraphics.StartGraphicsThread();
        }
    }
}
