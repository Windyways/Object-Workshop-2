namespace ObjectWorkshop.CovenRoles;

public sealed class InfiltratorOptions : AbstractOptionGroup
{
    public override string GroupName => "Infiltrator Settings";
    public override uint GroupPriority => 1;

    [ModdedNumberOption("Max Infiltrator Roles Per Game", 1f, 10f, 1f, MiraNumberSuffixes.None)]
    public float MaxInfiltrator { get; set; } = 2;

    [ModdedNumberOption("Infiltrator Kill Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedToggleOption("Infiltrator Roles Can Vent")]
    public bool CanVent { get; set; } = true;

    [ModdedToggleOption("Infiltrator Roles Can Sabotage")]
    public bool CanSabotage { get; set; } = true;
}