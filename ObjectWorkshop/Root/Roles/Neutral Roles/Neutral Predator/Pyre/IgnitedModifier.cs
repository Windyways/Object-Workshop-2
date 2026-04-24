namespace ObjectWorkshop.Roles;

// This is used to increase their vision and to prevent Pyres from targeting ignited players.
// Also prevents the game from crashing LOL.
public sealed class IgnitedModifier : BaseModifier
{
    public override string ModifierName => "Ignited";
    public override bool Unique => false;
    public override bool HideOnUi => true;
}