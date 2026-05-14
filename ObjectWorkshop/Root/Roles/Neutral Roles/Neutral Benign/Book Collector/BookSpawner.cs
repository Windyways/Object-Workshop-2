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

        { (SystemTypes.MainHall, CurrentMap.Airship), new Vector2(09.77f, -00.10f) },
        { (SystemTypes.Engine, CurrentMap.Airship), new Vector2(-02.60f, -00.94f) },
        { (SystemTypes.Comms, CurrentMap.Airship), new Vector2(-13.28f, 01.40f) },
        { (SystemTypes.Cockpit, CurrentMap.Airship), new Vector2(-20.99f, -00.99f) },
        { (SystemTypes.Armory, CurrentMap.Airship), new Vector2(-10.17f, -06.07f) },
        { (SystemTypes.Kitchen, CurrentMap.Airship), new Vector2(-04.29f, -11.03f) },
        { (SystemTypes.ViewingDeck, CurrentMap.Airship), new Vector2(-13.46f, -12.29f) },
        { (SystemTypes.Security, CurrentMap.Airship), new Vector2(07.10f, -11.39f) },
        { (SystemTypes.Electrical, CurrentMap.Airship), new Vector2(16.53f, -08.71f) },
        { (SystemTypes.Medical, CurrentMap.Airship), new Vector2(26.35f, -05.80f) },
        { (SystemTypes.CargoBay, CurrentMap.Airship), new Vector2(38.15f, 00.21f) },
        { (SystemTypes.Lounge, CurrentMap.Airship), new Vector2(27.13f, 05.61f) },
        { (SystemTypes.Records, CurrentMap.Airship), new Vector2(19.96f, 08.35f) },
        { (SystemTypes.Showers, CurrentMap.Airship), new Vector2(21.44f, -00.47f) },
        { (SystemTypes.GapRoom, CurrentMap.Airship), new Vector2(12.02f, 09.13f) },
        { (SystemTypes.Brig, CurrentMap.Airship), new Vector2(01.31f, 08.83f) },
        { (SystemTypes.VaultRoom, CurrentMap.Airship), new Vector2(-07.55f, 08.83f) },
        { (SystemTypes.MeetingRoom, CurrentMap.Airship), new Vector2(06.90f, 15.15f) },

        { (SystemTypes.Cafeteria, CurrentMap.Fungle), new Vector2(-16.68f, 07.14f) },
        { (SystemTypes.RecRoom, CurrentMap.Fungle), new Vector2(-16.10f, -00.06f) }, // WHY IS SPLASH ZONE 'REC ROOM' ??????
        { (SystemTypes.Kitchen, CurrentMap.Fungle), new Vector2(-15.45f, -07.42f) },
        { (SystemTypes.FishingDock, CurrentMap.Fungle), new Vector2(-22.27f, -07.06f) },
        { (SystemTypes.Laboratory, CurrentMap.Fungle), new Vector2(-05.10f, -08.91f) },
        { (SystemTypes.Greenhouse, CurrentMap.Fungle), new Vector2(09.02f, -10.19f) },
        { (SystemTypes.Reactor, CurrentMap.Fungle), new Vector2(21.02f, -07.28f) },
        { (SystemTypes.UpperEngine, CurrentMap.Fungle), new Vector2(22.14f, 03.21f) },
        { (SystemTypes.MiningPit, CurrentMap.Fungle), new Vector2(12.22f, 09.9f) },
        { (SystemTypes.Lookout, CurrentMap.Fungle), new Vector2(07.38f, 02.20f) },
        { (SystemTypes.Comms, CurrentMap.Fungle), new Vector2(22.40f, 13.62f) },
        { (SystemTypes.Storage, CurrentMap.Fungle), new Vector2(-00.25f, 06.33f) },
        { (SystemTypes.Dropship, CurrentMap.Fungle), new Vector2(-07.85f, 10.92f) },
        { (SystemTypes.MeetingRoom, CurrentMap.Fungle), new Vector2(-02.88f, -03.16f) },
        { (SystemTypes.SleepingQuarters, CurrentMap.Fungle), new Vector2(01.94f, -01.47f) }, // BRUH 'SLEEPING QUARTERS' IS THE DORM?!!? INNER SLOTH WHYYYYYYYY
    };
}