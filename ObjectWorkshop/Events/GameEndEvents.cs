using MiraAPI.GameEnd;
using TownOfUs.GameOver;
using TownOfUs.Patches;

namespace TownOfUs.Events;

public static class EndGameEvents
{
    public static int winType;

    [RegisterEvent(-100)]
    public static void OnGameEnd(GameEndEvent @event)
    {
        winType = 0;
        var reason = EndGameResult.CachedGameOverReason;
        var neutralWinner = CustomRoleUtils.GetActiveRolesOfTeam(ModdedRoleTeams.Custom)
            .Any(x => x is ICustomAURole role && role.WinConditionMet());

        if (neutralWinner)
        {
            return;
        }

        if (reason is GameOverReason.CrewmatesByVote or GameOverReason.CrewmatesByTask
            or GameOverReason.ImpostorDisconnect)
        {
            winType = 1;
            GameHistory.WinningFaction = $"<color=#{RoleColors.Crewmate.ToHtmlStringRGBA()}>Crewmate</color>";
        }
        else if (reason is GameOverReason.ImpostorsByKill or GameOverReason.ImpostorsBySabotage
                 or GameOverReason.ImpostorsByVote or GameOverReason.CrewmateDisconnect)
        {
            winType = 2;
            GameHistory.WinningFaction = $"<color=#{RoleColors.Infiltrator.ToHtmlStringRGBA()}>Infiltrator</color>";
        }

        if (reason == CustomGameOver.GameOverReason<DrawGameOver>())
        {
            winType = 0;
        }
    }

    [RegisterEvent]
    public static void GameEndEventHandler(GameEndEvent @event)
    {
        EndGamePatches.BuildEndGameSummary(@event.EndGameManager);
    }
}