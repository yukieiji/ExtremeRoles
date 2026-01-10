using ExtremeRoles.Core.Abstract;
using System.Numerics;

#nullable enable

namespace ExtremeRoles.Core.Infrastructure.PRNG;

public sealed class RomuQuad : RNG64Base
{
	/*
        以下のURLの実装を元に実装
         https://arxiv.org/pdf/2002.11331.pdf
        
    */
	private const ulong a = 15241094284759029579ul;
	private ulong wState, xState, yState, zState;

	public override string InitState { get; }

	public RomuQuad(SeedInfo seed)
	{
		do
		{

			wState = seed.CreateULong();
			xState = seed.CreateULong();

			yState = seed.CreateULong();
			zState = seed.CreateULong();
		}
		while ((xState | yState | zState | wState) == 0); // at least one value must be non-zero

		InitState = $"x:{xState}, y:{yState}, z:{zState}, w:{wState}";
	}

	public override ulong NextUInt64()
	{

		ulong wp = wState, xp = xState, yp = yState, zp = zState;

		wState = a * zp; // a-mult

		xState = zp + BitOperations.RotateLeft(wp, 52);  // b-rotl, c-add

		yState = yp - xp; // d-sub

		zState = yp + wp; // e-add
		zState = BitOperations.RotateLeft(zState, 19); // f-rotl

		return xp;
	}
}
