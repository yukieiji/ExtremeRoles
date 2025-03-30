using System.Collections.Generic;
using System.Linq;

// from TOR : https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Utilities/MapUtilities.cs

namespace ExtremeRoles.Performance;

public static class ShipStatusCache
{
    public static Dictionary<SystemTypes, PlainShipRoom> KeyedRoom { get; private set; }

    public static void SetUp(ShipStatus instance)
    {
        KeyedRoom = instance.AllRooms.Where(
            (PlainShipRoom p) => p.RoomId > SystemTypes.Hallway
                ).ToDictionary((PlainShipRoom r) => r.RoomId);
    }

    public static void Destroy()
    {
        KeyedRoom.Clear();
    }
}
