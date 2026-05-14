using ObjectWorkshop.MCI;
using System.Globalization;
using UnityEngine;

namespace ObjectWorkshop.Misc
{
    public class RoleStats
    {
        public string RoleName { get; set; }

        public int Wins { get; set; }
        public int GamesPlayed { get; set; }
        public Color Color { get; set; }
        
        public int Kills { get; set; }

        public float WinRate
        {
            get
            {
                if (GamesPlayed == 0) return 0;
                return (float)Wins / GamesPlayed;
            }
        }

        public RoleStats(string roleName, Color color)
        {
            RoleName = roleName;
            Wins = 0;
            GamesPlayed = 0;
            Kills = 0;
            Color = color;
        }
    }

    public static class RoleReferences
    {
        public static Dictionary<string, RoleStats> roleStats = new Dictionary<string, RoleStats>();
        public static string filePath = "WinData.txt";
        public static bool CountRoundToLeaderboard = true;

        public static void Initialize()
        {
            // --- CREWMATE ---
            // -- CI --
            roleStats.Add("Alarum", new RoleStats("Alarum", RoleColors.Crewmate));
            roleStats.Add("Totemist", new RoleStats("Totemist", RoleColors.Crewmate));

            // -- CK --
            roleStats.Add("Duelist", new RoleStats("Duelist", RoleColors.Crewmate));

            // -- CP --
            roleStats.Add("Oasis", new RoleStats("Oasis", RoleColors.Crewmate));

            // -- CS --
            roleStats.Add("Luminescence", new RoleStats("Luminescence", RoleColors.Crewmate));

            // -- CU --
            roleStats.Add("Cadet", new RoleStats("Cadet", RoleColors.Crewmate));
            roleStats.Add("UFO", new RoleStats("UFO", RoleColors.Crewmate));

            // --- NEUTRAL ---
            // -- NA --
            roleStats.Add("Peacock", new RoleStats("Peacock", RoleColors.Peacock));
            roleStats.Add("Ambiguator", new RoleStats("Ambiguator", RoleColors.Ambiguator));

            // -- NB --
            roleStats.Add("Book Collector", new RoleStats("Book Collector", RoleColors.BookCollector));

            // -- NC --
            roleStats.Add("Shikari", new RoleStats("Shikari", RoleColors.Shikari));
            roleStats.Add("Gravekeeper", new RoleStats("Gravekeeper", RoleColors.Gravekeeper));

            // -- NE --
            roleStats.Add("Enticer", new RoleStats("Enticer", RoleColors.Enticer));
            roleStats.Add("Settler", new RoleStats("Settler", RoleColors.Settler));

            // -- NP --
            roleStats.Add("Pyre", new RoleStats("Pyre", RoleColors.Pyre));
            roleStats.Add("Reaper", new RoleStats("Reaper", RoleColors.Reaper));
            roleStats.Add("Undead Reaper", new RoleStats("Undead Reaper", RoleColors.UndeadReaper));

            // --- INFILTRATOR ---
            // -- ID --
            roleStats.Add("Obstructor", new RoleStats("Obstructor", RoleColors.Infiltrator));

            // -- IE --
            roleStats.Add("Arachnid", new RoleStats("Arachnid", RoleColors.Infiltrator));

            // -- IK --
            roleStats.Add("Aimsman", new RoleStats("Aimsman", RoleColors.Infiltrator));
            roleStats.Add("Culverin", new RoleStats("Culverin", RoleColors.Infiltrator));

            // -- IU --
            roleStats.Add("Marauder", new RoleStats("Marauder", RoleColors.Infiltrator));
            roleStats.Add("Claylamity", new RoleStats("Claylamity", RoleColors.Claylamity));

            LoadRoleStats(filePath);
        }

