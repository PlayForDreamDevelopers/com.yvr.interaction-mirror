using UnityEngine;
using UnityEngine.EventSystems;
using YVR.Utilities;

namespace YVR.Interaction.Runtime
{
    public class HMDLaserBeam : LaserBeamBase
    {
        protected override void UpdateEffect()
        {
            rayInteractor.TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine,
                out bool isValidTarget);
            cursor.SetActive(isValidTarget);
            if (isValidTarget)
            {
                cursor.UpdateEffect(rayInteractor);
            }
        }
    }
}