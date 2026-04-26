using System.Collections;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class AlarmClock(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;
    public SpriteRenderer myRend;
    public bool emitWaves;
    public GameObject aura;
    public Color bubbleColor;

    public static List<AlarmClock> AllClocks = new List<AlarmClock>();

    public void Start()
    {
        //Lockdown.AvailableObjects.Add(gameObject);

        bubbleColor = new Color32(179, 255, 255, 85);
        myRend = gameObject.GetComponent<SpriteRenderer>();
        
        // Bounce down. ty WanderingPix!
        DestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Bounce(transform, 0.7f, 0.45f));
        AllClocks.Add(this);
    }

    private void OnDestroy()
    {
        AllClocks.Remove(this);
    }

    private void Update()
    {
        UpdateColor();

        if (!emitWaves) // This is being called ONCE!
        {
            var inRange = new List<(PlayerControl, bool)>();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.HasDied())
                    continue;

                float dist = Vector2.Distance(player.GetTruePosition(), transform.position);
                if (dist > OptionGroupSingleton<Alarum_Options>.Instance.Radius)
                    continue;

                if (player.Is(CalculatedFaction.Infiltrator))
                {
                    bubbleColor = new Color32(255, 80, 80, 85);
                    emitWaves = true;
                    Coroutines.Start(EmitSoundWaves(transform, 1f));

                    inRange.Add((player, true));
                }
                else inRange.Add((player, false));
            }

            // Smart MCI stuff.
            if (Debugger.IsDebuggerActive)
            {
                foreach (var ir in inRange.ToArray())
                {
                    var player = ir.Item1;
                    var isSusp = ir.Item2;
                    if (WitnessKill.BotCanSee(player, Owner, player.transform.position) && player != Owner)
                    {
                        Owner.AddModifier<TI>();
                        if (inRange.Any(x => x.Item2) && inRange.Count > 1)
                        {
                            player.AddModifier<Suspicion>(Owner, Owner.Data.Role.NiceName, 35);
                        }
                        else if (!player.HasModifier<SoftCleared>() && !player.HasModifier<Suspicion>())
                        {
                            if (!isSusp) player.AddModifier<SoftCleared>();
                            else if (isSusp) player.AddModifier<Suspicion>(Owner, Owner.Data.Role.NiceName, 95);
                        }
                    }
                }
            }
        }
    }

    private bool Visibility() =>
            (PlayerControl.LocalPlayer == Owner) ||
            PlayerControl.LocalPlayer.HasDied() ||
            (PlayerControl.LocalPlayer.Is(CalculatedFaction.Infiltrator) && OptionGroupSingleton<Alarum_Options>.Instance.InfiltSeeClock);
    private void UpdateColor()
    {
        if (Visibility())
        {
            myRend.Show();
            aura.GetComponent<SpriteRenderer>().color = bubbleColor;
        }
        else
        {
            myRend.Hide();
            aura.GetComponent<SpriteRenderer>().Hide();
        }
    }

    public IEnumerator EmitSoundWaves(Transform origin, float interval = 1f) // Created by Chat GPT.
    {
        while (true)
        {
            if (Visibility())
            {
                // Create wave object
                var waveObj = new GameObject("AlarmWave");
                var lr = waveObj.AddComponent<LineRenderer>();

                // Configure line renderer
                lr.useWorldSpace = false;
                lr.loop = true;
                lr.positionCount = 64; // circle points
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = lr.endColor = Color.white;

                if (this.IsDestroyedOrNull())
                    yield break;

                waveObj.transform.position = origin.position;

                // Start the wave animation
                Coroutines.Start(AnimateWave(lr, OptionGroupSingleton<Alarum_Options>.Instance.Radius, 0.7f));
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator AnimateWave(LineRenderer lr, float maxRadius, float duration) // Created by Chat GPT.
    {
        float time = 0f;
        Color startColor = lr.startColor;

        while (time < duration)
        {
            float t = time / duration;
            float radius = Mathf.Lerp(0f, maxRadius, t);

            SetCirclePoints(lr, radius);

            // Fade alpha over time
            float alpha = Mathf.Lerp(1f, 0f, t);
            var color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            lr.startColor = lr.endColor = color;

            time += Time.deltaTime;
            yield return null;
        }

        Destroy(lr.gameObject);
    }

    private void SetCirclePoints(LineRenderer lr, float radius) // Created by Chat GPT.
    {
        if (this.IsDestroyedOrNull())
           return;
           
        int points = lr.positionCount;
        for (int i = 0; i < points; i++)
        {
            float angle = Mathf.PI * 2f / points * i;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

    public static void Begin(PlayerControl player)
    {
        var bubble = new GameObject("AlarumBubble");
        bubble.AddSpriteRenderer(OWAssets.Bubble.LoadAsset(), 0, 100, player.GetAdjustedPosition(), Color.clear, Vector3.one);
        bubble.transform.localScale = Vector3.one * OptionGroupSingleton<Alarum_Options>.Instance.Radius * 2f;

        var clockObj = new GameObject("AlarmClock");
        clockObj.AddSpriteRenderer(OWAssets.Alarum_AlarmClock.LoadAsset(), 10, 0, player.GetAdjustedPosition(), ObjectExtentions.noColor(), Vector3.one);
        var clock = clockObj.AddComponent<AlarmClock>();
        clock.Owner = player;
        clock.aura = bubble;

        bubble.transform.SetParent(clockObj.transform);

        bubble.AddOWObject();
        clockObj.AddOWObject();
    }

    public static void DestroyAll()
    {
        foreach (var clock in AllClocks)
        {
            Destroy(clock.gameObject);
        }
    }

    public static void CleanUp()
    {
        DestroyAll();
    }
}