using System.Text;
using UnityEngine;

namespace ObjectWorkshop.Roles;

public sealed class Reaper(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Reaper";
    public string RoleDescription => "Bring all of them down with you.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Reaper;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Neutral;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.NeutralPredator;

    public string RevealText => "wants to kill everyone with their partner.";
    public string Description => $"Enable Catastrophe to convert a victim into an Undead Reaper. Kill everyone to win.";
    public string Intro => "You are a grim reaper that wants to cause catastrophe to all with their companion.";
    public string VictoryCondition => $"Kill everyone that opposes you. Cause death upon those with your partner. You will win with other Undead Reapers and Reapers.";

    public override float Vision() => OptionGroupSingleton<Reaper_Options>.Instance.Vision;
    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        CanUseVent = OptionGroupSingleton<Reaper_Options>.Instance.CanVent,
        Icon = OWAssets.Reaper,
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
        new("Attack",
            "You can Attack a player during the round.\n" +
            "You will kill your target.",
            OWAssets.KillSprite),
            
        new("Catastrophe",
            "You can cause a Catastrophe during the round.\n" +
            "The next player you kill will become the Undead Reaper with a special ability to kill while dead, although their speed will be halved.\n" +
            "All players can see the Undead Reaper, and they will win with you.",
            OWAssets.KillSprite)
    ];

    public string GetPassives()
    {
        return "- You have a Shield to protect you from the first attack you receive. The Shield is removed upon summoning an Undead Reaper.";
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (player.AmOwner() && OptionGroupSingleton<Reaper_Options>.Instance.EnableShield) player.RpcAddModifier<Shielded>();
    }

    [MethodRpc((uint)Rpcs.RpcReaperAttack)]
    public static void RpcReaperAttack(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not Reaper)
        {
            Logger<OWPlugin>.Error("RpcReaperAttack - Invalid Reaper");
            return;
        }

        if (player.Data.Role is Reaper reaper && reaper.causingCatastrophe)
        {
            player.RpcRemoveModifier<Shielded>();

            target.RpcAddModifier<GlobalReveal>();
            player.RpcAddModifier<RoleLearn>(target, false);

            MiscUtils.ChangeRoleObjectsPatch(target);

            // Change role here.
            target.RpcChangeRole(RoleId.Get<UndeadReaper>());
            ReaperVisual.Begin(reaper.Player, target);

            if (player.AmOwner())
            {
                var button = CustomButtonSingleton<Reaper_Catastrophe>.Instance;
                button.DecreaseUses();
                button.OverrideName("Catastrophe");
            }

            reaper.causingCatastrophe = false;
        }
    }

    [MethodRpc((uint)Rpcs.RpcCatastrophe)]
    public static void RpcCatastrophe(PlayerControl player)
    {
        if (player.Data.Role is not Reaper)
        {
            Logger<OWPlugin>.Error("RpcCatastrophe - Invalid reaper");
            return;
        }

        var reaper = player.GetRole<Reaper>();
        reaper.causingCatastrophe = !reaper.causingCatastrophe;
    }

    public bool WinConditionMet()
    {
        var aliveTeammates = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x.IsRole<Reaper>());
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

    public void Role_OnMeetingStart()
    {
        if (causingCatastrophe)
        {
            causingCatastrophe = false;

            var button = CustomButtonSingleton<Reaper_Catastrophe>.Instance;
            button.OverrideName("Catastrophe");
        }
    }

    public void LobbyStart()
    {
        ReaperVisual.CleanUp();
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            Player.RpcCustomMurder(target);
            VisitingMechanic.RpcAddDeathReason(target, (int)DeathReasonShow.Killed);

            RpcReaperAttack(Player, target);
        }
        else if (Button == 2)
        {
            RpcCatastrophe(Player);

            var button = CustomButtonSingleton<Reaper_Catastrophe>.Instance;
            if (causingCatastrophe) button.OverrideName("ENABLED");
            else button.OverrideName("Catastrophe");

        }
    }

    public bool causingCatastrophe;
}

public sealed class Reaper_Attack : ObjectWorkshopRoleButton<Reaper, PlayerControl>
{
    public override string Name => "Attack";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Reaper;
    public override float Cooldown => OptionGroupSingleton<Reaper_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: x => x.IsTargetable());
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, true, true);
}

public sealed class Reaper_Catastrophe : ObjectWorkshopRoleButton<Reaper>
{
    public override string Name => "Catastrophe";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Reaper;
    public override float Cooldown => 1f;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override int MaxUses => (int)OptionGroupSingleton<Reaper_Options>.Instance.Charges;
    public override bool DecreaseCharge => false;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
}

public sealed class Reaper_Options : AbstractOptionGroup<Reaper>
{
    public override string GroupName => "Reaper";

    [ModdedNumberOption("<color=#5a8129>Reaper</color> <color=#4a86e8>Attack</color> Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25;

    [ModdedNumberOption("<color=#5a8129>Reaper</color> Max <color=#4a86e8>Catastrophies</color>", 1f, 15f, 1f)]
    public float Charges { get; set; } = 1f;

    [ModdedNumberOption("<color=#5a8129>Reaper</color> Vision", 0.25f, 60f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float Vision { get; set; } = 1f;

    [ModdedToggleOption("<color=#5a8129>Reaper</color> Can Vent")]
    public bool CanVent { get; set; } = true;

    [ModdedToggleOption("Enable <color=#5a8129>Reaper</color> <color=#0000ff>Shield</color>")]
    public bool EnableShield { get; set; } = true;
}