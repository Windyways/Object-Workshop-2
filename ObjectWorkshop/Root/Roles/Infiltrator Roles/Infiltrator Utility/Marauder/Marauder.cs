using Il2CppInterop.Runtime.Attributes;
using System.Text;
using UnityEngine;

namespace ObjectWorkshop.Roles;

public sealed class Marauder(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Marauder";
    public string RoleDescription => "";
    public string RoleLongDescription => "";
    public Color RoleColor { get; set; } = RoleColors.Infiltrator;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public Faction Faction { get; set; } = Faction.Infiltrator;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Infiltrator;
    public Alignment Alignment => Alignment.InfiltratorUtility;

    public string RevealText => "is a space invader.";
    public string Description => "Kill all those that oppose the Infiltrators.";
    public string Intro => "You are an invader that wants to take down the crew.";
    public string VictoryCondition => "Kill everyone that opposes the Infiltrators. You will win with other Infiltrator members.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseSabotage = OptionGroupSingleton<InfiltratorOptions>.Instance.CanSabotage,
        CanUseVent = OptionGroupSingleton<InfiltratorOptions>.Instance.CanVent,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
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
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Attack ",
            $"You can Attack a player during the round.\n" +
            $"You will kill your target.",
            OWAssets.KillSprite),
    ];

    public bool WinConditionMet() => InfiltratorGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || InfiltratorGameOver.AnyWon(gameOverReason);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            if (Player.CanKill(target))
            {
                Player.RpcCustomMurder(target);
                VisitingMechanic.RpcAddDeathReason(target, (int)DeathReasonShow.Killed);
            }
        }
    }
}

public sealed class Marauder_Attack : ObjectWorkshopRoleButton<Marauder, PlayerControl>
{
    public override string Name => "Attack";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Infiltrator;
    public override float Cooldown => OptionGroupSingleton<InfiltratorOptions>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(true, Distance, predicate: x => 
            !x.Is(Faction.Infiltrator));
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, true, true);
}