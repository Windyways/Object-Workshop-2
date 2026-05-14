using UnityEngine;
using Random = UnityEngine.Random;

namespace ObjectWorkshop.Objects;

[RegisterInIl2Cpp]
public class Nest(IntPtr ptr) : MonoBehaviour(ptr)
{
    public SpriteRenderer myRend;
    public int eggsInNest;
    public List<GameObject> eggs = new List<GameObject>();
    public float radius = 0.4f; 

    public void Start()
    {
        myRend = gameObject.GetComponent<SpriteRenderer>();
        SpawnEggs(eggsInNest);
    }

    private void Update()
    {
        UpdateColor();
    }

    private static bool Visibility() => true;
    public void UpdateColor()
    {
        if (Visibility())
        {
            myRend.Show();
            foreach (var egg in eggs) egg.GetComponent<SpriteRenderer>().Show();
        }
        else
        {
            myRend.Hide();
            foreach (var egg in eggs) egg.GetComponent<SpriteRenderer>().Hide();
        }
    }

    public void SpawnEggs(int count)
    {
        for (var i = 0; i < count; i++)
        {
            GameObject egg = new GameObject($"Egg {i + 1}");
            egg.AddSpriteRenderer(OWAssets.Settler_Egg.LoadAsset(), 0, 101, transform.position, ObjectExtentions.noColor(), Vector3.one);
            egg.transform.SetParent(transform);

            // -- Created by Chat GPT
            var bounds = myRend.bounds;
            Vector2 random = UnityEngine.Random.insideUnitCircle;
            float rangeX = bounds.size.x * 0.35f;
            float rangeY = bounds.size.y * 0.25f;
            egg.transform.localPosition = new Vector3(random.x * rangeX, random.y * rangeY, 0f);

            float scale = Random.Range(0.2f, 0.32f);
            egg.transform.localScale = Vector3.one * scale;
            // --

            eggs.Add(egg);
        }
    }

    [MethodRpc((uint)Rpcs.RpcSpawnNest)]
    public static void RpcSpawnNest(int assignedRoomInt, Vector2 assignedPosition)
    {
        var assignedRoom = (SystemTypes)assignedRoomInt;

        GameObject gameObject = new GameObject("Nest");
        gameObject.AddSpriteRenderer(OWAssets.Settler_Nest.LoadAsset(), 0, 100, assignedPosition, ObjectExtentions.noColor(), Vector3.one);
        gameObject.transform.localScale = new Vector2(0.8f, 0.8f);

        var nest = gameObject.AddComponent<Nest>();
        currentNest = nest;

        var settler = MiscUtils.GetRoles<Settler>().OrderByDescending(x => x.EggsLayed).FirstOrDefault();
        if (settler != null) nest.eggsInNest = settler.EggsLayed;

        OWPlugin.DebugLogMessage($"Settler Nest spawned in {assignedRoom} at coordinates {assignedPosition}!");
    }

    public static SystemTypes GetAssignedRoom()
    {
        List<SystemTypes> tryGetRoom = AvailableRoomsToSpawn;
        SystemTypes roomToSpawnIn = SystemTypes.Outside; // default.

        if (tryGetRoom.Count > 0)
        {
            tryGetRoom.Shuffle();
            roomToSpawnIn = tryGetRoom.Random();
        }

        return roomToSpawnIn;
    }

    public static Vector2 GetAssignedPosition(SystemTypes roomToSpawnIn)
    {
        if (NestDestinations.TryGetValue((roomToSpawnIn, CheckMap.MapSelected), out var position))
        {
            return position;
        }

        return new Vector2(0, 0);
    }

    public static void SpawnNest()
    {
        AvailableRoomsToSpawn.Clear();
        AvailableRoomsToSpawn = GetAvailableRoomsToSpawn();

        if (currentNest == null) // In case of multiple Settlers, only one Nest will spawn.
        {
            var room = GetAssignedRoom();
            var pos = GetAssignedPosition(room);
            if (room == SystemTypes.Outside || pos == new Vector2(0, 0))
            {
                OWPlugin.DebugLogMessage("Nest couldn't find a valid spot!", OWPlugin.MsgType.Error);
                return;
            }

            RpcSpawnNest((int)room, pos);
        }
    }

