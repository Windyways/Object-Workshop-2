namespace ObjectWorkshop.Mechanics;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class WhisperPatches
{
    public static void Prefix(ref string chatText, ref PlayerControl sourcePlayer)
    {
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                // if (player.IsBlackmailed() && player.AmOwner) chatText = ""; - test for Banshee maybe.
                if (chatText.Contains("/w " + player.Data.PlayerName))
                {
                    if (player.HasDied() || sourcePlayer.HasDied())
                    {
                        break;
                    }

                    if (player.Data.PlayerName == PlayerControl.LocalPlayer.Data.PlayerName)
                    {
                        chatText = chatText.Replace("/w " + player.Data.PlayerName, "<color=#9a71e6>From " + sourcePlayer.Data.PlayerName + ": ");
                    }
                    else if (sourcePlayer == PlayerControl.LocalPlayer)
                    {
                        chatText = chatText.Replace("/w " + player.Data.PlayerName, "<color=#9a71e6>To " + player.Data.PlayerName + ":");
                    }
                }
            }
        }
    }
}