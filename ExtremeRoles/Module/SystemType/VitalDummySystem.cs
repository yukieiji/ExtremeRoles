using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Hazel;
using HarmonyLib;

using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class VitalDummySystem : IExtremeSystemType
{
	public bool IsActive { get; set; } = false;

	public DummyMode Mode { get; set; } = DummyMode.No;

	public enum DummyMode
	{
		No,
		Random,
		OnceRandom,
	}

	public enum Option
	{
		AddAlive,
		AddDead,
		AddDisconnect,
		RemoveAlive,
		RemoveDead,
		RemoveDisconnect,
	}

	private readonly HashSet<byte> alivePlayer = new HashSet<byte>();
	private readonly HashSet<byte> deadPlayer = new HashSet<byte>();
	private readonly HashSet<byte> disconectPlayer = new HashSet<byte>();

	private bool isUpdate = false;

	private float time;

	public static VitalDummySystem Get()
	=> ExtremeSystemTypeManager.Instance.CreateOrGet<VitalDummySystem>(ExtremeSystemType.VitalDummySystem);

	public static bool TryGet([NotNullWhen(true)] out VitalDummySystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.VitalDummySystem, out system);

	public void AddAlive(params byte[] playerIds)
	{
		lock (alivePlayer)
		{
			foreach (byte playerId in playerIds)
			{
				this.alivePlayer.Add(playerId);
			}
		}
	}

	public void AddDead(params byte[] playerIds)
	{
		lock (deadPlayer)
		{
			foreach (byte playerId in playerIds)
			{
				this.deadPlayer.Add(playerId);
			}
		}
	}

	public void AddDisconnect(params byte[] playerIds)
	{
		lock (disconectPlayer)
		{
			foreach (byte playerId in playerIds)
			{
				this.disconectPlayer.Add(playerId);
			}
		}
	}

	public void RemoveAlive(params byte[] playerIds)
	{
		lock (alivePlayer)
		{
			foreach (byte playerId in playerIds)
			{
				this.alivePlayer.Remove(playerId);
			}
		}
	}

	public void RemoveDead(params byte[] playerIds)
	{
		lock (deadPlayer)
		{
			foreach (byte playerId in playerIds)
			{
				this.deadPlayer.Remove(playerId);
			}
		}
	}

	public void RemoveDisconnect(params byte[] playerIds)
	{
		lock (disconectPlayer)
		{
			foreach (byte playerId in playerIds)
			{
				this.disconectPlayer.Remove(playerId);
			}
		}
	}

	public void VitalBeginPostfix()
	{
		this.isUpdate = false;
	}

	public void OverrideVitalUpdate(VitalsMinigame minigame)
	{
		bool isSabActive =
			PlayerControl.LocalPlayer != null &&
			PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer);

		if (!isSabActive)
		{
			minigame.SabText.gameObject.SetActive(false);
			minigame.vitals.Do(x => x.gameObject.SetActive(true));
		}
		else if (!minigame.SabText.isActiveAndEnabled)
		{
			minigame.SabText.gameObject.SetActive(true);
			minigame.vitals.Do(x => x.gameObject.SetActive(false));
		}

		switch (this.Mode)
		{

			case DummyMode.No:
				noUpdate(minigame.vitals);
				break;
			case DummyMode.Random:
				randomUpdate(minigame.vitals);
				break;
			case DummyMode.OnceRandom:
				onceUpdate(minigame.vitals);
				break;
			default:
				break;
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.OnPlayer)
		{
			this.disconectPlayer.Clear();
			this.deadPlayer.Clear();
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var opt = (Option)msgReader.ReadByte();
		byte playerId = msgReader.ReadByte();

		switch (opt)
		{
			case Option.AddAlive:
				this.AddAlive(playerId);
				break;
			case Option.AddDead:
				this.AddDead(playerId);
				break;
			case Option.AddDisconnect:
				this.AddDisconnect(playerId);
				break;
			case Option.RemoveAlive:
				this.RemoveAlive(playerId);
				break;
			case Option.RemoveDead:
				this.RemoveDead(playerId);
				break;
			case Option.RemoveDisconnect:
				this.RemoveDisconnect(playerId);
				break;
			default:
				break;
		}
	}

	private void onceUpdate(IList<VitalsPanel> vitals)
	{
		if (this.isUpdate)
		{
			return;
		}
		foreach (var panel in vitals)
		{
			byte playerId = panel.PlayerInfo.PlayerId;

			if (alivePlayer.Contains(playerId))
			{
				continue;
			}

			if (!panel.PlayerInfo.IsDead && panel.PlayerInfo.Disconnected && !panel.IsDiscon)
			{
				panel.SetDisconnected();
			}
			else if (panel.PlayerInfo.IsDead && !panel.IsDead && !panel.IsDiscon)
			{
				panel.SetDead();
			}
			else if (disconectPlayer.Contains(playerId))
			{
				panel.SetDisconnected();
			}
			else if (deadPlayer.Contains(playerId))
			{
				panel.SetDead();
			}
			else
			{
				setToRandomStatus(panel);
			}
		}

		this.isUpdate = true;
	}

	private void noUpdate(IList<VitalsPanel> vitals)
	{
		if (this.isUpdate)
		{
			return;
		}
		foreach (var panel in vitals)
		{
			byte playerId = panel.PlayerInfo.PlayerId;

			if (alivePlayer.Contains(playerId))
			{
				continue;
			}

			if (!panel.PlayerInfo.IsDead && panel.PlayerInfo.Disconnected && !panel.IsDiscon)
			{
				panel.SetDisconnected();
			}
			else if (panel.PlayerInfo.IsDead && !panel.IsDead && !panel.IsDiscon)
			{
				panel.SetDead();
			}
			else if (disconectPlayer.Contains(playerId))
			{
				panel.SetDisconnected();
			}
			else if (deadPlayer.Contains(playerId))
			{
				panel.SetDead();
			}
		}
		this.isUpdate = true;
	}

	private void randomUpdate(IList<VitalsPanel> vitals)
	{
		if (this.time <= 1.0)
		{
			this.time += UnityEngine.Time.deltaTime;
			return;
		}

		foreach (var panel in vitals)
		{
			byte playerId = panel.PlayerInfo.PlayerId;

			if (alivePlayer.Contains(playerId))
			{
				continue;
			}

			if (disconectPlayer.Contains(playerId))
			{
				panel.SetDisconnected();
			}
			else if (deadPlayer.Contains(playerId))
			{
				panel.SetDead();
			}
			else
			{
				setToRandomStatus(panel);
			}
		}
	}

	private static void setToRandomStatus(VitalsPanel panel)
	{
		switch (RandomGenerator.Instance.Next(2))
		{
			case 1:
				panel.SetDisconnected();
				break;
			case 2:
				panel.SetDead();
				break;
			default:
				break;
		}
	}
}
