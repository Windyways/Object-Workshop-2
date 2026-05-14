using UnityEngine;
using Color = UnityEngine.Color;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Totem : MonoBehaviour
{
    public byte id;
    public PlayerControl Owner;
    public SpriteRenderer myRend;

    public void Start()
    {
        //Lockdown.AvailableObjects.Add(gameObject);
        DestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Bounce(gameObject.transform, 0.7f, 0.45f));
        DestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.ColorFade(myRend, new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 1f), 0.4f));

        myRend = gameObject.GetComponent<SpriteRenderer>();

        id = Totem.GetAvailableId();
        Totem.AllTotems.Add(this);
    }

    public void Update()
    {
        UpdateColor();
        if (Owner.Data.Role is Totemist totemist && totemist.isWatching && Owner.AmOwner)
        {
            GameObject target = totemist.TotemOrder[totemist.totemViewing - 1].gameObject;
            if (target == gameObject) gameObject.SetCameraToObject(Owner);
        }
    }

    public void OnDestroy()
    {
        if (Owner.Data.Role is Totemist totemist && totemist.isWatching)
        {
            GameObject target = totemist.TotemOrder[totemist.totemViewing - 1].gameObject;
            if (target == gameObject)
            {
                totemist.isWatching = false;
                LightSource light = Owner.lightSource;
                light.transform.SetParent(Owner.transform);
                light.transform.localPosition = Owner.Collider.offset;
            }

            totemist.TotemOrder.Remove(this);
            AllTotems.Remove(this);
        }
    }

    private static bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();
    }

    public void DestroyGameObject()
    {
        if (this != null)
        {
            AllTotems.Remove(this);
            Destroy(gameObject);
        }
    }

    public static Totem Begin(PlayerControl player)
    {
        WitnessKill.WitnessGoodDeed(player);

        GameObject gameObject = new GameObject("Totem");
        gameObject.AddSpriteRenderer(OWAssets.Totemist_Totem.LoadAsset(), 20, 100, player.transform.position, ObjectExtentions.noColor(), Vector3.one);

        Totem totem = gameObject.AddComponent<Totem>();
        totem.Owner = player;

        gameObject.AddOWObject();
        return totem;
    }

    public static byte GetAvailableId()
    {
        byte id = 0;
        while (AllTotems.Any(x => x.id == id))
        {
            id++;
        }
        return id;
    }

    public static void CleanUp()
    {
        foreach (Totem totem in AllTotems)
        {
            Destroy(totem.gameObject);
        }
        AllTotems.Clear();
    }

    public static IEnumerable<Totem> GetAll() => AllTotems;
    public static List<Totem> AllTotems = new List<Totem>();
}