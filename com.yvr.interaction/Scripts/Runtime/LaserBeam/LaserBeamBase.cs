using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


namespace YVR.Interaction.Runtime
{
    public class LaserBeamBase : MonoBehaviour
    {
        public CursorBase cursor;
        public XRRayInteractor rayInteractor;
        public LineRenderer lineRenderer;
        public Vector2 hitDistanceRange = new Vector2(0.1f, 1f);
        public bool isShowCursor = true;
        public LaserBeamConfiguration idleConfiguration = new LaserBeamConfiguration()
        {
            startWidth = 0.0055f,
            endWidth = 0.0015f,
            startColor = new Color32(255, 255, 255, 205),
            endColor = new Color32(255, 255, 255, 0),
        };

        public LaserBeamConfiguration holdConfiguration = new LaserBeamConfiguration()
        {
            startWidth = 0.0041f,
            endWidth = 0.0015f,
            startColor = new Color32(23, 92, 230, 205),
            endColor = new Color32(23, 92, 230, 0),
        };


        private void LateUpdate()
        {
            if (isShowCursor)
            {
                UpdateEffect();
            }
            else if (cursor != null && cursor.gameObject.activeSelf)
            {
                cursor.gameObject.SetActive(isShowCursor);
            }
        }

        protected virtual void UpdateEffect() { }
    }
}