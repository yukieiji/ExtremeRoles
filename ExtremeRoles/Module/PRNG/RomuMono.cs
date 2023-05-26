using System.Numerics;

namespace ExtremeRoles.Module.PRNG;

public sealed class RomuMono : RNG32Base
{
    /*
        以下のURLの実装を元に実装
         https://arxiv.org/pdf/2002.11331.pdf
        
    */
    private uint state;

    public RomuMono(ulong seed, ulong state) : base(seed, state)
    { }

    public override uint NextUInt()
    {
        uint result = state >> 16;
        state *= 3611795771u;
        state = BitOperations.RotateLeft(state, 12);
        return result;
    }

    protected override void Initialize(ulong seed, ulong initStete)
    {
        state = ((uint)(seed >> 32) & 0x1fffffffu) + 1156979152u;
        while (state == 0)
        {
            state = RandomGenerator.CreateStrongSeed();
        }
    }
}