        public static List<string> PendingNotifications = new List<string>();
        public static void UpdateRoleResult(RoleBehaviour roleBehaviour, int kills, bool won)
        {
            string roleName = roleBehaviour.NiceName;
            if (!CountRoundToLeaderboard || !Debugger.IsDebuggerActive)
            {
                OWPlugin.DebugLogMessage("CountRoundToLeaderboard is false or Debugger is inactive, wins and loses do not count this game.");
                return;
            }

            if (roleStats.TryGetValue(roleName, out RoleStats? stats))
            {
                // Store snapshot before updating
                var oldStats = new RoleStats(stats.RoleName, stats.Color)
                {
                    Wins = stats.Wins,
                    GamesPlayed = stats.GamesPlayed,
                    Kills = stats.Kills
                };

                int oldRank = GetLeaderboardPosition(roleName);

                // Update stats
                stats.GamesPlayed++;
                stats.Kills += kills;
                if (won) stats.Wins++;

                int newRank = GetLeaderboardPosition(roleName);

                // Create notification text
                //string arrow = newRank < oldRank ? "^" : (newRank > oldRank ? "?" : ">");
                string colorArrow = newRank < oldRank ? "<color=#00ff00>^</color>" : (newRank > oldRank ? "<color=#ff0000>?</color>" : ">");
                string hexColor = ColorUtility.ToHtmlStringRGB(stats.Color);
                string coloredRole = $"<b><color=#{hexColor}>{stats.RoleName}</color></b>";

                string msg = $"{coloredRole} {oldStats.WinRate * 100:F2}% > {stats.WinRate * 100:F2}% ({colorArrow} #{oldRank} > #{newRank})";

                // Add to pending notifications
                PendingNotifications.Add(msg);

            }

            SaveRoleStats(filePath);
        }

        private static int GetLeaderboardPosition(string roleName)
        {
            var sorted = roleStats.Values
                .Where(r => r.GamesPlayed > 0)
                .OrderByDescending(r => r.WinRate)
                .ToList();

            return sorted.FindIndex(r => r.RoleName == roleName) + 1; // +1 because 0-based index
        }

