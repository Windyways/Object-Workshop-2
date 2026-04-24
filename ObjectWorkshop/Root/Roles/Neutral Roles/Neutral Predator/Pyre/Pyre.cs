using System.Text;
using UnityEngine;

namespace ObjectWorkshop.Roles;

public sealed class Pyre(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Pyre";
    public string RoleDescription => "Spread fire to kill them all!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Pyre;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Neutral;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.NeutralPredator;

    public string RevealText => "is a master at spreading fire. ";
    public string Description => $"Ignite players on fire and have them spread it to nearby players.";
    public string Intro => "You are an unknown welder that enjoys using their profession for violence.";
    public string VictoryCondition => $"Burn everyone in flames, well just all that opposes you. You will win with other Pyres.";

    public override float Vision() => OptionGroupSingleton<Pyre_Options>.Instance.Vision;
    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        CanUseVent = OptionGroupSingleton<Pyre_Options>.Instance.CanVent,
        //Icon = OWAssets.PyreRoleCard,
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
        new("Ignite",
            "You can Ignite a player with fire during the round.\n" +
            "After a delay, your target will go on fire, appearing to be on fire to other players.\n" +
            "After a certain amount of time, your target will die. Any players the target gets too close to will get caught on fire immediately.",
            OWAssets.KillSprite)
    ];

    [MethodRpc((uint)Rpcs.RpcIgnite)]
    public static void RpcIgnite(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not Pyre)
        {
            Logger<OWPlugin>.Error("RpcIgnite - Invalid pyre");
            return;
        }

        var pyre = player.GetRole<Pyre>();
		if (pyre != null) Flames.Begin(pyre.Player, target);
    }

    public bool WinConditionMet()
    {
        var aliveTeammates = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x.IsRole<Pyre>());
        var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied());
        if (aliveTeammates == 0) return false;

        var result = alivePlayers == 1;
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
        Flames.CleanUp();
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1) ignitedPlayer = target;
    }

    public PlayerControl ignitedPlayer;
}

public sealed class Pyre_Ignite : ObjectWorkshopRoleButton<Pyre, PlayerControl>
{
    public override string Name => "Ignite";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Pyre;
    public override float Cooldown => OptionGroupSingleton<Pyre_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override float EffectDuration => OptionGroupSingleton<Pyre_Options>.Instance.Delay;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: x => x.IsTargetable() && !x.HasModifier<IgnitedModifier>());
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
    public override void OnEffectEnd()
    {
        if (Role.ignitedPlayer == null)
            return;

        Pyre.RpcIgnite(Player, Role.ignitedPlayer);
    }
}

public sealed class Pyre_Options : AbstractOptionGroup<Pyre>
{
    public override string GroupName => "Pyre";

    [ModdedNumberOption("<color=#ff3771>Pyre</color> <color=#4a86e8>Ignite</color> Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25;

    [ModdedNumberOption("<color=#ff3771>Pyre</color> <color=#4a86e8>Ignite</color> Duration", 0.5f, 60f, 0.1f, MiraNumberSuffixes.Seconds, "0.0")]
    public float Duration { get; set; } = 5f;

    [ModdedNumberOption("<color=#ff3771>Pyre</color> <color=#4a86e8>Ignite</color> Delay", 0f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float Delay { get; set; } = 3f;

    [ModdedNumberOption("<color=#ff3771>Pyre</color> Vision", 0.25f, 60f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float Vision { get; set; } = 1f;

    [ModdedToggleOption("<color=#ff3771>Pyre</color> Can Vent")]
    public bool CanVent { get; set; } = true;
}