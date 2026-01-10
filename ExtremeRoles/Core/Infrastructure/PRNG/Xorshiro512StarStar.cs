using ExtremeRoles.Core.Abstract;
using System.Numerics;

#nullable enable

namespace ExtremeRoles.Core.Infrastructure.PRNG;

public sealed class Xorshiro512StarStar : RNG64Base
{
	/*
        以下のURLの実装を元に実装
         https://github.com/colgreen/Redzen/blob/main/Redzen/Random/Xoshiro512StarStarRandom.cs
        
    */

	private ulong state0, state1, state2, state3, state4, state5, state6, state7;

	public override string InitState { get; }

	public Xorshiro512StarStar(SeedInfo seed)
	{

		do
		{
			state0 = seed.CreateULong();
			state1 = seed.CreateULong();
			state2 = seed.CreateULong();
			state3 = seed.CreateULong();
			state4 = seed.CreateULong();
			state5 = seed.CreateULong();
			state6 = seed.CreateULong();
			state7 = seed.CreateULong();
		}
		while ((state0 | state1 | state2 | state3 | state4 | state5 | state6 | state7) == 0); // at least one value must be non-zero

		InitState = $"s0:{state0}, s1:{state1}, s2:{state2}, s3:{state3}, s4:{state4}, s5:{state5}, s6:{state6}, s7:{state7}";
	}

	public override ulong NextUInt64()
	{
		ulong s0 = state0, s1 = state1, s2 = state2, s3 = state3, s4 = state4, s5 = state5, s6 = state6, s7 = state7;

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

		state0 = s0;
		state1 = s1;
		state2 = s2;
		state3 = s3;
		state4 = s4;
		state5 = s5;
		state6 = s6;
		state7 = s7;

		return result;
	}
}
