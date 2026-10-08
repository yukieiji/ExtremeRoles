using System;
using System.Collections.Generic;
using Hazel;
using TMPro;
using UnityEngine;

using ExtremeRoles.Compat;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Neutral;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class DeepOneSystem : IExtremeSystemType
{
	public enum Ops : byte
	{
		PlaceFrog = 0,
		RemoveFrog = 1,
	}

	public sealed class FrogBehavior : ExtremeConsole.IBehavior
	{
		public float CoolTime => 0.0f;
		public bool IsCheckWall => false;

		public int FrogId { get; }
		public ExtremeConsole? ConsoleObj { get; set; }
		public TextMeshPro? TextLabel { get; set; }

		private int reqClicks;
		private int currentClicks;

		public FrogBehavior(int frogId, int reqClicks)
		{
			this.FrogId = frogId;
			this.reqClicks = reqClicks;
			this.currentClicks = 0;
		}

		public void UpdateReqClicks(int newReqClicks)
		{
			this.reqClicks = newReqClicks;
			this.updateText();
		}

		public bool CanUse(NetworkedPlayerInfo pc)
		{
			if (pc == null || pc.Object == null || pc.IsDead)
			{
				return false;
			}
			return true;
		}

		public void Use()
		{
			this.currentClicks++;
			this.updateText();

			if (this.currentClicks >= this.reqClicks)
			{
				int id = this.FrogId;
				ExtremeSystemTypeManager.RpcUpdateSystem(
					ExtremeSystemType.DeepOneSystem,
					writer =>
					{
						writer.Write((byte)Ops.RemoveFrog);
						writer.WritePacked(id);
					});
			}
		}

		private void updateText()
		{
			if (this.TextLabel != null)
			{
				int remaining = Math.Max(0, this.reqClicks - this.currentClicks);
				this.TextLabel.text = remaining.ToString();
			}
		}
	}

	public int BaseRequiredClicks { get; private set; }
	public int RequiredWinFrogs { get; private set; }
	public bool EnableClickMultiplier { get; private set; }
	public bool EnableKillBlock { get; private set; }
	public bool EnableExileBlock { get; private set; }
	public bool EnableVentUnlock { get; private set; }
	public bool EnableMaxSpeed { get; private set; }

	public int ActiveFrogsCount => this.frogs.Count;

	private readonly Dictionary<int, FrogBehavior> frogs = new Dictionary<int, FrogBehavior>();
	private int nextFrogId = 0;

	public DeepOneSystem(
		int baseRequiredClicks,
		int requiredWinFrogs,
		bool enableClickMultiplier,
		bool enableKillBlock,
		bool enableExileBlock,
		bool enableVentUnlock,
		bool enableMaxSpeed)
	{
		this.UpdateOptions(
			baseRequiredClicks,
			requiredWinFrogs,
			enableClickMultiplier,
			enableKillBlock,
			enableExileBlock,
			enableVentUnlock,
			enableMaxSpeed);
	}

	public void UpdateOptions(
		int baseRequiredClicks,
		int requiredWinFrogs,
		bool enableClickMultiplier,
		bool enableKillBlock,
		bool enableExileBlock,
		bool enableVentUnlock,
		bool enableMaxSpeed)
	{
		this.BaseRequiredClicks = baseRequiredClicks;
		this.RequiredWinFrogs = requiredWinFrogs;
		this.EnableClickMultiplier = enableClickMultiplier;
		this.EnableKillBlock = enableKillBlock;
		this.EnableExileBlock = enableExileBlock;
		this.EnableVentUnlock = enableVentUnlock;
		this.EnableMaxSpeed = enableMaxSpeed;
	}

	public int GetRequiredClicks()
	{
		int count = this.ActiveFrogsCount;
		if (this.EnableClickMultiplier && count >= 5)
		{
			int multiplier = 1 + (count - 4);
			return this.BaseRequiredClicks * multiplier;
		}
		return this.BaseRequiredClicks;
	}

	public void PlaceFrogLocally(Vector2 pos)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.DeepOneSystem,
			writer =>
			{
				writer.Write((byte)Ops.PlaceFrog);
				writer.Write(pos.x);
				writer.Write(pos.y);
			});
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing == ResetTiming.OnPlayer)
		{
			return;
		}

		List<int> ids = new List<int>(this.frogs.Keys);
		foreach (int id in ids)
		{
			this.destroyFrog(id);
		}
		this.frogs.Clear();
		this.nextFrogId = 0;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.PlaceFrog:
			{
				float x = msgReader.ReadSingle();
				float y = msgReader.ReadSingle();

				int id = this.nextFrogId;
				this.nextFrogId++;

				this.createFrog(id, new Vector2(x, y));
				this.onFrogCountChanged(player);
				break;
			}
			case Ops.RemoveFrog:
			{
				int id = msgReader.ReadPackedInt32();
				this.destroyFrog(id);
				this.onFrogCountChanged(player);
				break;
			}
			default:
				break;
		}
	}

	private void createFrog(int id, Vector2 pos)
	{
		if (this.frogs.ContainsKey(id))
		{
			return;
		}

		int reqClicks = this.GetRequiredClicks();
		var frogBehavior = new FrogBehavior(id, reqClicks);

		var consoleSystem = ExtremeConsoleSystem.Create();
		var console = consoleSystem.CreateConsoleObj(
			pos,
			$"DeepOneFrog_{id}",
			frogBehavior);

		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			modMap.AddCustomComponent(
				console.gameObject,
				Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
		}

		if (console.Image != null)
		{
			console.Image.sprite = UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton);
		}

		if (HudManager.Instance != null && HudManager.Instance.KillButton != null && HudManager.Instance.KillButton.cooldownTimerText != null)
		{
			var textObj = UnityEngine.Object.Instantiate(
				HudManager.Instance.KillButton.cooldownTimerText,
				console.transform,
				false);
			textObj.transform.localPosition = new Vector3(0.0f, 0.4f, -1.0f);
			textObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			textObj.alignment = TextAlignmentOptions.Center;
			textObj.color = Color.white;
			frogBehavior.TextLabel = textObj;
			frogBehavior.UpdateReqClicks(reqClicks);
		}

		frogBehavior.ConsoleObj = console;
		this.frogs.Add(id, frogBehavior);
	}

	private void destroyFrog(int id)
	{
		if (this.frogs.TryGetValue(id, out var frog))
		{
			if (frog.ConsoleObj != null)
			{
				UnityEngine.Object.Destroy(frog.ConsoleObj.gameObject);
			}
			this.frogs.Remove(id);
		}
	}

	private void onFrogCountChanged(PlayerControl? triggerPlayer)
	{
		int reqClicks = this.GetRequiredClicks();
		foreach (var frog in this.frogs.Values)
		{
			frog.UpdateReqClicks(reqClicks);
		}

		int activeFrogs = this.ActiveFrogsCount;
		bool canBlockKill = activeFrogs >= 1 && this.EnableKillBlock;
		bool canBlockExile = activeFrogs >= 2 && this.EnableExileBlock;
		bool canUseVent = activeFrogs >= 3 && this.EnableVentUnlock;
		float moveSpeed = activeFrogs >= 4 && this.EnableMaxSpeed ? 3.0f : 1.0f;

		foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
		{
			if (ExtremeRoleManager.TryGetSafeCastedRole<DeepOne>(playerId, out var deepOne))
			{
				deepOne.IsBlockKill = canBlockKill;
				deepOne.IsBlockExile = canBlockExile;
				deepOne.UseVent = canUseVent;
				deepOne.MoveSpeed = moveSpeed;
			}
		}

		if (triggerPlayer != null && activeFrogs >= this.RequiredWinFrogs && this.RequiredWinFrogs > 0)
		{
			if (ExtremeRoleManager.TryGetRole(triggerPlayer.PlayerId, out var placerRole))
			{
				foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
				{
					if (role.IsSameTeam(placerRole) && ExtremeRoleManager.TryGetSafeCastedRole<DeepOne>(playerId, out var deepOne) && !deepOne.IsWin)
					{
						deepOne.IsWin = true;
						ExtremeRolesPlugin.ShipState.RpcRoleIsWin(playerId);
					}
				}
			}
		}
	}
}
