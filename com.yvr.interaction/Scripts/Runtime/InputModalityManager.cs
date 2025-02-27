using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using YVR.Core;
using YVR.Core.XR.InputDevices;
using YVR.Interaction.Runtime.InputDeviceModeFactory;
using YVR.Utilities;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;
using XR = UnityEngine.InputSystem.XR;

namespace YVR.Interaction.Runtime
{
    public class InputModalityManager : MonoBehaviorSingleton<InputModalityManager>
    {
        public Action onInputModeChanged;
        public InputMode currentInputMode { get; private set; }

        public bool requireHandSubsystem = true;

        #region Device

        private static TrackedDevice s_LeftController = null;

        public static TrackedDevice leftController =>
            s_LeftController ??= InputSystem.GetDevice<XR.XRController>(CommonUsages.LeftHand);

        private static TrackedDevice s_RightController = null;

        public static TrackedDevice rightController =>
            s_RightController ??= InputSystem.GetDevice<XR.XRController>(CommonUsages.RightHand);

        private static TrackedDevice s_LeftHand = null;

        public static TrackedDevice leftHand =>
            s_LeftHand ??= InputSystem.GetDevice<YVRAimHand>(CommonUsages.LeftHand);

        private static TrackedDevice s_RightHand = null;

        public static TrackedDevice rightHand =>
            s_RightHand ??= InputSystem.GetDevice<YVRAimHand>(CommonUsages.RightHand);

        private static TrackedDevice s_EyeTracking = null;

        private static TrackedDevice eyeTracking =>
            s_EyeTracking ??= InputSystem.GetDevice<YVREyeDevice>("EyesTracking");

        #endregion

        private static TrackingStatus GetTrackingStatus(TrackedDevice device)
        {
            if (device == null || device.added == false) return default;

            return new TrackingStatus
            {
                isConnected = device.added,
                isTracked = device.isTracked.isPressed,
                trackingState = (InputTrackingState) device.trackingState.value,
            };
        }

        public static TrackingStatus leftControllerTrackingStatus { get; private set; }
        public static TrackingStatus rightControllerTrackingStatus { get; private set; }
        public static TrackingStatus leftHandTrackingStatus { get; private set; }
        public static TrackingStatus rightHandTrackingStatus { get; private set; }
        public static TrackingStatus eyeTrackingStatus { get; private set; }

        public InputActionProperty leftClick;
        public InputActionProperty rightClick;

        public InputActionProperty leftTouch;
        public InputActionProperty rightTouch;

        private XRHandSubsystem m_XRHandSubsystem;
        private BaseInputDeviceFactory m_InputDeviceFactory;

        public XRHandSubsystem handSubsystem
        {
            get
            {
                if (m_XRHandSubsystem != null) return m_XRHandSubsystem;

                List<XRHandSubsystem> subsystems = new();
                SubsystemManager.GetInstances(subsystems);

                return m_XRHandSubsystem = subsystems.FirstOrDefault();
            }
        }

        private ProfilerMarker m_ProfileMaker = new("InputModalityManager");

        public ControllerState controllerState;

        protected override void Init()
        {
            base.Init();
            m_InputDeviceFactory = new SystemInputDeviceFactory();

            if (requireHandSubsystem)
                handSubsystem.Start();
            else
                handSubsystem.Stop();
        }

        public void SetInputDeviceMode(BaseInputDeviceFactory inputDeviceFactory)
        {
            if (inputDeviceFactory != null)
                m_InputDeviceFactory = inputDeviceFactory;
        }

        private void Update()
        {
            using (m_ProfileMaker.Auto())
            {
                UpdateTrackingStatus();
                SwitchInputMode(m_InputDeviceFactory.UpdateDevice());
                UpdateControllerType();
            }
        }

        private void UpdateTrackingStatus()
        {
            leftControllerTrackingStatus = GetTrackingStatus(leftController);
            rightControllerTrackingStatus = GetTrackingStatus(rightController);
            leftHandTrackingStatus = GetTrackingStatus(leftHand);
            rightHandTrackingStatus = GetTrackingStatus(rightHand);
            eyeTrackingStatus = GetTrackingStatus(eyeTracking);
        }

        private void UpdateControllerType()
        {
            controllerState.trackedController =
                (leftControllerTrackingStatus.isTracked ? ControllerType.LeftTouch : ControllerType.None) |
                (rightControllerTrackingStatus.isTracked ? ControllerType.RightTouch : ControllerType.None);

            controllerState.connectedController =
                (leftControllerTrackingStatus.isConnected ? ControllerType.LeftTouch : ControllerType.None) |
                (rightControllerTrackingStatus.isConnected ? ControllerType.RightTouch : ControllerType.None);

            if ((controllerState.trackedController & ControllerType.Touch) != ControllerType.Touch)
                controllerState.clickedController = controllerState.trackedController;
            else if (leftClick.action.IsPressed())
                controllerState.clickedController = ControllerType.LeftTouch;
            else if (rightClick.action.IsPressed())
                controllerState.clickedController = ControllerType.RightTouch;

            if ((controllerState.trackedController & ControllerType.Touch) != ControllerType.Touch)
                controllerState.touchedController = controllerState.trackedController;
            else if (leftTouch.action.IsPressed())
                controllerState.touchedController = ControllerType.LeftTouch;
            else if (rightTouch.action.IsPressed())
                controllerState.touchedController = ControllerType.RightTouch;
        }

        private void SwitchInputMode(InputMode inputMode)
        {
            if (currentInputMode == inputMode) return;

            currentInputMode = inputMode;
            SwitchHandDeviceBasedOnInputMode();

            onInputModeChanged?.Invoke();
        }

        private void SwitchHandDeviceBasedOnInputMode()
        {
            if (currentInputMode == InputMode.Controller)
            {
                YVRAimHand.left?.Destroy();
                YVRAimHand.right?.Destroy();
                s_RightHand = null;
                s_LeftHand = null;
                if (requireHandSubsystem) handSubsystem?.Stop();
            }
            else
            {
                if (requireHandSubsystem) handSubsystem?.Start();

                if (YVRAimHand.left == null)
                    YVRAimHand.CreateHand(InputDeviceCharacteristics.Left);
                if (YVRAimHand.right == null)
                    YVRAimHand.CreateHand(InputDeviceCharacteristics.Right);
            }
        }
    }
}