using System.Text;
using UnityEngine;

namespace ObjectWorkshop.Roles;

public sealed class Peacock(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Peacock";
    public string RoleDescription => "Paralyze players, win with your Associate!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Peacock;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Neutral;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.NeutralAssociative;

    public string RevealText => "is a beautiful bird.";
    public string Description => $"Declare an Associate, and Bloom to paralyze nearby opponents. Ensure your Associate’s victory.";
    public string Intro => "You are a beautiful bird that uses their appearance to immobilize potential enemies.";
    public string VictoryCondition => $"Make sure your Associate is victorious. You will win with your Associate.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = OWAssets.Peacock,
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
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Declare",
            "You can Declare a player during the round.\n" +
            "You will select your target as your Associate. You will learn their role. Your target will learn your identity. Your target will become Pale Immune for the rest of the game.\n" +
            "If you do not pick an Associate during the first round, one will be randomly assigned.\n" +
            "You cannot whisper.\n" +
            "You will die if your Associate dies, unless they become an Undead Reaper.",
            OWAssets.KillSprite),
            
        new("Bloom",
            "You can Bloom your feathers during the round.\n" +
            "You will become anonymous and you will grow colorful feathers.\n" +
            "Players that see you will become pale, immobilizing them. Your Associate is notified whenever a player is immobilized. Arachnids are immune.\n" +
            "If you have 3 Paled players at once, they will all gain confidence and become Pale Immune until the next round.\n" +
            "Using this ability while transformed will return yourself to normal. You cannot Bloom without an Associate.",
            OWAssets.KillSprite),
    ];

    public string GetPassives()
    {
        return
            "- At the start of the game, you are assigned an Associate.\n" +
            "- If your Associate dies before you find them, you are assigned a new one.\n" +
            "- You have a Shield to protect you from the first attack you receive. The Shield is removed upon Determining an Associate.";
    }

    [MethodRpc((uint)Rpcs.RpcDeclare)]
    public static void RpcDeclare(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not Peacock)
        {
            Logger<OWPlugin>.Error("RpcDeclare - Invalid Peacock");
            return;
        }

        var peacock = player.GetRole<Peacock>();
        player.RpcRemoveModifier<Shielded>();
        if (peacock != null)
        {
            target.AddModifier<Associate>(player);

            target.RpcAddModifier<RoleLearn>(player, false);
            player.RpcAddModifier<RoleLearn>(target, false);

            if (target.AmOwner()) target.Notify(Peacock_Feedback.NotifyAssociate(player), NotifyMode.InstantlyAndMeeting);
            if (player.AmOwner()) player.Notify(Peacock_Feedback.SetAssociate(target), NotifyMode.InstantlyAndMeeting);
        }
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (player.AmOwner() && OptionGroupSingleton<Peacock_Options>.Instance.EnableShield) player.RpcAddModifier<Shielded>();
    }

    [MethodRpc((uint)Rpcs.RpcBloom)]
    public static void RpcBloom(PlayerControl player)
    {
        if (player.Data.Role is not Peacock)
        {
            Logger<OWPlugin>.Error("RpcBloom - Invalid Peacock");
            return;
        }

        var peacock = player.GetRole<Peacock>();
        if (peacock != null)
        {
            if (peacock.isBloomed)
            {
                peacock.isBloomed = false;
                player.RpcRemoveModifier<Anonymous>();
                
                var peacockVisual = PeacockVisual.GetObjectByOwner(player);
                if (peacockVisual != null)
                {
                    peacockVisual.ForceMobilePlayers();
                    Destroy(peacockVisual.gameObject);
                }
            }
            else
            {
                peacock.isBloomed = true;
                player.RpcAddModifier<Anonymous>();

                PeacockVisual.Begin(peacock);
            }
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var Associate = Player.GetAssociate();
        if (Associate == null) return false;
        return Associate.Data.Role.DidWin(gameOverReason);
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public void LobbyStart()
    {
        PeacockVisual.CleanUp();
    }

    public void Role_OnMeetingStart()
    {
        var Associate = Player.GetAssociate();
        if (isBloomed)
        {
            isBloomed = false;
            Player.RpcRemoveModifier<Anonymous>();

            var peacock = PeacockVisual.GetObjectByOwner(Player);
            if (peacock != null)
            {
                peacock.ForceMobilePlayers();
                Destroy(peacock.gameObject);
            }
        }
        else if (Associate == null)
        {
            List<PlayerControl> potentialTargets = new List<PlayerControl>();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player != Player && !player.HasDied())
                {
                    potentialTargets.Add(player);
                }
            }

            potentialTargets.Shuffle();
            RpcDeclare(Player, potentialTargets[0]);
        }
    }

    public void Role_OnDeath(PlayerControl? player)
    {
        var Associate = Player.GetAssociate();
        if (Associate != null && player != null)
        {
            if (Associate == player && Associate.HasDied())
            {
                if (Associate.IsRole<UndeadReaper>())
                {
                    if (!UndeadReaper.ReapersAlive()) Player.RpcCustomMurder(Player);
                }
                else
                {
                    Player.RpcCustomMurder(Player);
                    VisitingMechanic.RpcAddDeathReason(Player, (int)DeathReasonShow.Suicide);
                }
            }
        }
        else if (player == Player && isBloomed)
        {
            isBloomed = false;
            player.RpcRemoveModifier<Anonymous>();

            var peacockVisual = PeacockVisual.GetObjectByOwner(player);
            if (peacockVisual != null)
            {
                peacockVisual.ForceMobilePlayers();
                Destroy(peacockVisual.gameObject);
            }
        }
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            RpcDeclare(Player, target);
        }
        else if (Button == 2) RpcBloom(Player);
    }

    public bool isBloomed;
}

public sealed class Peacock_Declare : ObjectWorkshopRoleButton<Peacock, PlayerControl>
{
    public override string Name => "Declare";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Peacock;
    public override float Cooldown => 1f;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: x => 
            x.IsTargetable());
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, false, true);
    public override bool CanUse()
    {
        return base.CanUse() && !Player.HasAssociate();
    }
}

public sealed class Peacock_Bloom : ObjectWorkshopRoleButton<Peacock>
{
    public override string Name => "Bloom";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Peacock;
    public override float Cooldown => OptionGroupSingleton<Peacock_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override bool CanUse()
    {
        return base.CanUse() && Player.HasAssociate();
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
}

public static class Peacock_Feedback
{
    public static string SetAssociate(PlayerControl player)
    {
        return $"You have declared {player.Name()} as your Associate!".ApplyKeywords();
    }

    public static string VictimFroze()
    {
        return $"You were stunned by the Peacock's beautiful feathers!!".ApplyKeywords();
    }

    public static string Immobilize()
    {
        return $"The Peacock has Immobilized a player!".ApplyKeywords();
    }

    public static string NotifyAssociate(PlayerControl player)
    {
        return $"{player.Name()} has shown you their colorful feathers. They're the Peacock and you are now their Associate!".ApplyKeywords();
    }
}

public sealed class Peacock_Options : AbstractOptionGroup<Peacock>
{
    public override string GroupName => "Peacock";

    [ModdedNumberOption("<color=#d8a0ff>Peacock</color> <color=#4a86e8>Bloom</color> Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25;

    [ModdedToggleOption("<color=#4a86e8>Paled</color> Players Are <color=#4a86e8>Blocked</color>")]
    public bool BlockParalyzed { get; set; } = false;

    [ModdedToggleOption("Enable <color=#d8a0ff>Peacock</color> <color=#0000ff>Shield</color>")]
    public bool EnableShield { get; set; } = true;
}