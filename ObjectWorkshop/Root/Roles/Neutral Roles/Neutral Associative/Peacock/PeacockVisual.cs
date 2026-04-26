using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class PeacockVisual(IntPtr ptr) : MonoBehaviour(ptr)
{
    public byte id;
    public PlayerControl Owner;
    public PlayerControl associate;
    public List<GameObject> feathers = new List<GameObject>();
    public List<GameObject> rightLookingFeathers = new List<GameObject>();
    public List<GameObject> leftLookingFeathers = new List<GameObject>();

    public static List<PeacockVisual> AllPeacockVisuals = new List<PeacockVisual>();
    public List<PlayerControl> ParalyzedPlayers = new List<PlayerControl>();

    public void Start()
    {
        //Lockdown.AvailableObjects.Add(gameObject);
        AllPeacockVisuals.Add(this);
        CreateFeathers();
    }

    private void OnDestroy()
    {
        AllPeacockVisuals.Remove(this);
    }

    private void Update()
    {
        UpdateColor();
        //if (myRend != null) myRend.flipX = Owner.cosmetics?.FlipX ?? false;

        if (Owner.HasDied())
        {
            ForceMobilePlayers();
            Destroy(gameObject);
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (SeesPeacock(Owner, player, Owner.transform.position))
            {
                if (!ParalyzedPlayers.Contains(player)) ImmobilizePlayers(player);
            }
            else MobilizePlayers(player);
        }
    }

    private bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility())
        {
            bool playerLookingLeft = Owner.cosmetics?.FlipX ?? false;
            foreach (var feather in feathers)
            {
                var myRend = feather.GetComponent<SpriteRenderer>();
                myRend.Show();
            }
            foreach (var rfeather in rightLookingFeathers)
            {
                var myRend = rfeather.GetComponent<SpriteRenderer>();
                if (playerLookingLeft) myRend.Hide();
            }
            foreach (var lfeather in leftLookingFeathers)
            {
                var myRend = lfeather.GetComponent<SpriteRenderer>();
                if (!playerLookingLeft) myRend.Hide();
            }
        }
        else
        {
            foreach (var feather in feathers)
            {
                var myRend = feather.GetComponent<SpriteRenderer>();
                myRend.Hide();
            }
        }
    }

    public void ImmobilizePlayers(PlayerControl player)
    {
        if (player == associate)
            return;

        //if (player.IsRole<UndeadReaper>() && !UndeadReaper.ReapersAlive())
        //    return;

        player.Immobilize();
        if (!ParalyzedPlayers.Contains(player))
        {
            ParalyzedPlayers.Add(player);
            player.RpcAddModifier<Anonymous>();
        }

        var palePlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && ParalyzedPlayers.Contains(x)).ToList();
        if (palePlayers.Count == 3)
        {
            MobilizePlayers(palePlayers[0], true);
            MobilizePlayers(palePlayers[1], true);
        }

        if (associate.AmOwner() && Owner.Data.Role is Peacock)
        {
            associate.Notify(Peacock_Feedback.Immobilize(), NotifyMode.InstantlyAndMeeting);
            Coroutines.Start(MiscUtils.CoFlash(RoleColors.Peacock, 1f, 0.3f));
        }
    }

    public void MobilizePlayers(PlayerControl player, bool grownConfidence = false, bool forced = false)
    {
        if (!ParalyzedPlayers.Contains(player))
            return;

        player.Mobilize();
        player.RpcRemoveModifier<Anonymous>();
        player?.ResetAppearance(); // Might patch mobilized players (past paralyzed) being gray.

        if (!forced) ParalyzedPlayers.Remove(player);

        if (grownConfidence)
        {
            player.RpcAddModifier<PaleImmunityModifier>();
        }
    }

    public void ForceMobilePlayers()
    {
        foreach (var player in ParalyzedPlayers)
        {
            MobilizePlayers(player, forced: true);
        }

        ParalyzedPlayers.Clear();
    }

    public void CreateFeathers() // Bruv this took like an hour to make:sob:
    {
        for (int i = 0; i < 7; i++) // 7 feathers.
        {
            var obj = new GameObject($"Feather {i + 1}");
            obj.transform.SetParent(gameObject.transform);
            obj.AddSpriteRenderer(OWAssets.Peacock_Feather.LoadAsset(), 0, 100, gameObject.transform.position, ObjectExtentions.noColor(), Vector3.one);
            feathers.Add(obj);

            obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Now we position each here.
            if (i == 0)
            {
                obj.transform.localPosition = new Vector2(-9.0945f, -2.1418f);
                obj.transform.localRotation = Quaternion.Euler(0f, 0f, 16.4366f);
            }
            if (i == 1)
            {
                obj.transform.localPosition = new Vector2(-8.9473f, -2.2328f);
                obj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            if (i == 2)
            {
                obj.transform.localPosition = new Vector2(-9.2215f, -2.2255f);
                obj.transform.localRotation = Quaternion.Euler(0f, 0f, 50f);
            }
            if (i == 3)
            {
                obj.transform.localPosition = new Vector2(-8.8818f, -2.3927f);
                obj.transform.localRotation = Quaternion.Euler(0f, 0f, 330f);
                leftLookingFeathers.Add(obj);
            }
            if (i == 4)
            {
                obj.transform.localPosition = new Vector2(-8.8509f, -2.5255f);
                obj.transform.localRotation = Quaternion.Euler(0f, 0f, 310f);
                leftLookingFeathers.Add(obj);
            }
            if (i == 5)
            {
                obj.transform.localPosition = new Vector2(-9.3233f, -2.3927f);
                obj.transform.localRotation = Quaternion.Euler(0f, 0f, 50f);
                rightLookingFeathers.Add(obj);
            }
            if (i == 6)
            {
                obj.transform.localPosition = new Vector2(-9.3265f, -2.5255f);
                obj.transform.localRotation = Quaternion.Euler(0f, 0f, 94.7003f);
                rightLookingFeathers.Add(obj);
            }
        }
    }

    public static void Begin(Peacock peacock)
    {
        var player = peacock.Player;
        WitnessKill.WitnessEvilDeed(player);

        var obj = new GameObject("Peacock Handler");
        obj.transform.SetParent(player.transform);
        obj.transform.localPosition = new Vector2(12.9745f, 3.4378f);

        var peacockVisual = obj.AddComponent<PeacockVisual>();
        peacockVisual.Owner = player;
        peacockVisual.associate = player.GetAssociate();
    }

    public static bool IsPlayerAnyParalyzed(PlayerControl target)
    {
        if (target == null) return false;

        foreach (var obj in AllPeacockVisuals)
        {
            if (obj != null && obj.ParalyzedPlayers.Contains(target))
                return true;
        }

        return false;
    }

    public static void DestroyAll()
    {
        foreach (var clock in AllPeacockVisuals)
        {
            AllPeacockVisuals.Remove(clock);
            Destroy(clock.gameObject);
        }
    }

    public static PeacockVisual? GetObjectByOwner(PlayerControl player)
    {
        foreach (var peacockVisual in AllPeacockVisuals)
        {
            if (peacockVisual.Owner == player) return peacockVisual;
        }
        return null;
    }

    public static void CleanUp()
    {
        AllPeacockVisuals.Clear();
    }

    public bool SeesPeacock(PlayerControl peacock, PlayerControl target, Vector3 killPos)
    {
        if (MeetingHud.Instance) return false;
        if (target.HasDied()) return false;
        if (target.IsPeacock()) return false;
        if (target.inVent || peacock.inVent/* || target.IsUnderground()*/) return false;
        if (target.HasModifier<PaleImmunityModifier>()) return false;
        //if (target.HasModifier<PaleModifier>()) return false;

        return WitnessKill.BotCanSee(peacock, target, killPos, true);
    }
}