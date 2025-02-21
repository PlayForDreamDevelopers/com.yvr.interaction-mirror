using UnityEngine;
using YVR.Core;
using YVR.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace YVR.Interaction.Runtime
{
    public class ControllerLaserBeam : LaserBeamBase
    {
        public bool isMutuallyExclusive;
        public ControllerType controllerType;
        private void Start()
        {
            lineRenderer.positionCount = 2;
        }

        protected override void UpdateEffect()
        {
            if ((isMutuallyExclusive &&
                InputModalityManager.instance.controllerState.clickedController != controllerType) ||
                InputModalityManager.instance.currentInputMode != InputMode.Controller ||
                rayInteractor.IsBlockedByInteractionWithinGroup())
            {
                lineRenderer.enabled = false;
                cursor.SetActive(false);
                return;
            }

            lineRenderer.enabled = true;
            LaserBeamConfiguration configuration =
                rayInteractor.uiPressInput.ReadIsPerformed() ? holdConfiguration : idleConfiguration;
            rayInteractor.TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine,
                out bool isValidTarget);
            float distance = isValidTarget ? Vector3.Distance(position, rayInteractor.transform.position) : 0;

            UpdateLaserBeam(configuration, distance, normal);

            if (cursor)
            {
                if (isValidTarget)
                {
                    cursor.UpdateEffect(rayInteractor);
                }

                cursor.gameObject.SetActive(isValidTarget);
            }
        }

        private void UpdateLaserBeam(LaserBeamConfiguration configuration, float distance, Vector3 normal)
        {
            if (lineRenderer)
            {
                lineRenderer.startColor = configuration.startColor;
                lineRenderer.endColor = configuration.endColor;
                lineRenderer.startWidth = configuration.startWidth;
                lineRenderer.endWidth = configuration.endWidth;
                lineRenderer.SetPosition(0, transform.position + transform.forward * configuration.startPointOffset);
                float beamLength = Mathf.Clamp(distance, hitDistanceRange.x, hitDistanceRange.y);
                lineRenderer.SetPosition(1, transform.position + transform.forward * beamLength);
            }
        }

        private void OnDisable()
        {
            lineRenderer.SetPosition(0, Vector3.one * 9998);
            lineRenderer.SetPosition(1, Vector3.one * 9998);

            lineRenderer.enabled = false;
            cursor.gameObject.SetActive(false);
        }
    }
}