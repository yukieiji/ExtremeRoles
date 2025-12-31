#nullable enable

namespace ExtremeRoles.Module.PRNG;

public sealed class Xorshift128 : RNG32Base
{
	/*
        以下のURLの実装を元に実装
         https://ja.wikipedia.org/wiki/Xorshift
        
    */

	private uint state0, state1, state2, state3;

	public Xorshift128(SeedInfo seed)
	{
		// at least one value must be non-zero
		do
		{
			state0 = seed.CreateUint();
			state1 = seed.CreateUint();
			state2 = seed.CreateUint();
			state3 = seed.CreateUint();
		}
		while ((state0 | state1 | state2 | state3) == 0);
	}

	public override uint NextUInt()
	{
		/*
        
        uint32_t xorshift128(struct xorshift128_state *state)
        {
	        uint32_t t = state->x[3];

            uint32_t s = state->x[0];  
            state->x[3] = state->x[2];
	        state->x[2] = state->x[1];
	        state->x[1] = s;

	        t ^= t << 11;
	        t ^= t >> 8;
	        return state->x[0] = t ^ s ^ (s >> 19);
        }
         */

		uint s0 = state0, s1 = state1, s2 = state2, s3 = state3;

		uint t = s3;

		state3 = s2;
		state2 = s1;
		state1 = s0;

		t ^= t << 11;
		t ^= t >> 8;

		state0 = t ^ s0 ^ (s0 >> 19);

		return state0;
	}
}
