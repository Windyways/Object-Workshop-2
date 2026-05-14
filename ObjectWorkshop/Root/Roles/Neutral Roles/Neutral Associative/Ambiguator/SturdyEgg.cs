using System.Collections;
using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class SturdyEgg(IntPtr ptr) : MonoBehaviour(ptr)
{
    public byte id;
    public PlayerControl Owner;
    public PlayerControl victim;

    public bool JumpAnimationPlaying;
    public SpriteRenderer myRend;
    public static List<SturdyEgg> AllSturdyEggs = new List<SturdyEgg>();

    public void Start()
    {
        //Lockdown.AvailableObjects.Add(gameObject);
        myRend = GetComponent<SpriteRenderer>();
        AllSturdyEggs.Add(this);
        Coroutines.Start(EncapsulateDeathTimer());
    }

    private void OnDestroy()
    {
        AllSturdyEggs.Remove(this);
    }

    private void Update()
    {
        UpdateColor();

        if (victim.AmOwner && Input.anyKeyDown && !JumpAnimationPlaying)
        {
            RpcPlayJumpAnim(Owner);
        }
    }

    private IEnumerator EncapsulateDeathTimer()
    {
        yield return new WaitForSeconds(OptionGroupSingleton<Ambiguator_Options>.Instance.KillTimer);
        if (MeetingHud.Instance)
            yield break;

        SetOff();
    }

    public void SetOff()
    {
        Owner.RpcCustomMurder(victim, teleportMurderer: false);
        VisitingMechanic.RpcAddDeathReason(victim, (int)DeathReasonShow.Crushed);

        Destroy(gameObject);
    }

    [MethodRpc((uint)Rpcs.RpcPlayJumpAnim)]
    public static void RpcPlayJumpAnim(PlayerControl owner)
    {
        var sturdyEgg = GetObjectByPlayer(owner);
        if (sturdyEgg == null)
            return;

        Coroutines.Start(sturdyEgg.PlayJumpAnim());
    }

    public IEnumerator PlayJumpAnim()
    {
        var pos = transform.position;

        JumpAnimationPlaying = true;

        float t = 0;
        while (t < 0.5f)
        {
            transform.position = Vector2.Lerp(pos, new Vector2(pos.x, pos.y + 0.4f), 0.5f);
            t += Time.deltaTime;
        }

        yield return new WaitForSeconds(0.3f);

        t = 0;
        while (t < 0.5f)
        {
            transform.position = Vector2.Lerp(transform.position, pos, 0.5f);
            t += Time.deltaTime;
        }
        yield return new WaitForSeconds(0.3f);

        JumpAnimationPlaying = false;
    }

    private static bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();
    }

    public static void Begin(PlayerControl player, PlayerControl target)
    {
        WitnessKill.WitnessEvilDeed(player);

        var obj = new GameObject("Sturdy Egg");
        obj.AddSpriteRenderer(OWAssets.Ambiguator_SturdyEgg.LoadAsset(), 0, 100, target.GetAdjustedPosition(), ObjectExtentions.noColor(), Vector3.one);
        obj.transform.SetParent(target.transform);

        var sturdyEgg = obj.AddComponent<SturdyEgg>();
        sturdyEgg.Owner = player;
        sturdyEgg.victim = target;
    }

    public static bool IsWithinEgg(PlayerControl target)
    {
        if (target == null) return false;

        foreach (var obj in AllSturdyEggs)
        {
            if (obj != null && obj.victim == target)
                return true;
        }

        return false;
    }

    public static void CleanUp()
    {
        AllSturdyEggs.Clear();
    }

    public static SturdyEgg? GetObjectByPlayer(PlayerControl player)
    {
        foreach (var sturdyEgg in AllSturdyEggs)
        {
            if (sturdyEgg.Owner == player) return sturdyEgg;
        }
        return null;
    }
}