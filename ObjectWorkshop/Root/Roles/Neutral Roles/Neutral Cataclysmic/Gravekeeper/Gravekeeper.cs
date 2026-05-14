using System.Collections;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Roles;

public sealed class Gravekeeper(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Gravekeeper";
    public string RoleDescription => "Install Tombstones, bring out Spirits.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Gravekeeper;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Neutral;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.NeutralCataclysmic;

    public string RevealText => "lifts spirits from the underworld.";
    public string Description => $"Install Tombstones on dead bodies. Dig them Up to spawn spirits once there are an equal amount of Tombstones to alive players.";
    public string Intro => "You are a tomb warden that uses knowledge of spirituality to kill everyone.";
    public string VictoryCondition => $"Install tombs to lift the spirits. Kill everyone. You will win alone.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = OWAssets.Gravekeeper,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"{Intro}\n\n{Description}\n\n" +
            $"<color=#00ff00>Victory Condition:</color>\n" +
            $"{VictoryCondition}" +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Tombstone",
            "You can install a Tombstone on a dead body during the round.\n" +
            "You will destroy the dead body and replace it with a Tombstone, visible to everyone.\n" +
            "Once there are an equal amount of Tombstones to alive players (excluding yourself), the Gravekeeper Season will begin, and you gain an arrow to all Tombstones.",
            OWAssets.KillSprite),
            
        new("Dig Up",
            "You can Dig Up a spirit from a Tombstone during the round.\n" +
            "You will destroy the Tombstone, spawning a spirit and a vent, which will make its way slowly to a random player. The spirit will get faster the longer the target is alive.\n" +
            "Once the spirit collides with its target, it will kill them and dispose of their body. Afterwards it will fade away.\n" +
            "Spirits will pursue their targets even during meetings.\n" +
            "Vents created by tombstones can only be used by you. This ability is only usable during Gravekeeper Season.",
            OWAssets.KillSprite)
    ];

    public string GetPassives()
    {
        return "- You have a Shield to protect you from the first attack you receive. The Shield is removed upon starting the Gravekeeper Season.";
    }

    [MethodRpc((uint)Rpcs.RpcTombstone)]
    public static void RpcTombstone(PlayerControl player, DeadBody target)
    {
        if (player.Data.Role is not Gravekeeper)
        {
            Logger<OWPlugin>.Error("RpcTombstone - Invalid Gravekeeper");
            return;
        }

        //var gravekeeper = player.GetRole<Gravekeeper>();

        Coroutines.Start(target.CoClean(0.25f));
        Tombstone.Begin(player, target.transform.position);
    }

    [MethodRpc((uint)Rpcs.RpcDigUp)]
    public static void RpcDigUp(PlayerControl player, byte target)
    {
        if (player.Data.Role is not Gravekeeper)
        {
            Logger<OWPlugin>.Error("RpcDigUp - Invalid Gravekeeper");
            return;
        }

        var obj = OWObject.GetById(target);
        var tombstone = obj.gameObject.GetComponent<Tombstone>();
        tombstone.DigUpTombstone();
    }

    public bool WinConditionMet()
    {
        var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied());
        if (Player.HasDied() || currentPhase == Phase.Tombstone) return false;

        return alivePlayers == 1;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (player.AmOwner() && OptionGroupSingleton<Gravekeeper_Options>.Instance.EnableShield) player.RpcAddModifier<Shielded>();
    }

    public void LobbyStart()
    {
        Tombstone.CleanUp();
        Spirit.CleanUp();
    }

    public void TryStartSeason()
    {
        if (Tombstone.AllTombstones.Count > 0)
        {
            var allPlayers = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x != Player);
            if (Graves >= allPlayers && currentPhase == Phase.Tombstone)
            {
                currentPhase = Phase.DigUp;
                Player.RpcAddModifier<Shielded>();
                foreach (var tombstone in Tombstone.AllTombstones)
                {
                    if (Player.AmOwner())
                    {
                        tombstone.AddArrow();
                    }
                }

                HuntMechanic.BeginHunt(this);
            }
        }
    }

    private IEnumerator CoCreateArrow(PlayerControl target)
    {
        yield return new WaitForSeconds(0.1f);

        var deadBody = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == target.PlayerId);

        if (deadBody == null)
        {
            yield break;
        }

        if (Player.AmOwner())
        {
            Player.AddModifier<BodyArrow>(deadBody, RoleColors.Gravekeeper);
        }
    }

    public void Role_AfterMurder(PlayerControl killer, PlayerControl victim)
    {
        if (Player.AmOwner())
        {
            TryStartSeason();
        }

        Coroutines.Start(CoCreateArrow(victim));
    }

    public bool GravekeeperSeason() =>
        Graves >= PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x != Player);

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            var bodies = Object.FindObjectsOfType<DeadBody>();
            foreach (var body in bodies)
            {
                if (body.ParentId == target.PlayerId)
                {
                    RpcTombstone(Player, body);
                    break;
                }
            }

            TryStartSeason();
        }
    }

    public int Graves => Tombstone.AllTombstones.Count(x => x.Owner == Player);
    public enum Phase { Tombstone, DigUp }
    public Phase currentPhase = Phase.Tombstone;
    public List<Vent> GravekeeperVents = new List<Vent>();
}

