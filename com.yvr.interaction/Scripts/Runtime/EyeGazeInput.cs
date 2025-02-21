using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace YVR.Interaction.Runtime
{
    public class EyeGazeInput : MonoBehaviour
    {
        public enum HandType { Left, Right, Both }
        public XRRayInteractor gazeInteractor;
        public float dragSensitivity = 0.25f;
        public Vector3 pinchingHandPosition { get; set; }
        public InteractorHandedness dragInputHand { get; set; }
        private bool m_WasPressed = default;
        private int m_PressedDeviceId;
        private YVRInputActions m_InputActions;
        private YVRInputActions yvrInputActions { get { return m_InputActions ??= new YVRInputActions(); } }
        private Vector3 m_EyeRayOriginPosition;
        private Quaternion m_EyeRayQuaternion;
        private Vector3 m_StartEyeRayForward;
        private Vector3 m_StartPinchEyePosition;
        private Vector3 m_StartPinchHandPosition;
        private Transform m_EyeRayOriginTransform;
        private XRInputButtonReader m_InputButtonReader = new();

        void Start()
        {
            if (gazeInteractor == null)
            {
                gazeInteractor = GetComponent<XRRayInteractor>();
            }

            yvrInputActions.YVREye.Enable();
            m_EyeRayOriginTransform = gazeInteractor.transform;
            SetLeftHandInput();
            SetRightHandInput();
        }

        public void SwitchHandInputType(HandType handType)
        {
            yvrInputActions.YVRRight.Disable();
            yvrInputActions.YVRLeft.Disable();
            switch (handType)
            {
                case HandType.Left:
                    yvrInputActions.YVRLeft.Enable();
                    m_InputButtonReader.inputActionReferencePerformed =
                        InputActionReference.Create(yvrInputActions.YVRLeft.IndexPressed);
                    break;
                case HandType.Right:
                    yvrInputActions.YVRRight.Enable();
                    m_InputButtonReader.inputActionReferencePerformed =
                        InputActionReference.Create(yvrInputActions.YVRRight.IndexPressed);
                    break;
                case HandType.Both:
                    yvrInputActions.YVRLeft.Enable();
                    yvrInputActions.YVRRight.Enable();
                    m_InputButtonReader.inputActionReferencePerformed =
                        InputActionReference.Create(yvrInputActions.YVREye.EyePress);
                    break;
            }

            gazeInteractor.uiPressInput = m_InputButtonReader;
            gazeInteractor.selectInput = m_InputButtonReader;
            gazeInteractor.activateInput = m_InputButtonReader;
        }

        private void SetLeftHandInput()
        {
            yvrInputActions.YVRLeft.IndexPressed.started += (context) =>
            {
                Debug.LogError($"SetLeftHandInput left index pressed");
                if (!m_WasPressed)
                {
                    dragInputHand = InteractorHandedness.Left;
                    m_StartPinchHandPosition = yvrInputActions.YVRLeft.AimPosition.ReadValue<Vector3>();
                    pinchingHandPosition = m_StartPinchHandPosition;
                }

                PressedStartedCallback(context);
            };

            yvrInputActions.YVRLeft.IndexPressed.canceled += PressedCanceledCallback;
        }

        private void SetRightHandInput()
        {
            yvrInputActions.YVRRight.IndexPressed.started += (context) =>
            {
                if (!m_WasPressed)
                {
                    dragInputHand = InteractorHandedness.Right;
                    m_StartPinchHandPosition = yvrInputActions.YVRRight.AimPosition.ReadValue<Vector3>();
                    pinchingHandPosition = m_StartPinchHandPosition;
                }

                PressedStartedCallback(context);
            };

            yvrInputActions.YVRRight.IndexPressed.canceled += PressedCanceledCallback;
        }

        private void PressedStartedCallback(InputAction.CallbackContext context)
        {
            if (m_WasPressed) return;

            m_PressedDeviceId = context.control.device.deviceId;
            m_StartPinchEyePosition = yvrInputActions.YVREye.devicePosition.ReadValue<Vector3>();
            m_StartEyeRayForward = m_EyeRayOriginTransform.transform.forward;
            m_WasPressed = true;
        }

        private void PressedCanceledCallback(InputAction.CallbackContext context)
        {
            if (m_PressedDeviceId != context.control.device.deviceId) return;
            m_WasPressed = false;
            dragInputHand = InteractorHandedness.None;
        }

        private void Update()
        {
            m_EyeRayOriginPosition = yvrInputActions.YVREye.devicePosition.ReadValue<Vector3>();
            m_EyeRayQuaternion = yvrInputActions.YVREye.deviceRotation.ReadValue<Quaternion>();
            switch (dragInputHand)
            {
                case InteractorHandedness.Left:
                    pinchingHandPosition = yvrInputActions.YVRLeft.AimPosition.ReadValue<Vector3>();
                    break;
                case InteractorHandedness.Right:
                    pinchingHandPosition = yvrInputActions.YVRRight.AimPosition.ReadValue<Vector3>();
                    break;
            }

            if (m_WasPressed)
            {
                m_EyeRayOriginTransform.position = m_StartPinchEyePosition;
                CalcPressRayOriginRotation();
            }
            else
            {
                m_EyeRayOriginTransform.position = m_EyeRayOriginPosition;
                m_EyeRayOriginTransform.rotation = m_EyeRayQuaternion;
            }
        }

        private void CalcPressRayOriginRotation()
        {
            Vector3 virtualPosition = m_StartPinchEyePosition + m_StartEyeRayForward * dragSensitivity;
            Vector3 offsetProjectOnPlanePinch = Vector3.ProjectOnPlane(pinchingHandPosition,
                (virtualPosition - m_StartPinchEyePosition).normalized);
            Vector3 offsetProjectOnPlaneStart = Vector3.ProjectOnPlane(m_StartPinchHandPosition,
                (virtualPosition - m_StartPinchEyePosition).normalized);
            Vector3 offsetProjectOnPlane = offsetProjectOnPlanePinch - offsetProjectOnPlaneStart;
            virtualPosition += offsetProjectOnPlane;
            m_EyeRayOriginTransform.LookAt(virtualPosition);
        }
    }
}