using System.Collections;
using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Marked(IntPtr ptr) : MonoBehaviour(ptr)
{
    public byte id;
    public PlayerControl Owner;
    public SpriteRenderer myRend;

    public static List<Marked> AllMarks = new List<Marked>();
    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();

        id = GetAvailableId();
        AllMarks.Add(this);

        Coroutines.Start(SpinRoutine());
    }

    private void Update()
    {
        UpdateColor();
    }

    private bool Visibility() => 
        PlayerControl.LocalPlayer != Owner && WitnessKill.BotCanSee(Owner, PlayerControl.LocalPlayer, Owner.transform.position) &&
        !Owner.IsSpider() && !Owner.inVent && !Owner.HasDied();
    private void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();
    }

    private IEnumerator SpinRoutine() // Created by Chat GPT.
    {
        var spinDuration = 1;

        float elapsed = 0f;
        float startRotation = transform.eulerAngles.z;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / spinDuration;
            //float angle = startRotation + (360f * t);
            float eased = Mathf.SmoothStep(0, 1, t);
            float angle = startRotation + (360f * eased);

            transform.eulerAngles = new Vector3(0, 0, angle);

            yield return null;
        }

        // snap cleanly back to normal orientation
        transform.eulerAngles = new Vector3(0, 0, startRotation);
    }

    public static void Begin(PlayerControl player)
    {
        var obj = new GameObject("Shikari_Mark");
        obj.AddSpriteRenderer(OWAssets.Shikari_Mark.LoadAsset(), 1, 0, player.transform.position, ObjectExtentions.noColor(), Vector3.one);
        obj.AddOWObject();
        obj.transform.SetParent(player.transform);
        obj.transform.localScale = new Vector2(0.6f, 0.6f);
        obj.transform.localPosition = new Vector2(0.05f, 0.15f);

        var mark = obj.AddComponent<Marked>();
        mark.Owner = player;
    }

    public static byte GetAvailableId()
    {
        byte id = 0;
        while (AllMarks.Any(x => x.id == id))
        {
            id++;
        }
        return id;
    }

    public static void DestroyAll()
    {
        foreach (var mark in AllMarks)
        {
            Destroy(mark.gameObject);
        }
    }

    public static void CleanUp()
    {
        DestroyAll();
        AllMarks.Clear();
    }
}