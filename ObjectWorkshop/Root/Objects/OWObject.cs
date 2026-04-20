using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class OWObject(IntPtr ptr) : MonoBehaviour(ptr)
{
    public byte id;
    public bool isTombstone;
    public SpriteRenderer myRend;
    public SpriteRenderer originalMyRend;
    public PlayerControl Owner;

    public bool hideInVent;

    private void Start()
    {
        OWObjects.Add(this);
        myRend = gameObject.GetComponent<SpriteRenderer>();
        originalMyRend = myRend;

        id = GetAvailableId();
    }

    public void SetOutline(bool on, bool mainTarget, Color color)
    {
        // ill fix this issue later.
        //myRend.color = (on && mainTarget) ? color : originalMyRend.color;
    }

    private void OnDestroy()
    {
        OWObjects.Remove(this);
    }

    public static List<OWObject> OWObjects = new List<OWObject>();

    public static OWObject GetById(byte id)
    {
        foreach (var obj in OWObjects)
        {
            if (obj.id == id) return obj;
        }

        return null;
    }

    public static byte GetAvailableId()
    {
        byte id = 0;
        while (OWObjects.Any(x => x.id == id))
        {
            id++;
        }
        return id;
    }
}
