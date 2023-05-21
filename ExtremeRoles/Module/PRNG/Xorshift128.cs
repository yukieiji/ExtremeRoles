namespace ExtremeRoles.Module.PRNG;

public sealed class Xorshift128 : RNG32Base
{
    /*
        以下のURLの実装を元に実装
         https://ja.wikipedia.org/wiki/Xorshift
        
    */

    private uint _s0, _s1, _s2, _s3;

    public Xorshift128(
        ulong seed, ulong state) : base(seed, state)
    { }

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

        uint s0 = _s0, s1 = _s1, s2 = _s2, s3 = _s3;

        uint t = s3;

        _s3 = s2;
        _s2 = s1;
        _s1 = s0;

        t ^= t << 11;
        t ^= t >> 8;

        _s0 = t ^ s0 ^ (s0 >> 19);

        return _s0;
    }

    protected override void Initialize(ulong seed, ulong initStete)
    {
        _s0 = (uint)(seed >> 32);
        _s1 = (uint)(initStete >> 32);
        do
        {
            _s2 = RandomGenerator.CreateStrongSeed();
            _s3 = RandomGenerator.CreateStrongSeed();
        } 
        while ((_s2 | _s3) == 0); // at least one value must be non-zero

        while ((_s0 | _s1) == 0)
        {
            _s0 = RandomGenerator.CreateStrongSeed();
            _s1 = RandomGenerator.CreateStrongSeed();
        }
    }
}
