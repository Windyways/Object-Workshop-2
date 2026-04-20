namespace ObjectWorkshop.MCI;

/// <summary>
/// Marks a player as Confirmed Crewmater. Used for Smart AI.
/// c = The Caster.
/// t = The Confirm Type.
/// s = The Source, how they're being Confirmed.
/// </summary>
/// <param name="c">The Caster.</param>
/// <param name="t">The Confirm Type.</param>
/// <param name="s">The Source, how they're being Confirmed.</param>
public class Confirmed(PlayerControl c, ConfirmType t, string s) : BaseModifier
{
    public override string ModifierName => "Confirmed";
    public override bool HideOnUi => !Debugger.IsDebuggerActive;
    public override string GetDescription()
    {
        return $"You are Confirmed from the Town!";
    }

    public string Source => s;
    public ConfirmType type => t;
    public PlayerControl Caster => c;
    public bool IsConfirmed()
    {
        if (type == ConfirmType.Instantly || type == ConfirmType.EvilButDontTargetYet) return true;
        if (type == ConfirmType.UponDeath && Caster.HasDied()) return true;

        return false;
    }
}

public enum ConfirmType
{
    Instantly,
    UponDeath,
    EvilButDontTargetYet
}