using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using YVR.Core;
using YVR.Utilities;

namespace YVR.Interaction.Runtime
{
    public class HandLaserBeam : LaserBeamBase
    {
        public InputActionProperty aimState;
        protected override void UpdateEffect()
        {
            if (InputModalityManager.instance.currentInputMode != InputMode.HandTracking || rayInteractor.IsBlockedByInteractionWithinGroup())
            {
                cursor.SetActive(false);
                return;
            }
            rayInteractor.TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine,
                out bool isValidTarget);
            bool aimValid = ((HandStatus)aimState.action.ReadValue<int>() & HandStatus.InputStateValid) != 0;
            cursor.SetActive(aimValid && isValidTarget);
            if (aimValid && isValidTarget)
            {
                cursor.UpdateEffect(rayInteractor);
            }
        }
    }
}