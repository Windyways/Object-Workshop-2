using MiraAPI.GameEnd;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.NeutralRoles;

public sealed class NeutralGameOver : CustomGameOver
{
    private Color _roleColor;
    private string _roleName = "PLACEHOLDER";

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

        _roleName = mainRole!.NiceName;
        _roleColor = mainRole.TeamColor;

        return tRole.WinConditionMet();
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, _roleColor);

        var text = Object.Instantiate(endGameManager.WinText);
        text.text = $"{_roleName} Wins!";
        text.color = _roleColor;
        GameHistory.WinningFaction = $"<color=#{_roleColor.ToHtmlStringRGBA()}>{_roleName}</color>";

        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localScale = new Vector3(1f, 1f, 1f);

        text.transform.position = pos;
        text.text = $"<size=4>{text.text}</size>";
    }

    public static bool WinConditionMet(RoleBehaviour role)
    {
        if (role.Player.HasDied()) return false;

        var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x != role.Player && x.Data.Role is not INotThreatable);
        return alivePlayers == 0;
    }

    public static bool AnyWon(GameOverReason gameOverReason, RoleBehaviour role)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            var pRole = player.Data.Role;
            var pICR = player.Data.Role as ICustomAURole;
            if (pRole == role && pICR.WinConditionMet()) return true;
        }
        return false;
    }
}