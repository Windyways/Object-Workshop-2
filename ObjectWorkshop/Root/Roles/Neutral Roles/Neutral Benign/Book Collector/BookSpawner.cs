using System.Collections;
using UnityEngine;

namespace ObjectWorkshop.Roles;

public static class BookSpawner
{
    public static IEnumerator Start()
    {
        while (OWPlugin.InGame())
        {
            int timeToSpawn = 0;
            while (timeToSpawn < OptionGroupSingleton<BookCollector_Options>.Instance.SpawnCooldown)
            {
                if (!OWPlugin.InGame())
                    yield break;

                timeToSpawn++;
                yield return new WaitForSeconds(1f);
            }

            while (AllBooks.Count(x => !x.isScraps) == OptionGroupSingleton<BookCollector_Options>.Instance.MaxBooksAtOnce)
            {
                if (!OWPlugin.InGame())
                    yield break;

                yield return new WaitForSeconds(2.5f); // try again in 2.5s if at max limit.
            }

            var room = Book.GetAssignedRoom();
            var pos = Book.GetAssignedPosition(room);
            while (room == SystemTypes.Outside || pos == new Vector2(0, 0))
            {
                if (!OWPlugin.InGame())
                    yield break;

                yield return new WaitForSeconds(2.5f); // try again in 2.5s until a room is available.
            }

            Book.RpcSpawnBook((int)room, pos);

        }
    }

    // Book, isScrap
    public static List<Book> AllBooks = new List<Book>();
    public static List<SystemTypes> AvailableRoomsToSpawn = new List<SystemTypes>();

    public static Dictionary<(SystemTypes, CurrentMap), Vector2> BookDestinations = new Dictionary<(SystemTypes, CurrentMap), Vector2>()
    {
        { (SystemTypes.Cafeteria, CurrentMap.Skeld), new Vector2(-00.96f, -02.93f) },
        { (SystemTypes.Weapons, CurrentMap.Skeld), new Vector2(09.34f, 00.68f) },
        { (SystemTypes.LifeSupp, CurrentMap.Skeld), new Vector2(06.60f, -03.51f) },
        { (SystemTypes.Nav, CurrentMap.Skeld), new Vector2(16.93f, -04.54f) },
        { (SystemTypes.Shields, CurrentMap.Skeld), new Vector2(09.38f, -12.30f) },
        { (SystemTypes.Comms, CurrentMap.Skeld), new Vector2(04.00f, -15.39f) },
        { (SystemTypes.Storage, CurrentMap.Skeld), new Vector2(-01.47f, -15.87f) },
        { (SystemTypes.Electrical, CurrentMap.Skeld), new Vector2(-08.27f, -11.14f) },
        { (SystemTypes.LowerEngine, CurrentMap.Skeld), new Vector2(-15.76f, -13.17f) },
        { (SystemTypes.Reactor, CurrentMap.Skeld), new Vector2(-20.36f, -05.18f) },
        { (SystemTypes.Security, CurrentMap.Skeld), new Vector2(-13.59f, -04.53f) },
        { (SystemTypes.UpperEngine, CurrentMap.Skeld), new Vector2(-15.68f, 02.57f) },
        { (SystemTypes.MedBay, CurrentMap.Skeld), new Vector2(-08.70f, -04.08f) },
        { (SystemTypes.Admin, CurrentMap.Skeld), new Vector2(04.41f, -07.44f) },

        { (SystemTypes.Launchpad, CurrentMap.MiraHQ), new Vector2(-04.42f, 02.70f) },
        { (SystemTypes.MedBay, CurrentMap.MiraHQ), new Vector2(15.38f, 00.01f) },
        { (SystemTypes.Comms, CurrentMap.MiraHQ), new Vector2(15.34f, 04.10f) },
        { (SystemTypes.LockerRoom, CurrentMap.MiraHQ), new Vector2(08.74f, 01.53f) },
        { (SystemTypes.Decontamination, CurrentMap.MiraHQ), new Vector2(06.04f, 06.32f) },
        { (SystemTypes.Reactor, CurrentMap.MiraHQ), new Vector2(02.61f, 11.08f) },
        { (SystemTypes.Laboratory, CurrentMap.MiraHQ), new Vector2(09.35f, 12.60f) },
        { (SystemTypes.Office, CurrentMap.MiraHQ), new Vector2(14.96f, 19.27f) },
        { (SystemTypes.Admin, CurrentMap.MiraHQ), new Vector2(20.96f, 20.60f) },
        { (SystemTypes.Greenhouse, CurrentMap.MiraHQ), new Vector2(17.71f, 23.32f) },
        { (SystemTypes.Storage, CurrentMap.MiraHQ), new Vector2(19.53f, 04.64f) },
        { (SystemTypes.Balcony, CurrentMap.MiraHQ), new Vector2(22.36f, -01.75f) },
        { (SystemTypes.Cafeteria, CurrentMap.MiraHQ), new Vector2(25.65f, 04.78f) },

        { (SystemTypes.Dropship, CurrentMap.Polus), new Vector2(16.63f, -04.10f) },
        { (SystemTypes.Electrical, CurrentMap.Polus), new Vector2(05.41f, -09.91f) },
        { (SystemTypes.Security, CurrentMap.Polus), new Vector2(02.89f, -12.01f) },
        { (SystemTypes.LifeSupp, CurrentMap.Polus), new Vector2(03.00f, -20.19f) },
        { (SystemTypes.BoilerRoom, CurrentMap.Polus), new Vector2(02.18f, -23.83f) },
        { (SystemTypes.Comms, CurrentMap.Polus), new Vector2(12.31f, -16.38f) },
        { (SystemTypes.Weapons, CurrentMap.Polus), new Vector2(12.32f, -23.18f) },
        { (SystemTypes.Office, CurrentMap.Polus), new Vector2(20.95f, -19.16f) },
        { (SystemTypes.Admin, CurrentMap.Polus), new Vector2(21.28f, -22.89f) },
        { (SystemTypes.Storage, CurrentMap.Polus), new Vector2(20.17f, -11.63f) },
        { (SystemTypes.Laboratory, CurrentMap.Polus), new Vector2(34.92f, -06.50f) },
        { (SystemTypes.Specimens, CurrentMap.Polus), new Vector2(36.41f, -21.29f) },

        // Add Compat for Airship and Fungle later.
    };
}