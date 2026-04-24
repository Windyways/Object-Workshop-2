namespace ObjectWorkshop.Events;

public static class AUv_HandleVoteEvent
{
    [RegisterEvent()]
    public static void HandleVoteEvent(HandleVoteEvent @event)
    {
        if (@event.VoteData.Owner.TryGetModifier<VoteCount>(out var voteMod))
        {
            if (!voteMod.CanVote)
            {
                @event.VoteData.SetRemainingVotes(0);
                @event.Cancel();
                return;
            }

            @event.VoteData.SetRemainingVotes(0);

            for (var i = 0; i < voteMod.CurrentVotes; i++) 
            {
                @event.VoteData.VoteForPlayer(@event.TargetId);
            }

            @event.Cancel();
        }
    }
}