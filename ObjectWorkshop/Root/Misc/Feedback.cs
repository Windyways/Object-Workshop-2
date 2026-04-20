using UnityEngine;

namespace ObjectWorkshop.Misc;

public static class Feedback
{
    public static void Notify(this PlayerControl player, string feedback, NotifyMode type, Color color = new(), Sprite? sprite = null, NetworkedPlayerInfo? basePlayer = null)
    {
        if (player.AmOwner())
        {
            if (type == NotifyMode.Instantly) MiscUtils.ShowNotification(feedback, color, sprite);
            if (type == NotifyMode.InstantlyAndMeeting)
            {
                MiscUtils.ShowNotification(feedback, color, sprite);
                MiscUtils.AddFakeChat(basePlayer == null ? player.CachedPlayerData : basePlayer, "FEEDBACK", feedback);
            }
            if (type == NotifyMode.OnlyMeeting) MiscUtils.AddFakeChat(basePlayer == null ? player.CachedPlayerData : basePlayer, "FEEDBACK", feedback);
        }
    }

    [MethodRpc((uint)Rpcs.RpcNotifyAll)]
    public static void RpcNotifyAll(PlayerControl player, string info, NotifyMode mode, bool showRoleIcon)
    {
        PlayerControl.LocalPlayer.Notify(info, mode, sprite: showRoleIcon ? player.GetRoleIcon() : null);
    }
}