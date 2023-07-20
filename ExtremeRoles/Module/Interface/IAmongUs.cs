using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.Interface;

// AmonUs Interfaces: v2023.7.11
public static class IAmongUs
{
	public interface IUsable
	{
		public float UsableDistance { get; }

		public float PercentCool { get; }

		public ImageNames UseIcon { get; }

		public void SetOutline(bool on, bool mainTarget);

		public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse);

		public void Use();
	}
}
