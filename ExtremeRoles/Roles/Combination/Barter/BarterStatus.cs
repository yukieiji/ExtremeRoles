using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API.Interface.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Roles.Combination.Barter;

public class CastlingNumInfo(int all, int maxNumWithOne)
{
	public int MaxNum { get; } = maxNumWithOne;
	public int All { get; private set; } = all;

	private int curNum = 0;

	public bool CanUse()
		=> All > 0 && curNum < MaxNum;

	public void Use()
	{
		++curNum;
		--All;
	}
	public void Reset()
	{
		curNum = 0;
	}
}

public readonly record struct RandomCastling(bool On, int Num);

public sealed class BarterStatus(IOptionLoader loader) : IStatusModel
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

	public bool IsRandomCastling => this.random.On;
	public int OneCastlingNum => this.random.Num;

	public bool IsAwake { get; set; }

	public void UpdateAwakeStatus(PlayerControl player)
	{

	}

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