    public static List<SystemTypes> GetAvailableRoomsToSpawn()
    {
        var list = new List<SystemTypes>();
        if (CheckMap.MapSelected == CurrentMap.Skeld)
        {
            list.Add(SystemTypes.LowerEngine);
            list.Add(SystemTypes.Shields);
            list.Add(SystemTypes.UpperEngine);
            list.Add(SystemTypes.Weapons);
        }
        else if (CheckMap.MapSelected == CurrentMap.MiraHQ)
        {
            list.Add(SystemTypes.Launchpad);
            list.Add(SystemTypes.Reactor);
            list.Add(SystemTypes.Greenhouse);
            list.Add(SystemTypes.Balcony);
        }
        else if (CheckMap.MapSelected == CurrentMap.Polus)
        {
            list.Add(SystemTypes.Reactor);
            list.Add(SystemTypes.BoilerRoom);
            list.Add(SystemTypes.Laboratory);
            list.Add(SystemTypes.Specimens);
        }
        else if (CheckMap.MapSelected == CurrentMap.Airship)
        {
            list.Add(SystemTypes.ViewingDeck);
            list.Add(SystemTypes.Medical);
            list.Add(SystemTypes.Lounge);
            list.Add(SystemTypes.VaultRoom);
        }
        else if (CheckMap.MapSelected == CurrentMap.Fungle)
        {
            list.Add(SystemTypes.Cafeteria);
            list.Add(SystemTypes.Jungle);
            list.Add(SystemTypes.Reactor);
            list.Add(SystemTypes.Comms);
        }

        return list;
    }

    public static void CleanUp()
    {
        currentNest = null;
    }

    public static bool IsInRange(PlayerControl player)
    {
        var nest = currentNest;
        if (nest == null) return false;
        return Vector2.Distance(player.transform.position, nest.transform.position) <= nest.radius;
    }

    public static Nest? currentNest;
    public static List<SystemTypes> AvailableRoomsToSpawn = new List<SystemTypes>();
    public static Dictionary<(SystemTypes, CurrentMap), Vector2> NestDestinations = new Dictionary<(SystemTypes, CurrentMap), Vector2>()
    {
        { (SystemTypes.Weapons, CurrentMap.Skeld), new Vector2(10.09f, 2.66f) },
        { (SystemTypes.Shields, CurrentMap.Skeld), new Vector2(09.79f, -13.49f) },
        { (SystemTypes.LowerEngine, CurrentMap.Skeld), new Vector2(-17.98f, -13.20f) },
        { (SystemTypes.UpperEngine, CurrentMap.Skeld), new Vector2(-18f, 02.54f) },

        { (SystemTypes.Launchpad, CurrentMap.MiraHQ), new Vector2(-05.61f, -1.79f) },
        { (SystemTypes.Reactor, CurrentMap.MiraHQ), new Vector2(01.15f, 13.84f) },
        { (SystemTypes.Greenhouse, CurrentMap.MiraHQ), new Vector2(21.10f, 24.34f) },
        { (SystemTypes.Balcony, CurrentMap.MiraHQ), new Vector2(27.89f, -01.82f) },

        { (SystemTypes.Reactor, CurrentMap.Polus), new Vector2(04.62f, -04.40f) }, // Seismic!
        { (SystemTypes.BoilerRoom, CurrentMap.Polus), new Vector2(01.37f, -23.73f) },
        { (SystemTypes.Specimens, CurrentMap.Polus), new Vector2(38.87f, -20.96f) },
        { (SystemTypes.Laboratory, CurrentMap.Polus), new Vector2(40.54f, -07.84f) },

        { (SystemTypes.ViewingDeck, CurrentMap.Airship), new Vector2(-13.85f, -14.66f) },
        { (SystemTypes.Medical, CurrentMap.Airship), new Vector2(32.91f, -05.97f) },
        { (SystemTypes.Lounge, CurrentMap.Airship), new Vector2(27.13f, 09.89f) },
        { (SystemTypes.VaultRoom, CurrentMap.Airship), new Vector2(-11.26f, 12.33f) },

        { (SystemTypes.Cafeteria, CurrentMap.Fungle), new Vector2(-18.70f, 06.93f) },
        { (SystemTypes.Jungle, CurrentMap.Fungle), new Vector2(-09.73f, -13.76f) },
        { (SystemTypes.Reactor, CurrentMap.Fungle), new Vector2(23.59f, -07.86f) },
        { (SystemTypes.Comms, CurrentMap.Fungle), new Vector2(24.43f, 14.50f) },
    };
}