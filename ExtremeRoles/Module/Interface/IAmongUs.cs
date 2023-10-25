using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.Interface;

// AmonUs Interfaces: v2023.10.24
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

	public interface ISystemType
	{
		public bool IsDirty { get; }

		public void Deteriorate(float deltaTime);

		public void UpdateSystem(PlayerControl player, MessageReader msgReader);

		public void Serialize(MessageWriter writer, bool initialState);

		public void Deserialize(MessageReader reader, bool initialState);
	}
}
