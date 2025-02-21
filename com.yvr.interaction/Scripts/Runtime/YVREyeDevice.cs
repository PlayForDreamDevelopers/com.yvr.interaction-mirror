using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace YVR.Interaction.Runtime
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    [InputControlLayout(displayName = "YVR Eye Device", canRunInBackground = true,
                           commonUsages = new[] {"EyesTracking"})]
    public class YVREyeDevice : TrackedDevice
    {
        private const string k_YVREyeDeviceProductName = "EyesTracking";
        private const string k_YVREyeDeviceManufacturerName = "YVR";

        public EyesControl eyesData { get; protected set; }

        protected override void FinishSetup()
        {
            eyesData = GetChildControl<EyesControl>("eyesData");
            isTracked = GetChildControl<ButtonControl>("isTracked");
            trackingState = GetChildControl<IntegerControl>("trackingState");
        }

        static YVREyeDevice() { RegisterLayout(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterLayout()
        {
            InputSystem.RegisterLayout<YVREyeDevice>(matches: new InputDeviceMatcher()
                                                             .WithProduct(k_YVREyeDeviceProductName)
                                                             .WithManufacturer(k_YVREyeDeviceManufacturerName));
        }
    }
}