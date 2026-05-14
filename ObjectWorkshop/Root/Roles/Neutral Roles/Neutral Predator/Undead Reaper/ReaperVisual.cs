using UnityEngine;
using Color = UnityEngine.Color;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class ReaperVisual(IntPtr ptr) : MonoBehaviour(ptr)
{
    public SpriteRenderer myRend;
    public PlayerControl Owner;
    public PlayerControl ReaperOwner;
    public byte id;
    private Vector3 basePos;

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();

        id = GetAvailableId();
        AllUndeadReapers.Add(this);

        Owner.RpcAddModifier<InvisibleTogglable>();
        basePos = transform.localPosition;
    }

    private void Update()
    {
        if (!UndeadReaper.ReapersAlive())
        {
            // For Peacock!
            var victim = Owner;
            var killer = Owner;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.HasDied())
                {
                    var role = player.GetRoleWhenAlive();
                    if (role is ICustomAURole cr)
                    {
                        cr.Role_AfterMurder(killer, victim);
                        cr.Role_OnDeath(victim);
                    }
                }
                else if (player.Data.Role is ICustomAURole customRole)
                {
                    customRole.Role_AfterMurder(killer, victim);
                    customRole.Role_OnDeath(victim);
                }
            }

            CleanUp();
            Owner.RpcRemoveModifier<InvisibleTogglable>();

            Destroy(gameObject);
        }
        else
        {
            if (myRend != null) myRend.flipX = Owner.cosmetics?.FlipX ?? false;
            UpdateColor();
            IdleFloat();
        }
    }

    private bool Visibility() => true;
    public void UpdateColor()
    {
        float alpha = 0.5f + Mathf.Sin(Time.time * 2f) * 0.25f;
        if (Visibility()) myRend.color = new Color(1f, 1f, 1f, alpha);
        else myRend.Hide();
    }

    public void IdleFloat()
    {
        float offset = Mathf.Sin(Time.time * 2f) * 0.05f;
        transform.localPosition = basePos + new Vector3(0, offset, 0);
    }

    public static void Begin(PlayerControl reaper, PlayerControl player)
    {
        var obj = new GameObject("ReaperVisual");
        obj.transform.SetParent(player.transform);
        obj.AddSpriteRenderer(OWAssets.UndeadReaper_Visual.LoadAsset(), 100, 100, player.transform.position, ObjectExtentions.noColor(), Vector3.one);
        
        var undeadreaper = obj.AddComponent<ReaperVisual>();
        undeadreaper.Owner = player;
        undeadreaper.ReaperOwner = reaper;

        obj.AddOWObject();
    }

    public static void DestroyAll()
    {
        foreach (var undeadreaper in AllUndeadReapers)
        {
            Destroy(undeadreaper.gameObject);
        }
        AllUndeadReapers.Clear();
    }

    public static void CleanUp()
    {
        AllUndeadReapers.Clear();
    }

    public static List<ReaperVisual> AllUndeadReapers = new List<ReaperVisual>();
    public static byte GetAvailableId()
    {
        byte id = 0;
        while (AllUndeadReapers.Any(x => x.id == id))
        {
            id++;
        }
        return id;
    }

    public static ReaperVisual GetObjectByOwner(PlayerControl player)
    {
        foreach (var undeadReaper in AllUndeadReapers)
        {
            if (undeadReaper.Owner == player) return undeadReaper;
        }
        return null;
    }
}