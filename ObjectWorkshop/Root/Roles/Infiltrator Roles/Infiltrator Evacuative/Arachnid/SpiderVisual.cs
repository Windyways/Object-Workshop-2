using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class SpiderVisual(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;
    public SpriteRenderer myRend;

    public Vector3 lastPos;

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();

        AllSpiders.Add(this);
        lastPos = Owner.transform.position;
    }

    private bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();
    }

    private void Update() // Created by Chat GPT.
    {
        UpdateColor();

        Vector3 currentPos = Owner.transform.position;
        Vector3 moveDir = currentPos - lastPos;

        // Only update facing when actually moving
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle + -90);
        }

        lastPos = currentPos;
    }

    public void Stop()
    {
        AllSpiders.Remove(this);
        Destroy(gameObject);
    }

    public static void Begin(PlayerControl player)
    {
        WitnessKill.WitnessEvilDeed(player);

        GameObject gameObject = new GameObject("SpiderVisual");
        gameObject.transform.SetParent(player.transform);
        gameObject.AddSpriteRenderer(
            OWPlugin.ArachnophobiaMode.Value ? OWAssets.Arachnid_SpiderCensored.LoadAsset() : OWAssets.Arachnid_Spider.LoadAsset(), 
            0, 100, player.transform.position, ObjectExtentions.noColor(), Vector3.one);

        SpiderVisual spiderVisual = gameObject.AddComponent<SpiderVisual>();
        spiderVisual.Owner = player;

        gameObject.AddOWObject();
    }

    public static SpiderVisual GetObjectByPlayer(PlayerControl player)
    {
        foreach (SpiderVisual spiderVisual in AllSpiders)
        {
            if (spiderVisual.Owner == player) return spiderVisual;
        }
        return null;
    }

    public static void CleanUp()
    {
        AllSpiders.Clear();
    }

    public static List<SpiderVisual> AllSpiders = new List<SpiderVisual>();
}