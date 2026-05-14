using System.Text;
using UnityEngine;

namespace ObjectWorkshop.Roles;

public sealed class UndeadReaper(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Undead Reaper";
    public string RoleDescription => "Bring all of them down with you.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.UndeadReaper;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Neutral;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.NeutralPredator;

    public string RevealText => "wants to bring the living to the dead.";
    public string Description => $"Enable Catastrophe to convert a victim into an Undead Reaper. Kill everyone to win.";
    public string Intro => "Kill players from the dead, although you will fully perish once all Reapers die.";
    public string VictoryCondition => $"Kill everyone that opposes you. Cause death upon those with your partner. You will win with other Reapers.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        MaxRoleCount = 0,
        CanModifyChance = false,
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
    ];

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        Player?.ResetAppearance(fullReset: true);
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        bool otherReapersWon = false;
        var reapers = PlayerControl.AllPlayerControls.ToArray().Where(x => x.IsRole<Reaper>() && x != Player).ToList();
        if (reapers.Count > 0)
        {
            foreach (var reaper in reapers)
            {
                if (reaper.Data.Role.DidWin(gameOverReason)) otherReapersWon = true;
            }
        }
        return otherReapersWon;
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

    public static bool ReapersAlive()
    {
        var reapersAlive = PlayerControl.AllPlayerControls.ToArray().Where(x => x.IsRole<Reaper>() && !x.HasDied());
        return reapersAlive.Any();
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            Player.RpcCustomMurder(target);
            VisitingMechanic.RpcAddDeathReason(target, (int)DeathReasonShow.Killed);
        }
    }
}

public sealed class UndeadReaper_Attack : ObjectWorkshopRoleButton<UndeadReaper, PlayerControl>
{
    public override string Name => "Attack";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.UndeadReaper;
    public override float Cooldown => OptionGroupSingleton<UndeadReaper_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override bool UsableInDeath => UndeadReaper.ReapersAlive();

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: x => 
            !x.IsRole<Reaper>());
    }

    public override bool CanUse()
    {
        return base.CanUse() && UndeadReaper.ReapersAlive();
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && UndeadReaper.ReapersAlive();
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, true, true);
}

public sealed class UndeadReaper_Options : AbstractOptionGroup<UndeadReaper>
{
    public override string GroupName => "Undead Reaper";

    [ModdedNumberOption("<color=#5a8129>Undead Reaper</color> <color=#4a86e8>Attack</color> Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25;

    [ModdedNumberOption("<color=#5a8129>Undead Reaper</color> Speed", 0.25f, 5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float Speed { get; set; } = 0.25f;
}