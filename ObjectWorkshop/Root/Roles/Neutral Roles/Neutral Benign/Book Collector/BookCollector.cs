using MiraAPI.Events.Mira;
using Rewired;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace ObjectWorkshop.Roles;

public sealed class BookCollector(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Book Collector";
    public string RoleDescription => "Learn roles and guess players.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.BookCollector;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Neutral;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.NeutralBenign;

    public string RevealText => "lost books before departure.";
    public string Description => $"Guess the roles of {OptionGroupSingleton<BookCollector_Options>.Instance.Required} players, learning the roles in play by collecting books that spawn around the map.";
    public string Intro => "You are a librarian searching for lost books, revealing others true intentions in the process.";
    public string VictoryCondition => $"Guess {OptionGroupSingleton<BookCollector_Options>.Instance.Required} players and exit in victory. You will win with the winning team.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = OWAssets.BookCollector,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ICustomAURole.SetNewTabText(this);
        if (LearnedRoles.Count > 0)
        {
            stringB.AppendLine("Known Roles:");
            foreach (var info in LearnedRoles)
            {
                if (info is ICustomAURole customRole) stringB.AppendLine(OWPlugin.Culture, $"<color=#{customRole.RoleColor.ToHtmlStringRGBA()}>{info.NiceName}</color>");
            }
        }

        return stringB;
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
        new("Read",
            "You can Read a Book during the round.\n" +
            "You will learn a role in play that is not dead. You will be notified if that role dies.\n" +
            "ain a charge of Read whenever you collect a Book.\n" +
            "If you're attacked directly and have an alive guessed player, your attacker learns your identity and indirectly kills one of your guessed players.",
            OWAssets.KillSprite),

        new("Guess",
            "You can Guess a player's role during the meeting.\n" +
            "If you guess their role correctly, gain a point towards winning. If you’re incorrect, lose the ability to Guess until the next meeting.\n" +
            "Upon winning, reveal the roles of all players you guessed to all and exit in victory.",
            OWAssets.BookCollector_Guess),
    ];

    public string GetPassives()
    {
        return "- Books randomly spawn around the map, only visible to you. Upon collecting it, it will leave scraps behind, which all players can see.";
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return GuessedPlayers.Count >= (int)OptionGroupSingleton<BookCollector_Options>.Instance.Required;
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

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner())
        {
            meetingMenu = new MeetingMenu(
                this,
                ClickGuess,
                MeetingAbilityType.Click,
                OWAssets.BookCollector_Guess,
                null!,
                IsExempt)
            {
                Position = new Vector3(-0.6f, 0.18f, -3f),
                Scale = new Vector3(0.4f, 0.4f, 0.4f)
            };

            // Spawn Books!
            BookSpawner.AllBooks.Clear();
            BookSpawner.AvailableRoomsToSpawn.Clear();
            BookSpawner.AvailableRoomsToSpawn.GetAvailableRooms();
            Coroutines.Start(BookSpawner.Start());
        }
    }

    public void Role_OnMeetingStart()
    {
        if (Debugger.IsDebuggerActive && Debugger.SmartBotsEnabled) SmartBookCollector.Start(this);
        if (Player.AmOwner)
        {
            meetingMenu.GenButtons(MeetingHud.Instance, Player.AmOwner && !Player.HasDied());
        }
    }

    public void Role_OnDeath(PlayerControl? player)
    {
        if (player == null || Player.HasDied())
            return;

        if (LearnedRoles.Contains(player.GetRoleWhenAlive()))
        {
            Player.Notify(BookCollector_Feedback.RolePerished(player.GetRoleWhenAlive()), NotifyMode.InstantlyAndMeeting);
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);
        if (Player.AmOwner)
        {
            meetingMenu.HideButtons();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner())
        {
            meetingMenu.Dispose();
            meetingMenu = null!;
        }
    }

    public void ClickGuess(PlayerVoteArea voteArea, MeetingHud meetingHud)
    {
        if (meetingHud.state == MeetingHud.VoteStates.Discussion)
            return;

        if (Minigame.Instance != null)
            return;

        var target = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId).Object;

        var shapeMenu = GuesserMenu.Create();
        shapeMenu.Begin(IsRoleValid, ClickRoleHandle);

        void ClickRoleHandle(RoleBehaviour role)
        {
            ClickHandler(role, target);
        }

        void ClickHandler(RoleBehaviour role, PlayerControl target)
        {
            if (role.NiceName == target.Data.Role.NiceName)
            {
                target.AddModifier<RoleLearn>(Player, false);

                GuessedPlayers.Add(target);
                Player.Notify(BookCollector_Feedback.GuessedCorrectly(role, target), NotifyMode.InstantlyAndMeeting);

                if (GuessedPlayers.Count >= (int)OptionGroupSingleton<BookCollector_Options>.Instance.Required)
                {
                    Feedback.RpcNotifyAll(Player, BookCollector_Feedback.ExitInVictory(Player), NotifyMode.InstantlyAndMeeting, false);
                    foreach (var guessed in GuessedPlayers) guessed.RpcAddModifier<GlobalReveal>();

                    if (Player.AmOwner())
                    {
                        Player.RpcCustomMurder(Player, createDeadBody: false, teleportMurderer: false, showKillAnim: false);
                        VisitingMechanic.RpcAddDeathReason(Player, (int)DeathReasonShow.ExitedInVictory);
                    }
                }

                meetingMenu.HideSingle(target.PlayerId);
            }
            else
            {
                meetingMenu.HideButtons();
                Player.Notify(BookCollector_Feedback.FailedToGuess(role, target), NotifyMode.InstantlyAndMeeting);
            }

            shapeMenu.Close();
        }
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea?.TargetPlayerId == Player.PlayerId || Player.Data.IsDead || voteArea!.AmDead || 
            GuessedPlayers.Contains(MiscUtils.PlayerById(voteArea.TargetPlayerId));
    }

    private static bool IsRoleValid(RoleBehaviour role)
    {
        if (role.IsDead) return false;
        return true;
    }

    [MethodRpc((uint)Rpcs.RpcNotifyBC)]
    public static void RpcNotifyBC(PlayerControl player)
    {
        if (player.AmOwner())
        {
            player.Notify(BookCollector_Feedback.AttackedButRedirectVictim(), NotifyMode.OnlyMeeting);
        }
    }

    public (PlayerControl?, int) PerformInteraction(PlayerControl visitor)
    {
        var guessed = GuessedPlayers.Where(x => !x.HasDied());
        if (guessed.Count() == 0) return (null, 0);
        var newTarget = guessed.Random();

        Player.RpcAddModifier<RoleLearn>(visitor, false);

        RpcNotifyBC(Player);
        visitor.Notify(BookCollector_Feedback.AttackedButRedirectVisitor(Player, newTarget), NotifyMode.InstantlyAndMeeting);
        return (newTarget, 0);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            Charges--;

            var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x != Player && !LearnedRoles.Contains(x.Data.Role));
            if (allPlayers.Any())
            {
                var roleInPlay = allPlayers?.Random()?.Data.Role;
                if (roleInPlay != null)
                {
                    LearnedRoles.Add(roleInPlay);
                    Player.Notify(BookCollector_Feedback.Read(roleInPlay), NotifyMode.InstantlyAndMeeting);
                }
            }
            else Player.Notify(BookCollector_Feedback.ReadButNoMoreToLearn(), NotifyMode.InstantlyAndMeeting);
        }
    }

    public MeetingMenu meetingMenu;
    public int Charges;

    public List<PlayerControl> GuessedPlayers = new List<PlayerControl>();
    public List<RoleBehaviour> LearnedRoles = new List<RoleBehaviour>();
}

