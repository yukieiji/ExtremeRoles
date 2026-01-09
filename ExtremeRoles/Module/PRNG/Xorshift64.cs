namespace ExtremeRoles.Module.PRNG;

#nullable enable

public sealed class Xorshift64 : RNG64Base
{
	/*
        以下のURLの実装を元に実装
         https://ja.wikipedia.org/wiki/Xorshift
        
    */

	private ulong x;

	public Xorshift64(SeedInfo seed)
	{
		do
		{
			this.x = seed.CreateULong();
		} while (this.x == 0);


		InitState = $"x: {this.x}";
	}

	public override string InitState { get; }

	public override ulong NextUInt64()
	{
		ulong x0 = x;

		x0 ^= x0 << 7;
		x0 ^= x0 >> 9;

		x = x0;

		return x;
	}
}
