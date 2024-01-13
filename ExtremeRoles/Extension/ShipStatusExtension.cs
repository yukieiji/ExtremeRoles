using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module;

using ExtremeRoles.Extension.VentModule;

#nullable enable

namespace ExtremeRoles.Extension.Ship;

public static class VentExtension
{
	public static void StartVentAnimation(this ShipStatus? instance, int id)
	{
		if (instance == null) { return; }

		Vent? vent = instance.AllVents.FirstOrDefault((x) => x.Id == id);

		vent.PlayVentAnimation();
	}

    public static void AddVent(
        this ShipStatus? instance, Vent newVent, CustomVent.Type type)
    {
		if (instance == null) { return; }

        var allVents = instance.AllVents.ToList();
        allVents.Add(newVent);
        instance.AllVents = allVents.ToArray();

		CustomVent.Instance.Add(type, newVent);
    }

    public static bool TryGetCustomVent(this ShipStatus _, CustomVent.Type type, out List<Vent>? vent)
	{
		if (!CustomVent.IsExist)
		{
			vent = new List<Vent>();
			return false;
		}
		return CustomVent.Instance.TryGet(type, out vent);
	}

	public static void ResetCustomVent()
    {
		CustomVent.TryDestroy();
    }
}
