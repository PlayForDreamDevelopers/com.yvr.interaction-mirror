namespace YVR.Interaction.Runtime.InputDeviceModeFactory
{
    public class ControllerOnlyDeviceFactory : BaseInputDeviceFactory
    {
        public ControllerOnlyDeviceFactory()
        {
            currentMode = InputMode.Controller;
        }
        public override InputMode UpdateDevice() { return currentMode; }
    }
}