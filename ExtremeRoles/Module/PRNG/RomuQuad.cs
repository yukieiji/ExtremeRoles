using System.Numerics;

namespace ExtremeRoles.Module.PRNG;

public sealed class RomuQuad : RNG64Base
{
    /*
        以下のURLの実装を元に実装
         https://arxiv.org/pdf/2002.11331.pdf
        
    */
    private const ulong a = 15241094284759029579ul;
    private ulong wState, xState, yState, zState;

    public RomuQuad(ulong seed, ulong state) : base(seed, state)
    { }

    public override ulong NextUInt64()
    {

        ulong wp = wState, xp = xState, yp = yState, zp = zState;

        wState = a * zp; // a-mult

        xState = zp + BitOperations.RotateLeft(wp, 52);  // b-rotl, c-add

        yState = yp - xp; // d-sub

        zState = yp + wp; // e-add
        zState = BitOperations.RotateLeft(zState, 19); // f-rotl

        return xp;
    }

    protected override void Initialize(ulong seed, ulong initStete)
    {
        wState = seed;
        xState = initStete;
        do
        {
            yState = RandomGenerator.CreateLongStrongSeed();
            zState = RandomGenerator.CreateLongStrongSeed();
        } 
        while ((yState | zState) == 0); // at least one value must be non-zero

        while ((xState | wState) == 0)
        {
            xState = RandomGenerator.CreateLongStrongSeed();
            wState = RandomGenerator.CreateLongStrongSeed();
        }
    }
}
