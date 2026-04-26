using UnityEngine;
using Rewired.Utils;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Webs(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;
    public SpriteRenderer myRend;

    public bool isTriggered;
    private ArrowBehaviour? _arrow;

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();
        AllWebs.Add(this);
    }

    private bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility())
        {
            if (isTriggered) myRend.color = new Color(1f, 0.7f, 0.7f, 0.75f);
            else myRend.Show();
        }
        else myRend.Hide();

        if (_arrow != null && Debugger.IsDebuggerActive) // MCI patch.
        {
            var sr = _arrow.GetComponent<SpriteRenderer>();
            if (PlayerControl.LocalPlayer == Owner) sr.Show();
            else sr.Hide();
        }
    }

    private void Update()
    {
        UpdateColor();
        CheckWebTriggers();
    }

    private void OnDestroy()
    {
        AllWebs.Remove(this);
        if (_arrow != null) RemoveArrow();
    }

    public void AddArrow()
    {
        _arrow = MiscUtils.CreateArrow(transform, RoleColors.Infiltrator);
        _arrow.target = transform.position;
        _arrow.Update();
    }

    public void RemoveArrow()
    {
        _arrow?.gameObject.Destroy();
        _arrow?.Destroy();
    }

    public void CheckWebTriggers()
    {
        if (gameObject.IsNullOrDestroyed())
            return;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.HasDied()) // and not underground.
            {
                if (IsInWebs(player))
                {
                    var arachnid = Owner.GetRole<Arachnid>();
                    if (Owner.AmOwner() && player != Owner)
                    {
                        isTriggered = true;
                        arachnid.WebsToClear.Add(this);

                        if (_arrow == null)
                        {
                            Owner.Notify("A player has entered one of your Webs!", NotifyMode.Instantly);
                            AddArrow();
                            Coroutines.Start(MiscUtils.CoFlash(Color.yellow, 1f, 0.3f));
                        }
                    }
                }
            }
        }
    }

    public static void Begin(PlayerControl player, Vector3 pos)
    {
        WitnessKill.WitnessEvilDeed(player);

        GameObject gameObject = new GameObject("Web");
        gameObject.AddSpriteRenderer(OWAssets.Arachnid_Web.LoadAsset(), 0, 100, pos, ObjectExtentions.noColor(), Vector3.one);

        Webs web = gameObject.AddComponent<Webs>();
        web.Owner = player;

        gameObject.AddOWObject();
    }

    public static bool IsInWebs(PlayerControl player)
    {
        if (player.HasDied()) return false;

        foreach (var web in AllWebs)
        {
            float range = player.IsSpider() ? 1.1f : 0.7f;
            float dist = Vector2.Distance(player.GetTruePosition(), web.transform.position);
            if (dist < range) return true;
        }

        return false;
    }

    public static void CleanUp()
    {
        AllWebs.Clear();
    }

    public static List<Webs> AllWebs = new List<Webs>();
}