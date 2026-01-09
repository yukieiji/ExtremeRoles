using System.Numerics;

#nullable enable

namespace ExtremeRoles.Module.PRNG;

public sealed class JFT32 : RNG32Base
{
	/*
        以下のURLの実装を元に実装
         https://github.com/Shiroechi/Litdex.Security.RNG/blob/main/Source/Security/RNG/PRNG/JSF32.cs
        
    */

	private uint state0, state1, state2, state3;

	public JFT32(SeedInfo seed)
	{
		do
		{
			state0 = seed.CreateUint();
			state1 = seed.CreateUint();
			state2 = seed.CreateUint();
			state3 = seed.CreateUint();
		}
		while ((state0 | state1 | state2 | state3) == 0); // at least one value must be non-zero

		InitState = $"s0:{state0}, s1:{state1}, s2: {state2}, s3: {state3}";
	}

	public override string InitState { get; }

	public override uint NextUInt()
	{
		uint s0 = state0, s1 = state1, s2 = state2, s3 = state3;

		uint t = s0 - BitOperations.RotateLeft(s1, 27);

		state0 = s1 ^ BitOperations.RotateLeft(s2, 17);
		state1 = s2 + s3;
		state2 = s3 + t;
		state3 = t + s0;

		return state3;
	}
}
