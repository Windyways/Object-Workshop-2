using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class FollowCamera(IntPtr ptr) : MonoBehaviour(ptr)
{
    private void Update()
    {
        if (Camera.main != null)
        {
            transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10f);
        }
    }
}