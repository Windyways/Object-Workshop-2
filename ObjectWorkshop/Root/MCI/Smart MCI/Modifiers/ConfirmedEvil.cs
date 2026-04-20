namespace ObjectWorkshop.MCI;

/// <summary>
/// s = Source, how they are Confirmed Infiltrator.
/// </summary>
/// <param name="s"></param>
public class ConfirmedEvil(string s) : BaseModifier
{
    public override string ModifierName => "ConfirmedEvil";
    public override bool HideOnUi => !Debugger.IsDebuggerActive;
    public override string GetDescription()
    {
        return "You are confirmed evil to the Town.";
    }

    public string Source => s;
    public static ConfirmedEvil GetAll()
    {
        var modifiers = new List<ConfirmedEvil>();
        foreach (var modifier in ModifierUtils.GetActiveModifiers<ConfirmedEvil>(x => !x.Player.HasDied())) modifiers.Add(modifier);

        if (modifiers.Count == 0) return null;
        return modifiers.Random();
    }
}