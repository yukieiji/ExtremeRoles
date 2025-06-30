using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class RaiseHandSystem : IDirtableSystemType
{
	public sealed class Behavior
	{
		private readonly SpriteRenderer hand;

		public Behavior(PlayerVoteArea pva)
		{
			this.hand = Object.Instantiate(
				pva.Background, pva.LevelNumberText.transform);
			this.hand.name = $"raisehand_{pva.TargetPlayerId}";
			this.hand.sprite = Resources.UnityObjectLoader.LoadSpriteFromResources(
				Resources.ObjectPath.RaiseHandIcon);
			this.hand.transform.localPosition = new Vector3(2.0f, -0.75f, -3f);
			this.hand.transform.localScale = new Vector3(0.75f, 2.5f, 1.0f);
			this.hand.gameObject.layer = 5;

			this.Down();
		}

		public void Raise()
		{
			this.rendSetActive(true);
		}

		public void Down()
		{
			this.rendSetActive(false);
		}

		private void rendSetActive(bool active)
		{
			if (this.hand != null)
			{
				this.hand.enabled = active;
			}
		}
	}

	public bool IsInit => this.raiseHandButton != null;
	public const ExtremeSystemType Type = ExtremeSystemType.RaiseHandSystem;

	private readonly Dictionary<byte, Behavior> allHand = new Dictionary<byte, Behavior>();

	private Dictionary<byte, float> raisedHand = new Dictionary<byte, float>();
	private SimpleButton? raiseHandButton = null;

	private const float time = 5.0f;

	public bool IsDirty { get; private set; }

	public static RaiseHandSystem Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<RaiseHandSystem>(Type);

	public void CreateRaiseHandButton()
	{
		this.raiseHandButton = Resources.UnityObjectLoader.CreateSimpleButton(
			MeetingHud.Instance.transform);

		this.raiseHandButton.Text.text = "挙手する";
		this.raiseHandButton.Text.fontSize =
			this.raiseHandButton.Text.fontSizeMax =
			this.raiseHandButton.Text.fontSizeMin = 2.0f;
		this.raiseHandButton.Scale = new Vector3(0.35f, 0.25f, 1.0f);
		this.raiseHandButton.Layer = 5;
		this.raiseHandButton.transform.localPosition = new Vector3(0.0f, -2.25f, -125f);

		this.raiseHandButton.ClickedEvent.AddListener(() =>
		{
			ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
				Type, (x) =>
				{
					x.Write(PlayerControl.LocalPlayer.PlayerId);
				});
		});
	}

	public void AddHand(PlayerVoteArea player)
	{
		this.allHand[player.TargetPlayerId] = new Behavior(player);
	}

	public void RaiseHandButtonSetActive(bool active)
	{
		if (this.raiseHandButton != null)
		{
			this.raiseHandButton.gameObject.SetActive(active);
		}
	}

	public void MarkClean()
	{
		this.IsDirty = false;
	}

	public void Deteriorate(float deltaTime)
	{
		if (this.IsDirty || !AmongUsClient.Instance.AmHost) { return; }

		var newRaisedHand = new Dictionary<byte, float>(this.raisedHand.Count);
		foreach (var (playerId, time) in this.raisedHand)
		{
			float newTime = time - deltaTime;
			if (newTime > 0)
			{
				newRaisedHand[playerId] = newTime;
			}
			else if (
				this.allHand.TryGetValue(playerId, out var hand) ||
				hand != null)
			{
				hand.Down();
				this.IsDirty = true;
			}
		}
		this.raisedHand = newRaisedHand;
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing == ResetTiming.MeetingEnd)
		{
			this.allHand.Clear();
			this.raisedHand.Clear();
			this.raiseHandButton = null;
		}
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int readNum = reader.ReadPackedInt32();

		var newRaiseHand = new HashSet<byte>();
		for (int i = 0; i < readNum; ++i)
		{
			byte id = reader.ReadByte();
			newRaiseHand.Add(id);

			if (this.raisedHand.TryAdd(id, time) &&
				this.allHand.TryGetValue(id, out var hand) &&
				hand != null)
			{
				hand.Raise();
			}
		}

		var downHand = new List<byte>(this.raisedHand.Count);
		foreach (byte id in this.raisedHand.Keys)
		{
			if (!newRaiseHand.Remove(id) &&
				this.allHand.TryGetValue(id, out var hand) &&
				hand != null)
			{
				downHand.Add(id);
				hand.Down();
			}
		}

		foreach (byte id in downHand)
		{
			this.raisedHand.Remove(id);
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.raisedHand.Count);
		foreach (byte playerId in this.raisedHand.Keys)
		{
			writer.Write(playerId);
		}
		this.IsDirty = initialState;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte playerId = msgReader.ReadByte();
		this.raiseHand(playerId);
	}

	private void raiseHand(byte playerId)
	{
		if (!this.allHand.TryGetValue(playerId, out var hand) ||
			hand == null ||
			this.raisedHand.ContainsKey(playerId))
		{ return; }

		hand.Raise();
		this.raisedHand.Add(playerId, time);
		this.IsDirty = true;
	}
}
