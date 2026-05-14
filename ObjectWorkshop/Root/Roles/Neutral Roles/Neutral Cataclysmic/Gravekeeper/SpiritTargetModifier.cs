namespace ObjectWorkshop.Roles;

// Checks if the player is being targeted by a Gravekeeper spirit.
// Used to prevent multiple spirits going on the same player, making it impossible for Gravekeeper to win.
public sealed class SpiritTargetModifier : BaseModifier
{
    public override string ModifierName => "Spirit Target";
    public override bool Unique => false;
    public override bool HideOnUi => true;
}