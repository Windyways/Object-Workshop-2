using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Sanctuary(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;
    public SpriteRenderer myRend;

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();

        AllSanctuarys.Add(this);
        Destroy(gameObject, OptionGroupSingleton<Oasis_Options>.Instance.Duration);
    }

    private void Update()
    {
        UpdateColor();
    }

    private void OnDestroy()
    {
        AllSanctuarys.Remove(this);
    }

    private bool Visibility() => Owner == PlayerControl.LocalPlayer;
    public void UpdateColor()
    {
        if (Visibility()) myRend.color = new Color(1f, 1f, 0f, 0.3f);
        else myRend.Hide();
    }

    public bool IsInRange(PlayerControl target)
    {
        if (target == null || gameObject == null) return false;
        if (target.HasDied() || target.inVent || target.IsSpider() || target.IsPeacock() /*|| target.IsUnderground()*/) return false;
        
        float dist = Vector2.Distance(gameObject.transform.position, target.transform.position);
        return dist <= OptionGroupSingleton<Oasis_Options>.Instance.Radius;
    }

    public static void Begin(PlayerControl player)
    {
        GameObject gameObject = new GameObject("Sanctuary");
        gameObject.AddSpriteRenderer(OWAssets.Bubble.LoadAsset(), 30, 100, player.transform.position, ObjectExtentions.One3rdColor(), Vector3.one * OptionGroupSingleton<Oasis_Options>.Instance.Radius);

        Sanctuary sanctuary = gameObject.AddComponent<Sanctuary>();
        sanctuary.Owner = player;
    }

    public static void DestroyAll()
    {
        foreach (Sanctuary sanctuarys in AllSanctuarys)
        {
            Destroy(sanctuarys.gameObject);
        }
        AllSanctuarys.Clear();
    }

    public static Sanctuary? GetObjectByPlayer(PlayerControl player)
    {
        foreach (Sanctuary sanctuary in AllSanctuarys)
        {
            if (sanctuary.Owner == player) return sanctuary;
        }
        return null;
    }

    public static IEnumerable<Sanctuary> GetAll()
    {
        return AllSanctuarys;
    }

    public static bool IsPlayerInAnyRange(PlayerControl? target, out PlayerControl? Owner)
    {
        Owner = null;
        if (target == null) return false;

        foreach (var obj in AllSanctuarys.Where(s => s != null))
        {
            if (obj.IsInRange(target))
            {
                Owner = obj.Owner;
                return true;
            }
        }

        return false;
    }

    public static void CleanUp()
    {
        AllSanctuarys.Clear();
    }

    public static List<Sanctuary> AllSanctuarys = new List<Sanctuary>();
}
