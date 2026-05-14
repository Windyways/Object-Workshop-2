using UnityEngine;
using System.Collections;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Spirit(IntPtr ptr) : MonoBehaviour(ptr)
{
    public byte id;
    public SpriteRenderer myRend;
    public PlayerControl Owner;
    public PlayerControl target;

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();
        id = GetAvailableId();

        AllSpirits.Add(this);

        Coroutines.Start(HuntRoutine(Owner.GetAdjustedPosition()));
    }

    private void Update()
    {
        UpdateColor();
    }

    private static bool Visibility() => true;
    private void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();
    }

    public void OnDestroy()
    {
        AllSpirits.Remove(this);
    }

    public IEnumerator HuntRoutine(Vector3 startPos)
    {
        Vector3 velocity = Vector3.zero;
        float accel = 0.25f;
        float maxSpeed = 50f;

        transform.position = startPos;

        while (target != null && target.PlayerId != 0)
        {
            Vector3 tPos = target.transform.position;
            Vector3 dir = (tPos - transform.position).normalized;

            // accelerate velocity over time toward maxSpeed
            float speed = Mathf.Min(maxSpeed, velocity.magnitude + accel * Time.deltaTime);
            velocity = dir * speed;

            transform.position += velocity * Time.deltaTime;

            // exit if reached or almost reached target
            if ((tPos - transform.position).sqrMagnitude < 0.01f)
                break;

            yield return null;
        }

        var tombstone = Tombstone.Begin(Owner, target.transform.position);
        tombstone.AddArrow();

        Owner.RpcCustomMurder(target, teleportMurderer: false, createDeadBody: false);
        VisitingMechanic.RpcAddDeathReason(target, (int)DeathReasonShow.Haunted);

        Destroy(gameObject);
    }

    public static void Begin(PlayerControl player, PlayerControl target)
    {
        WitnessKill.WitnessEvilDeed(player);

        GameObject gameObject = new GameObject("Spirit");
        gameObject.AddSpriteRenderer(OWAssets.Gravekeeper_Spirit.LoadAsset(), 10, 100, player.GetTruePosition(),
            new Color(0, 0, 1, 1), Vector3.one);

        Spirit spirit = gameObject.AddComponent<Spirit>();
        spirit.Owner = player;
        spirit.target = target;

        gameObject.AddOWObject();
    }

    public static byte GetAvailableId()
    {
        byte id = 0;
        while (AllSpirits.Any(x => x.id == id))
        {
            id++;
        }
        return id;
    }

    public static void CleanUp()
    {
        AllSpirits.Clear();
    }

    public static List<Spirit> AllSpirits = new List<Spirit>();
}