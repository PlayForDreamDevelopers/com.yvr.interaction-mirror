using YVR.Core;

namespace YVR.Interaction.Runtime.InputDeviceModeFactory
{
    public class SystemInputDeviceFactory : BaseInputDeviceFactory
    {
        private ActiveInputDevice m_Device;
        
        public override InputMode UpdateDevice()
        {
            YVRPlugin.Instance.GetCurrentInputDevice(ref m_Device);
            currentMode = m_Device switch
            {
                ActiveInputDevice.None => InputMode.HMD,
                ActiveInputDevice.ControllerActive => InputMode.Controller,
                ActiveInputDevice.HandTrackingActive => InputMode.HandTracking,
                _ => InputMode.HMD
            };

            return currentMode;
        }
    }
}