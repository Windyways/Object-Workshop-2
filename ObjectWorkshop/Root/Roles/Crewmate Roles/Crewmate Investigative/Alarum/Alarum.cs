using System.Text;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ObjectWorkshop.Roles;

public sealed class Alarum(IntPtr cppPtr) 
    : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Alarum";
    public string RoleDescription => "Place clocks to catch Infiltrators!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Crewmate;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public Faction Faction { get; set; } = Faction.Crewmate;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Crewmate;
    public Alignment Alignment => Alignment.CrewmateInvestigative;

    public string RevealText => "has a hard time sleeping.";
    public string Description => "Place down a clock, only visible to you and other Infiltrators. Infiltrators will turn the clock red when entering the radius.";
    public string Intro => "You are a sleepy crewmate who has an alarm that rings to keep the evil at bay.";
    public string VictoryCondition => "Dispose of all evil. You will win with other Crewmate members.";

    public CustomRoleConfiguration Configuration => new(this)
    {

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
        new("Activate",
            "You can Activate your alarm clock during the round.\n" +
            "You will place down your alarm clock on the ground, visible to only you.\n" +
            "If an Infiltrator gets into range, the alarm clock radius will turn red. Claylim and Claylamity are immune to this.\n" +
            "Alarm clocks reset at the start of each meeting.",
            OWAssets.KillSprite)
    ];

    public void Role_OnMeetingStart()
    {
        AlarmClock.CleanUp();
    }

    public void LobbyStart()
    {
        AlarmClock.CleanUp();
    }

    [MethodRpc((uint)Rpcs.RpcActivate)]
    public static void RpcActivate(PlayerControl player)
    {
        if (player.Data.Role is not Alarum)
        {
            Logger<OWPlugin>.Error("RpcActivate - Invalid Alarum");
            return;
        }

        AlarmClock.Begin(player);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            RpcActivate(Player);
        }
    }
}

public sealed class Alarum_Activate : ObjectWorkshopRoleButton<Alarum>
{
    public override string Name => "Activate";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<Alarum_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override int MaxUses => (int)OptionGroupSingleton<Alarum_Options>.Instance.MaxAlarmsAtOnce;
    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
}

public sealed class Alarum_Options : AbstractOptionGroup<Alarum>
{
    public override string GroupName => "Alarum";

    [ModdedNumberOption("<color=#b3ffff>Alarum</color> <color=#4a86e8>Activate</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedNumberOption("<color=#b3ffff>Alarum</color> <color=#4a86e8>Activate</color> Radius", 0.1f, 60f, 0.1f, MiraNumberSuffixes.Multiplier, "0.0")]
    public float Radius { get; set; } = 3f;

    [ModdedNumberOption("<color=#b3ffff>Alarum</color> Max Alarms At Once", 1f, 15f, 1f)]
    public float MaxAlarmsAtOnce { get; set; } = 3f;

    [ModdedToggleOption("<color=#ff5050>Infiltrators</color> Can See Alarms")]
    public bool InfiltSeeClock { get; set; } = true;
}