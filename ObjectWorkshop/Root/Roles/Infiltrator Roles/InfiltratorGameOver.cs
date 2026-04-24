using MiraAPI.GameEnd;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.CovenRoles;

public sealed class InfiltratorGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners is not [{ Role: RoleBehaviour role and ICustomAURole tRole }])
        {
            return false;
        }

        var mainRole = role;

        Logger<OWPlugin>.Error($"VerifyCondition - mainRole: '{mainRole.NiceName}', IsDead: '{role.IsDead}'");

        if (role.IsDead)
        {
            mainRole = role.Player.GetRoleWhenAlive();

            Logger<OWPlugin>.Error($"VerifyCondition - RoleWhenAlive: '{mainRole?.NiceName}'");
        }

        return tRole.WinConditionMet();
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, RoleColors.Infiltrator);

        var text = Object.Instantiate(endGameManager.WinText);
        text.text = $"Infiltrator Win!";
        text.color = RoleColors.Infiltrator;
        GameHistory.WinningFaction = $"<color=#{RoleColors.Infiltrator.ToHtmlStringRGBA()}>Infiltrator</color>";

        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localScale = new Vector3(1f, 1f, 1f);

        text.transform.position = pos;
        text.text = $"<size=4>{text.text}</size>";
        
        //AUSAssets.PlaySound(AUSAssets.CovenWin_SFX);
    }

    public static bool SabotageWin;
    public static bool AnyWon(GameOverReason gameOverReason)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Is(Faction.Infiltrator) && WinConditionMet()) return true;
        }
        return false;
    }

    public static bool WinConditionMet()
    {
        var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && !x.Is(Faction.Infiltrator));
        return alivePlayers == 0 || SabotageWin;
    }
}