using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API.Interface.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

public sealed class CastlingNumInfo(int all, int maxNumWithOne)
{
	public int MaxNum { get; } = maxNumWithOne;
	public int All { get; private set; } = all;

	private int curNum = 0;

	public bool CanUse()
		=> this.All > 0 && this.curNum < this.MaxNum;

	public void Use()
	{
		++this.curNum;
		--this.All;
	}
	public void Reset()
	{
		this.curNum = 0;
	}
}

public readonly record struct RandomCastling(bool On, int Num);

public sealed class BarterStatus(IOptionLoader loader, bool isImpostor) : IStatusModel
{
	private readonly CastlingNumInfo castlingNum = new CastlingNumInfo(
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
		=> this.awake.Update(player);

	public string CastlingStatus()
		=> Tr.GetString(
			"castlingInfo",
			castlingNum.MaxNum, castlingNum.All);

	public void UseCastling()
	{
		this.castlingNum.Use();
	}

	public bool CanUseCastling()
		=> this.castlingNum.CanUse() && !this.random.On;

	public void Reset()
	{
		this.castlingNum.Reset();
	}
}
