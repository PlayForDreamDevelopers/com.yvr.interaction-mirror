namespace YVR.Interaction.Runtime.InputDeviceModeFactory
{
    public abstract class BaseInputDeviceFactory
    {
        public InputMode currentMode { get; protected set; }
        public abstract InputMode UpdateDevice();
    }
}