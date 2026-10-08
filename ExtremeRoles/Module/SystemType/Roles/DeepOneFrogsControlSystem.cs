using Hazel;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

using ExtremeRoles.Compat;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Neutral;


#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class FrogBehavior(int frogId, int reqClicks, TextMeshPro textLabel) : ExtremeConsole.IBehavior
{
	public float CoolTime => 0.0f;
	public bool IsCheckWall => false;

	public int FrogId { get; } = frogId;
	private readonly TextMeshPro textLabel = textLabel;

	private int reqClicks = reqClicks;

	public void UpdateReqClicks(int newReqClicks)
	{
		this.reqClicks = newReqClicks;
		this.updateText();
	}

	public bool CanUse(NetworkedPlayerInfo pc)
		=> pc.IsAlive();

	public void Use()
	{
		this.reqClicks--;
		this.updateText();

		if (this.reqClicks <= 0)
		{
			ExtremeSystemTypeManager.RpcUpdateSystem(
				ExtremeSystemType.DeepOneFrogsControlSystem,
				writer =>
				{
					writer.Write((byte)DeepOneFrogsControlSystem.Ops.RemoveFrog);
					writer.WritePacked(this.FrogId);
				});
		}
	}

	private void updateText()
	{
		this.textLabel.text = this.reqClicks.ToString();
	}
}

public sealed class Frog(ExtremeConsole console, FrogBehavior frogBehavior)
{
	private readonly GameObject obj = console.gameObject;
	private readonly FrogBehavior frogBehavior = frogBehavior;

	public void UpdateReqClicks(int newReqClicks)
	{
		this.frogBehavior.UpdateReqClicks(newReqClicks);
	}

	public void Clear()
	{
		UnityEngine.Object.Destroy(this.obj);
	}
}

public sealed class DeepOneFrogsControlSystem(
	int baseRequiredClicks,
	bool enableClickMultiplier) : IExtremeSystemType
{
	public enum Ops : byte
	{
		PlaceFrog = 0,
		RemoveFrog = 1,
	}

	private readonly int baseRequiredClicks = baseRequiredClicks;
	private readonly bool enableClickMultiplier = enableClickMultiplier;

	private readonly Dictionary<byte, HashSet<int>> playerFrogs = new Dictionary<byte, HashSet<int>>();
	private readonly Dictionary<int, Frog> frogs = new Dictionary<int, Frog>();
	private int nextFrogId = 0;

	public int GetRequiredClicks(byte frogPlayerKey)
	{
		int activeCount = this.playerFrogs.TryGetValue(frogPlayerKey, out var frogSet) ? frogSet.Count : 0;
		if (this.enableClickMultiplier && activeCount >= 5)
		{
			int multiplier = 1 + (activeCount - 4);
			return this.baseRequiredClicks * multiplier;
		}
		return this.baseRequiredClicks;
	}

	public void PlaceFrogLocally(Vector2 pos)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.DeepOneFrogsControlSystem,
			writer =>
			{
				writer.Write((byte)Ops.PlaceFrog);
				writer.Write(pos.x);
				writer.Write(pos.y);
			});
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{

	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.PlaceFrog:
				float x = msgReader.ReadSingle();
				float y = msgReader.ReadSingle();

				int newId = this.nextFrogId;
				this.nextFrogId++;
				if (player == null)
				{
					return;
				}
				this.createFrog(player, newId, new Vector2(x, y));
				this.onFrogCountChanged(player);
				break;
			case Ops.RemoveFrog:
				int id = msgReader.ReadPackedInt32();
				if (player == null)
				{
					return;
				}
				this.destroyFrog(id);
				this.onFrogCountChanged(player);
				break;
			default:
				break;
		}
	}

	private void createFrog(PlayerControl player, int id, Vector2 pos)
	{
		var hud = HudManager.Instance;
		if (this.frogs.ContainsKey(id) ||
			hud == null || 
			hud.KillButton == null || 
			hud.KillButton.cooldownTimerText == null)
		{
			return;
		}

		var consoleSystem = ExtremeConsoleSystem.Create();
		var console = consoleSystem.CreateConsoleObj(pos, $"DeepOneFrog_{id}");

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

		var textObj = Object.Instantiate(
			HudManager.Instance.KillButton.cooldownTimerText,
			console.transform,
			false);
		textObj.transform.localPosition = new Vector3(0.0f, 0.4f, -1.0f);
		textObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		textObj.alignment = TextAlignmentOptions.Center;
		textObj.color = Color.white;

		byte frogPlayerKey = ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin ? byte.MaxValue : player.PlayerId;

		int reqClicks = this.GetRequiredClicks(frogPlayerKey);
		var frogBehavior = new FrogBehavior(id, reqClicks, textObj);

		this.frogs.Add(id, new Frog(console, frogBehavior));
		if (!this.playerFrogs.TryGetValue(frogPlayerKey, out var frogSet))
		{
			frogSet = new HashSet<int>();
		}
		frogSet.Add(id);
		this.playerFrogs[frogPlayerKey] = frogSet;
	}

	private void destroyFrog(int id)
	{
		if (this.frogs.TryGetValue(id, out var frog))
		{
			frog.Clear();
			this.frogs.Remove(id);
		}

		foreach (var frogSet in this.playerFrogs.Values)
		{
			frogSet.Remove(id);
		}
	}

	private void onFrogCountChanged(PlayerControl triggerPlayer)
	{
		byte frogPlayerKey = ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin ? byte.MaxValue : triggerPlayer.PlayerId;
		
		int reqClicks = this.GetRequiredClicks(frogPlayerKey);
		foreach (var frog in this.frogs.Values)
		{
			frog.UpdateReqClicks(reqClicks);
		}

		if (ExtremeRoleManager.TryGetSafeCastedRole<DeepOne>(triggerPlayer.PlayerId, out var placerRole) &&
			this.playerFrogs.TryGetValue(frogPlayerKey, out var frogSet))
		{
			// 専用のパブリックメソッドを生やして勝利チェック、このクラスはあくまで🐸の管理だけを重点的にする
			placerRole.UpdateFrogs(frogSet.Count);
		}
	}
}
