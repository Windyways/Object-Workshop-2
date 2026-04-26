namespace ObjectWorkshop.Roles;

// This is used to slow down players that die by Claylamity.
public sealed class Submerged : BaseModifier
{
    public override string ModifierName => "Submerged";
    public override bool Unique => false;
    public override bool HideOnUi => true;
}