using System.Text;
using UnityEngine;

namespace ObjectWorkshop.Roles;

public sealed class Obstructor(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Obstructor";
    public string RoleDescription => "Place traffic cones to block movement!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Infiltrator;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Infiltrator;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Infiltrator;
    public Alignment Alignment => Alignment.InfiltratorDisruption;

    public string RevealText => "blocks passageways to disrupt opposing threats.";
    public string Description => "Place down traffic cones to prevent non-Infiltrators from passing through.";
    public string Intro => "You are a limited obturator that blocks paths using a traffic cone.";
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
        new("Attack",
            $"You can Attack a player during the round.\n" +
            $"You will kill your target.",
            OWAssets.KillSprite),

        new("Barricade",
            $"You can plant a Barricade during the round.\n" +
            "You will plant a traffic cone at your position, blocking people from passing it. Using this ability again will override the original Barricade.\n" +
            "Infiltrators are immune to your Barricade. Players are immune to your Barricades if a sabotage is active.\n" +
            "Barricades reset at the start of each round and Your Barricades are affected by Sandstorms.",
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
        else if (Button == 2) RpcBarricade(Player);
    }

    public void LobbyStart()
    {
        Barricade.CleanUp();
    }

    public override void OnMeetingStart()
    {
        Barricade.DestroyAll();
    }

    [MethodRpc((uint)Rpcs.RpcBarricade)]
    public static void RpcBarricade(PlayerControl player)
    {
        if (player.Data.Role is not Obstructor)
        {
            Logger<OWPlugin>.Error("RpcBarricade - Invalid Obstructor");
            return;
        }

        var obstructor = player.GetRole<Obstructor>();

		obstructor.ActiveBarricades.Add(Barricade.Begin(obstructor.Player));
    }

    public List<Barricade> ActiveBarricades = new List<Barricade>();
}

public sealed class Obstructor_Attack : ObjectWorkshopRoleButton<Obstructor, PlayerControl>
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

public sealed class Obstructor_Barricade : ObjectWorkshopRoleButton<Obstructor>
{
    public override string Name => "Barricade";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Infiltrator;
    public override float Cooldown => OptionGroupSingleton<Obstructor_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
}

public sealed class Obstructor_Options : AbstractOptionGroup<Obstructor>
{
    public override string GroupName => "Obstructor";

    [ModdedNumberOption("<color=#ff5050>Obstructor</color> <color=#4a86e8>Barricade</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
    
    [ModdedNumberOption("<color=#ff5050>Obstructor</color> Max <color=#4a86e8>Barricades</color> At Once", 1f, 3f, 1f)]
    public float MaxAtOnce { get; set; } = 1f;
}