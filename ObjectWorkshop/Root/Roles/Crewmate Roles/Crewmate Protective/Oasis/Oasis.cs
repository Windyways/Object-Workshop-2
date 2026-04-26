using System.Text;
using MiraAPI.Keybinds;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ObjectWorkshop.Roles;

public sealed class Oasis(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Oasis";
    public string RoleDescription => "Create Sanctuaries, start Sandstorms!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Crewmate;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public Faction Faction { get; set; } = Faction.Crewmate;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Crewmate;
    public Alignment Alignment => Alignment.CrewmateProtective;

    public string RevealText => "is the protector of the desert.";
    public string Description => "Sanctify an area to protect those within. Conjure a Sandstorm to make evils miss their attacks.";
    public string Intro => "You are a desert celebrity that provides protection to others.\r\n";
    public string VictoryCondition => "Dispose of all evil. You will win with other Crewmate members.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        // Icon = OWAssets.OasisRoleCard,
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
        new("Sanctify",
            $"You can Sanctify the area around you during the round.\n" +
            $"You will create a Sanctuary, preventing all players inside from dying from direct attacks, which only lasts {OptionGroupSingleton<Oasis_Options>.Instance.Duration}s. Only you can see the Sanctuary.\n" +
            $"You will be notified if your target is attacked. Your target will be notified if they’re attacked. The attacker will be notified their target was protected by an Oasis.",
            OWAssets.KillSprite),

        new("Sandstorm",
            $"You can cause a Sandstorm during the round.\n" +
            $"A sandstorm will brew, and all players attempting to attack in a sandstorm will have a {OptionGroupSingleton<Oasis_Options>.Instance.MissChance}% chance to miss. Sandstorms last {OptionGroupSingleton<Oasis_Options>.Instance.SandstormDuration}s before evaporating.\n" +
            $"You will be notified if your target is attacked. Your target will be notified if they’re attacked. The attacker will be notified their target was protected by a Sandstorm.",
            OWAssets.KillSprite),
    ];

    public void Role_OnMeetingStart()
    {
        Sanctuary.DestroyAll();
        RpcStopSandstorm(Player);
    }

    public void LobbyStart()
    {
        Sanctuary.CleanUp();
        Sandstorm.CleanUp();
    }

    [MethodRpc((uint)Rpcs.RpcSanctify)]
    public static void RpcSanctify(PlayerControl player)
    {
        if (player.Data.Role is not Oasis)
        {
            Logger<OWPlugin>.Error("RpcSanctify - Invalid Oasis");
            return;
        }

        Sanctuary.Begin(player);
    }

    [MethodRpc((uint)Rpcs.RpcStartSandstorm)]
    public static void RpcStartSandstorm(PlayerControl player)
    {
        Sandstorm.Begin(player);
    }
    
    [MethodRpc((uint)Rpcs.RpcStopSandstorm)]
    public static void RpcStopSandstorm(PlayerControl player)
    {
        var sandstorm = Sandstorm.GetObjectByPlayer(player);
        if (sandstorm != null) sandstorm.Stop();
    }

    [MethodRpc((uint)Rpcs.RpcSandstormMissNotif)]
    public static void RpcSandstormMissNotif(PlayerControl player, PlayerControl target, PlayerControl oasis)
    {
        if (player.AmOwner()) player.Notify(Oasis_Feedback.AttackerMiss(target), NotifyMode.InstantlyAndMeeting);
        if (target.AmOwner()) target.Notify(Oasis_Feedback.VictimMiss(target), NotifyMode.InstantlyAndMeeting);
        if (oasis.AmOwner()) oasis.Notify(Oasis_Feedback.OasisMiss(), NotifyMode.InstantlyAndMeeting);
    }

    [MethodRpc((uint)Rpcs.RpcSanctifyNotif)]
    public static void RpcSanctifyNotif(PlayerControl player, PlayerControl target, PlayerControl oasis)
    {
        if (player.AmOwner()) player.Notify(Oasis_Feedback.AttackerSanctify(target), NotifyMode.InstantlyAndMeeting);
        if (target.AmOwner()) target.Notify(Oasis_Feedback.VictimSanctify(), NotifyMode.InstantlyAndMeeting);
        if (oasis.AmOwner()) oasis.Notify(Oasis_Feedback.OasisSanctify(target), NotifyMode.InstantlyAndMeeting);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1) RpcSanctify(Player);
        else if (Button == 2) RpcStartSandstorm(Player);
    }
}

public sealed class Oasis_Sanctify : ObjectWorkshopRoleButton<Oasis>
{
    public override string Name => "Sanctify";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<Oasis_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override float EffectDuration => OptionGroupSingleton<Oasis_Options>.Instance.Duration;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
}

public sealed class Oasis_Sandstorm : ObjectWorkshopRoleButton<Oasis>
{
    public override string Name => "Sandstorm";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<Oasis_Options>.Instance.SandstormCD;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override float EffectDuration => OptionGroupSingleton<Oasis_Options>.Instance.SandstormDuration;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
}

public static class Oasis_Feedback
{
    public static string AttackerSanctify(PlayerControl target)
    {
        return $"You Attacked {target.Name()}, but an Oasis protected them!".ApplyKeywords();
    }

    public static string VictimSanctify()
    {
        return $"You were attacked, but a Sanctuary protected you!".ApplyKeywords();
    }

    public static string OasisSanctify(PlayerControl target)
    {
        return $"{target.Name()} was attacked, but you saved them!".ApplyKeywords();
    }

    public static string AttackerMiss(PlayerControl target)
    {
        return $"You Attacked {target.Name()}, but a Sandstorm made you miss!".ApplyKeywords();
    }

    public static string VictimMiss(PlayerControl target)
    {
        return $"You Attacked {target.Name()}, but a Sandstorm made you miss!".ApplyKeywords();
    }

    public static string OasisMiss()
    {
        return $"You feel one with the storm as you hear someone miss an attack!".ApplyKeywords();
    }

}

public sealed class Oasis_Options : AbstractOptionGroup<Oasis>
{
    public override string GroupName => "Oasis";

    [ModdedNumberOption("<color=#b3ffff>Oasis</color> <color=#4a86e8>Sanctify</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedNumberOption("<color=#b3ffff>Oasis</color> <color=#4a86e8>Sandstorm</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SandstormCD { get; set; } = 25f;
    
    [ModdedNumberOption("<color=#b3ffff>Oasis</color> <color=#4a86e8>Sanctify</color> Duration", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Duration { get; set; } = 10f;
    
    [ModdedNumberOption("<color=#b3ffff>Oasis</color> <color=#4a86e8>Sanctify</color> Radius", 0.1f, 30f, 0.1f, MiraNumberSuffixes.Multiplier, "0.0")]
    public float Radius { get; set; } = 7f;
    
    [ModdedNumberOption("<color=#b3ffff>Oasis</color> <color=#4a86e8>Sandstorm</color> Duration", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SandstormDuration { get; set; } = 25f;

    [ModdedNumberOption("<color=#b3ffff>Oasis</color> <color=#4a86e8>Sandstorm</color> Miss Chance", 0, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float MissChance { get; set; } = 25f;
}