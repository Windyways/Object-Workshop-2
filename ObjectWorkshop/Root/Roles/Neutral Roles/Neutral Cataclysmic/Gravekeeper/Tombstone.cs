using TownOfUs.Patches;
using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Tombstone(IntPtr ptr) : MonoBehaviour(ptr)
{
    public byte id;
    public SpriteRenderer myRend;
    public PlayerControl Owner;
    private ArrowBehaviour? _arrow;

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();
        id = GetAvailableId();

        // This fades it in maybe?
        DestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.ColorFade(myRend, ObjectExtentions.noColor(), ObjectExtentions.fullColor(), 0.5f));

        AllTombstones.Add(this);

        var owObj = gameObject.GetComponent<OWObject>();
        owObj.isTombstone = true;
    }

    private void OnDestroy()
    {
        AllTombstones.Remove(this);
        if (!_arrow.IsDestroyedOrNull()) RemoveArrow();
    }

    private void Update()
    {
        UpdateColor();
    }

    private static bool Visibility() => true;
    private void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();

        if (_arrow != null && Debugger.IsDebuggerActive) // MCI patch.
        {
            var sr = _arrow.GetComponent<SpriteRenderer>();
            if (PlayerControl.LocalPlayer == Owner) sr.Show();
            else sr.Hide();
        }
    }

    public void DigUpTombstone()
    {
        VentPatches.RpcSpawnVent(Owner, transform.position);
        var targets = PlayerControl.AllPlayerControls.ToArray().Where(
            x => !x.HasDied() && x != Owner && !x.HasModifier<SpiritTargetModifier>()
            ).ToList();

        if (targets.Count > 0)
        {
            var target = targets.Random();
            target.RpcAddModifier<SpiritTargetModifier>();

            Spirit.Begin(Owner, target);

            AllTombstones.Remove(this);
            Destroy(gameObject);
        }
    }

    public void AddArrow()
    {
        _arrow = MiscUtils.CreateArrow(transform, RoleColors.Gravekeeper);
        _arrow.target = transform.position;
        _arrow.Update();
    }

    public void RemoveArrow()
    {
        _arrow?.gameObject.Destroy();
        _arrow?.Destroy();
    }

    public static Tombstone Begin(PlayerControl player, Vector3 pos)
    {
        WitnessKill.WitnessEvilDeed(player);

        GameObject gameObject = new GameObject("Tombstone");
        gameObject.AddSpriteRenderer(OWAssets.Gravekeeper_Tombstone.LoadAsset(), 0, 100, pos, ObjectExtentions.noColor(), Vector3.one);

        Tombstone tombstone = gameObject.AddComponent<Tombstone>();
        tombstone.Owner = player;

        gameObject.AddOWObject(true);
        return tombstone;
        //Lockdown.AvailableObjects.Add(gameObject);
    }

    public static byte GetAvailableId()
    {
        byte id = 0;
        while (AllTombstones.Any(x => x.id == id))
        {
            id++;
        }
        return id;
    }


    public static void CleanUp()
    {
        AllTombstones.Clear();
    }

    public static List<Tombstone> AllTombstones = new List<Tombstone>();
}