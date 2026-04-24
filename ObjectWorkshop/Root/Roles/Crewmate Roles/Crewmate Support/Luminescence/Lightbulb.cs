using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Lightbulb(IntPtr ptr) : MonoBehaviour(ptr)
{
    public byte id;
    public PlayerControl Owner;
    public SpriteRenderer myRend;
    public GameObject aura;

    public static List<Lightbulb> AllLightbulbs = new List<Lightbulb>();

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();

        DestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Bounce(transform, 0.7f, 0.25f));

        AllLightbulbs.Add(this);

        Destroy(aura, OptionGroupSingleton<Luminescence_Options>.Instance.Duration);
        Destroy(gameObject, OptionGroupSingleton<Luminescence_Options>.Instance.Duration);
    }

    private void Update()
    {
        UpdateColor();
    }

    private bool Visibility() => true;
    private void UpdateColor()
    {
        if (Visibility())
        {
            myRend.Show();
            aura.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 0, 85);
        }
        else
        {
            myRend.Hide();
            aura.GetComponent<SpriteRenderer>().Hide();
        }
    }

    private void OnDestroy()
    {
        AllLightbulbs.Remove(this);
    }

    public bool IsInRange(PlayerControl target)
    {
        if (target == null || gameObject == null) return false;
        if (target.HasDied() || target.inVent || target.IsSpider() /*|| target.IsPeacock() || target.IsUnderground()*/) return false;

        float dist = Vector2.Distance(gameObject.transform.position, target.transform.position);
        return dist <= OptionGroupSingleton<Luminescence_Options>.Instance.Radius;
    }

    public static void Begin(PlayerControl player)
    {
        WitnessKill.WitnessGoodDeed(player);

        var bubble = new GameObject("LightbulbAura");
        bubble.AddSpriteRenderer(OWAssets.Bubble.LoadAsset(), 0, 100, player.transform.position, ObjectExtentions.noColor(), Vector3.one);
        bubble.transform.localScale = Vector3.one * OptionGroupSingleton<Luminescence_Options>.Instance.Radius * 2f;

        var gameObject = new GameObject("Lightbulb");
        gameObject.AddSpriteRenderer(OWAssets.Luminescence_Lightbulb.LoadAsset(), 0, 0, player.transform.position, ObjectExtentions.noColor(), Vector3.one);
        var lightbulb = gameObject.AddComponent<Lightbulb>();
        lightbulb.Owner = player;
        lightbulb.aura = bubble;

        bubble.transform.SetParent(gameObject.transform);

        bubble.AddOWObject();
        gameObject.AddOWObject();
    }

    public static void DestroyAll()
    {
        List<Lightbulb> current = AllLightbulbs;
        foreach (var clock in current) // need a new list so it doesnt throw an error for editing a list while in a loop.
        {
            AllLightbulbs.Remove(clock);
            Destroy(clock.aura);
            Destroy(clock.gameObject);
        }
    }

    public static IEnumerable<Lightbulb> GetAll()
    {
        // Return all currently active lightbulbs
        return AllLightbulbs;
    }

    // get all affecting player:
    // var bulbsAffectingPlayer = Lightbulb.GetAll().Where(b => b.IsInRange(pc)).ToList();
    public static bool IsPlayerInAnyRange(PlayerControl target)
    {
        if (target == null) return false;

        foreach (var bulb in AllLightbulbs)
        {
            if (bulb != null && bulb.IsInRange(target))
                return true;
        }

        return false;
    }


    public static void CleanUp()
    {
        DestroyAll();
        AllLightbulbs.Clear();
    }
}