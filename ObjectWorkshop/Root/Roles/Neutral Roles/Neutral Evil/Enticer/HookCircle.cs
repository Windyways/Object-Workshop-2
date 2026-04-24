using UnityEngine;
using Color = UnityEngine.Color;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class HookCircle(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl Owner;
    public SpriteRenderer myRend;

    public static List<HookCircle> AllHookCircles = new List<HookCircle>();


    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();
        AllHookCircles.Add(this);
    }

    private void Update()
    {
        UpdateColor();
        if (Owner.HasDied()) Destroy(gameObject);
    }

    private bool Visibility() => PlayerControl.LocalPlayer == Owner;
    private void UpdateColor()
    {
        if (Visibility()) myRend.color = new Color(0, 0, 0, 0.4f);
        else myRend.Hide();
    }

    private void OnDestroy()
    {
        AllHookCircles.Remove(this);
    }

    public static void Begin(PlayerControl player)
    {
        var gameObject = new GameObject("HookCircle");
        gameObject.AddSpriteRenderer(OWAssets.FillerCircle.LoadAsset(), 0, 100, player.transform.position, ObjectExtentions.noColor(), Vector3.one * OptionGroupSingleton<Enticer_Options>.Instance.Radius * 2f);

        var hookCircle = gameObject.AddComponent<HookCircle>();
        hookCircle.Owner = player;
    }

    public static HookCircle GetObjectByOwner(PlayerControl player)
    {
        foreach (var hook in AllHookCircles)
        {
            if (hook.Owner == player) return hook;
        }
        return null;
    }

    public static void CleanUp()
    {
        AllHookCircles.Clear();
    }
}