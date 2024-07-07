using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using Hazel;

using System.Collections.Generic;

using UnityEngine;

namespace ExtremeRoles.Module.SystemType;
#nullable enable

public sealed class ExtremeConsoleSystem : IExtremeSystemType
{
	public readonly Dictionary<int, ExtremeConsole> console = new Dictionary<int, ExtremeConsole>();
	private int id = 0;
	private const ExtremeSystemType systemType = ExtremeSystemType.ExtremeConsoleSystem;

	public static ExtremeConsoleSystem Create()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<ExtremeConsoleSystem>(systemType);

	public ExtremeConsole CreateConsoleObj(Vector3 pos, string? name = null)
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

	public ExtremeConsole CreateConsoleObj<T>(Vector2 pos, string? name = null, T? behaviour = default(T))
		where T : ExtremeConsole.IBehavior
	{
		var console = CreateConsoleObj(new Vector3(pos.x, pos.y, pos.y / 1000.0f), name);

		if (behaviour != null)
		{
			console.Behavior = behaviour;
		}
		return console;
	}

	public ExtremeConsole CreateConsoleObj<T>(Vector3 pos, string? name = null, T? behaviour = default(T))
		where T : ExtremeConsole.IBehavior
	{
		var console = CreateConsoleObj(pos, name);

		if (behaviour != null)
		{
			console.Behavior = behaviour;
		}
		return console;
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{ }
}
