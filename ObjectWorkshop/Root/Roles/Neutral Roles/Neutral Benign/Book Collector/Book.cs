using UnityEngine;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Book(IntPtr ptr) : MonoBehaviour(ptr)
{
    public byte id;
    public SpriteRenderer myRend;
    public SystemTypes room;
    public bool isScraps;

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();
        BookSpawner.AllBooks.Add(this);
    }

    private void Update()
    {
        UpdateColor();
        CollectUponRangeEnter();
    }

    private void OnDestroy()
    {
        BookSpawner.AllBooks.Remove(this);
    }

    private bool Visibility()
    {
        if (!OWPlugin.InGame()) return false;
        if (isScraps) return true;
        return PlayerControl.LocalPlayer.IsRole<BookCollector>() || PlayerControl.LocalPlayer.HasDied();
    }

    public void UpdateColor()
    {
        if (Visibility()) myRend.Show();
        else myRend.Hide();
    }

    public void CollectUponRangeEnter()
    {
        if (isScraps)
            return;

        float radius = 0.4f;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (!player.HasDied() && player.Data.Role is BookCollector bookCollector)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= radius)
                {
                    bookCollector.Charges++;
                    isScraps = true;
                    myRend.sprite = OWAssets.BookCollector_BookScrap.LoadAsset();
                }
            }
        }
    }

    [MethodRpc((uint)Rpcs.RpcSpawnBook)]
    public static void RpcSpawnBook(int assignedRoomInt, Vector2 assignedPosition)
    {
        var assignedRoom = (SystemTypes)assignedRoomInt;

        GameObject gameObject = new GameObject("Book");
        gameObject.AddSpriteRenderer(OWAssets.BookCollector_Book.LoadAsset(), 0, 100, assignedPosition, ObjectExtentions.noColor(), Vector3.one);
        gameObject.transform.localScale = new Vector2(0.3f, 0.3f);

        var book = gameObject.AddComponent<Book>();
        book.room = assignedRoom;

        OWPlugin.DebugLogMessage($"Book spawned in {assignedRoom} at coordinates {assignedPosition}!");
    }

    public static SystemTypes GetAssignedRoom()
    {
        List<SystemTypes> tryGetRoom = BookSpawner.AvailableRoomsToSpawn;
        SystemTypes roomToSpawnIn = SystemTypes.Outside; // default.

        foreach (var book in BookSpawner.AllBooks)
        {
            tryGetRoom.Remove(book.room);
        }

        if (tryGetRoom.Count > 0)
        {
            tryGetRoom.Shuffle();
            roomToSpawnIn = tryGetRoom.Random();
        }

        return roomToSpawnIn;
    }

    public static Vector2 GetAssignedPosition(SystemTypes roomToSpawnIn)
    {
        if (BookSpawner.BookDestinations.TryGetValue((roomToSpawnIn, CheckMap.MapSelected), out var position))
        {
            return position;
        }

        return new Vector2(0, 0);
    }
}