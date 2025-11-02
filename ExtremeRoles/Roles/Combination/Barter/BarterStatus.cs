using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API.Interface.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtremeRoles.Module.CustomOption.Interfaces.Old;

namespace ExtremeRoles.Roles.Combination.Barter;

public interface IAwakeCheck
{
	public bool IsAwake { get; }

	public void Update(PlayerControl player);
}

public sealed class ImpostorAwakeCheck(int killNum) : IAwakeCheck
{
	public bool IsAwake => this.remainKill <= 0;

	private int remainKill = killNum;

	public void Update(PlayerControl player)
	{
		--remainKill;
	}
}

public sealed class CrewmateAwakeCheck(int taskGage, int deadNum) : IAwakeCheck
{
	public bool IsAwake { get; private set; } = deadNum <= 0 && taskGage <= 0;

	private readonly float targetTaskGage = taskGage / 100.0f;
	private readonly int deadNum = deadNum;

	public void Update(PlayerControl rolePlayer)
	{
		int deadPlayerNum = 0;
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (player == null ||
				player.IsDead ||
				player.Disconnected)
			{
				++deadPlayerNum;
			}
		}

		this.IsAwake =
			deadPlayerNum >= this.deadNum &&
			Player.GetPlayerTaskGage(rolePlayer) >= this.targetTaskGage;
	}
}

public sealed class CastlingNumInfo(int maxNumPerMeeting, int totalNum)
{
	public int PerMeeting { get; private set; } = maxNumPerMeeting;
	public int Total { get; private set; } = totalNum;
	
	private readonly int maxNumPerMeeting = maxNumPerMeeting;

	public bool CanUse()
		=> this.Total > 0 && this.PerMeeting > 0;

	public void Use()
	{
		--this.Total;
		--this.PerMeeting;
	}
	public void Reset()
	{
		this.PerMeeting = this.maxNumPerMeeting;
	}
}

public readonly record struct RandomCastling(bool On, int Num);

public sealed class BarterStatus(IOldOptionLoader loader, bool isImpostor) : IStatusModel
{
	private readonly CastlingNumInfo castlingNum = new CastlingNumInfo(
			// オプション名が間違っているが後方互換性を持たせるためそのままにする
			// CastlingNum => 1会議あたりの「キャスリング」回数
			// BarterMaxCastlingNumWhenMeeting => 全体の「キャスリング」回数
			loader.GetValue<BarterRole.Option, int>(
				BarterRole.Option.CastlingNum),
			loader.GetValue<BarterRole.Option, int>(
				BarterRole.Option.MaxCastlingNumWhenMeeting));
	private readonly RandomCastling random = new RandomCastling(
			loader.GetValue<BarterRole.Option, bool>(
				BarterRole.Option.RandomCastling),
			loader.GetValue<BarterRole.Option, int>(
				BarterRole.Option.OneCastlingNum));
	private readonly IAwakeCheck awake = 
		isImpostor ? 
			new ImpostorAwakeCheck(
				loader.GetValue<BarterRole.Option, int>(BarterRole.Option.AwakeKillNum)) : 
			new CrewmateAwakeCheck(
				loader.GetValue<BarterRole.Option, int>(BarterRole.Option.AwakeTaskRate),
				loader.GetValue<BarterRole.Option, int>(BarterRole.Option.AwakeDeadPlayerNum));

	public bool IsRandomCastling => this.random.On;
	public int OneCastlingNum => this.random.Num;

	public bool IsAwake => this.awake.IsAwake;

	public void UpdateAwakeStatus(PlayerControl player)
	{
		if (!this.awake.IsAwake)
		{
			this.awake.Update(player);
		}
	}

	public string CastlingStatus()
		=> Tr.GetString(
			"BarterCastlingInfo",
			castlingNum.Total, castlingNum.PerMeeting);

	public void UseCastling()
	{
		this.castlingNum.Use();
	}

	public bool CanUseNoneRandomCastling()
		=> this.castlingNum.CanUse() && !this.random.On;
	public bool CanUseRandomCastling()
		=> this.castlingNum.CanUse();

	public void Reset()
	{
		this.castlingNum.Reset();
	}
}
