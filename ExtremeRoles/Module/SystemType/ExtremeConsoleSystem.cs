using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ExtremeRoles.Module.SystemType;
#nullable enable

public sealed class ExtremeConsoleSystem : IExtremeSystemType
{
	public readonly Dictionary<int, ExtremeConsole> console = new Dictionary<int, ExtremeConsole>();

	public bool IsDirty => false;

	private int id = 0;
	private const ExtremeSystemType systemType = ExtremeSystemType.ExtremeConsoleSystem;

	public static ExtremeConsoleSystem Create()
	{
		if (!ExtremeSystemTypeManager.Instance.TryGet<ExtremeConsoleSystem>(
				systemType, out var system) ||
			system is null)
		{
			system = new ExtremeConsoleSystem();
			ExtremeSystemTypeManager.Instance.TryAdd(systemType, system);
		}

		return system;
	}

	public ExtremeConsole CreateConsoleObj(in Vector3 pos, string? name = null)
	{
		if (string.IsNullOrEmpty(name))
		{
			name = $"ExtremeConsole";
		}

		name = $"{name}_{this.id}";

		this.id++;

		var obj = new GameObject(name);
		obj.transform.position = pos;

		var console = obj.AddComponent<ExtremeConsole>();
		return console;
	}

	public ExtremeConsole CreateConsoleObj<T>(in Vector2 pos, string? name = null, T? behaviour = default(T))
	{
		return CreateConsoleObj(new Vector3(pos.x, pos.y, pos.y / 1000.0f), name, behaviour);
	}

	public ExtremeConsole CreateConsoleObj<T>(in Vector3 pos, string? name = null, T? behaviour = default(T))
		where T : ExtremeConsole.IBehavior
	{
		var console = CreateConsoleObj(pos, name);

		if (behaviour != null)
		{
			console.Behavior = behaviour;
		}
		return console;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{ }

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void Serialize(MessageWriter writer, bool initialState)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{ }
}
