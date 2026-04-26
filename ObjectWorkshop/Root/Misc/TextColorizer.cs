using System.Text.RegularExpressions;

namespace ObjectWorkshop.Misc;

public static class TextColorizer
{
    // Define the words and their colors
    private static readonly Dictionary<string, Func<string>> colorMap = new()
    {
        // Keywords
        { "Crewmate", () => $"#{RoleColors.Crewmate.ToHtmlStringRGBA()}" },
        { "Neutral", () => $"#{RoleColors.Neutral.ToHtmlStringRGBA()}" },
        { "Infiltrator", () => $"#{RoleColors.Infiltrator.ToHtmlStringRGBA()}" },
        { "Infiltrators", () => $"#{RoleColors.Infiltrator.ToHtmlStringRGBA()}" },

        { "Investigative", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Killing", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Protective", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Support", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Utility", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Associative", () => $"#91bbff" },
        { "Benign", () => $"#c6db3c" },
        { "Cataclysmic", () => $"#bd19b7" },
        { "Evil", () => $"#d34a72" },
        { "Predator", () => $"#3562ac" },
        { "Disruption", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Evacuative", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Militant", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Cadet ---
        { "Cadet", () => $"#{RoleColors.Crewmate.ToHtmlStringRGBA()}" },

        // --- Marauder ---
        { "Marauder", () => $"#{RoleColors.Crewmate.ToHtmlStringRGBA()}" },
        { "Attack", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Alarum ---
        { "Alarum", () => $"#{RoleColors.Crewmate.ToHtmlStringRGBA()}" },
        { "Activate", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Obstructor ---
        { "Obstructor", () => $"#{RoleColors.Infiltrator.ToHtmlStringRGBA()}" },
        { "Barricade", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Arachnid ---
        { "Arachnid", () => $"#{RoleColors.Infiltrator.ToHtmlStringRGBA()}" },
        { "Spin", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Inner Spider", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Inner Infiltrator", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Shikari ---
        { "Shikari", () => $"#{RoleColors.Shikari.ToHtmlStringRGBA()}" },
        { "Mark", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Execute", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- UFO ---
        { "UFO", () => $"#{RoleColors.Crewmate.ToHtmlStringRGBA()}" },
        { "Abduct", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" }, { "Abducted", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Destination", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Abduct Immune", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Luminescence ---
        { "Luminescence", () => $"#{RoleColors.Crewmate.ToHtmlStringRGBA()}" },
        { "Radiate", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Auto Radiate", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Brightness Meter", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Duelist ---
        { "Duelist", () => $"#{RoleColors.Crewmate.ToHtmlStringRGBA()}" },
        { "Sharpen", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Duel", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Aimsman ---
        { "Aimsman", () => $"#{RoleColors.Infiltrator.ToHtmlStringRGBA()}" },
        { "Fire", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Aim", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Claylamity ---
        { "Claylamity", () => $"#{RoleColors.Claylamity.ToHtmlStringRGBA()}" },
        { "SUbMerGE", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Metamorphosis", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Book Collector ---
        { "Book Collector", () => $"#{RoleColors.BookCollector.ToHtmlStringRGBA()}" },
        { "Read", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Guess", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Enticer ---
        { "Enticer", () => $"#{RoleColors.Enticer.ToHtmlStringRGBA()}" },
        { "Prepare", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Devour", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Pyre ---
        { "Pyre", () => $"#{RoleColors.Pyre.ToHtmlStringRGBA()}" },
        { "Ignite", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Peacock ---
        { "Peacock", () => $"#{RoleColors.Peacock.ToHtmlStringRGBA()}" },
        { "Bloom", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Declare", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Associate", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },

        // --- Oasis ---
        { "Oasis", () => $"#{RoleColors.Crewmate.ToHtmlStringRGBA()}" },
        { "Sanctify", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
        { "Sandstorm", () => $"#{RoleColors.Keyword.ToHtmlStringRGBA()}" },
    };

    // Build a single regex that matches any keyword. Longer keys are listed first to prefer them when overlapping.
    private static readonly Regex KeywordRegex = new Regex(
        string.Join("|", colorMap.Keys.OrderByDescending(k => k.Length).Select(Regex.Escape)),
        RegexOptions.Compiled
    );

    public static string ApplyKeywords(this string input)
    {
        return KeywordRegex.Replace(
            input,
            match =>
            {
                var word = match.Value;
                var color = colorMap[word](); // call the function

                return $"<b><color={color}>{word}</color></b>";
            }
        );
    }
}