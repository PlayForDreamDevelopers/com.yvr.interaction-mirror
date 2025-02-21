using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace YVR.Interaction.Runtime
{
    public class InteractableFeedback : MonoBehaviour
    {
        private XRUIInputModule m_XRUIInputModule;

        public Action<TrackedDeviceEventData> onPointerEnter;

        public Action<TrackedDeviceEventData> onPointerExit;

        public Action<TrackedDeviceEventData> onPointerClick;

        public Action<TrackedDeviceEventData> onPointerDown;

        private int m_FrameCount;

        private void Awake()
        {
            m_XRUIInputModule = FindObjectOfType<XRUIInputModule>();
        }

        private void OnEnable()
        {
            if (m_XRUIInputModule == null) return;

            m_XRUIInputModule.pointerEnter += PointerEnterEffect;
            m_XRUIInputModule.pointerExit += PointerExitEffect;
            m_XRUIInputModule.pointerClick += PointerClickEffect;
            m_XRUIInputModule.pointerDown += PointerDownEffect;
        }

        private void HandlePointerEvent(PointerEventData eventData, Action<TrackedDeviceEventData> action)
        {
            if (m_FrameCount != Time.frameCount)
            {
                var trackedDeviceEventData = eventData as TrackedDeviceEventData;
                if (trackedDeviceEventData != null)
                {
                    action?.Invoke(trackedDeviceEventData);
                }

                m_FrameCount = Time.frameCount;
            }
        }

        private void PointerEnterEffect(GameObject go, PointerEventData eventData)
        {
            HandlePointerEvent(eventData,onPointerEnter);
        }

        private void PointerExitEffect(GameObject go, PointerEventData eventData)
        {
            HandlePointerEvent(eventData,onPointerExit);
        }

        private void PointerClickEffect(GameObject go, PointerEventData eventData)
        {
            if (go != null)
                HandlePointerEvent(eventData,onPointerClick);
        }

        private void PointerDownEffect(GameObject go, PointerEventData eventData)
        {
            if (go != null)
                HandlePointerEvent(eventData,onPointerDown);
        }

        private void OnDisable()
        {
            if (m_XRUIInputModule == null) return;

            m_XRUIInputModule.pointerEnter -= PointerEnterEffect;
            m_XRUIInputModule.pointerExit -= PointerExitEffect;
            m_XRUIInputModule.pointerClick -= PointerClickEffect;
            m_XRUIInputModule.pointerDown -= PointerDownEffect;
        }
    }
}