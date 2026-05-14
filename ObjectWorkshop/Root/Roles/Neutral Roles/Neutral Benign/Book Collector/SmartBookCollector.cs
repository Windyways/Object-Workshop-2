using System.Collections;
using UnityEngine;

namespace ObjectWorkshop.MCI.SmartMCI;

public static class SmartBookCollector
{
    public static void Start(BookCollector bookCollector)
    {
        if (Debugger.IsDebuggerActive && Debugger.SmartBotsEnabled && !bookCollector.Player.AmOwner && !bookCollector.Player.HasDied()) Coroutines.Start(DelayStart(bookCollector));
    }

    public static IEnumerator DelayStart(BookCollector bookCollector)
    {
        yield return new WaitForSeconds(DayNightMechanic.PostMeetingIntroTime);
        var deadRoles = new List<RoleBehaviour>();
        foreach (var player in PlayerControl.AllPlayerControls.ToArray().Where(x => x.HasDied()))
        {
            var role = player.GetTrueRole();
            if (role != null && role is not Cadet) deadRoles.Add(role);
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray().Where(x =>
            x != bookCollector.Player && !x.HasDied() && !bookCollector.GuessedPlayers.Contains(x))
            .OrderByDescending(x => x.HasModifier<GlobalReveal>())
            .ThenByDescending(x => x.HasModifier<TI>())
            .ThenByDescending(x => x.HasModifier<Confirmed>())
            .ThenByDescending(x => x.HasModifier<ConfirmedEvil>())
            .ThenByDescending(x => x.HasModifier<SoftCleared>())
            .ThenByDescending(x => x.HasModifier<SeenKill>())
            .ThenByDescending(x => UnityEngine.Random.value)
            )
        {

            if (bookCollector.GuessedPlayers.Count >= (int)OptionGroupSingleton<BookCollector_Options>.Instance.Required)
                break;

            if (player.TryGetModifier<Confirmed>(out var confirmed))
            {
                if (confirmed.Source == "UFO")
                {
                    if (!ClickHandler(bookCollector, CustomExtentions.GetRoleBehaviourFromRole<UFO>()!, player))
                        break;
                }
                else if (confirmed.Source == "Luminescence")
                {
                    if (!ClickHandler(bookCollector, CustomExtentions.GetRoleBehaviourFromRole<Luminescence>()!, player))
                        break;
                }
            }
            else if (player.TryGetModifier<TI>(out var ti))
            {
                if (!ClickHandler(bookCollector, CustomExtentions.GetRoleBehaviourFromRole<Alarum>()!, player))
                    break;
            }
            else if (player.TryGetModifier<GlobalReveal>(out var globalReveal))
            {
                if (!ClickHandler(bookCollector, player.Data.Role, player))
                    break;
            }
            else if (player.TryGetModifier<SoftCleared>(out var softCleared))
            {
                var guessed = GuessedAs.Where(x => x.Item1 == player);
                var roleToGuess = CustomExtentions.GetRandomRolesBehaviourFromFaction(Faction.Crewmate).Where(x => !guessed.Contains((player, x)) &&
                    !deadRoles.Contains(x)).Random();
                if (roleToGuess != null)
                {
                    if (!ClickHandler(bookCollector, roleToGuess, player))
                        break;
                }
            }
            else if (player.TryGetModifier<ConfirmedEvil>(out var confirmedEvil))
            {
                var guessed = GuessedAs.Where(x => x.Item1 == player);
                var roleToGuess = CustomExtentions.GetRandomRolesBehaviourFromFaction(Faction.Infiltrator).Where(x => !guessed.Contains((player, x)) &&
                    !deadRoles.Contains(x)).Random();
                if (roleToGuess != null)
                {
                    if (!ClickHandler(bookCollector, roleToGuess, player))
                        break;
                }
            }
            else if (player.TryGetModifier<SeenKill>(out var seenKill))
            {
                var guessed = GuessedAs.Where(x => x.Item1 == player);
                var roleToGuess = CustomExtentions.GetRandomRolesBehaviourFromFaction(Faction.Infiltrator).Where(x => !guessed.Contains((player, x)) &&
                    !deadRoles.Contains(x)).Random();
                if (roleToGuess != null)
                {
                    if (!ClickHandler(bookCollector, roleToGuess, player))
                        break;
                }
            }
            else
            {
                var guessed2 = GuessedAs.Where(x => x.Item1 == player);
                var roleToGuess2 = CustomExtentions.GetRandomRolesBehaviourFromFaction(Faction.Crewmate).Where(x => !guessed2.Contains((player, x)) &&
                        !deadRoles.Contains(x)).Random();
                if (roleToGuess2 != null)
                {
                    if (!ClickHandler(bookCollector, roleToGuess2, player))
                        break;
                }
            }
        }
    }

    public static List<(PlayerControl, RoleBehaviour)> GuessedAs = new List<(PlayerControl, RoleBehaviour)>();

    public static bool ClickHandler(BookCollector bookCollector, RoleBehaviour role, PlayerControl target)
    {
        var Player = bookCollector.Player;

        if (role.NiceName == target.Data.Role.NiceName)
        {
            target.AddModifier<RoleLearn>(Player, false);

            bookCollector.GuessedPlayers.Add(target);
            Player.Notify(BookCollector_Feedback.GuessedCorrectly(role, target), NotifyMode.InstantlyAndMeeting);

            if (bookCollector.GuessedPlayers.Count >= (int)OptionGroupSingleton<BookCollector_Options>.Instance.Required)
            {
                Feedback.RpcNotifyAll(Player, BookCollector_Feedback.ExitInVictory(Player), NotifyMode.InstantlyAndMeeting, false);
                foreach (var guessed in bookCollector.GuessedPlayers) guessed.RpcAddModifier<GlobalReveal>();

                if (Player.AmOwner())
                {
                    Player.RpcCustomMurder(Player, createDeadBody: false, teleportMurderer: false, showKillAnim: false);
                    VisitingMechanic.RpcAddDeathReason(Player, (int)DeathReasonShow.ExitedInVictory);
                }
            }

            return true;
        }
        else
        {
            bookCollector.meetingMenu.HideButtons();
            Player.Notify(BookCollector_Feedback.FailedToGuess(role, target), NotifyMode.InstantlyAndMeeting);
        }

        return false;
    }
}
