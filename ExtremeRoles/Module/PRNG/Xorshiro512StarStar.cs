using System.Numerics;

namespace ExtremeRoles.Module.PRNG;

public sealed class Xorshiro512StarStar : RNG64Base
{
	/*
        以下のURLの実装を元に実装
         https://github.com/colgreen/Redzen/blob/main/Redzen/Random/Xoshiro512StarStarRandom.cs
        
    */

	private ulong _s0, _s1, _s2, _s3, _s4, _s5, _s6, _s7;

	public Xorshiro512StarStar(SeedInfo seed)
	{

		do
		{
			_s0 = seed.CreateULong();
			_s1 = seed.CreateULong();
			_s2 = seed.CreateULong();
			_s3 = seed.CreateULong();
			_s4 = seed.CreateULong();
			_s5 = seed.CreateULong();
			_s6 = seed.CreateULong();
			_s7 = seed.CreateULong();
		}
		while ((_s0 | _s1 | _s2 | _s3 | _s4 | _s5 | _s6 | _s7) == 0); // at least one value must be non-zero
	}

	public override ulong NextUInt64()
	{
		ulong s0 = _s0, s1 = _s1, s2 = _s2, s3 = _s3, s4 = _s4, s5 = _s5, s6 = _s6, s7 = _s7;

		ulong result = BitOperations.RotateLeft(s1 * 5, 7) * 9;
		ulong t = s1 << 11;

		s2 ^= s0;
		s5 ^= s1;
		s1 ^= s2;
		s7 ^= s3;
		s3 ^= s4;
		s4 ^= s5;
		s0 ^= s6;
		s6 ^= s7;
		s6 ^= t;

		s7 = BitOperations.RotateLeft(s7, 21);

		_s0 = s0;
		_s1 = s1;
		_s2 = s2;
		_s3 = s3;
		_s4 = s4;
		_s5 = s5;
		_s6 = s6;
		_s7 = s7;

		return result;
	}
}
