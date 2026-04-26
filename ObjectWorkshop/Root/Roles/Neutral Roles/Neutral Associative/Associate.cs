namespace ObjectWorkshop.Roles;

public sealed class Associate(PlayerControl of) : BaseModifier
{
    public override string ModifierName => "Associate";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Caster => of;
}