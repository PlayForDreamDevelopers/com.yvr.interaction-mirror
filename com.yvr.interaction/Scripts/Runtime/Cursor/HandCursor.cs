using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace YVR.Interaction.Runtime
{
    public class HandCursor : CursorBase
    {
        public GameObject course;
        public GameObject dot;
        private float m_CompressLimit = 0.5f;
        private float m_CourseMinLimitSize = 0.6f;

        public override void UpdateEffect(XRRayInteractor interactor)
        {
            m_RayInteractor = interactor;
            interactor.TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine,
                out bool isValidTarget);
            float distance = Vector3.Distance(position, m_RayInteractor.transform.position) - 0.01f;
            CursorConfiguration configuration = m_RayInteractor.uiPressInput.ReadIsPerformed() ? selectConfiguration : idleConfiguration;
            float scaleParam = Mathf.Max(configuration.cursorMinScale, distance) * 0.1f;
            transform.localPosition = Vector3.forward * distance;
            transform.forward = normal;
            transform.localScale = new Vector3(scaleParam, scaleParam, transform.localScale.z);
            UpdateCursorSize();
        }

        private void UpdateCursorSize()
        {
            dot.gameObject.SetActive(m_RayInteractor.uiPressInput.ReadIsPerformed());
            float pinchStrength = m_RayInteractor.uiPressInput.ReadValue();
            float pinchFactor = 1 - Mathf.Clamp(pinchStrength - m_CompressLimit, 0, 1 - m_CompressLimit) /
                (1 - m_CompressLimit);
            float courseSize = m_CompressLimit * (1 + pinchFactor);
            courseSize = Mathf.Clamp(courseSize, m_CourseMinLimitSize, 1f);
            course.transform.localScale = new Vector3(courseSize, courseSize, courseSize);
        }

        private void OnDisable()
        {
            course.transform.localScale = Vector3.one;
        }
    }
}