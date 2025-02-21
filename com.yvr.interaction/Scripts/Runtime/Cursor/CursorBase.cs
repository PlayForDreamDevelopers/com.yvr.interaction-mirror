using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace YVR.Interaction.Runtime
{
    public abstract class CursorBase : MonoBehaviour
    {
        protected XRRayInteractor m_RayInteractor;
        public CursorConfiguration idleConfiguration = new CursorConfiguration()
        {
            cursorMinScale = 1f,
            cursorDotColor = Color.white
        };

        public CursorConfiguration selectConfiguration = new CursorConfiguration()
        {
            cursorMinScale = 0.8f,
            cursorDotColor = new Color32(23, 92, 230, 255)
        };

        public abstract void UpdateEffect(XRRayInteractor interactor);
    }

}