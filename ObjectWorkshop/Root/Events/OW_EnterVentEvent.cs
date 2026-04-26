namespace ObjectWorkshop.Events;

public static class OW_EnterVentEvent
{
    [RegisterEvent()]
    public static void EnterVentEvent(EnterVentEvent @event)
    {
        if (@event.Player.Data.Role is Aimsman aimsman && aimsman.isAiming)
            @event.Cancel();
    }
}