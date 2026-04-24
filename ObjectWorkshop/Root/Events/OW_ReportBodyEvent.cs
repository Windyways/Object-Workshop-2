namespace ObjectWorkshop.Events;

public static class OW_ReportBodyEvent
{
    [RegisterEvent(1)]
    public static void ReportBodyEvent(ReportBodyEvent @event)
    {
        var shikari = PlayerControl.AllPlayerControls.ToArray().Any(x => !x.HasDied() && x.Data.Role is Shikari shikari && shikari.ExecutionPhase());
        if (shikari) @event.Cancel();
    }
}