public sealed class Gravekeeper_Tombstone : ObjectWorkshopRoleButton<Gravekeeper, DeadBody>
{
    public override string Name => "Tombstone";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Gravekeeper;
    public override float Cooldown => OptionGroupSingleton<Gravekeeper_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (KeybindIcon != null) KeybindIcon.transform.localPosition = new Vector3(0.4f, 0.45f, -9f);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (playerControl == Player)
        {
            Button?.usesRemainingText.gameObject.SetActive(true);
            Button?.usesRemainingSprite.gameObject.SetActive(true);
            Button!.usesRemainingText.text = Role.Graves + "/" + PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x != Player);
        }

        base.FixedUpdate(playerControl);
    }

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    }

    protected override void OnClick()
    {
        var targetId = Target.ParentId;
        var targetPlayer = MiscUtils.PlayerById(targetId);

        if (targetPlayer == null)
        {
            OWPlugin.DebugLogMessage("Gravekeeper target could not be found!", OWPlugin.MsgType.Error);
            return; 
        }

        VisitingMechanic.CheckVisit(Player, targetPlayer, 1, false, true);
    }

    public override bool CanUse()
    {
        return base.CanUse() && Role.currentPhase == Gravekeeper.Phase.Tombstone;
    }
}

public sealed class Gravekeeper_DigUp : ObjectWorkshopRoleButton<Gravekeeper, OWObject>
{
    public override string Name => "Dig Up";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Gravekeeper;
    public override float Cooldown => OptionGroupSingleton<Gravekeeper_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (KeybindIcon != null) KeybindIcon.transform.localPosition = new Vector3(0.4f, 0.45f, -9f);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (playerControl == Player)
        {
            Button?.usesRemainingText.gameObject.SetActive(true);
            Button?.usesRemainingSprite.gameObject.SetActive(true);
            Button!.usesRemainingText.text = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x != Player) + "";
        }

        base.FixedUpdate(playerControl);
    }

    protected override void OnClick()
    {
        if (Target == null)
            return;

        Gravekeeper.RpcDigUp(Player, Target.id);
    }

    public override OWObject? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestObject();
    }

    public override bool CanUse()
    {
        return base.CanUse() && Role.currentPhase == Gravekeeper.Phase.DigUp;
    }
}

public sealed class Gravekeeper_Vent : ObjectWorkshopRoleButton<Gravekeeper, Vent>
{
    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.Usables);
    public override string Name => "Vent";
    public override BaseKeybind Keybind => Keybinds.VentAction;
    public override Color TextOutlineColor => RoleColors.Gravekeeper;
    public override float Cooldown => 0.1f;
    public override LoadableAsset<Sprite> Sprite => OWAssets.VentSprite;

    public override Vent? GetTarget()
    {
        var vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 4, Filter);
        if (vent == null) vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 3, Filter);
        if (vent == null) vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 2, Filter);
        if (vent == null) vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance, Filter);
        if (vent != null && PlayerControl.LocalPlayer.CanUseVent(vent)) return vent;
        return null;
    }

    public override bool CanUse()
    {
        var newTarget = GetTarget();
        if (newTarget != Target) Target?.SetOutline(false, false);

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance) return false;

        return Role.currentPhase == Gravekeeper.Phase.DigUp &&
            ((Timer <= 0 && !PlayerControl.LocalPlayer.inVent && Target != null && Role.GravekeeperVents.Contains(Target)) || 
            PlayerControl.LocalPlayer.inVent);
    }

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();

        Timer = !PlayerControl.LocalPlayer.inVent ? 0.001f : Cooldown;
    }

    protected override void OnClick()
    {
        if (!PlayerControl.LocalPlayer.inVent)
        {
            if (Target != null)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(Target.Id);
                Target.SetButtons(true);
            }
        }
        else if (Timer != 0)
        {
            if (Player.inVent)
            {
                Vent.currentVent.SetButtons(false);
                Player.MyPhysics.RpcExitVent(Vent.currentVent.Id);
            }
        }
    }
}

public sealed class Gravekeeper_Options : AbstractOptionGroup<Gravekeeper>
{
    public override string GroupName => "Gravekeeper";

    [ModdedNumberOption("<color=#4873ff>Gravekeeper</color> <color=#4a86e8>Tombstone</color> Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25;

    [ModdedToggleOption("Enable <color=#4873ff>Gravekeeper</color> <color=#0000ff>Shield</color>")]
    public bool EnableShield { get; set; } = true;
}