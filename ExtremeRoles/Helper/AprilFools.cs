using ExtremeRoles.Extension.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Helper;

public static class AprilFools
{
	public static void UpdateApilSkinToggle(
		CreateGameOptions instance)
	{
		var mng = ServerManager.Instance;
		instance.AprilFoolsToggle.SetActive(
			mng != null && mng.IsCustomServer());
	}
}
