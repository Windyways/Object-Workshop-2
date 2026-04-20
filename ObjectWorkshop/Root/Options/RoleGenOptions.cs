namespace ObjectWorkshop.Options;

public sealed class RoleGenOptions : AbstractOptionGroup
{
    public override string GroupName => "Role Generation Options";
    public override uint GroupPriority => 1;

    [ModdedToggleOption("Legacy Role Generation")]
    public bool LegacyRoleGen { get; set; } = false;
}