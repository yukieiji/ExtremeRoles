using System.Numerics;

#nullable enable

namespace ExtremeRoles.Module.PRNG;

public sealed class Xorshiro256StarStar : RNG64Base
{
	/*
        以下のURLの実装を元に実装
         https://source.dot.net/#System.Private.CoreLib/Random.Xoshiro256StarStarImpl.cs,bb77e610694e64ca
        
    */

	private ulong _s0, _s1, _s2, _s3;

	public Xorshiro256StarStar(SeedInfo seed)
	{
		do
		{
			_s0 = seed.CreateULong();
			_s1 = seed.CreateULong();
			_s2 = seed.CreateULong();
			_s3 = seed.CreateULong();
		}
		while ((_s0 | _s1 | _s2 | _s3) == 0); // at least one value must be non-zero
	}

	public override ulong NextUInt64()
	{
		ulong s0 = _s0, s1 = _s1, s2 = _s2, s3 = _s3;

		ulong result = BitOperations.RotateLeft(s1 * 5, 7) * 9;
		ulong t = s1 << 17;

		s2 ^= s0;
		s3 ^= s1;
		s1 ^= s2;
		s0 ^= s3;

		s2 ^= t;
		s3 = BitOperations.RotateLeft(s3, 45);

		_s0 = s0;
		_s1 = s1;
		_s2 = s2;
		_s3 = s3;

		return result;
	}
}
