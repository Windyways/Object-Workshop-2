using UnityEngine;
using Rewired.Utils;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Crosshair(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;
    public SpriteRenderer myRend;
    public bool isChronoNullified;
    public PlayerControl? currentTarget;
    public Vector2 targetPos;
    public float moveSpeed = 5f;

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();
        AllCrosshairs.Add(this);
    }

    private bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();
    }

    private void Update()
    {
        UpdateColor();
        if (!this.IsNullOrDestroyed()) // might fix the error at line 41. (now 44)
        {
            gameObject.SetCameraToObject(Owner);

            /*if (Shield.IsInRange(gameObject) || isChronoNullified) For Chronoguard!!!
            {
                if (!isChronoNullified)
                {
                    Coroutines.Start(Shield.DestroyCrosshair(aimsman));
                }
                isChronoNullified = true;
                moveSpeed = 0f;
            }*/

            Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            targetPos += move * moveSpeed * Time.deltaTime;
            //gameObject.transform.position = Vector2.Lerp(gameObject.transform.position, targetPos, moveSpeed * Time.deltaTime);
            gameObject.transform.position = Vector2.Lerp(gameObject.transform.position, targetPos, 0.3f);

            Aimsman.RpcMoveCrosshair(Owner, targetPos.x, targetPos.y);
            currentTarget = FindTarget();
        }
    }

    public PlayerControl? FindTarget()
    {
        float killRadius = 0.5f;
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.HasDied()/* || player.IsInvincible() || player.IsUnderground()*/ && !ConditionalTargeting(player) && !player.Is(Faction.Infiltrator) &&
                player.IsTargetable())
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= killRadius) return player;
            }
        }

        return null;
    }

    public bool ConditionalTargeting(PlayerControl player)
    {
        return player.onLadder && OptionGroupSingleton<Aimsman_Options>.Instance.LadderImmunity;
    }

    public void TryShoot(Aimsman aimsman, PlayerControl? overrideTarget = null)
    {
        if (overrideTarget != null)
        {
            aimsman.Player.RpcCustomMurder(overrideTarget, true, true, true, false);
            VisitingMechanic.RpcAddDeathReason(overrideTarget, (int)DeathReasonShow.Shot);
            Aimsman.RpcDestroyCrosshair(Owner);
        }
        else if (currentTarget != null)
        {
            aimsman.Player.RpcCustomMurder(currentTarget, true, true, true, false);
            VisitingMechanic.RpcAddDeathReason(currentTarget, (int)DeathReasonShow.Shot);
            Aimsman.RpcDestroyCrosshair(Owner);
        }
    }

    public void DestroyGameObject()
    {
        if (Owner.GetTrueRole() is Aimsman aimsman)
        {
            if (Owner.AmOwner())
            {
                aimsman.isAiming = false;

                var button = CustomButtonSingleton<Aimsman_Aim>.Instance;
                button.EffectActive = false;
                button.Timer = OptionGroupSingleton<Aimsman_Options>.Instance.Cooldown;

                LightSource light = Owner.lightSource;
                light.transform.SetParent(Owner.transform);
                light.transform.localPosition = Owner.Collider.offset;
            }

            AllCrosshairs.Remove(this);
            Destroy(gameObject);
        }
    }

    public static void Begin(PlayerControl player)
    {
        WitnessKill.WitnessEvilDeed(player);

        //DestroyGameObject(GetObjectByPlayer(player));

        GameObject gameObject = new GameObject("Crosshair");
        gameObject.AddSpriteRenderer(OWAssets.Aimsman_Crosshair.LoadAsset(), 70, 100, player.GetAdjustedPosition(), ObjectExtentions.noColor(), Vector3.one);

        Crosshair crosshair = gameObject.AddComponent<Crosshair>();
        crosshair.Owner = player;
        crosshair.targetPos = player.GetAdjustedPosition();

        gameObject.AddOWObject();
    }

    public static void DestroyGameObject(Crosshair crosshair)
    {
        if (crosshair != null)
        {
            AllCrosshairs.Remove(crosshair);
            Destroy(crosshair.gameObject);
        }
    }

    public static void DestroyAll()
    {
        foreach (Crosshair crosshairs in AllCrosshairs)
        {
            Destroy(crosshairs.gameObject);
        }
        AllCrosshairs.Clear();
    }

    public static Crosshair GetObjectByPlayer(PlayerControl player)
    {
        foreach (Crosshair crosshair in AllCrosshairs)
        {
            if (crosshair.Owner == player) return crosshair;
        }
        return null;
    }

    public static void CleanUp()
    {
        AllCrosshairs.Clear();
    }

    public static List<Crosshair> AllCrosshairs = new List<Crosshair>();
}