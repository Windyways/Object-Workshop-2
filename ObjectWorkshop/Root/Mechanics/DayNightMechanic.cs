namespace ObjectWorkshop.Mechanics;

public static class DayNightMechanic
{
    public static float PostMeetingIntroTime = 7f;
    public static int DayCount;
    public static int NightCount = 1;
    public static float NightTimer;

    [RegisterEvent(-1)]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        DayCount++;
    }

    [RegisterEvent(-1)]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            DayCount = 0;
            NightCount = 1;
            return; // Only run when round starts.
        }

        NightCount++;
        SmartClientSwapping.RoundStart();
    }
    
    [RegisterEvent]
    public static void GameStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return; // Only run when game starts.
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            ShowRoleIcon.Add(player);
            player.RpcAddModifier<VoteCount>();
        }

        SmartClientSwapping.RoundStart();
    }
}