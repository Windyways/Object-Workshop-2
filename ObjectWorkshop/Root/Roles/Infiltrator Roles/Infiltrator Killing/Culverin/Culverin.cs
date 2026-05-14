using System.Text;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;
using static UnityEngine.GraphicsBuffer;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Roles;

public sealed class Culverin(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Culverin";
    public string RoleDescription => "Take players down with a Cannon.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Infiltrator;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Infiltrator;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Infiltrator;
    public Alignment Alignment => Alignment.InfiltratorKilling;

    public string RevealText => "is an experienced pirate.";
    public string Description => "Fire cannonballs at players to kill them and anyone else that collides with it.";
    public string Intro => "You are a retired pirate that has expertise in cannons.";
    public string VictoryCondition => "Kill everyone that opposes the Infiltrators. You will win with other Infiltrator members.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseSabotage = OptionGroupSingleton<InfiltratorOptions>.Instance.CanSabotage,
        CanUseVent = OptionGroupSingleton<InfiltratorOptions>.Instance.CanVent,
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
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Attack",
            "You can Attack a player during the round.\n" +
            "You will kill your target.",
            OWAssets.KillSprite),

        new("Load",
            $"You can Load a cannonball into your cannon during the round.\n" +
            "You will load up your cannon and go into an aiming state, being unable to move.\n" +
            "You may use keys A and D to change the direction where you want to shoot. Using this ability again will fire a cannonball at the direction.\n" +
            "The cannonball will kill any players except yourself that collide with it. Cannonballs will expire after 20s.\n" +
            "Performing a critical sabotage earns you gold.",
            OWAssets.KillSprite),
    ];

    public void Role_OnMeetingStart()
    {
        CannonballBase.ClearAll();
    }

    public void Role_OnDeath(PlayerControl? player)
    {
        if (player == Player && isAiming)
        {
            var cannonballBase = CannonballBase.GetObjectByPlayer(Player);
            Object.Destroy(cannonballBase.directionLine);
            Object.Destroy(cannonballBase.gameObject);
        }
    }

    public void LobbyStart()
    {
        Cannonball.CleanUp();
        CannonballBase.CleanUp();
    }

    [MethodRpc((uint)Rpcs.RpcLoadAndShoot)]
    public static void RpcLoadAndShoot(PlayerControl player)
    {
        if (player.Data.Role is not Culverin)
        {
            Logger<OWPlugin>.Error("RpcLoadAndShoot - Invalid Culverin");
            return;
        }

        var culverin = player.GetRole<Culverin>();
        if (culverin.isAiming)
        {
            culverin.isAiming = false;

            var cannonballBase = CannonballBase.GetObjectByPlayer(player);
            cannonballBase.StartFire();

            if (player.AmOwner())
            {
                var button = CustomButtonSingleton<Culverin_LoadAndShoot>.Instance;
                button.OverrideName("Load");
            }
        }
        else
        {
            culverin.isAiming = true;
            CannonballBase.Begin(player);

            if (player.AmOwner())
            {
                var button = CustomButtonSingleton<Culverin_LoadAndShoot>.Instance;
                button.OverrideName("Shoot");
            }
        }
    }

    public void GainGold()
    {
        if (Player.AmOwner())
        {
            var button = CustomButtonSingleton<Culverin_LoadAndShoot>.Instance;
            button.IncreaseUses();
        }
    }

    [MethodRpc((uint)Rpcs.RpcMoveCannonball)]
    public static void RpcMoveCannonball(PlayerControl player, float speed)
    {
        if (player.Data.Role is not Culverin)
        {
            Logger<OWPlugin>.Error("RpcMoveCannonball - Invalid Culverin");
            return;
        }

        var culverin = player.GetRole<Culverin>();
        var cannonball = Cannonball.GetObjectByPlayer(culverin.Player);
        cannonball.transform.position += (Vector3)(cannonball.currentDirection * speed * Time.deltaTime);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            if (Player.CanKill(target))
            {
                Player.RpcCustomMurder(target);
                VisitingMechanic.RpcAddDeathReason(target, (int)DeathReasonShow.Killed);
            }
        }
        else if (Button == 2)
        {
            if (isAiming && Player.AmOwner)
            {
                var button = CustomButtonSingleton<Culverin_LoadAndShoot>.Instance;
                if (button.UsesLeft > 0) button.DecreaseUses();
            }

            RpcLoadAndShoot(Player);
        }
    }
    
    public bool isAiming;
}

public sealed class Culverin_Attack : ObjectWorkshopRoleButton<Culverin, PlayerControl>
{
    public override string Name => "Attack";
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
}

public sealed class Culverin_LoadAndShoot : ObjectWorkshopRoleButton<Culverin>
{
    public override string Name => "Load";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Infiltrator;
    public override float Cooldown => Role.isAiming ? 1f : OptionGroupSingleton<Culverin_Options>.Instance.Cooldown;
    public override int MaxUses => (int)OptionGroupSingleton<Culverin_Options>.Instance.Charges;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override bool DecreaseCharge => false;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (Button == null)
        {
            Logger<OWPlugin>.Error($"Button is null for {GetType().FullName}");
            return;
        }

        Button.usesRemainingSprite.sprite = OWAssets.AbilitySpriteGoldCounter.LoadAsset();

        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
            Button.usesRemainingSprite.color = new Color(1, 1, 0);
        }

        PassiveComp = Button.GetComponent<PassiveButton>();
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
}

public sealed class Culverin_Options : AbstractOptionGroup<Culverin>
{
    public override string GroupName => "Culverin";

    [ModdedNumberOption("<color=#ff5050>Culverin</color> <color=#4a86e8>Shoot</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedNumberOption("<color=#ff5050>Culverin</color> <color=#4a86e8>Cannonball</color> Speed", 1f, 25f, 0.25f, MiraNumberSuffixes.Seconds, "0.00")]
    public float CannonballSpeed { get; set; } = 5f;

    [ModdedNumberOption("<color=#ff5050>Culverin</color> Initial <color=#4a86e8>Cannonballs</color>", 1f, 30f, 1f, MiraNumberSuffixes.None)]
    public float Charges { get; set; } = 3f;
}