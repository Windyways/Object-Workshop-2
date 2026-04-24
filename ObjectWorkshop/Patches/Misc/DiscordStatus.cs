using Discord;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch]
public static class DiscordStatus
{
    [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
    [HarmonyPrefix]
    public static void Prefix([HarmonyArgument(0)] Activity activity)
    {
        activity.Details += $" - Object Workshop 2 v{OWPlugin.Version}" + (OWPlugin.IsDevBuild ? $" (DEV Build {OWPlugin.BetaVersion})" : string.Empty);
    }
}