public sealed class BookCollector_Read : ObjectWorkshopRoleButton<BookCollector>
{
    public override string Name => "Read";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.BookCollector;
    public override float Cooldown => 1f;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public GameObject? ChargesButton { get; set; }
    public TextMeshPro ChargesButtonText { get; set; }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        if (KeybindIcon != null) KeybindIcon.transform.localPosition = new Vector3(0.4f, 0.45f, -9f);
        if (Button == null)
        {
            OWPlugin.DebugLogMessage("Button is null for Read Button!", OWPlugin.MsgType.Error);
        }
        else ChargesButton = CreateChargesButton(Button.gameObject, new Vector3(-0.341f, 0.45f, -0.1f));

        //ChargesButton = UnityEngine.Object.Instantiate(Button, new Vector3(-0.341f, 0.45f, -0.1f), Quaternion.identity);
    }

    public GameObject CreateChargesButton(GameObject button, Vector3 localPos)
    {
        var chargesIcon = UnityEngine.Object.Instantiate(HudManager.Instance.AbilityButton.usesRemainingSprite.gameObject, button.transform);
        chargesIcon.GetComponent<SpriteRenderer>().sprite = OWAssets.AbilityCounterBookSprite.LoadAsset();
        chargesIcon.GetComponent<SpriteRenderer>().color = TextOutlineColor;

        ChargesButtonText = chargesIcon.transform.GetComponentInChildren<TextMeshPro>();

        chargesIcon.name = "ChargesIcon";
        chargesIcon.transform.localPosition = localPos;
        return chargesIcon;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (playerControl == Player)
        {
            Button?.usesRemainingText.gameObject.SetActive(true);
            Button?.usesRemainingSprite.gameObject.SetActive(true);
            Button!.usesRemainingText.text = Role.Charges.ToString() + "";

            ChargesButton.transform.localPosition = new Vector3(0.4f, -0.1f, -0.1f);
            ChargesButtonText!.text = Role.GuessedPlayers.Count + "/" + (int)OptionGroupSingleton<BookCollector_Options>.Instance.Required; 
        }

        base.FixedUpdate(playerControl);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
    public override bool CanUse()
    {
        return base.CanUse() && Role.Charges > 0;
    }
}

