using System.Numerics;

#nullable enable

namespace ExtremeRoles.Core.PRNG;

public sealed class RomuMono : RNG32Base
{
	/*
        以下のURLの実装を元に実装
         https://arxiv.org/pdf/2002.11331.pdf
        
    */
	private uint state;

	public override string InitState { get; }

	public RomuMono(SeedInfo seed)
	{
		do
		{
			state = (seed.CreateUint() & 0x1fffffffu) + 1156979152u;
		}
		while (state == 0);

		InitState = $"state:{state}";
	}

	public override uint NextUInt()
	{
		uint result = state >> 16;
		state *= 3611795771u;
		state = BitOperations.RotateLeft(state, 12);
		return result;
	}
}
