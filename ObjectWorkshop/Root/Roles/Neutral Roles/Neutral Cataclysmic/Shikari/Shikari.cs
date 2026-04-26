using System.Text;
using UnityEngine;

namespace ObjectWorkshop.Roles;

public sealed class Shikari(IntPtr cppPtr) : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Shikari";
    public string RoleDescription => "Mark and Execute everyone.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Shikari;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Neutral;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.NeutralCataclysmic;

    public string RevealText => "is a silent executioner.";
    public string Description => "Mark players by clicking on them. Mark everyone to begin the Execution Phase, clicking to kill. Know the locations of those who are not marked, and then those who are marked during the Execution Phase.";
    public string Intro => "You are a hunter that sneakily marks those who oppose you for execution.";
    public string VictoryCondition => "Mark and Execute all who oppose you. Kill everyone. You will win alone.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<Shikari_Options>.Instance.CanVent,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"{Intro}\n\n{Description}\n\n" +
            $"<color=#00ff00>Victory Condition:</color>\n" +
            $"{VictoryCondition}" +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Mark",
            $"You can Mark a player for execution during the round by clicking on them.\n" +
            "You will Mark your target for execution. All players except your target can see the Mark. Once all players are Marked, the Execution Phase will begin.\n" +
            "You know the locations of non-Marked players.",
            OWAssets.KillSprite),

        new("Execute",
            $"You can Execute a Marked player during the round by clicking on them..\n" +
            "You will kill your target remotely.\n" +
            "You know the locations of Marked players in this phase. This ability is only usable once all players are Marked.",
            OWAssets.KillSprite),

        new("EXECUTION PHASE",
            $"When the Execution Phase begins, all players are notified.\n" +
            "During the Execution Phase, bodies cannot be reported.",
            TouAssets.BlankSprite),
    ];

    public string GetPassives()
    {
        return "- You have a Shield to protect you from the first attack you receive. The Shield is removed upon entering the Execution Phase.";
    }

    public bool WinConditionMet()
    {
        var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied());
        if (Player.HasDied()) return false;
        return alivePlayers == 1;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (player.AmOwner())
        {
            if (OptionGroupSingleton<Shikari_Options>.Instance.EnableShield) player.RpcAddModifier<Shielded>();
            foreach (var p in PlayerControl.AllPlayerControls.ToArray().Where(x => x != Player))
            {
                Color color = Color.white;
                p.AddModifier<TrackerArrowTargetModifier>(player, color, 0.1f);
            }
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        Clear();
    }

    [MethodRpc((uint)Rpcs.RpcMark)]
    public static void RpcMark(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not Shikari)
        {
            Logger<OWPlugin>.Error("RpcMark - Invalid Shikari");
            return;
        }

        var shikari = player.GetRole<Shikari>();
        shikari.MarkedPlayers.Add(target);
        shikari.Clear(target);

        Marked.Begin(target);

        if (player.AmOwner) CustomButtonSingleton<Shikari_Mark>.Instance.ResetCooldownAndOrEffect();
        if (shikari.ExecutionPhase()) shikari.TriggerExe();
    }

    public void Role_OnDeath(PlayerControl? player)
    {
        if (ExecutionPhase())
        {
            if (!HuntMechanic.Enabled) TriggerExe();
            if (player == Player) HuntMechanic.StopHunt();
        }
    }

    public void Role_OnRoundStart()
    {
        if (ExecutionPhase() && !HuntMechanic.Enabled) TriggerExe();
    }

    public void TriggerExe()
    {
        if (Player.HasDied())
            return;

        Player.RpcRemoveModifier<Shielded>();

        HuntMechanic.roleHunt = this;
        HuntMechanic.BeginHunt();

        if (Player.AmOwner())
        {
            foreach (var players in MarkedPlayers.ToArray().Where(x => !x.HasDied()))
            {
                Color color = Color.white;
                players.AddModifier<TrackerArrowTargetModifier>(Player, color, 0.1f);
            }
        }
    }

    public void Clear()
    {
        var players = ModifierUtils.GetPlayersWithModifier<TrackerArrowTargetModifier>([HideFromIl2Cpp] (x) => x.Owner == Player);
        foreach (var player in players)
        {
            player.RemoveModifier<TrackerArrowTargetModifier>();
        }
    }

    public void Clear(PlayerControl p)
    {
        var players = ModifierUtils.GetPlayersWithModifier<TrackerArrowTargetModifier>([HideFromIl2Cpp] (x) => x.Owner == Player && x.Player == p);
        foreach (var player in players)
        {
            player.RemoveModifier<TrackerArrowTargetModifier>();
        }
    }

    public void Mark(PlayerControl target)
    {
        RpcMark(Player, target);
    }

    public void Execute(PlayerControl target)
    {
        Player.RpcCustomMurder(target, teleportMurderer: false);
        VisitingMechanic.RpcAddDeathReason(target, (int)DeathReasonShow.Executed);
    }

    public bool ExecutionPhase() => 
        MarkedPlayers.Count(x => !x.HasDied()) >= PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x != Player);

    public List<PlayerControl> MarkedPlayers = new List<PlayerControl>();
}

public sealed class Shikari_Mark : ObjectWorkshopRoleButton<Shikari>
{
    public override string Name => "Mark";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Shikari;
    public override float Cooldown => OptionGroupSingleton<Shikari_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, true, true);

    public override bool CanUse()
    {
        return false;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && !Role.ExecutionPhase();
    }
}

public sealed class Shikari_Execute : ObjectWorkshopRoleButton<Shikari>
{
    public override string Name => "Execute";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Shikari;
    public override float Cooldown => 0.1f;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, true, true);

    public override bool CanUse()
    {
        return false;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role.ExecutionPhase();
    }
}

public sealed class Shikari_Options : AbstractOptionGroup<Shikari>
{
    public override string GroupName => "Shikari";

    [ModdedNumberOption("<color=#52d65a>Shikari</color> <color=#4a86e8>Mark</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedToggleOption("<color=#52d65a>Shikari</color> Can Vent")]
    public bool CanVent { get; set; } = false;

    [ModdedToggleOption("Enable <color=#52d65a>Shikari</color> <color=#0000ff>Shield</color>")]
    public bool EnableShield { get; set; } = true;
}