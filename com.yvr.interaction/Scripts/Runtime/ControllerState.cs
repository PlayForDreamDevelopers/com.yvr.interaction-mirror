using YVR.Core;

namespace YVR.Interaction.Runtime
{
    public struct ControllerState
    {
        public ControllerType connectedController;
        public ControllerType trackedController;
        public ControllerType clickedController;
        public ControllerType touchedController;
    }
}