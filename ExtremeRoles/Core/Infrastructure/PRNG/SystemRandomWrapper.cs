using ExtremeRoles.Core.Abstract;
using System;

namespace ExtremeRoles.Core.Infrastructure.PRNG;

public sealed class SystemRandomWrapper : IRng
{

	private Random rand;

	public string InitState { get; }

	public SystemRandomWrapper(int seed)
	{
		this.rand = new Random(seed);

		InitState = $"seed: {seed}";
	}

	public int Next() => this.rand.Next();

	public int Next(int maxExclusive) => this.rand.Next(maxExclusive);

	public int Next(int minInclusive, int maxExclusive) => this.rand.Next(minInclusive, maxExclusive);
}
