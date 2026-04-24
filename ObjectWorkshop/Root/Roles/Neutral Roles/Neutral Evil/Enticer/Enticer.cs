using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Roles;

public sealed class Enticer(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Enticer";
    public string RoleDescription => "Hook and devour prey.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Enticer;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Neutral;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.NeutralEvil;

    public string RevealText => "waits and lures prey.";
    public string Description => $"Vent and Prepare your hook to drag nearby victims into your vent and devour them. Devour {OptionGroupSingleton<Enticer_Options>.Instance.Required} players to win.";
    public string Intro => "You are a seductive siren that hooks in and devours prey by surprise.";
    public string VictoryCondition => $"Lure and devour {OptionGroupSingleton<Enticer_Options>.Instance.Required} players. You will win alone.";

    public override float Vision() => OptionGroupSingleton<Enticer_Options>.Instance.Vision;
    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        //Icon = OWAssets.EnticerRoleCard,
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
        new("Prepare",
            "You can Prepare while in a vent during the round.\n" +
            "While Prepared, you will attach a hook onto the closest player to the vent, and begin to pull them towards it. Hooked players will have their vision lowered the closer they are.\n" +
            "If your target gets out of range, they will break free of the hook. You may only hook in one player at a time.\n" +
            "Once the target is on top of the vent, you will devour them, leaving no trace of the body.\n" +
            "Using this ability again disables your Prepared state.\n" +
            "Once you are in a vent, you cannot exit it until a meeting begins. You are Abduct immune while in a vent.",
            OWAssets.KillSprite),
    ];

    [MethodRpc((uint)Rpcs.RpcPrepare)]
    public static void RpcPrepare(PlayerControl player)
    {
        if (player.Data.Role is not Enticer)
        {
            Logger<OWPlugin>.Error("RpcPrepare - Invalid Enticer");
            return;
        }

        var enticer = player.GetRole<Enticer>();
        enticer.isPrepared = !enticer.isPrepared;

        if (enticer.isPrepared)
        {
            if (player.AmOwner()) HookCircle.Begin(player);
        }
        else
        {
            var hookCircle = HookCircle.GetObjectByOwner(player);
            Object.Destroy(hookCircle.gameObject);

            var hook = EnticerHook.GetObjectByOwner(player);
            if (hook != null) hook.Destroy();
        }
    }

    public bool WinConditionMet()
    {
        if (Player.HasDied()) return false;

        var result = DevourCount >= (int)OptionGroupSingleton<Enticer_Options>.Instance.Required;
        return result;
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

    public void LobbyStart()
    {
        EnticerHook.CleanUp();
        HookCircle.CleanUp();
    }

    public void FixedUpdate()
    {
        if (!OWPlugin.InGame())
            return;

        if (Player == null || Player.HasDied())
            return;

        HookIn();
    }

    public void HookIn()
    {
        if (isPrepared && Player.inVent)
        {
            EnticerHook.Begin(Player); // Rpc here.
        }
    }

    public void Role_OnMeetingStart()
    {
        if (isPrepared)
        {   
            var hookCircle = HookCircle.GetObjectByOwner(Player);
            Object.Destroy(hookCircle.gameObject);

            var hook = EnticerHook.GetObjectByOwner(Player);
            if (hook != null) hook.Destroy();
        }
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1) RpcPrepare(Player);
    }

    public int DevourCount;
    public bool isPrepared;
}

public sealed class Enticer_Prepare : ObjectWorkshopRoleButton<Enticer>
{
    public override string Name => "Prepare";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Enticer;
    public override float Cooldown => 1f;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
    public override bool CanUse()
    {
        return Player.inVent;
    }
}

public sealed class Enticer_Vent : ObjectWorkshopRoleButton<Enticer, Vent>
{
    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.Usables);
    public override string Name => "Vent";
    public override BaseKeybind Keybind => Keybinds.VentAction;
    public override Color TextOutlineColor => RoleColors.Enticer;
    public override float Cooldown => 0.1f;
    public override LoadableAsset<Sprite> Sprite => TouAssets.VentSprite;

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

        return !Player.inVent &&
            ((Timer <= 0 && !PlayerControl.LocalPlayer.inVent && Target != null) || 
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

public sealed class Enticer_Options : AbstractOptionGroup<Enticer>
{
    public override string GroupName => "Enticer";

    [ModdedNumberOption("<color=#4ee6be>Enticer</color> <color=#4a86e8>Hook</color> Radius", 0.1f, 5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float Radius { get; set; } = 2.5f;

    [ModdedNumberOption("<color=#4ee6be>Enticer</color> <color=#4a86e8>Devours</color> Required", 1f, 10f, 1f, MiraNumberSuffixes.None)]
    public float Required { get; set; } = 3f;

    [ModdedNumberOption("<color=#4ee6be>Enticer</color> Pull Speed", 0.25f, 5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float PullSpeed { get; set; } = 1f;

    [ModdedNumberOption("<color=#4ee6be>Enticer</color> Vision", 0.25f, 60f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float Vision { get; set; } = 0.5f;
}