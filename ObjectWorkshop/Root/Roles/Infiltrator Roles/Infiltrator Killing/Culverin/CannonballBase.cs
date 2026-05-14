using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class CannonballBase(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;

    public Vector2 currentDirection = Vector2.up;
    public GameObject directionLine;
    private readonly float rotationStep = 3f;
    public Vector2 targetPosition;

    private void Start()
    {
        AllCannonballBases.Add(this);
    }

    private void OnDestroy()
    {
        AllCannonballBases.Remove(this);
    }

    private void Update()
    {
        // Read input (A/D or Left/Right)
        if (Input.GetKey(KeyCode.A)) RotateCannon(clockwise: false);
        else if (Input.GetKey(KeyCode.D)) RotateCannon(clockwise: true);

        // Continuously update line direction while aiming
        CreateOrUpdateDirectionLine(Owner.transform.position, currentDirection);
    }

    public void RotateCannon(bool clockwise)
    {
        float angle = clockwise ? (-rotationStep) : rotationStep;
        currentDirection = RotateVector(currentDirection, angle);
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * 0.017453292f;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
    }

    private void CreateOrUpdateDirectionLine(Vector2 origin, Vector2 direction)
    {
        if (directionLine == null)
        {
            directionLine = new GameObject("CannonDirectionLine");
            directionLine.AddSpriteRenderer(OWAssets.Culverin_FireDir.LoadAsset(), 1, 100, Owner.transform.position, ObjectExtentions.noColor(), Vector3.one);
        }

        float length = 1.5f;
        Vector2 normalizedDir = direction.normalized;
        Vector2 startOffset = normalizedDir * (length / 2f);

        directionLine.transform.position = origin + startOffset;
        float angle = Mathf.Atan2(normalizedDir.y, normalizedDir.x) * 57.29578f;
        directionLine.transform.eulerAngles = new Vector3(0f, 0f, angle);
        directionLine.transform.localScale = new Vector3(length, 1f, 1f);
    }

    public void StartFire()
    {
        FireCannonball(Owner, Owner.transform.position);
    }

    public void FireCannonball(PlayerControl player, Vector2 origin)
    {
        WitnessKill.WitnessEvilDeed(player);

        GameObject gameObject = new GameObject("Cannonball");
        gameObject.AddSpriteRenderer(OWAssets.Culverin_Cannonball.LoadAsset(), 5, 100, player.transform.position, ObjectExtentions.noColor(), Vector3.one);

        Cannonball cannonball = gameObject.AddComponent<Cannonball>();
        cannonball.Owner = player;
        cannonball.transform.position = origin;
        cannonball.currentDirection = currentDirection;
        cannonball.cannonballBase = GetObjectByPlayer(player);

        gameObject.AddOWObject();
    }

    public static void ClearAll(bool clearBall = true)
    {
        foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
        {
            if (obj.name == "Cannonball" && clearBall) Object.Destroy(obj);
            if (obj.name == "CannonDirectionLine") Object.Destroy(obj);
        }
    }

    public static CannonballBase GetObjectByPlayer(PlayerControl player)
    {
        foreach (CannonballBase cannonballBase in AllCannonballBases)
        {
            if (cannonballBase.Owner == player) return cannonballBase;
        }
        return null;
    }

    public static void CleanUp()
    {
        AllCannonballBases.Clear();
    }

    public static void Begin(PlayerControl player)
    {
        GameObject gameObject = new GameObject("CannonballBase");

        CannonballBase cannonballBase = gameObject.AddComponent<CannonballBase>();
        cannonballBase.Owner = player;
    }

    public static List<CannonballBase> AllCannonballBases = new List<CannonballBase>();
}