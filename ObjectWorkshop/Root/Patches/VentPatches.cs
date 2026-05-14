using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Patches;

[HarmonyPatch]
public static class VentPatches
{
    [MethodRpc((uint)Rpcs.RpcSpawnVent)]
    public static void RpcSpawnVent(PlayerControl player, Vector2 position, bool hideVent = false, int ventCount = 0)
    {
        float zAxis = player.transform.position.z + 0.0004f;

        var ventPrefab = ShipStatus.Instance.AllVents[0];
        var vent = Object.Instantiate(ventPrefab, ventPrefab.transform.parent);
        vent.Id = GetNextVentId();
        vent.transform.position = new Vector3(position.x, position.y, zAxis);
        vent.Left = null;
        vent.Right = null;
        vent.Center = null;

        List<Vent> allVents = ShipStatus.Instance.AllVents.ToList<Vent>();
        allVents.Add(vent);

        ShipStatus.Instance.AllVents = allVents.ToArray();

        vent.name = $"Vent-{player.PlayerId}-{vent.Id}";

        Logger<OWPlugin>.Error($"RpcPlaceVent - vent: {vent.name}");

        if (player.Data.Role is Gravekeeper gravekeeper) gravekeeper.GravekeeperVents.Add(vent);
        /*else if (player.Data.Role is Excavator)
        {
            var validHoleHandlers = HoleHandler.GetAll().Where(x => x.Hole2 == null && x.Owner == player).ToList();
            var holeHandler = validHoleHandlers[0];
            if (holeHandler == null)
                return;

            if (ventCount == 1) holeHandler.Hole1 = vent;
            else if (ventCount == 2) holeHandler.Hole2 = vent;
        }*/

        if (hideVent && vent.GetComponent<SpriteRenderer>() != null) vent.GetComponent<SpriteRenderer>().sortingOrder = -100;






        // Other stuff here ig.
        PlainShipRoom? plainShipRoom = null;

        var allRooms2 = ShipStatus.Instance.FastRooms;
        foreach (var plainShipRoom2 in allRooms2.Values)
        {
            if (plainShipRoom2.roomArea && plainShipRoom2.roomArea.OverlapPoint(vent.transform.position))
            {
                plainShipRoom = plainShipRoom2;
            }
        }

        var mapId = (MapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId;
        if (TutorialManager.InstanceExists)
        {
            mapId = (MapNames)AmongUsClient.Instance.TutorialMapId;
        }

        if (mapId is MapNames.Polus && plainShipRoom?.RoomId is SystemTypes.Weapons)
        {
            vent.gameObject.transform.position = new Vector3(vent.gameObject.transform.position.x,
                vent.gameObject.transform.position.y, -0.0209f);
        }

        if (ModCompatibility.SubLoaded)
        {
            vent.gameObject.layer = 12;
            vent.gameObject.AddSubmergedComponent("ElevatorMover"); // just in case elevator vent is not blocked
            if (vent.gameObject.transform.position.y > -7)
            {
                vent.gameObject.transform.position = new Vector3(vent.gameObject.transform.position.x,
                    vent.gameObject.transform.position.y, 0.03f);
            }
            else
            {
                vent.gameObject.transform.position = new Vector3(vent.gameObject.transform.position.x,
                    vent.gameObject.transform.position.y, 0.0009f);
                vent.gameObject.transform.localPosition = new Vector3(vent.gameObject.transform.localPosition.x,
                    vent.gameObject.transform.localPosition.y, -0.003f);
            }
        }
    }

    public static int GetNextVentId()
    {
        var id = 0;

        while (true)
        {
            if (ShipStatus.Instance.AllVents.All(v => v.Id != id))
            {
                return id;
            }

            id++;
        }
    }

    [RegisterEvent]
    public static void EnterVentEvent(EnterVentEvent @event)
    {
        var vent = @event.Vent;
        if (/*HoleHandler.ExcavatedHoles.Contains(vent) || */vent.IsGravekeeperVent())
        {
            @event.Cancel();
        }
    }
}