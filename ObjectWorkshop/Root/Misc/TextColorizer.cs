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