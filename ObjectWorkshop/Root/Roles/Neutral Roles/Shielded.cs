public sealed class Shielded : BaseModifier
{
    public override string ModifierName => "Shielded";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public int PerformInteraction(PlayerControl player, PlayerControl target)
    {
        RpcPerformInteraction(player, target);
        return 1;
    }

    [MethodRpc((uint)Rpcs.RpcPerformInteraction)]
    public static void RpcPerformInteraction(PlayerControl player, PlayerControl target)
    {
        if (player.AmOwner())
        {
            player.Notify($"You tried to attack {target.Name()}, but they were shielded!", NotifyMode.InstantlyAndMeeting);
        }
        else if (target.AmOwner())
        {
            target.Notify($"You were attacked, but your shield protected you!", NotifyMode.OnlyMeeting);
        }

        target.RemoveModifier<Shielded>();
    }
}