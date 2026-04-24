using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Mechanics;

[HarmonyPatch]
public static class HuntMechanic
{
    public static ICustomAURole roleHunt;

    public static GameObject HuntTimeObj;
    public static GameObject TimerSpriteObj;
    public static SpriteRenderer TimerSprite;
    public static bool Enabled { get; set; }
    public static float HuntTime { get; set; }

    private static void CreateHuntTime(HudManager instance)
    {
        var pingTracker = Object.FindObjectOfType<PingTracker>(true);
        HuntTimeObj = Object.Instantiate(pingTracker.gameObject, instance.transform);
        HuntTimeObj.name = "HuntTimeText";

        HuntTimeObj.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-0.6f, 5.5f);
        HuntTimeObj.GetComponent<AspectPosition>().Alignment = AspectPosition.EdgeAlignments.Bottom;

        TimerSpriteObj = new GameObject("TimerSprite");
        TimerSpriteObj.transform.SetParent(HuntTimeObj.transform);
        TimerSpriteObj.transform.localPosition = new Vector3(-1f, -0.4f, 1f);
        TimerSpriteObj.gameObject.layer = HuntTimeObj.gameObject.layer;
        TimerSpriteObj.SetActive(true);

        var ts = TimeSpan.FromSeconds(HuntTime);

        var timerText = HuntTimeObj.GetComponent<TextMeshPro>();
        timerText.text = $"<size=200%>Time:{ts.ToString(@"mm\:ss", OWPlugin.Culture)}</size>";
        timerText.alignment = TextAlignmentOptions.TopLeft;
        timerText.verticalAlignment = VerticalAlignmentOptions.Top;

        HuntTimeObj.SetActive(false);
    }

    public static void UpdateHuntTime(HudManager instance)
    {
        if (HuntTimeObj != null)
        {
            HuntTimeObj.SetActive(false);
        }

        if (HuntTimeObj == null)
        {
            CreateHuntTime(instance);
        }

        if (HuntTimeObj == null)
        {
            return;
        }

        var inMeeting = MeetingHud.Instance || ExileController.Instance;

        if (Enabled && HuntTime > 0 && !inMeeting)
        {
            HuntTime -= Time.deltaTime;
            HuntTime = Math.Max(HuntTime, 0);

            if (AmongUsClient.Instance.AmHost && HuntTime <= 0)
            {
                //StopHunt();
            }
        }

        var ts = TimeSpan.FromSeconds(HuntTime);

        var timerText = HuntTimeObj.GetComponent<TextMeshPro>();

        var colour = HuntTime switch
        {
            <= 10f => Color.red,
            <= 20f => Color.yellow,
            _ => Color.white
        };

        if (!MeetingHud.Instance && roleHunt != null)
        {
            HuntTimeObj.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-0.6f, 5.5f);
            HuntTimeObj.GetComponent<AspectPosition>().Alignment = AspectPosition.EdgeAlignments.Bottom;

            string text = "";
            if (roleHunt is Shikari) text = "A Shikari is in Execution!";

            timerText.text =
                $"<size=150%>{roleHunt.RoleColor.ToTextColor()}{text}</color></size>\n";// +
                //$"<size=200%>{colour.ToTextColor()}Time:{colour.ToTextColor()}{ts.ToString(@"mm\:ss", OWPlugin.Culture)}</color></size>\n";

            TimerSpriteObj.transform.localPosition = new Vector3(-1f, -0.4f, 1f);
        }
        else
        {
            HuntTimeObj.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-0.25f, 0.9f);
            HuntTimeObj.GetComponent<AspectPosition>().Alignment = AspectPosition.EdgeAlignments.Bottom;
            //timerText.text = $"<size=130%>{colour.ToTextColor()}Day {DayNightMechanic.DayCount}</color></size>";
            timerText.text = "";
            TimerSpriteObj.transform.localPosition = new Vector3(-1f, -0.25f, 1f);
        }

        HuntTimeObj.SetActive(!ExileController.Instance && roleHunt != null);
    }

    public static void BeginHunt()
    {
        if (Enabled)
            return;

        Enabled = true;
    }

    public static void StopHunt()
    {
        Enabled = false;
        HuntTime = 0;
        roleHunt = null;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePatch(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null || PlayerControl.LocalPlayer.Data.Role == null || !ShipStatus.Instance || TutorialManager.InstanceExists || AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
            return;

        UpdateHuntTime(__instance);
    }

    [RegisterEvent] // Stops Hunt if they die.
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (roleHunt != null)
        {
            var target = @event.Target;
            if (target.Data.Role is ICustomAURole customRole && customRole == roleHunt)
            {
                StopHunt();
            }
        }
    }
}