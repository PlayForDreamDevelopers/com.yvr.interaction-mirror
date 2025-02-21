using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace YVR.Interaction.Runtime
{
    public class SphereCursor : CursorBase
    {
        public MeshRenderer cursorRenderer;
        public override void UpdateEffect(XRRayInteractor rayInteractor)
        {
            rayInteractor.TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine,
                out bool isValidTarget);
            float distance = Vector3.Distance(position, rayInteractor.transform.position) - 0.01f;
            CursorConfiguration configuration = rayInteractor.uiPressInput.ReadIsPerformed() ? selectConfiguration : idleConfiguration;
            float scaleParam = Mathf.Max(configuration.cursorMinScale, distance) * 0.1f;
            transform.localPosition = Vector3.forward * distance;
            transform.forward = normal * -1;
            transform.localScale = new Vector3(scaleParam, scaleParam, transform.localScale.z);
            cursorRenderer.material.color = configuration.cursorDotColor;
        }
    }
}