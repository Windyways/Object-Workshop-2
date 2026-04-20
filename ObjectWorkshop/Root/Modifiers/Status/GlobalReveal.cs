namespace ObjectWorkshop.Modifiers;

public sealed class GlobalReveal : BaseModifier
{
    public override string ModifierName => "Global Reveal";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        if (Player.Is(Faction.Crewmate))
        {
            Player.AddModifier<Confirmed>(Player, ConfirmType.Instantly, "Global Revealed");
            Player.RemoveModifier<Suspicion>();
        }
    }
}