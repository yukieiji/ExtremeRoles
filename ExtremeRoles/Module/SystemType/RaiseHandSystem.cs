using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class RaiseHandSystem : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.RaiseHandSystem;

	private readonly Dictionary<byte, RaiseHandBehaviour> allHand = new Dictionary<byte, RaiseHandBehaviour>();

	private Dictionary<byte, float> raisedHand = new Dictionary<byte, float>();
	private SimpleButton? raiseHandButton = null;

	private const float time = 5.0f;

	public bool IsDirty { get; private set; }

	public static RaiseHandSystem Get()
	{
		var systemMng = ExtremeSystemTypeManager.Instance;
		if (!systemMng.TryGet<RaiseHandSystem>(Type, out var sytem) ||
			sytem == null)
		{
			sytem = new RaiseHandSystem();
			systemMng.TryAdd(Type, sytem);
		}

		return sytem;
	}

	public void AddHand(PlayerVoteArea player)
	{
		var hand = player.gameObject.AddComponent<RaiseHandBehaviour>();

		byte playerId = player.TargetPlayerId;
		this.allHand[playerId] = hand;

		if (playerId == CachedPlayerControl.LocalPlayer.PlayerId)
		{
			this.raiseHandButton = Resources.Loader.CreateSimpleButton(
				MeetingHud.Instance.transform);
			this.raiseHandButton.ClickedEvent.AddListener(() =>
			{
				this.RaiseHand(player);
			});
		}
	}

	public void RaiseHandButtonSetActive(bool active)
	{
		if (this.raiseHandButton != null)
		{
			this.raiseHandButton.gameObject.SetActive(active);
		}
	}

	public void RaiseHand(PlayerVoteArea player)
	{
		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
			Type, (x) =>
			{
				x.Write(player.TargetPlayerId);
			});
	}

	public void Deteriorate(float deltaTime)
	{
		if (this.IsDirty || AmongUsClient.Instance.AmClient) { return; }

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
		if (timing == ResetTiming.MeetingEnd ||
			timing == ResetTiming.MeetingStart)
		{
			this.allHand.Clear();
			this.raisedHand.Clear();
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

		foreach (byte id in this.raisedHand.Keys)
		{
			if (!newRaiseHand.Remove(id) &&
				this.allHand.TryGetValue(id, out var hand) &&
				hand != null)
			{
				hand.Down();
			}
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.raisedHand.Count);
		foreach (byte playerId in this.raisedHand.Keys)
		{
			writer.Write(playerId);
		}
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