        public static void SaveRoleStats(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var role in roleStats.Values)
                {
                    //                    TryParse 0    TryParse 1     TryParse 2        TryParse 3 
                    writer.WriteLine($"{role.RoleName},{role.Wins},{role.GamesPlayed},{role.Kills}");
                }
            }
        }

        public static void LoadRoleStats(string filePath)
        {
            if (!File.Exists(filePath))
            {
                SaveRoleStats(filePath); // Save the current roleStats (even if empty/default)
                OWPlugin.DebugLogMessage(".txt file not found, creating a new one.", OWPlugin.MsgType.Error);
                return;
            }

            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split(',');
                if (parts.Length != 4) continue; // skip broken lines
                                                 // Add this by 1 for each saved stat i want.

                string roleName = parts[0];
                Color color = Color.white;
                if (int.TryParse(parts[1], out int wins) && int.TryParse(parts[2], out int gamesPlayed) && int.TryParse(parts[3], out int kills))
                {
                    if (roleStats.TryGetValue(roleName, out RoleStats? value))
                    {
                        value.Wins = wins;
                        value.GamesPlayed = gamesPlayed;
                        value.Kills = kills;
                    }
                    else
                    {
                        // Optionally add new roles if missing
                        roleStats.Add(roleName, new RoleStats(roleName, color)
                        {
                            Wins = wins,
                            GamesPlayed = gamesPlayed,
                            Kills = kills,
                            Color = color
                        });
                    }
                }
            }
        }

        public static void ResetLeaderboard(string filePath)
        {
            // Clear the file (or you can delete it)
            if (File.Exists(filePath))
            {
                File.WriteAllText(filePath, string.Empty);  // This just empties the file
                OWPlugin.DebugLogMessage("Leaderboard has been reset!");
            }

            // Optional: Reset in-memory role stats as well (you could leave it as-is)
            foreach (var role in roleStats.Values)
            {
                role.Wins = 0;
                role.GamesPlayed = 0;
                role.Kills = 0;
            }

            // Optionally save the empty stats back to the file
            SaveRoleStats(filePath);  // This will save an empty leaderboard
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class WinRateCommand
    {
        public static bool Prefix(ChatController __instance)
        {
            if (__instance.freeChatField.Text.ToLower(CultureInfo.CurrentCulture).Contains("/lb", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!string.IsNullOrWhiteSpace(WinRate()) && player == PlayerControl.LocalPlayer)  DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, WinRate());
                }
                return true;
            }

            if (__instance.freeChatField.Text.ToLower(CultureInfo.CurrentCulture).Contains("/resetlb", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    var playerResults = ResetLeaderboard();
                    RoleReferences.ResetLeaderboard(RoleReferences.filePath);
                    
                    if (!string.IsNullOrWhiteSpace(playerResults) && player == PlayerControl.LocalPlayer)
                    {
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, playerResults);
                    }
                }
                return true;
            }

            if (__instance.freeChatField.Text.ToLower(CultureInfo.CurrentCulture).Contains("/state", StringComparison.CurrentCultureIgnoreCase))
            {
                if (Debugger.IsDebuggerActive && OWPlugin.InGame())
                {
                    MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.CachedPlayerData, "Stats", GetState());
                }
                return true;
            }
            return true;
        }

        public static string WinRate()
        {
            string rates = "";

            var sortedRoles = RoleReferences.roleStats.Values
                                        .Where(r => r.GamesPlayed > 0) 
                                        .OrderByDescending(r => r.WinRate)  // Sort by WinRate first
                                        .ThenByDescending(r => r.Wins - r.GamesPlayed) // Then by number of losses 
                                        .ThenByDescending(r => r.Kills) // Then by number of kills 
                                        .ToList();

            foreach (var role in sortedRoles)
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(role.Color);
                string coloredRoleName = $"<b><color=#{hexColor}>{role.RoleName}</color></b>";

                string KillsMSG = "";
                rates += $"{coloredRoleName} | <b><color=#ff0000>{role.GamesPlayed - role.Wins}</color></b> | <b><color=#00ff00>{role.Wins}</color></b> |{KillsMSG} {role.WinRate * 100:F2}% |\n";
            }

            if (rates == "") rates = "There are no data logged on this slot.";
            return "<size=62%>" + rates + "</size>";
        }

        public static string ResetLeaderboard()
        {
            return "The leaderboard has been reset successfully.";
        }

        public static string GetState()
        {
            string state = "";

            state += "--- Evil Activity ---\n";
            var confirmedEvils = ModifierUtils.GetActiveModifiers<ConfirmedEvil>(x => !x.Player.HasDied());
            if (confirmedEvils.Any())
            {
                foreach (var confirmedEvil in confirmedEvils) state += $"{confirmedEvil.Player.Name()} (Confirmed Evil - {confirmedEvil.Source})\n";
            }

            var suspicions = ModifierUtils.GetActiveModifiers<Suspicion>(x => !x.Player.HasDied());
            if (suspicions.Any())
            {
                foreach (var suspicion in suspicions)
                {
                    state += $"{suspicion.Player.Name()} (Suspicion - ({suspicion.VotedChance}% {suspicion.Source})\n";
                }
            }

            var seenKills = ModifierUtils.GetActiveModifiers<SeenKill>(x => !x.Player.HasDied());
            if (seenKills.Any())
            {
                foreach (var seenKill in seenKills) state += $"{seenKill.Player.Name()} (Seen Kill)\n";
            }

            state += "\n--- Good Activity ---\n";
            var confirmeds = ModifierUtils.GetActiveModifiers<Confirmed>(x => !x.Player.HasDied());
            if (confirmeds.Any())
            {
                foreach (var confirmed in confirmeds) state += $"{confirmed.Player.Name()} (Confirmed - {confirmed.Source})\n";
            }

            var tis = ModifierUtils.GetActiveModifiers<TI>(x => !x.Player.HasDied());
            if (tis.Any())
            {
                foreach (var ti in tis) state += $"{ti.Player.Name()} (Investigative)\n";
            }

            var softCleareds = ModifierUtils.GetActiveModifiers<SoftCleared>(x => !x.Player.HasDied());
            if (softCleareds.Any())
            {
                foreach (var softCleared in softCleareds) state += $"{softCleared.Player.Name()} (Soft Cleared)\n";
            }
            return state;
        }
    }
}