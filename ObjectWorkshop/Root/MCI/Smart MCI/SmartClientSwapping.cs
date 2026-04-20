namespace ObjectWorkshop.MCI.SmartMCI;

public static class SmartClientSwapping
{
    // SMART AI PERFORM ORDER!
    public static float AnarchistDelay = 0.5f;
    public static float MayorDelay = 1;
    public static float JuniorInfiltratorDelay(bool firsttag) => firsttag ? 1.1f : 1.6f; // Post-Mayor - Post-Gunner
    public static float NightmareInfiltratorDelay = 1.11f;
    public static float AstronomerDelay = 1.12f;
    public static float BlightDelay = 1.13f;
    public static float VigilanteDelay(bool revealing) => revealing ? 1.2f : 1.35f; // Post-Astronomer - Post-Priest
    public static float PriestDelay = 1.3f; // 1.3f - 1.7f.
    public static float GunnerDelay = 1.5f;

    public static void RoundStart()
    {
        if (Debugger.IsDebuggerActive)
        {
            Keyboard_Joystick.RefreshSwapTargets();
            if (Debugger.smartSwapping)
            {
                Keyboard_Joystick.Switch(true);
                HudManager.Instance.Chat.gameObject.SetActive(true);
            }
        }
    }
}
