using System.Collections;
using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Flames : MonoBehaviour
{
    public PlayerControl PyreOwner;
    public PlayerControl PlayerOnFire;
    public SpriteRenderer myRend;

    public void Start()
    {
        OWPlugin.DebugLogMessage(PlayerOnFire.Data.PlayerName + " has been light on fire!");

        myRend = gameObject.GetComponent<SpriteRenderer>();
        AllFlames.Add(this);

        Coroutines.Start(Death());

        PlayerOnFire.RpcAddModifier<IgnitedModifier>();
    }

    public void Update()
    {
        UpdateColor();
        SpreadFire();
    }

    public IEnumerator Death()
    {
        yield return new WaitForSeconds(OptionGroupSingleton<Pyre_Options>.Instance.Duration);

        if (PyreOwner.AmOwner())
        {
            PyreOwner.RpcCustomMurder(PlayerOnFire, true, false, true, false);
            VisitingMechanic.RpcAddDeathReason(PlayerOnFire, (int)DeathReasonShow.Incinerated);

            PlayerOnFire.RpcRemoveModifier<IgnitedModifier>();
        }

        DestroyGameObject();
    }

    private bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();
    }

    public void DestroyGameObject()
    {
        if (this != null)
        {
            AllFlames.Remove(this);
            Destroy(gameObject);
        }
    }

    public void SpreadFire()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (IsInRange(player))
            {
                Begin(PyreOwner, player);
            }
        }
    }

    public bool IsInRange(PlayerControl target)
    {
        if (target == null || gameObject == null || PlayerOnFire.HasDied()) return false;
        if (target.HasDied()/* || target.IsUnderground()*/) return false;
        if (target == PlayerOnFire || target.inVent) return false;
        if (target.IsRole<Pyre>() || target.HasModifier<IgnitedModifier>()) return false;
        
        float dist = Vector2.Distance(gameObject.transform.position, target.transform.position);
        return dist <= 0.5f;
    }

    public static void Begin(PlayerControl player, PlayerControl target)
    {
        GameObject obj = new GameObject("FireEffect");
        obj.transform.SetParent(target.transform);
        obj.AddSpriteRenderer(OWAssets.Pyre_FireSprite.LoadAsset(), 0, 100, 
            new Vector3(target.transform.position.x, target.transform.position.y + 0.4f, target.transform.position.z), ObjectExtentions.noColor(), Vector3.one);

        Flames flames = obj.AddComponent<Flames>();
        flames.PyreOwner = player;
        flames.PlayerOnFire = target;

        obj.AddOWObject();
    }

    public static void CleanUp()
    {
        AllFlames.Clear();
    }

    public static List<Flames> AllFlames = new List<Flames>();
}