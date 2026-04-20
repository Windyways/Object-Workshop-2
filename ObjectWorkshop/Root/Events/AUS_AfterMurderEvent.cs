namespace ObjectWorkshop.Events;

public static class AUS_AfterMurderEvent
{
    [RegisterEvent(1)]
    public static void AfterMurderEvent(AfterMurderEvent @event)
    {
        var victim = @event.Target;
        var killer = @event.Source;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.HasDied())
            {
                var role = player.GetRoleWhenAlive();
                if (role is ICustomAURole cr)
                {
                    cr.Role_AfterMurder(killer, victim);
                    cr.Role_OnDeath(victim);
                }
            }
            else if (player.Data.Role is ICustomAURole customRole)
            {
                customRole.Role_AfterMurder(killer, victim);
                customRole.Role_OnDeath(victim);
            }
        }
    }
}