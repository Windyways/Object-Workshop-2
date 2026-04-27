using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class FollowCamera(IntPtr ptr) : MonoBehaviour(ptr)
{
    private void Update()
    {
        var Owner = PlayerControl.LocalPlayer;
        if (Owner.AmOwner)
        {
            if (Owner == null || Owner.HasDied())
                return;

            if (Owner.Data.Role is Aimsman aimsman && aimsman.isAiming)
            {
                var crosshair = Crosshair.GetObjectByPlayer(Owner);
                transform.position = crosshair.transform.position;
            }
            else if (Camera.main != null)
            {
                transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10f);
            }
        }
    }
}