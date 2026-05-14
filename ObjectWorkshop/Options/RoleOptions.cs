
namespace TownOfUs.Options;

public sealed class RoleOptions : AbstractOptionGroup
{
    public static readonly string[] OptionStrings =
    [
        "<color=#b3ffff>Crewmate</color> <color=#4a86e8>Investigative</color>",
        "<color=#b3ffff>Crewmate</color> <color=#4a86e8>Killing</color>",
        "<color=#b3ffff>Crewmate</color> <color=#4a86e8>Protective</color>",
        "<color=#b3ffff>Crewmate</color> <color=#4a86e8>Support</color>",
        "<color=#b3ffff>Crewmate</color> <color=#4a86e8>Utility</color>",
        "<color=#4a86e8>Random</color> <color=#b3ffff>Crewmate</color>",

        "<color=#a9a9a9>Neutral</color> <color=#91bbff>Associative</color>",
        "<color=#a9a9a9>Neutral</color> <color=#c6db3c>Benign</color>",
        "<color=#a9a9a9>Neutral</color> <color=#bd19b7>Cataclysmic</color>",
        "<color=#a9a9a9>Neutral</color> <color=#d34a72>Evil</color>",
        "<color=#a9a9a9>Neutral</color> <color=#3562ac>Predator</color>",
        "<color=#4a86e8>Random</color> <color=#a9a9a9>Neutral</color>",

        "<color=#ff5050>Infiltrator</color> <color=#4a86e8>Disruption</color>",
        "<color=#ff5050>Infiltrator</color> <color=#4a86e8>Evacuative</color>",
        "<color=#ff5050>Infiltrator</color> <color=#4a86e8>Killing</color>",
        "<color=#ff5050>Infiltrator</color> <color=#4a86e8>Utility</color>",
        "<color=#4a86e8>Random</color> <color=#ff5050>Infiltrator</color>",

        "Random",
        "Not <color=#ff5050>Infiltrator</color>"
    ];

    public override string GroupName => "Role";
    public override uint GroupPriority => 2;

    public ModdedEnumOption Slot1 { get; } =
        new("Slot 1", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot2 { get; } =
        new("Slot 2", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot3 { get; } =
        new("Slot 3", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot4 { get; } =
        new("Slot 4", (int)RoleListOption.RandomInfiltrator, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot5 { get; } =
        new("Slot 5", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot6 { get; } =
        new("Slot 6", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot7 { get; } =
        new("Slot 7", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot8 { get; } =
        new("Slot 8", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot9 { get; } =
        new("Slot 9", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot10 { get; } =
        new("Slot 10", (int)RoleListOption.RandomInfiltrator, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot11 { get; } =
        new("Slot 11", (int)RoleListOption.RandomInfiltrator, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot12 { get; } =
        new("Slot 12", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot13 { get; } =
        new("Slot 13", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot14 { get; } =
        new("Slot 14", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };

    public ModdedEnumOption Slot15 { get; } =
        new("Slot 15", (int)RoleListOption.RandomCrewmate, typeof(RoleListOption), OptionStrings)
        {
            Visible = () => true
        };
}

public enum RoleListOption
{
    CrewmateInvestigative,
    CrewmateKilling,
    CrewmateProtective,
    CrewmateSupport,
    CrewmateUtility,
    RandomCrewmate,

    NeutralAssociative,
    NeutralBenign,
    NeutralCataclysmic,
    NeutralEvil,
    NeutralPredator,
    RandomNeutral,

    InfiltratorDisruption,
    InfiltratorEvacuative,
    InfiltratorKilling,
    InfiltratorUtility,
    RandomInfiltrator,

    Any,

    NotInfiltrator,
    None
}