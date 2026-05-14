using UnityEngine;

namespace ObjectWorkshop.Misc;

public static class OWAssets
{
    private const string RoleCard = "TownOfUs.Resources.Root.Sprites.RoleCards";
    private const string Abilities = "TownOfUs.Resources.Root.Sprites.Abilities";
    private const string OtherPath = "TownOfUs.Resources.Root.Sprites.Other";
    private const string Audio = "TownOfUs.Resources.Root.Audio";
    private const string Object = "TownOfUs.Resources.Root.Sprites.Objects";
    private const string CounterPath = "TownOfUs.Resources.AbilityCounters";


    public static void PlaySound(LoadableAsset<AudioClip> clip, float vol = 1f)
    {
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(clip.LoadAsset(), false, vol);
        }
    }

    // --- Objects ---
    public static LoadableAsset<Sprite> Alarum_AlarmClock { get; } = new LoadableResourceAsset($"{Object}.Alarum_AlarmClock.png");
    public static LoadableAsset<Sprite> Obstructor_Barricade { get; } = new LoadableResourceAsset($"{Object}.Obstructor_Barricade.png");
    public static LoadableAsset<Sprite> Arachnid_Spider { get; } = new LoadableResourceAsset($"{Object}.Arachnid_Spider.png");
    public static LoadableAsset<Sprite> Arachnid_Web { get; } = new LoadableResourceAsset($"{Object}.Arachnid_Web.png");
    public static LoadableAsset<Sprite> Arachnid_SpiderCensored { get; } = new LoadableResourceAsset($"{Object}.Arachnid_SpiderCensored.png");
    public static LoadableAsset<Sprite> Shikari_Mark { get; } = new LoadableResourceAsset($"{Object}.Shikari_Mark.png");
    public static LoadableAsset<Sprite> Duelist_Sword { get; } = new LoadableResourceAsset($"{Object}.Duelist_Sword.png");
    public static LoadableAsset<Sprite> UFO_Moon { get; } = new LoadableResourceAsset($"{Object}.UFO_Moon.png");
    public static LoadableAsset<Sprite> Luminescence_Lightbulb { get; } = new LoadableResourceAsset($"{Object}.Luminescence_Lightbulb.png");
    public static LoadableAsset<Sprite> Pyre_FireSprite { get; } = new LoadableResourceAsset($"{Object}.Pyre_FireSprite.png");
    public static LoadableAsset<Sprite> Aimsman_Crosshair { get; } = new LoadableResourceAsset($"{Object}.Aimsman_Crosshair.png");
    public static LoadableAsset<Sprite> BookCollector_Book { get; } = new LoadableResourceAsset($"{Object}.BookCollector_Book.png");
    public static LoadableAsset<Sprite> Peacock_Feather { get; } = new LoadableResourceAsset($"{Object}.Peacock_Feather.png");
    public static LoadableAsset<Sprite> BookCollector_BookScrap { get; } = new LoadableResourceAsset($"{Object}.BookCollector_BookScrap.png");
    public static LoadableAsset<Sprite> Oasis_SandParticle1 { get; } = new LoadableResourceAsset($"{Object}.Oasis_SandParticle1.png");
    public static LoadableAsset<Sprite> Oasis_SandParticle2 { get; } = new LoadableResourceAsset($"{Object}.Oasis_SandParticle2.png");
    public static LoadableAsset<Sprite> Oasis_SandOverlay { get; } = new LoadableResourceAsset($"{Object}.Oasis_SandOverlay.png");
    public static LoadableAsset<Sprite> Settler_Egg { get; } = new LoadableResourceAsset($"{Object}.Settler_Egg.png");
    public static LoadableAsset<Sprite> Settler_Nest { get; } = new LoadableResourceAsset($"{Object}.Settler_Nest.png");
    public static LoadableAsset<Sprite> Ambiguator_SturdyEgg { get; } = new LoadableResourceAsset($"{Object}.Ambiguator_SturdyEgg.png");
    public static LoadableAsset<Sprite> UndeadReaper_Visual { get; } = new LoadableResourceAsset($"{Object}.UndeadReaper_Visual.png");
    public static LoadableAsset<Sprite> Gravekeeper_Spirit { get; } = new LoadableResourceAsset($"{Object}.Gravekeeper_Spirit.png");
    public static LoadableAsset<Sprite> Gravekeeper_Tombstone { get; } = new LoadableResourceAsset($"{Object}.Gravekeeper_Tombstone.png");
    public static LoadableAsset<Sprite> Totemist_Totem { get; } = new LoadableResourceAsset($"{Object}.Totemist_Totem.png");
    public static LoadableAsset<Sprite> Bubblemaker_Bubble { get; } = Bubble; //new LoadableResourceAsset($"{Object}.Bubblemaker_Bubble.png");
    public static LoadableAsset<Sprite> Culverin_Cannonball { get; } = new LoadableResourceAsset($"{Object}.Culverin_Cannonball.png");
    public static LoadableAsset<Sprite> Culverin_FireDir { get; } = new LoadableResourceAsset($"{Object}.Culverin_FireDir.png");

    // --- Abilities ---
    public static LoadableAsset<Sprite> MeetingKillSprite { get; } = new LoadableResourceAsset($"{Abilities}.MeetingKillButton.png");
    public static LoadableAsset<Sprite> KillSprite { get; } = TouAssets.KillSprite;
    public static LoadableAsset<Sprite> VentSprite { get; } = TouAssets.VentSprite;

    public static LoadableAsset<Sprite> BookCollector_Guess { get; } = new LoadableResourceAsset($"{Abilities}.BookCollector_Guess.png");
    public static LoadableAsset<Sprite> AbilitySpriteGoldCounter { get; } =
        new LoadableResourceAsset($"{OtherPath}.GoldCounter.png");

    // --- AUDIO ---
    public static LoadableAsset<AudioClip> DuelBegin_SFX { get; } = new LoadableAudioResourceAsset($"{Audio}.DuelBegin_SFX.wav");
    public static LoadableAsset<AudioClip> Duelist_Sharpen_SFX { get; } = new LoadableAudioResourceAsset($"{Audio}.Duelist_Sharpen_SFX.wav");
    public static LoadableAsset<AudioClip> DuelKill_SFX { get; } = new LoadableAudioResourceAsset($"{Audio}.DuelKill_SFX.wav");
    public static LoadableAsset<AudioClip> Abduct_SFX { get; } = new LoadableAudioResourceAsset($"{Audio}.Abduct_SFX.wav");
    public static LoadableAsset<AudioClip> Build_SFX { get; } = new LoadableAudioResourceAsset($"{Audio}.Build_SFX.wav");

    // --- Other ---
    public static LoadableAsset<Sprite> Bubble { get; } = new LoadableResourceAsset($"{Object}.Bubble.png");
    public static LoadableAsset<Sprite> FillerCircle { get; } = new LoadableResourceAsset($"{Object}.FillerCircle.png");

    // --- Role Cards ---
    public static LoadableAsset<Sprite> Alarum { get; } = new LoadableResourceAsset($"{RoleCard}.Alarum.png");
    public static LoadableAsset<Sprite> Oasis { get; } = new LoadableResourceAsset($"{RoleCard}.Oasis.png");
    public static LoadableAsset<Sprite> Luminescence { get; } = new LoadableResourceAsset($"{RoleCard}.Luminescence.png");
    public static LoadableAsset<Sprite> BookCollector { get; } = new LoadableResourceAsset($"{RoleCard}.BookCollector.png");
    public static LoadableAsset<Sprite> UFO { get; } = new LoadableResourceAsset($"{RoleCard}.UFO.png");
    public static LoadableAsset<Sprite> Settler { get; } = new LoadableResourceAsset($"{RoleCard}.Settler.png");
    public static LoadableAsset<Sprite> Arachnid { get; } = new LoadableResourceAsset($"{RoleCard}.Arachnid.png");
    public static LoadableAsset<Sprite> Ambiguator { get; } = new LoadableResourceAsset($"{RoleCard}.Ambiguator.png");
    public static LoadableAsset<Sprite> Aimsman { get; } = new LoadableResourceAsset($"{RoleCard}.Aimsman.png");
    public static LoadableAsset<Sprite> Peacock { get; } = new LoadableResourceAsset($"{RoleCard}.Peacock.png");
    public static LoadableAsset<Sprite> Gravekeeper { get; } = new LoadableResourceAsset($"{RoleCard}.Gravekeeper.png");
    public static LoadableAsset<Sprite> Reaper { get; } = new LoadableResourceAsset($"{RoleCard}.Reaper.png");

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