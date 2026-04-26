using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Roles;

public sealed class Arachnid(IntPtr cppPtr)
    : ImpostorRole(cppPtr), IWikiDiscoverable, ICustomAURole
{
    public string RoleName { get; set; } = "Arachnid";
    public string RoleDescription => "Slow prey in webs and take them down.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Infiltrator;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Infiltrator;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Infiltrator;
    public Alignment Alignment => Alignment.InfiltratorEvacuative;

    public string RevealText => "spins webs to catch prey.";
    public string Description => "Transform into a spider to kill players. Spin webs to slow them down and be notified. You cannot vent.";
    public string Intro => "You are a Sicarius that has a knack of knowing prey in webs.";
    public string VictoryCondition => "Kill everyone that opposes the Infiltrators. You will win with other Infiltrator members.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseSabotage = OptionGroupSingleton<InfiltratorOptions>.Instance.CanSabotage,
        CanUseVent = false,
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
        new("Bite",
            $"You can Bite a player during the round.\n" +
            "You will kill your target. You can only kill in your spider form.\n" +
            "If your target is in a Web, your cooldown will reset. If not, you will automatically place a Web.",
            OWAssets.KillSprite),

        new("Spin",
            $"You can Spin Webs during the round.\n" +
            "You will place a Web down, that persists through rounds. Players that collide with Webs are slowed down, and Webs that have slowed players down will destroy the next round.",
            OWAssets.KillSprite),

        new("Inner Spider",
            $"You can bring out your Inner Spider during the round.\n" +
            "You will transform into a spider, being anonymous to other players and move faster while in webs.\n" +
            "Using this ability again transforms you back to normal.",
            OWAssets.KillSprite),
    ];

    [MethodRpc((uint)Rpcs.RpcSpin)]
    public static void RpcSpin(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not Arachnid)
        {
            Logger<OWPlugin>.Error("RpcSpin - Invalid Arachnid");
            return;
        }
        
        var pos = player.GetAdjustedPosition();
        if (target != null) pos = target.GetAdjustedPosition();
        Webs.Begin(player, pos);
    }

    [MethodRpc((uint)Rpcs.RpcInnerSpider)]
    public static void RpcInnerSpider(PlayerControl player)
    {
        if (player.Data.Role is not Arachnid)
        {
            Logger<OWPlugin>.Error("RpcInnerSpider - Invalid Arachnid");
            return;
        }

        var arachnid = player.GetRole<Arachnid>();
        if (arachnid.isSpider)
        {
            arachnid.isSpider = false;

            player.RemoveModifier<InvisibleTogglable>();

            var spider = SpiderVisual.GetObjectByPlayer(player);
            spider.Stop();
        }
        else
        {
            arachnid.isSpider = true;

            player.AddModifier<InvisibleTogglable>();
            SpiderVisual.Begin(player);
        }
    }

    public void Role_OnDeath(PlayerControl? player)
    {
        if (player == null)
            return;

        if (player == Player && AmongUsClient.Instance.AmHost && isSpider)
        {
            isSpider = false;

            Player.RpcRemoveModifier<InvisibleTogglable>();

            var spider = SpiderVisual.GetObjectByPlayer(Player);
            spider.Stop();
        }
    }

    public void LobbyStart()
    {
        SpiderVisual.CleanUp();
        Webs.CleanUp();
    }

    public void Role_OnMeetingStart()
    {
        foreach (var web in Webs.AllWebs)
        {
            if (web.isTriggered) Object.Destroy(web.gameObject);
        }

        if (isSpider)
        {
            isSpider = false;

            Player.RpcRemoveModifier<InvisibleTogglable>();

            var spider = SpiderVisual.GetObjectByPlayer(Player);
            spider.Stop();
        }
    }

    public bool WinConditionMet() => InfiltratorGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || InfiltratorGameOver.AnyWon(gameOverReason);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            if (Player.CanKill(target))
            {
                Player.RpcCustomMurder(target);
                VisitingMechanic.RpcAddDeathReason(target, (int)DeathReasonShow.Bitten);

                if (!Webs.IsInWebs(target)) RpcSpin(Player, target);
            }
        }
        else if (Button == 2) RpcSpin(Player, null);
        else if (Button == 3) RpcInnerSpider(Player);
    }

    public bool isSpider;
    public List<Webs> WebsToClear = new List<Webs>();
}

public sealed class Arachnid_Bite : ObjectWorkshopRoleButton<Arachnid, PlayerControl>
{
    public override string Name => "Bite";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Infiltrator;
    public override float Cooldown => OptionGroupSingleton<InfiltratorOptions>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return Player.GetClosestLivingPlayer(true, Distance, predicate: x =>
            !x.Is(Faction.Infiltrator));
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, true, true);
    public override bool CanUse()
    {
        return base.CanUse() && Role.isSpider;
    }
}

public sealed class Arachnid_Spin : ObjectWorkshopRoleButton<Arachnid>
{
    public override string Name => "Spin";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Infiltrator;
    public override float Cooldown => OptionGroupSingleton<Arachnid_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
}

public sealed class Arachnid_InnerSpiderInfiltrator : ObjectWorkshopRoleButton<Arachnid>
{
    public override string Name => "Inner Spider";
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;
    public override Color TextOutlineColor => RoleColors.Infiltrator;
    public override float Cooldown => OptionGroupSingleton<Arachnid_Options>.Instance.InnerSpiderCD;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 3, false, false);
}

public sealed class Arachnid_Options : AbstractOptionGroup<Arachnid>
{
    public override string GroupName => "Arachnid";

    [ModdedNumberOption("<color=#ff5050>Arachnid</color> <color=#4a86e8>Spin</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedNumberOption("<color=#ff5050>Arachnid</color> <color=#4a86e8>Inner Spider</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float InnerSpiderCD { get; set; } = 25f;

    [ModdedNumberOption("<color=#ff5050>Arachnid</color> Speed In Webs", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float Speed { get; set; } = 1.75f;

    [ModdedNumberOption("Prey Speed In Webs", 0f, 5f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float PreySpeed { get; set; } = 0.75f;
}