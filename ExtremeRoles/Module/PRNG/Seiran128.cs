#nullable enable

namespace ExtremeRoles.Module.PRNG;

public sealed class Seiran128 : RNG64Base
{
	/*
        以下のURLの実装を元に実装
        https://github.com/andanteyk/prng-seiran
         ライセンス：https://creativecommons.org/publicdomain/zero/1.0/
        
    */
	private ulong state0, state1;

	public override string InitState { get; }

	public Seiran128(SeedInfo seed)
	{
		do
		{
			state0 = seed.CreateULong();
			state1 = seed.CreateULong();
		}
		while ((state0 | state1) == 0); // at least one value must be non-zero

		InitState = $"s0:{state0}, s1:{state1}";
	}

	public override ulong NextUInt64()
	{
		ulong s0 = state0, s1 = state1;
		ulong result = rotl((s0 + s1) * 9, 29) + s0;

		state0 = s0 ^ rotl(s1, 29);
		state1 = s0 ^ s1 << 9;

		return result;
	}
	private static ulong rotl(ulong x, int k) => (x << k) | (x >> (-k & 63));
}
