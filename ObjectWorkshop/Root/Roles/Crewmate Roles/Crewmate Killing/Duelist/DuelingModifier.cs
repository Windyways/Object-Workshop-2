namespace ObjectWorkshop.Roles;

// This is used to check if the player is currently in a Duel.
// There are issues when a player is in multiple duels, this makes sure that does not happen.
public sealed class DuelingModifier : BaseModifier
{
    public override string ModifierName => "Dueling";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        /*if (Player.Data.Role is Totemist totemist && totemist.isWatching)
        {
            totemist.isWatching = false;

            LightSource light = Player.lightSource;
            light.transform.SetParent(Player.transform);
            light.transform.localPosition = Player.Collider.offset;
        }
        else if (Player.Data.Role is Aimsman aimsman && aimsman.isAiming)
        {
            Aimsman.RpcAim(Player);
        }
        else if (Player.Data.Role is Culverin culverin && culverin.isAiming)
        {
            var cannonballBase = CannonballBase.GetObjectByPlayer(Player);

            Object.Destroy(cannonballBase.gameObject);
            Object.Destroy(cannonballBase.directionLine);
        }
        else if (Player.Data.Role is Specter specter && specter.isInvisible)
        {
            specter.isInvisible = false;
            Player.RpcRemoveModifier<InvisibleTogglable>();
        }*/
    }
}