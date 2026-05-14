namespace ObjectWorkshop.Misc;

public static class Sabotages
{
    public static bool AnyActive()
    {
        if (OWPlugin.InGame())
        {
            var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
            var specials = system.specials.ToArray();

            return specials.Any((IActivatable s) => s.IsActive);
        }

        return false;
    }
}