namespace ObjectWorkshop.Options;

public sealed class OWOptions : AbstractOptionGroup
{
    public override string GroupName => "Main Options";
    public override uint GroupPriority => 0;

    [ModdedToggleOption("Alive See Dead Roles")]
    public bool AliveSeeDead { get; set; } = true;

    public ModdedToggleOption AliveSeeDeathReason { get; } = new("Alive See Death Reason", true)
    {
        Visible = () => OptionGroupSingleton<OWOptions>.Instance.AliveSeeDead
    };
}