using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace YVR.Interaction.Runtime
{
    public struct InteractorRaycastData
    {
        public TrackedDeviceModel model;
        public RaycastResult raycastResult;
    }
}