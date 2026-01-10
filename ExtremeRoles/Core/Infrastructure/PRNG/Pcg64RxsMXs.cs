#nullable enable

using ExtremeRoles;
using ExtremeRoles.Core.Abstract;

namespace ExtremeRoles.Core.Infrastructure.PRNG;

public sealed class Pcg64RxsMXs : RNG64Base
{
	// 以下のURLを元に実装
	// https://github.com/Shiroechi/Litdex.Security.RNG/blob/main/Source/Security/RNG/PRNG/PcgRxsMXs64.cs

	private ulong state;
	private ulong increment = 1442695040888963407ul;

	// This shifted to the left and or'ed with 1ul results in the default increment.
	private const ulong ShiftedIncrement = 721347520444481703ul;
	private const ulong Multiplier = 6364136223846793005ul;

	public override string InitState { get; }

	public Pcg64RxsMXs(SeedInfo seed)
	{
		ulong initState = seed.CreateULong();
		this.state = (seed.CreateULong() + initState) * Multiplier + initState;
		this.increment = initState | 1;

		InitState = $"state:{this.state}, increment: {this.increment}";
	}

	public override ulong NextUInt64()
	{
		ulong oldSeed = this.state;
		this.state = unchecked((oldSeed * Multiplier) + this.increment);
		ulong word = ((oldSeed >> ((int)(oldSeed >> 59) + 5)) ^ oldSeed) * 12605985483714917081;
		return (word >> 43) ^ word;
	}
}
