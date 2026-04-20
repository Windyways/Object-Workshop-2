using UnityEngine;

namespace ObjectWorkshop.Misc;

public static class OWAssets
{
    private const string RoleCard = "TownOfUs.Resources.Root.Sprites.RoleCards";
    private const string Abilities = "TownOfUs.Resources.Root.Sprites.Abilities";
    private const string Other = "TownOfUs.Resources.Root.Sprites.Other";
    private const string Audio = "TownOfUs.Resources.Root.Audio";
    private const string Object = "TownOfUs.Resources.Root.Sprites.Objects";


    public static void PlaySound(LoadableAsset<AudioClip> clip, float vol = 1f)
    {
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(clip.LoadAsset(), false, vol);
        }
    }

    // --- Objects ---
    public static LoadableAsset<Sprite> Alarum_AlarmClock { get; } = new LoadableResourceAsset($"{Object}.Alarum_AlarmClock.png");

    // --- Abilities ---
    public static LoadableAsset<Sprite> MeetingKillSprite { get; } = new LoadableResourceAsset($"{Abilities}.MeetingKillButton.png");
    public static LoadableAsset<Sprite> KillSprite { get; } = TouAssets.KillSprite;

    // --- Other ---
    public static LoadableAsset<Sprite> Bubble { get; } = new LoadableResourceAsset($"{Object}.Bubble.png");

    // --- Role Cards ---

    // OTHER
    public static string RoleIconPosName
    {
        get
        {
            var name = "Next To Role";
            switch (OWPlugin.RoleIconSpot.Value)
            {
                case 1:
                    name = "Next To Name";
                    break;
                case 2:
                    name = "Hide";
                    break;
            }
            return name;
        }
    }
}