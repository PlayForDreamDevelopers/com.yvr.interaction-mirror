using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;
using YVR.Utilities;

namespace YVR.Interaction.Runtime
{
    public class InteractorManager : MonoBehaviorSingleton<InteractorManager>
    {
        [Header("HMD")] [Tooltip("HMD Gaze Interactor for UI interaction.")] [SerializeField]
        private GameObject m_HMD;

        public GameObject HMD => m_HMD;

        [Header("Controller")] [SerializeField]
        private GameObject m_LeftController;

        public GameObject leftController => m_LeftController;

        [SerializeField] private GameObject m_RightController;

        public GameObject rightController => m_RightController;

        [Header("HandTracking")] [SerializeField]
        private GameObject m_LeftHand;

        public GameObject leftHand => m_LeftHand;

        [SerializeField] private GameObject m_RightHand;

        public GameObject rightHand => m_RightHand;

        [Header("eyeGaze")] [Tooltip("Eye Gaze Interactor for UI interaction.")] [SerializeField]
        private GameObject m_EyeGaze;

        public GameObject eyeGaze => m_EyeGaze;

        public XRInteractionManager xrInteractionManager;

        private List<InteractorRaycastData> m_InteractorRaycastDatas = new List<InteractorRaycastData>();
        public List<InteractorRaycastData> allInteractorRaycastDatas => m_InteractorRaycastDatas;

        private List<IUIInteractor> m_UIInteractors = new List<IUIInteractor>();

        public List<IUIInteractor> uiInteractors => m_UIInteractors;

        public event Action<TrackedDeviceModel> onRaycastHit = null;

        private ProfilerMarker m_ProfileMaker = new ProfilerMarker("InteractorManager");

        protected override void Init()
        {
            base.Init();
            GetRegisteredInteractor();
            xrInteractionManager.interactorRegistered += OnInteractorRegistered;
            xrInteractionManager.interactorUnregistered += OnInteractorUnregistered;
        }

        private void OnDestroy()
        {
            xrInteractionManager.interactorRegistered -= OnInteractorRegistered;
            xrInteractionManager.interactorUnregistered -= OnInteractorUnregistered;
        }

        private void GetRegisteredInteractor()
        {
            List<IXRInteractor> xRInteractors = new List<IXRInteractor>();
            xrInteractionManager.GetRegisteredInteractors(xRInteractors);
            foreach (var interactor in xRInteractors)
            {
                if (interactor is IUIInteractor uiInteractor)
                {
                    m_UIInteractors.Add(uiInteractor);
                }
            }
        }

        private void OnInteractorRegistered(InteractorRegisteredEventArgs registeredEventArgs)
        {
            if (registeredEventArgs.interactorObject is IUIInteractor uiInteractor)
            {
                m_UIInteractors.Add(uiInteractor);
            }
        }

        private void OnInteractorUnregistered(InteractorUnregisteredEventArgs unregisteredEventArgs)
        {
            if (unregisteredEventArgs.interactorObject is IUIInteractor uiInteractor)
            {
                m_UIInteractors.Remove(uiInteractor);
            }
        }

        private void UpdateAllInteractorRaycast()
        {
            m_InteractorRaycastDatas.Clear();
            foreach (var interactor in uiInteractors)
            {
                if (interactor.TryGetUIModel(out var trackedDeviceModel) && trackedDeviceModel.currentRaycast.isValid)
                {
                    InteractorRaycastData interactorRaycastData = new InteractorRaycastData
                    {
                        model = trackedDeviceModel,
                        raycastResult = trackedDeviceModel.currentRaycast
                    };

                    onRaycastHit?.SafeInvoke(trackedDeviceModel);
                    m_InteractorRaycastDatas.Add(interactorRaycastData);
                }
            }
        }

        private void Update()
        {
            using (m_ProfileMaker.Auto())
            {
                UpdateAllInteractorRaycast();
                SwitchInteractionGroup();
            }
        }

        private void SwitchInteractionGroup()
        {
            InputMode mode = InputModalityManager.instance.currentInputMode;
            SafeSetActive(HMD, mode == InputMode.HMD);
            SafeSetActive(eyeGaze, mode == InputMode.EyeGaze);
            SafeSetActive(leftController, mode == InputMode.Controller && InputModalityManager.leftControllerTrackingStatus.isTracked);
            SafeSetActive(rightController, mode == InputMode.Controller && InputModalityManager.rightControllerTrackingStatus.isTracked);
            SafeSetActive(leftHand,  mode == InputMode.HandTracking && InputModalityManager.leftHandTrackingStatus.isTracked);
            SafeSetActive(rightHand, mode == InputMode.HandTracking && InputModalityManager.rightHandTrackingStatus.isTracked);
        }

        private void SafeSetActive(GameObject gameObject, bool active)
        {
            if (gameObject != null && gameObject.activeSelf != active)
                gameObject.SetActive(active);
        }
    }
}