namespace ObjectWorkshop.MCI;

/// <summary>
/// No parameters.
/// </summary>
public class TI : BaseModifier
{
    public override string ModifierName => "TI";
    public override bool HideOnUi => !Debugger.IsDebuggerActive;
    public override string GetDescription()
    {
        return $"You have gotten the Towns trust by getting info!";
    }
}