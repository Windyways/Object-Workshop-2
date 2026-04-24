using System.Collections;
using UnityEngine;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Moon(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;
    public GameObject beam;
    public bool isAbductOccuring;

    private SpriteRenderer myRend;

    public void Start()
    {
        // Lockdown.AvailableObjects.Add(moon);

        myRend = gameObject.GetComponent<SpriteRenderer>();
        AllMoons.Add(this);
    }

    public IEnumerator PlaySwirlAndTeleport(PlayerControl player, PlayerControl target) // Created By Chat GPT.
    {
        if (player == null || target == null)
            yield break;

        if ((target.Data.Role is Enticer enticer && enticer.Player.inVent) || /*target.IsUnderground() || */target.HasModifier<DuelingModifier>())
        {
            player.Notify(UFO.Info(target), NotifyMode.InstantlyAndMeeting);
            yield break;
        } // Abduct immune because these may cause issues...
        /*else if (target.Data.Role is Totemist totemist && totemist.isWatching)
        {
            totemist.isWatching = false;

            LightSource light = target.lightSource;
            light.transform.SetParent(target.transform);
            light.transform.localPosition = target.Collider.offset;
        }
        else if (target.Data.Role is Aimsman aimsman && aimsman.isAiming)
        {
            Aimsman.RpcAim(target);
        }
        else if (target.Data.Role is Culverin culverin && culverin.isAiming)
        {
            var cannonballBase = CannonballBase.GetObjectByPlayer(target);

            Object.Destroy(cannonballBase.gameObject);
            Object.Destroy(cannonballBase.directionLine);
        }
        else if (target.Data.Role is Specter specter && specter.isInvisible)
        {
            specter.isInvisible = false;
            target.RpcRemoveModifier<InvisibleTogglable>();
        } // Some patched when Abducted...
        */

        if (!target.HasDied()) target.Immobilize();

        isAbductOccuring = true;

        // Create beam effect
        beam = CreateAbductionBeam(target);

        if (target.AmOwner()) OWAssets.PlaySound(OWAssets.Abduct_SFX);

        yield return Coroutines.Start(AnimateBeam());

        // Teleport target to Moon position, or dead body if dead
        if (target.HasDied())
        {
            var targetBody = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == target.PlayerId);
            if (targetBody != null)
            {
                targetBody.transform.position = transform.position;
            }
        }
        else
        {
            target.transform.position = transform.position;
            target.Mobilize();
        }

        // Destroy effects after teleport
        if (beam != null) Object.Destroy(beam);

        isAbductOccuring = false;
    }

    private IEnumerator AnimateBeam() // Created By Chat GPT.
    {
        float duration = OptionGroupSingleton<UFO_Options>.Instance.Duration;
        float time = 0f;

        while (time < duration)
        {
            // Animate beam alpha fade-in/out
            if (beam != null)
            {
                LineRenderer lr = beam.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    float alpha = Mathf.PingPong(time * 2f, 1f); // shimmer
                    Color c = new Color(0f, 1f, 1f, alpha);
                    lr.startColor = c;
                    lr.endColor = new Color(c.r, c.g, c.b, 0f);
                }
            }

            time += Time.deltaTime;
            yield return null;
        }
    }

    private static GameObject CreateAbductionBeam(PlayerControl player) // Created By Chat GPT.
    {
        Transform? target = null;
        
        // Instantiate on the target, or target dead body if dead
        if (player.Data.IsDead)
        {
            var targetBody = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == player.PlayerId);
            if (targetBody != null)
            {
                target = targetBody.transform;
            }
        }
        else
        {
            target = player.transform;
        }

        GameObject newBeam = new GameObject("AbductionBeam");
        newBeam.transform.position = target.position;

        LineRenderer lr = newBeam.AddComponent<LineRenderer>();
        newBeam.AddComponent<SpriteRenderer>();
        
        lr.positionCount = 2;
        lr.widthMultiplier = 0.5f;
        lr.useWorldSpace = true;

        // Just set start/end colors directly
        lr.startColor = Color.cyan;
        lr.endColor = new Color(0f, 1f, 1f, 0f); // fades to transparent

        // Simple material (swap for glow shader if available)
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.cyan;
        lr.material = mat;

        // Top point above player, bottom at player
        Vector3 topPos = target.position + Vector3.up * 4f;
        lr.SetPosition(0, topPos);
        lr.SetPosition(1, target.position);

        return newBeam;
    }

    private void Update()
    {
        UpdateColor();
    }

    private bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();

        if (beam != null)
        {
            SpriteRenderer bsr = beam.GetComponent<SpriteRenderer>();
            if (Visibility()) bsr.Show();
            else bsr.Hide();
        }
    }
    
    public static void Begin(PlayerControl player)
    {
        WitnessKill.WitnessGoodDeed(player);

        var obj = new GameObject("Moon");
        obj.AddSpriteRenderer(OWAssets.UFO_Moon.LoadAsset(), 10, 0, player.transform.position, ObjectExtentions.noColor(), Vector3.one);

        var moon = obj.AddComponent<Moon>();
        moon.Owner = player;

        obj.AddOWObject();
    }

    public static void DestroyAll()
    {
        var moon = GetMoon();
        if (moon != null)
        {
            Destroy(moon.beam);
            Destroy(moon.gameObject); // This prevents UFO from working in the future. why? i dunno.
        }
        AllMoons.Clear();
    }

    public static void CleanUp()
    {
        AllMoons.Clear();
    }

    public static List<Moon> AllMoons = new List<Moon>();

    public static Moon GetMoon()
    {
        if (AllMoons.Count == 0) return null;
        return AllMoons[0];
    }
}