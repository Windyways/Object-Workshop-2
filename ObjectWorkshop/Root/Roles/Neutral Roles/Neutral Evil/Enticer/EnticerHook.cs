using UnityEngine;
using Color = UnityEngine.Color;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class EnticerHook(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;

    public LineRenderer lineRenderer;
    public GameObject hookLine;
    public PlayerControl currentTarget;
    public GameObject currentTargetGameObject;

    private static float devourRange = 0.5f;
    public float DevourVision;

    public static List<EnticerHook> AllHooks = new List<EnticerHook>();


    public void Start()
    {
        AllHooks.Add(this);
    }

    private void OnDestroy()
    {
        AllHooks.Remove(this);
    }

    public static void Begin(PlayerControl player) // Created By Chat GPT.
    {
        var existinghook = EnticerHook.GetObjectByOwner(player);
        if (existinghook != null)
            return;

        var obj = new GameObject("Hook");

        var hook = obj.AddComponent<EnticerHook>();

        hook.hookLine = new GameObject("HookLine");
        hook.lineRenderer = hook.hookLine.AddComponent<LineRenderer>();
        hook.lineRenderer.positionCount = 2;
        hook.lineRenderer.startWidth = 0.05f;
        hook.lineRenderer.endWidth = 0.05f;
        hook.lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        hook.lineRenderer.startColor = Color.gray;
        hook.lineRenderer.endColor = Color.gray;
        hook.lineRenderer.sortingOrder = 50;
        hook.lineRenderer.rendererPriority = 100;

        hook.Owner = player;
    }

    private bool Visibility() => true;
    private void UpdateColor()
    {
        if (Visibility()) lineRenderer.ShowLR(Color.gray, Color.gray);
        else lineRenderer.HideLR();
    }

    public void Update() // Created By Chat GPT.
    {
        if (hookLine == null || Owner == null || Owner.HasDied()) Destroy();
        else
        {
            UpdateColor();
            currentTarget = FindTarget();
            currentTargetGameObject = FindGameObjectTarget();
            if (currentTarget != null)
            {
                Vector3 playerPos = Owner.transform.position;
                Vector3 targetPos = currentTarget.transform.position;
                
                UpdateHookLine(playerPos, targetPos);
                PullTarget();

                float minValue = 0f;
                float maxValue = 0.75f;

                float distance = Vector2.Distance(currentTarget.transform.position, playerPos);
                distance = Mathf.Clamp(distance, 0f, OptionGroupSingleton<Enticer_Options>.Instance.Radius);

                float t = 1f - distance / (OptionGroupSingleton<Enticer_Options>.Instance.Radius + 0.85f);
                DevourVision = Mathf.Lerp(maxValue, minValue, t);

                if (Vector2.Distance(targetPos, playerPos) <= devourRange)
                {
                    DevourTarget(currentTarget);
                }
            }
            else
            {
                if (currentTargetGameObject != null)
                {
                    Vector3 playerPos = Owner.transform.position;
                    Vector3 targetPos = currentTargetGameObject.transform.position;

                    UpdateHookLine(playerPos, targetPos);
                    PullTarget();

                    float minValue2 = 0f;
                    float maxValue2 = 0.75f;

                    float distance2 = Vector2.Distance(currentTargetGameObject.transform.position, playerPos);
                    distance2 = Mathf.Clamp(distance2, 0f, OptionGroupSingleton<Enticer_Options>.Instance.Radius);

                    float t2 = 1f - distance2 / (OptionGroupSingleton<Enticer_Options>.Instance.Radius + 0.85f);
                    DevourVision = Mathf.Lerp(maxValue2, minValue2, t2);

                    if (Vector2.Distance(targetPos, playerPos) <= devourRange)
                    {
                        DevourEntity(currentTargetGameObject);
                    }
                }
                else
                {
                    Destroy();
                }
            }
        }
    }

    public void UpdateHookLine(Vector2 playerPos, Vector2 targetPos) // Created By Chat GPT.
    {
        if (hookLine != null)
        {
            LineRenderer line = hookLine.GetComponent<LineRenderer>();
            if (line != null)
            {
                int segments = 20;
                line.positionCount = segments;
                for (int i = 0; i < segments; i++)
                {
                    float t = (float)i / (float)(segments - 1);
                    Vector2 point = Vector2.Lerp(playerPos, targetPos, t);

                    float waveAmplitude = 0.15f;
                    float waveFrequency = 6f;
                    float time = Time.time;

                    Vector2 dir = (targetPos - playerPos).normalized;
                    Vector2 perp = new Vector2(-dir.y, dir.x);

                    float wave = Mathf.Sin(t * waveFrequency + time * 8f);
                    point += perp * wave * waveAmplitude;

                    line.SetPosition(i, point);
                }
            }
        }
    }

    public GameObject? FindGameObjectTarget()
    {
        if (MeetingHud.Instance) return null;

        float closest = OptionGroupSingleton<Enticer_Options>.Instance.Radius;
        GameObject? closestGameObject = null;
        foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
        {
            if (obj.name == "Locust")
            {
                if (obj != null)
                {
                    float dist = Vector2.Distance(obj.transform.position, Owner.transform.position);
                    bool flag4 = dist < closest;
                    if (flag4)
                    {
                        closest = dist;
                        closestGameObject = obj;
                    }
                }
            }
        }

        return closestGameObject;
    }

    public PlayerControl? FindTarget() // Created By Chat GPT.
    {
        if (MeetingHud.Instance) return null;

        float closest = OptionGroupSingleton<Enticer_Options>.Instance.Radius;
        PlayerControl? closestPlayer = null;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player != Owner && !player.HasDied() /*&& !player.IsSpecterInvisible() && !player.IsUnderground()*/)
            {
                float dist = Vector2.Distance(player.transform.position, Owner.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    closestPlayer = player;
                }
            }
        }

        return closestPlayer;
    }

    private void PullTarget() // Created By Chat GPT.
    {
        float pullSpeed = OptionGroupSingleton<Enticer_Options>.Instance.PullSpeed;
        if (currentTarget == null)
        {
            if (currentTargetGameObject != null)
            {
                pullSpeed *= 1.5f;
                Vector3 ventPos2 = Owner.transform.position;
                Vector3 targetPos2 = currentTargetGameObject.transform.position;
                Vector3 direction2 = (ventPos2 - targetPos2).normalized;
                currentTargetGameObject.transform.position += direction2 * pullSpeed * Time.deltaTime;
            }
        }
        else
        {
            if (currentTarget.IsSpider()/* || currentTarget.IsPeacock()*/) pullSpeed *= 1.5f; // Pull in 50% faster.
            //if (currentTarget.IsGiant()) pullSpeed /= 2; // Pull in at half speed.

            Vector3 ventPos3 = Owner.transform.position;
            Vector3 targetPos3 = currentTarget.transform.position;
            Vector3 direction3 = (ventPos3 - targetPos3).normalized;
            currentTarget.transform.position += direction3 * pullSpeed * Time.deltaTime;
        }
    }

    public void DevourTarget(PlayerControl target)
    {
        if (!MeetingHud.Instance && Owner.Data.Role is Enticer enticer)
        {
            Owner.RpcCustomMurder(target, createDeadBody: false, teleportMurderer: false);
            VisitingMechanic.RpcAddDeathReason(target, (int)DeathReasonShow.Devoured);

            enticer.DevourCount++; // Might need an rpc here, but probably not.
            Destroy();
        }
    }

    public void DevourEntity(GameObject gameObject)
    {
        if (!MeetingHud.Instance)
        {
            Destroy();
            // if (gameObject.name == "Locust") Locusts.LocustsAlive--;
            Destroy(gameObject);
        }
    }

    public void Destroy()
    {
        if (hookLine != null)
        {
            Destroy(hookLine);
            Destroy(gameObject);
        }
    }

    public static EnticerHook GetObjectByOwner(PlayerControl player)
    {
        foreach (var hook in AllHooks)
        {
            if (hook.Owner == player) return hook;
        }
        return null;
    }

    public static IEnumerable<EnticerHook> GetAll()
    {
        // Return all currently active lightbulbs
        return AllHooks;
    }

    public static bool IsHooked(PlayerControl target)
    {
        if (target == null) return false;

        foreach (var hook in AllHooks)
        {
            if (hook != null && hook.currentTarget != null && hook.currentTarget == target)
                return true;
        }

        return false;
    }

    public static void CleanUp()
    {
        AllHooks.Clear();
    }
}