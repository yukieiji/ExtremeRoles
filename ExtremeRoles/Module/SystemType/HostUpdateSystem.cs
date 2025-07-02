using ExtremeRoles.Module.Interface;

using Hazel;

using System.Collections.Generic;

namespace ExtremeRoles.Module.SystemType;

public sealed class HostUpdateSystem : IDirtableSystemType
{
	private readonly List<IUpdatableObject> updateObject = new List<IUpdatableObject>();

	public bool IsDirty => false;

	public void MarkClean()
	{
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{ }

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{ }

	public void Serialize(MessageWriter writer, bool initialState)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{ }

	public void Add(IUpdatableObject obj)
	{
		updateObject.Add(obj);
	}

	public void Remove(int index)
	{
		updateObject[index].Clear();
		updateObject.RemoveAt(index);
	}

	public void Remove(IUpdatableObject obj)
	{
		obj.Clear();
		updateObject.Remove(obj);
	}

	public IUpdatableObject Get(int index) => updateObject[index];

	public void Deteriorate(float deltaTime)
	{
		if (AmongUsClient.Instance == null ||
			!AmongUsClient.Instance.AmHost)
		{
			return;
		}

		for (int i = 0; i < updateObject.Count; i++)
		{
			updateObject[i].Update(i);
		}
	}
}
