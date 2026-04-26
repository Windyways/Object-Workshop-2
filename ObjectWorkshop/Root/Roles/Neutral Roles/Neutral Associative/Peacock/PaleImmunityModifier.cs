namespace ObjectWorkshop.Roles;

// This is used to check if the player is currently in a Duel.
// There are issues when a player is in multiple duels, this makes sure that does not happen.
public sealed class PaleImmunityModifier : BaseModifier
{
    public override string ModifierName => "Pale Immune";
    public override bool Unique => false;
    public override bool HideOnUi => false;

    public override void OnMeetingStart()
    {
        Player.RpcRemoveModifier<PaleImmunityModifier>();
    }
}