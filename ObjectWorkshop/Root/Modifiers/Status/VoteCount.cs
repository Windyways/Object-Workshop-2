namespace ObjectWorkshop.Modifiers;

public sealed class VoteCount : BaseModifier
{
    public override string ModifierName => "VoteCount";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public int BaseVotes = 1;
    public int CurrentVotes = 1;
    public bool CanVote = true;

    public void AddVotes(int amount, bool permanent)
    {
        CurrentVotes += amount;
        if (permanent) BaseVotes += amount;
    }

    public void PreventVoting()
    {
        CanVote = false;
    }

    // --- Global Methods ---
    public void OnRoundStart()
    {
        CanVote = true;
        CurrentVotes = BaseVotes;
    }
}