using Rewired.Utils;
using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Cannonball(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;
    public SpriteRenderer myRend;
    
	public Vector2 currentDirection = Vector2.up;
    public CannonballBase cannonballBase;

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();

        AllCannonballs.Add(this);

        Destroy(gameObject, 20f);

        Destroy(cannonballBase.directionLine);
        Destroy(cannonballBase.gameObject);

        //Lockdown.AvailableObjects.Add(cannonball);
    }

    private void OnDestroy()
    {
        AllCannonballs.Remove(this);
    }

    private bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();
    }

    private void Update()
    {
        UpdateColor();
        MoveCannonball();
    }

    private void MoveCannonball()
    {
        float speed = OptionGroupSingleton<Culverin_Options>.Instance.CannonballSpeed;
        float killRadius = 0.6f;

        if (!gameObject.IsNullOrDestroyed())
        {
            Culverin.RpcMoveCannonball(Owner, speed);

            // Check players in range
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player.HasDied()) continue;
                if (player.PlayerId == Owner.PlayerId) continue; // skip Culverin

                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= killRadius)
                {
                    Owner.RpcCustomMurder(player, teleportMurderer: false);
                    VisitingMechanic.RpcAddDeathReason(player, (int)DeathReasonShow.Shot);
                }
            }
        }
    }

    public static Cannonball? GetObjectByPlayer(PlayerControl player)
    {
        foreach (Cannonball cannonball in AllCannonballs)
        {
            if (cannonball.Owner == player) return cannonball;
        }
        return null;
    }

    public static void CleanUp()
    {
        AllCannonballs.Clear();
    }

    public static List<Cannonball> AllCannonballs = new List<Cannonball>();
}