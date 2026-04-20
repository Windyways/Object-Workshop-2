namespace ObjectWorkshop.Modifiers;

// This is used to make players see roles of their target.
// Example: Consigliere revealing a player, Coroner finding killer, etc.
public sealed class RoleLearn(PlayerControl visitor) : BaseModifier
{
    public override string ModifierName => "Role Learn";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Visitor = visitor;
    public override void OnActivate()
    {
        if (Visitor.Is(Faction.Crewmate))
        {
            if (Player.Is(Faction.Crewmate)) Player.AddModifier<Confirmed>(Visitor, ConfirmType.Instantly, $"Revealed By {Visitor.Data.Role.NiceName}");
            else Player.AddModifier<ConfirmedEvil>($"Revealed By {Visitor.Data.Role.NiceName}");
        }
    }
}