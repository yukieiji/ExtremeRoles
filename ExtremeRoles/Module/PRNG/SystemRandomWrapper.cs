using System;

namespace ExtremeRoles.Module.PRNG;

public sealed class SystemRandomWrapper : IRng
{

	private Random rand;

	public SystemRandomWrapper(int seed)
	{
		this.rand = new Random(seed);
	}

	public int Next() => this.rand.Next();

	public int Next(int maxExclusive) => this.rand.Next(maxExclusive);

	public int Next(int minInclusive, int maxExclusive) => this.rand.Next(minInclusive, maxExclusive);
}
