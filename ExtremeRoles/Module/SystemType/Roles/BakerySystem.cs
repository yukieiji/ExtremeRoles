using Hazel;
using System.Collections.Generic;


using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class BakerySystem : IExtremeSystemType
{
	public bool IsDirty { get; private set; }

	private readonly bool isChangeCooking = false;
	private readonly float goodTime = 0.0f;
	private readonly float badTime = 0.0f;

	private float timer = 0.0f;
	private bool isUnion = false;

	private HashSet<byte> aliveBakary = new HashSet<byte>();

	public BakerySystem(
		float goodCookTime,
		float badCookTime,
		bool isChangeCooking)
	{
		this.goodTime = goodCookTime;
		this.badTime = badCookTime;
		this.isChangeCooking = isChangeCooking;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.timer = reader.ReadSingle();
	}

	public void Detoriorate(float deltaTime)
	{
		if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
			!RoleAssignState.Instance.IsRoleSetUpEnd ||
			MeetingHud.Instance != null) { return; }

		if (!this.isUnion)
		{
			this.isUnion = true;
			organize();
		}
		if (this.aliveBakary.Count == 0) { return; }

		this.timer += deltaTime;
	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{
		if (timing == ResetTiming.MeetingEnd)
		{
			this.timer = 0;
			if (this.isEstablish())
			{
				MeetingReporter.Instance.AddMeetingEndReport(getBreadBakingCondition());
			}
		}
		if (timing == ResetTiming.MeetingStart)
		{
			this.IsDirty = true;
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(this.timer);
		this.IsDirty = initialState;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{ }

	private string getBreadBakingCondition()
	{
		if (!this.isChangeCooking)
		{
			return Translation.GetString("goodBread");
		}

		if (this.timer < this.goodTime)
		{
			return Translation.GetString("rawBread");
		}
		else if (this.goodTime <= this.timer && this.timer < this.badTime)
		{
			return Translation.GetString("goodBread");
		}
		else
		{
			return Translation.GetString("badBread");
		}
	}

	private bool isEstablish()
	{
		updateBakaryAlive();
		return this.aliveBakary.Count != 0;
	}

	private void updateBakaryAlive()
	{
		if (this.aliveBakary.Count == 0) { return; }

		HashSet<byte> updatedBakary = new HashSet<byte>();

		foreach (byte playerId in aliveBakary)
		{
			PlayerControl player = Player.GetPlayerControlById(playerId);
			if (!player.Data.IsDead && !player.Data.Disconnected)
			{
				updatedBakary.Add(playerId);
			}
		}

		this.aliveBakary = updatedBakary;
	}

	private void organize()
	{
		this.isUnion = true;
		foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
		{
			if (role.Id == ExtremeRoleId.Bakary)
			{
				this.aliveBakary.Add(playerId);
			}

			if (role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole != null &&
				multiAssignRole.AnotherRole.Id == ExtremeRoleId.Bakary)
			{
				this.aliveBakary.Add(playerId);
			}
		}
		this.IsDirty = true;
	}
}