public static class BookCollector_Feedback
{
    public static string GuessedCorrectly(RoleBehaviour role, PlayerControl target)
    {
        return $"You have correctly guessed {target.Name()}'s role as the {role.NiceName}!".ApplyKeywords();
    }

    public static string FailedToGuess(RoleBehaviour role, PlayerControl target)
    {
        return $"You have to guess {target.Name()} as the {role.NiceName}, but you were incorrect!".ApplyKeywords();
    }

    public static string Read(RoleBehaviour role)
    {
        var roleName = role.NiceName;
        return $"You read a book and acknowledged that {roleName.GetVowel()} {roleName} is in play!".ApplyKeywords();
    }

    public static string RolePerished(RoleBehaviour role)
    {
        var roleName = role.NiceName;
        return $"You had known the existence of {roleName.GetVowel()} {roleName}, but they have perished!".ApplyKeywords();
    }

    public static string ReadButNoMoreToLearn()
    {
        return $"You read a book, but it seems like there are no more roles to learn!".ApplyKeywords();
    }

    public static string AttackedButRedirectVisitor(PlayerControl target, PlayerControl victim)
    {
        return $"You attacked {target.Name()}, but they convinced you to attack {victim.Name()} instead! You see through their tricks and narrow down that they are the Book Collector!".ApplyKeywords();
    }

    public static string AttackedButRedirectVictim()
    {
        return "You were attacked, but you convinced them to Attack someone else!".ApplyKeywords();
    }

    public static string ExitInVictory(PlayerControl player)
    {
        return $"{player.Name()}, the Book Collector, has accomplished their goal and has left victorious! The Book Collector has revealed those they have researched!".ApplyKeywords();
    }
}

public sealed class BookCollector_Options : AbstractOptionGroup<BookCollector>
{
    public override string GroupName => "Book Collector";

    [ModdedNumberOption("<color=#ee943b>Book Collector</color> Guesses Required", 1f, 10f, 1f, MiraNumberSuffixes.None)]
    public float Required { get; set; } = 3f;

    [ModdedNumberOption("<color=#ee943b>Book Collector</color> <color=#4a86e8>Book</color> Spawn Cooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SpawnCooldown { get; set; } = 15f;

    [ModdedNumberOption("<color=#ee943b>Book Collector</color> Max <color=#4a86e8>Books</color> At Once", 1f, 10f, 1f, MiraNumberSuffixes.None)]
    public float MaxBooksAtOnce { get; set; } = 3f;
}