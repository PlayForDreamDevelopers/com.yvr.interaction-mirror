using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR;
using YVR.Core;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YVR.Interaction.Runtime
{
    /// <summary>
    ///     An <see cref="UnityEngine.InputSystem.InputDevice" /> that surfaces common controller data
    ///     populated by hand joint poses. Devices will only be created if
    ///     hand-tracking is enabled in the build settings for the target platform.
    /// </summary>
    /// <remarks>
    ///     The <see cref="TrackedDevice.devicePosition" /> and
    ///     <see cref="TrackedDevice.deviceRotation" /> inherited from <see cref="TrackedDevice" />
    ///     represent the wrist pose. This is reported in session space,
    ///     relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
    /// </remarks>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    [InputControlLayout(displayName = "YVR Hand Device", commonUsages = new[] { "LeftHand", "RightHand" })]
    public class YVRHandDevice : TrackedDevice
    {
        private const string deviceProductName = "YVRHandDevice";

        private HandType m_Handedness;
        private bool m_WereJointsValid;

        static YVRHandDevice()
        {
            Initialize();
        }

        /// <summary>
        ///     The left-hand <see cref="UnityEngine.InputSystem.InputDevice" /> that contains
        ///     <see cref="InputControl" />s that surface common controller data
        ///     populated by hand joint poses.
        /// </summary>
        public static YVRHandDevice leftHand { get; internal set; }

        /// <summary>
        ///     The right-hand <see cref="UnityEngine.InputSystem.InputDevice" /> that contains
        ///     <see cref="InputControl" />s that surface common controller data
        ///     populated by hand joint poses.
        /// </summary>
        public static YVRHandDevice rightHand { get; internal set; }

        /// <summary>
        ///     Position of the grip pose, representing the palm. This is reported in session
        ///     space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve]
        [InputControl]
        public Vector3Control gripPosition { get; private set; }

        /// <summary>
        ///     Rotation of the grip pose, representing the palm. This is reported in session
        ///     space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve]
        [InputControl]
        public QuaternionControl gripRotation { get; private set; }

        /// <summary>
        ///     Position of the poke pose, representing the index finger's tip. This is reported
        ///     in session space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve]
        [InputControl]
        public Vector3Control pokePosition { get; private set; }

        /// <summary>
        ///     Rotation of the poke pose, representing the index finger's tip. This is reported
        ///     in session space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve]
        [InputControl]
        public QuaternionControl pokeRotation { get; private set; }

        /// <summary>
        ///     Position of the pinch pose, representing the thumb's tip. This is reported in
        ///     session space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve]
        [InputControl]
        public Vector3Control pinchPosition { get; private set; }

        /// <summary>
        ///     Rotation of the pinch pose, representing the thumb's tip. This is reported in
        ///     session space, relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </summary>
        [Preserve]
        [InputControl]
        public QuaternionControl pinchRotation { get; private set; }

        /// <summary>
        ///     Perform final initialization tasks after the control hierarchy has been put into place.
        /// </summary>
        protected override void FinishSetup()
        {
            base.FinishSetup();

            gripPosition = GetChildControl<Vector3Control>("gripPosition");
            gripRotation = GetChildControl<QuaternionControl>("gripRotation");
            pokePosition = GetChildControl<Vector3Control>("pokePosition");
            pokeRotation = GetChildControl<QuaternionControl>("pokeRotation");
            pinchPosition = GetChildControl<Vector3Control>("pinchPosition");
            pinchRotation = GetChildControl<QuaternionControl>("pinchRotation");

            var deviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);
            if (deviceDescriptor != null)
            {
                if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Left) != 0)
                    InputSystem.SetDeviceUsage(this, CommonUsages.LeftHand);
                else if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Right) != 0)
                    InputSystem.SetDeviceUsage(this, CommonUsages.RightHand);
            }
        }

        internal static YVRHandDevice Create(HandType handedness)
        {
            var extraCharacteristics = handedness == HandType.HandLeft
                ? InputDeviceCharacteristics.Left
                : InputDeviceCharacteristics.Right;

            var desc = new InputDeviceDescription
            {
                product = deviceProductName,
                capabilities = new XRDeviceDescriptor
                {
                    characteristics = InputDeviceCharacteristics.HandTracking | extraCharacteristics,
                    inputFeatures = new List<XRFeatureDescriptor>
                    {
                        new XRFeatureDescriptor()
                        {
                            name = "grip_position",
                            featureType = FeatureType.Axis3D,
                        },
                        new XRFeatureDescriptor()
                        {
                            name = "grip_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor()
                        {
                            name = "poke_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor()
                        {
                            name = "poke_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor()
                        {
                            name = "pinch_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor()
                        {
                            name = "pinch_rotation",
                            featureType = FeatureType.Rotation
                        }
                    }
                }.ToJson()
            };
            var handDevice = InputSystem.AddDevice(desc) as YVRHandDevice;
            handDevice.m_Handedness = handedness;
            Application.onBeforeRender += handDevice.OnUpdatedHands;
            return handDevice;
        }

        public void Destroy()
        {
            if (this.added)
            {
                Application.onBeforeRender -= OnUpdatedHands;
                InputSystem.RemoveDevice(this);
            }
        }

        private void OnUpdatedHands()
        {
            HandJointLocations hand;
            var areJointsValid = false;

            hand = m_Handedness == HandType.HandLeft
                ? YVRHandManager.instance.leftHandData
                : hand = YVRHandManager.instance.rightHandData;

            areJointsValid = hand.isActive == 1;
            if (!areJointsValid)
            {
                InputSystem.QueueDeltaStateEvent(isTracked, false);
                InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.None);
                return;
            }

            if (!m_WereJointsValid)
            {
                InputSystem.QueueDeltaStateEvent(isTracked, true);
                InputSystem.QueueDeltaStateEvent(trackingState,
                    InputTrackingState.Position | InputTrackingState.Rotation);
            }

            InputSystem.QueueDeltaStateEvent(devicePosition,
                hand.jointLocations[(int)HandJoint.JointWrist].pose.position);
            InputSystem.QueueDeltaStateEvent(deviceRotation,
                hand.jointLocations[(int)HandJoint.JointWrist].pose.orientation);

            InputSystem.QueueDeltaStateEvent(gripPosition,
                hand.jointLocations[(int)HandJoint.JointIndexProximal].pose.position);
            InputSystem.QueueDeltaStateEvent(gripRotation,
                hand.jointLocations[(int)HandJoint.JointIndexProximal].pose.orientation);

            InputSystem.QueueDeltaStateEvent(pokePosition,
                hand.jointLocations[(int)HandJoint.JointIndexTip].pose.position);
            InputSystem.QueueDeltaStateEvent(pokeRotation,
                hand.jointLocations[(int)HandJoint.JointIndexTip].pose.orientation);

            InputSystem.QueueDeltaStateEvent(pinchPosition,
                hand.jointLocations[(int)HandJoint.JointThumbTip].pose.position);
            InputSystem.QueueDeltaStateEvent(pinchRotation,
                hand.jointLocations[(int)HandJoint.JointThumbTip].pose.orientation);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            InputSystem.RegisterLayout<YVRHandDevice>(
                matches: new InputDeviceMatcher()
                    .WithProduct(deviceProductName));
        }
    }
}