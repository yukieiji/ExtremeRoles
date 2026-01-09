using System.Numerics;

#nullable enable

namespace ExtremeRoles.Module.PRNG;

public sealed class Xorshiro256StarStar : RNG64Base
{
	/*
        以下のURLの実装を元に実装
         https://source.dot.net/#System.Private.CoreLib/Random.Xoshiro256StarStarImpl.cs,bb77e610694e64ca
        
    */

	private ulong state0, state1, state2, state3;

	public Xorshiro256StarStar(SeedInfo seed)
	{
		do
		{
			state0 = seed.CreateULong();
			state1 = seed.CreateULong();
			state2 = seed.CreateULong();
			state3 = seed.CreateULong();
		}
		while ((state0 | state1 | state2 | state3) == 0); // at least one value must be non-zero

		InitState = $"s0:{state0}, s1:{state1}, s2:{state2}, w:{state3}";
	}

	public override string InitState { get; }

	public override ulong NextUInt64()
	{
		ulong s0 = state0, s1 = state1, s2 = state2, s3 = state3;

		ulong result = BitOperations.RotateLeft(s1 * 5, 7) * 9;
		ulong t = s1 << 17;

		s2 ^= s0;
		s3 ^= s1;
		s1 ^= s2;
		s0 ^= s3;

		s2 ^= t;
		s3 = BitOperations.RotateLeft(s3, 45);

		state0 = s0;
		state1 = s1;
		state2 = s2;
		state3 = s3;

		return result;
	}
}
