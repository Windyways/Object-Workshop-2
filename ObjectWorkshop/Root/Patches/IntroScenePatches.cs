using MiraAPI.Modifiers.ModifierDisplay;
using MiraAPI.Modifiers.Types;
using TMPro;
using UnityEngine;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options;

namespace ObjectWorkshop.Patches;

[HarmonyPatch]
public static class IntroScenePatches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    [HarmonyPrefix]
    public static bool BeginCrewmatePatch(IntroCutscene __instance)
    {
        if (PlayerControl.LocalPlayer.Is(Faction.Crewmate))
        {
            __instance.TeamTitle.text = "Crewmate";
            __instance.TeamTitle.color = RoleColors.Crewmate;

            var yourTeam = PlayerControl.AllPlayerControls.ToArray().ToList();
            GenerateYourTeam(__instance, yourTeam);
        }
        else if (PlayerControl.LocalPlayer.Is(Faction.Infiltrator))
        {
            __instance.TeamTitle.text = "Infiltrator";
            __instance.TeamTitle.color = RoleColors.Infiltrator;

            var yourTeam = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Infiltrator)).ToList();
            GenerateYourTeam(__instance, yourTeam);
        }
        else if (PlayerControl.LocalPlayer.Is(Faction.Neutral))
        {
            __instance.TeamTitle.text = "Neutral";
            __instance.TeamTitle.color = RoleColors.Neutral;

            var player = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
            __instance.ourCrewmate = player;
        }
        else
        {
            var player = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
            __instance.ourCrewmate = player;
        }

        return false;
    }

    public static void GenerateYourTeam(IntroCutscene __instance, List<PlayerControl> yourTeam, bool infiltrator = false)
    {
        // Found via DNspy. Ty Le Killer.
        for (int i = 0; i < yourTeam.Count; i++)
        {
            PlayerControl playerControl = yourTeam[i];
            if (playerControl)
            {
                NetworkedPlayerInfo data = playerControl.Data;
                if (!(data == null))
                {
                    PoolablePlayer poolablePlayer = __instance.CreatePlayer(i, 1, data, infiltrator);
                    if (i == 0 && data.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        __instance.ourCrewmate = poolablePlayer;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    [HarmonyPrefix]
    public static void IntroCutsceneOnDestroyPatch()
    {
        HudManager.Instance.SetHudActive(false);
        HudManager.Instance.SetHudActive(true);

        foreach (var button in CustomButtonManager.Buttons.Where(x => x.Enabled(PlayerControl.LocalPlayer.Data.Role)))
        {
            if (button is FakeVentButton)
            {
                continue;
            }

            button.SetTimer(OptionGroupSingleton<GeneralOptions>.Instance.GameStartCd);
        }

        if (PlayerControl.LocalPlayer.IsImpostor())
        {
            PlayerControl.LocalPlayer.SetKillTimer(OptionGroupSingleton<GeneralOptions>.Instance.GameStartCd);
        }

        var modsTab = ModifierDisplayComponent.Instance;
        if (modsTab != null && !modsTab.IsOpen && PlayerControl.LocalPlayer.GetModifiers<GameModifier>()
                .Any(x => !x.HideOnUi && x.GetDescription() != string.Empty))
        {
            modsTab.ToggleTab();
        }

        var panelThing = HudManager.Instance.TaskStuff.transform.FindChild("RolePanel");
        if (panelThing != null)
        {
            var panel = panelThing.gameObject.GetComponent<TaskPanelBehaviour>();
            var role = PlayerControl.LocalPlayer.Data.Role as ICustomRole;
            if (role == null)
            {
                return;
            }

            panel.open = true;

            var tabText = panel.tab.gameObject.GetComponentInChildren<TextMeshPro>();
            var ogPanel = HudManager.Instance.TaskStuff.transform.FindChild("TaskPanel").gameObject
                .GetComponent<TaskPanelBehaviour>();
            if (tabText.text != role.RoleName)
            {
                tabText.text = role.RoleName;
            }

            var y = ogPanel.taskText.textBounds.size.y + 1;
            panel.closedPosition = new Vector3(ogPanel.closedPosition.x, ogPanel.open ? y + 0.2f : 2f,
                ogPanel.closedPosition.z);
            panel.openPosition = new Vector3(ogPanel.openPosition.x, ogPanel.open ? y : 2f, ogPanel.openPosition.z);

            panel.SetTaskText(role.SetTabText().ToString());
        }
    }

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close))]
    [HarmonyPrefix]
    public static void SpawnInMinigameClosePatch()
    {
        IntroCutsceneOnDestroyPatch();
    }
}

public static class ModifierIntroPatch
{
    private static TextMeshPro ModifierText;

    public static void RunModChecks()
    {
        ModifierText.text = string.Empty;
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    public static class IntroCutscene_BeginCrewmate
    {
        public static void Postfix(IntroCutscene __instance)
        {
            ModifierText =
                UnityEngine.Object.Instantiate(__instance.RoleText, __instance.RoleText.transform.parent, false);
            SetHiddenImpostors(__instance);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    public static class IntroCutscene_BeginImpostor
    {
        public static void Postfix(IntroCutscene __instance)
        {
            ModifierText =
                UnityEngine.Object.Instantiate(__instance.RoleText, __instance.RoleText.transform.parent, false);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__35), nameof(IntroCutscene._CoBegin_d__35.MoveNext))]
    public static class ShowModifierPatch_CoBegin
    {
        public static void Postfix(IntroCutscene._ShowRole_d__41 __instance)
        {
            HudManagerPatches.ResetZoom();
            if (PlayerControl.LocalPlayer.Data.Role is ICustomAURole custom)
            {
                __instance.__4__this.RoleText.text = custom.RoleName;
                if (__instance.__4__this.YouAreText.transform.TryGetComponent<TextTranslatorTMP>(out var tmp))
                {
                    tmp.defaultStr = custom.YouAreText;
                    tmp.TargetText = StringNames.None;
                    tmp.ResetText();
                }

                __instance.__4__this.RoleBlurbText.text = custom.RoleDescription;
            }

            if (ModifierText == null)
            {
                return;
            }

            RunModChecks();

            ModifierText.transform.position =
                __instance.__4__this.transform.position - new Vector3(0f, 1.6f, -10f);
            ModifierText.gameObject.SetActive(true);
            ModifierText.color.SetAlpha(0.8f);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene._ShowTeam_d__38), nameof(IntroCutscene._ShowTeam_d__38.MoveNext))]
    public static class ShowModifierPatch_MoveNext
    {
        public static void Postfix(IntroCutscene._ShowRole_d__41 __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role is ICustomAURole custom)
            {
                __instance.__4__this.RoleText.text = custom.RoleName;
                if (__instance.__4__this.YouAreText.transform.TryGetComponent<TextTranslatorTMP>(out var tmp))
                {
                    tmp.defaultStr = custom.YouAreText;
                    tmp.TargetText = StringNames.None;
                    tmp.ResetText();
                }

                __instance.__4__this.RoleBlurbText.text = custom.RoleDescription;
            }

            if (ModifierText == null)
            {
                return;
            }

            RunModChecks();

            ModifierText.transform.position =
                __instance.__4__this.transform.position - new Vector3(0f, 1.6f, -10f);
            ModifierText.gameObject.SetActive(true);
            ModifierText.color.SetAlpha(0.8f);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene._ShowRole_d__41), nameof(IntroCutscene._ShowRole_d__41.MoveNext))]
    [HarmonyPriority(Priority.Last)]
    public static class ShowModifierPatch_Role
    {
        public static void Postfix(IntroCutscene._ShowRole_d__41 __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role is ICustomAURole custom)
            {
                __instance.__4__this.RoleText.text = custom.RoleName;
                __instance.__4__this.YouAreText.text = custom.YouAreText;
                __instance.__4__this.RoleBlurbText.text = custom.RoleDescription;
            }

            var teamModifier = PlayerControl.LocalPlayer.GetModifiers<TouGameModifier>().FirstOrDefault();
            if (ModifierText == null)
            {
                return;
            }

            RunModChecks();

            ModifierText.transform.position =
                __instance.__4__this.transform.position - new Vector3(0f, 1.6f, -10f);
            ModifierText.gameObject.SetActive(true);
            ModifierText.color.SetAlpha(0.8f);
        }
    }

    public static void SetHiddenImpostors(IntroCutscene __instance)
    {
        var infiltratorAmount = Helpers.GetAlivePlayers().Count(x => x.IsImpostor());
        if (infiltratorAmount == 1) __instance.ImpostorText.text = $"There is {infiltratorAmount} <color=#ff5050>Infiltrator</color> among us.";
        else if (infiltratorAmount > 0) __instance.ImpostorText.text = $"There are {infiltratorAmount} <color=#ff5050>Infiltrators</color> among us.";

        var players = GameData.Instance.PlayerCount;

        if (players < 7)
        {
            return;
        }

        var list = OptionGroupSingleton<RoleOptions>.Instance;

        int maxSlots = players < 15 ? players : 15;

        List<RoleListOption> buckets = [];
        for (int i = 0; i < maxSlots; i++)
        {
            int slotValue = i switch
            {
                0 => list.Slot1,
                1 => list.Slot2,
                2 => list.Slot3,
                3 => list.Slot4,
                4 => list.Slot5,
                5 => list.Slot6,
                6 => list.Slot7,
                7 => list.Slot8,
                8 => list.Slot9,
                9 => list.Slot10,
                10 => list.Slot11,
                11 => list.Slot12,
                12 => list.Slot13,
                13 => list.Slot14,
                14 => list.Slot15,
                _ => -1
            };

            buckets.Add((RoleListOption)slotValue);
        }

        if (!buckets.Any(x => x is RoleListOption.Any)) return;

        __instance.ImpostorText.text = $"There is ??? <color=#ff5050>Infiltrators</color> among us.";
    }
}