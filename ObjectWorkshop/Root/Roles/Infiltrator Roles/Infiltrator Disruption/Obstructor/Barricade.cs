using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Barricade(IntPtr ptr) : MonoBehaviour(ptr)
{
    public byte id;
    public float radius = 0.75f;
    public PlayerControl Owner;
    public SpriteRenderer myRend;

    public void Start()
    {
        // Lockdown.AvailableObjects.Add(base.gameObject);
        DestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Bounce(gameObject.transform, 0.7f, 0.45f));

        myRend = gameObject.GetComponent<SpriteRenderer>();
        id = GetAvailableId();

        AllBarricades.Add(this);
        DestroyMoonOnPlace();
    }

    private void Update()
    {
        UpdateColor();

        PolygonCollider2D polygonCollider2D = gameObject.GetComponent<PolygonCollider2D>();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.onLadder || Sabotages.AnyActive() || player.Is(CalculatedFaction.Infiltrator) || /*player.IsUnderground() ||*/ player.HasDied())
            {
                ObjectExtentions.IgnoreCollision(polygonCollider2D, player.GetComponent<CircleCollider2D>());
            }
            else
            {
                ObjectExtentions.EnableCollision(polygonCollider2D, player.GetComponent<CircleCollider2D>());
            }
        }
    }

    private bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility())
        {
            if (Sabotages.AnyActive()) myRend.color = new Color(1f, 1f, 1f, 0.3f);
            else myRend.Show();
        }
        else myRend.Hide();
    }

    public void DestroyMoonOnPlace()
    {
        foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
        {
            if (obj.name == "Moon")
            {
                Vector2 pos = obj.transform.position;
                Vector2 barricadePos = transform.position;
                if (Vector2.Distance(pos, barricadePos) < radius)
                {
                    Destroy(obj);
                }
            }
        }
    }

    public static Barricade Begin(PlayerControl player)
    {
        WitnessKill.WitnessEvilDeed(player);

        if (player.Data.Role is Obstructor obstructor)
        {
            if (obstructor.ActiveBarricades.Count >= OptionGroupSingleton<Obstructor_Options>.Instance.MaxAtOnce)
            {
                // Allows for multiple Barricades, destroying the oldest one if reaches maximum.
                var btd = obstructor.ActiveBarricades[0]; // Barricade To Destroy.
                DestroyGameObject(btd);
                obstructor.ActiveBarricades.Remove(btd);
            }
        }

        GameObject gameObject = new GameObject("Barricade");
        gameObject.AddSpriteRenderer(OWAssets.Obstructor_Barricade.LoadAsset(), 0, 100, player.transform.position, ObjectExtentions.noColor(), Vector3.one);
        gameObject.AddPolygonCollider2D(false,
        [
                new Vector2(-0.5f, -0.85f),
                new Vector2(0.5f, -0.85f),
                new Vector2(0.15f, 0.5f),
                new Vector2(-0.15f, 0.5f)
        ], true);

        gameObject.AddRigidBody2D(0f, RigidbodyConstraints2D.FreezeAll);

        Barricade barricade = gameObject.AddComponent<Barricade>();
        barricade.Owner = player;

        gameObject.AddOWObject();
        return barricade;
    }

    public static byte GetAvailableId()
    {
        byte id = 0;
        while (AllBarricades.Any(x => x.id == id))
        {
            id++;
        }
        return id;
    }

    public static void DestroyGameObject(Barricade barricade)
    {
        if (barricade != null)
        {
            AllBarricades.Remove(barricade);
            Destroy(barricade.gameObject);
        }
    }

    public static void DestroyAll()
    {
        foreach (Barricade barricades in AllBarricades)
        {
            Destroy(barricades.gameObject);
        }
        AllBarricades.Clear();
    }

    public static Barricade GetObjectByPlayer(PlayerControl player)
    {
        foreach (Barricade barricade in AllBarricades)
        {
            if (barricade.Owner == player) return barricade;
        }
        return null;
    }

    public static void CleanUp()
    {
        AllBarricades.Clear();
    }

    public static IEnumerable<Barricade> GetAll() => AllBarricades;
    public static List<Barricade> AllBarricades = new List<Barricade>();
}
