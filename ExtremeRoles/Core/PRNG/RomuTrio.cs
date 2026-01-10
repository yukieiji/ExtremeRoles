using System.Numerics;

#nullable enable

namespace ExtremeRoles.Core.PRNG;

public sealed class RomuTrio : RNG64Base
{
	/*
        以下のURLの実装を元に実装
         https://arxiv.org/pdf/2002.11331.pdf
        
    */
	private const ulong a = 15241094284759029579ul;
	private ulong xState, yState, zState;

	public override string InitState { get; }

	public RomuTrio(SeedInfo seed)
	{
		do
		{
			xState = seed.CreateULong();
			yState = seed.CreateULong();
			zState = seed.CreateULong();
		}
		while ((xState | yState | zState) == 0);  // at least one value must be non-zero

		InitState = $"x:{xState}, y:{yState}, z:{zState}";
	}

	public override ulong NextUInt64()
	{
		ulong xp = xState, yp = yState, zp = zState;

		xState = a * zp;

		yState = yp - xp;
		yState = BitOperations.RotateLeft(yState, 12);

		zState = zp - yp;
		zState = BitOperations.RotateLeft(zState, 44);

		return xp;
	}
}
