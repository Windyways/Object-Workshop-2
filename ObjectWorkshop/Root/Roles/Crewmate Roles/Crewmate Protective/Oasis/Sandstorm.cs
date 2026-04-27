using Rewired.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Sandstorm(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;
    public SpriteRenderer myRend;

	public float spawnInterval = 0.03f;
	public float particleSpeedMin = 8f;
	public float particleSpeedMax = 18f;
	private float spawnTimer;
	private float effectTimer;
	private GameObject overlay;
        
    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();

        AllSandstorms.Add(this);

        effectTimer = OptionGroupSingleton<Oasis_Options>.Instance.SandstormDuration;
        CreateOverlay();
    }

    private void Update()
    {
		WhileActive();
    }

    private void OnDestroy()
    {
        AllSandstorms.Remove(this);
    }

    public void WhileActive()
    {
        if (!this.IsNullOrDestroyed())
        {
            effectTimer -= Time.deltaTime;
            if (effectTimer <= 0f) Oasis.RpcStopSandstorm(Owner);
            else
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0f)
                {
                    spawnTimer = spawnInterval;
                    SpawnSandParticle();
                }
            }
        }
    }

    private void SpawnSandParticle()
    {
        GameObject particle = new GameObject("SandParticle");
        SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
        sr.sprite = GetSandSprite();
        sr.sortingOrder = 100;

        Vector2 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(1.2f, Random.Range(0.1f, 0.9f), Camera.main.nearClipPlane));
        particle.transform.position = spawnPos;
        particle.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(-15f, 15f));

        SandParticle mover = particle.AddComponent<SandParticle>();
        mover.speed = Random.Range(particleSpeedMin, particleSpeedMax);
        mover.lifetime = 2f;
    }

    private void CreateOverlay()
    {
        overlay = new GameObject("SandstormOverlay");
        SpriteRenderer sr = overlay.AddComponent<SpriteRenderer>();
        sr.sprite = OWAssets.Oasis_SandOverlay.LoadAsset();
        sr.color = new Color(1f, 0.95f, 0.5f, 0.4f);
        sr.sortingOrder = 999;
        overlay.transform.localScale = new Vector3(40f, 28f, 1f);
        overlay.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10f);
        var followCamera = overlay.AddComponent<FollowCamera>();
        //followCamera.Owner = Owner;
    }

    public void Stop()
    {
        AllSandstorms.Remove(this);
        Destroy(overlay);
        Destroy(gameObject);
    }

    public Sprite GetSandSprite()
    {
        int num = Random.Range(0, 2);
        if (num == 0) return OWAssets.Oasis_SandParticle1.LoadAsset();
        return OWAssets.Oasis_SandParticle2.LoadAsset();
    }

    public static void Begin(PlayerControl player)
    {
        GameObject gameObject = new GameObject("SandstormEffect");

        Sandstorm sandstorm = gameObject.AddComponent<Sandstorm>();
        sandstorm.Owner = player;
    }

    public static void DestroyAll()
    {
        foreach (Sandstorm sandstorms in AllSandstorms)
        {
            Destroy(sandstorms.gameObject);
        }
        AllSandstorms.Clear();
    }

    public static Sandstorm? GetObjectByPlayer(PlayerControl player)
    {
        foreach (Sandstorm sandstorm in AllSandstorms)
        {
            if (sandstorm.Owner == player) return sandstorm;
        }
        return null;
    }

    public static void CleanUp()
    {
        AllSandstorms.Clear();
    }

    public static IEnumerable<Sandstorm> GetAll() => AllSandstorms;
    public static List<Sandstorm> AllSandstorms = new List<Sandstorm>();
}
