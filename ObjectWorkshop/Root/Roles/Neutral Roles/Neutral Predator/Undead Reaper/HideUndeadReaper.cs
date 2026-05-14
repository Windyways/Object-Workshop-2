using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using TMPro;
using TownOfUs.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ObjectWorkshop;

[HarmonyPatch]
public static class HideUndeadReaper
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    public static void HideGhosts()
    {
        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.Data.IsDead)
        {
            return;
        }

        if (MeetingHud.Instance)
        {
            return;
        }

        if (!OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data.IsDead)
            {
                continue;
            }

            var show = player.GetTrueRole() is not UndeadReaper;
            var bodyForms = player.gameObject.transform.GetChild(1).gameObject;

            foreach (var form in bodyForms.GetAllChildren())
            {
                if (form.activeSelf)
                {
                    form.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, show ? 1f : 0f);
                }
            }

            if (player.cosmetics.HasPetEquipped())
            {
                player.cosmetics.CurrentPet.Visible = show;
            }

            player.cosmetics.gameObject.SetActive(show);
            player.gameObject.transform.GetChild(3).gameObject.SetActive(show);
        }
    }
}