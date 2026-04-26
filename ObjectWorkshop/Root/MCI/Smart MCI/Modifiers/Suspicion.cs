namespace ObjectWorkshop.MCI;

public class Suspicion(PlayerControl f, string s, int sus) : BaseModifier
{
    public override string ModifierName => "Incriminating Evidence";
    public override bool HideOnUi => !Debugger.IsDebuggerActive;
    public override string GetDescription()
    {
        return $"You have Incriminating Evidence!";
    }

    public string Source => s;
    public PlayerControl Finder => f;
    public int VotedChance => sus;
    public static Suspicion GetRandom(List<PlayerControl>? exclude = null)
    {
        var modifiers = new List<Suspicion>();
        if (exclude != null)
        {
            foreach (var modifier in ModifierUtils.GetActiveModifiers<Suspicion>(x => !x.Player.HasDied() && !exclude.Contains(x.Player))) modifiers.Add(modifier);
        }
        else 
            foreach (var modifier in ModifierUtils.GetActiveModifiers<Suspicion>(x => !x.Player.HasDied())) modifiers.Add(modifier);

        if (modifiers.Count == 0) return null;
        return modifiers.Random();
    }

    public static List<Suspicion> GetAll()
    {
        var modifiers = new List<Suspicion>();
        foreach (var modifier in ModifierUtils.GetActiveModifiers<Suspicion>(x => !x.Player.HasDied()/* && 
            !x.Finder.HasModifier<ThreatenedModifier>()*/)) modifiers.Add(modifier);

        if (modifiers.Count == 0) return null;
        return modifiers;
    }
}