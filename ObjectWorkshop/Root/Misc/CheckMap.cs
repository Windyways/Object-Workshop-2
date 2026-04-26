namespace ObjectWorkshop.Misc;

public static class CheckMap
{
    public static CurrentMap MapSelected;

    [HarmonyPatch(typeof(LobbyBehaviour), "Update")]
    public static class LobbyBehaviourUpdate
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (MapSelected != (CurrentMap)GameOptionsManager.Instance.currentNormalGameOptions.MapId)
            {
                MapSelected = (CurrentMap)GameOptionsManager.Instance.currentNormalGameOptions.MapId;
                OWPlugin.DebugLogMessage($"Map set to {MapSelected}.", OWPlugin.MsgType.Message);
            }
        }
    }
}

public enum CurrentMap
{
    Skeld,
    MiraHQ,
    Polus,
    Airship = 4,
    Fungle,
    Submerged,
    LevelImpostor
}