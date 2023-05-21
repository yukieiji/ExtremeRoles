using System.Numerics;

namespace ExtremeRoles.Module.PRNG;

public sealed class JFT32 : RNG32Base
{
    /*
        以下のURLの実装を元に実装
         https://github.com/Shiroechi/Litdex.Security.RNG/blob/main/Source/Security/RNG/PRNG/JSF32.cs
        
    */

    private uint _s0, _s1, _s2, _s3;

    public JFT32(
        ulong seed, ulong state) : base(seed, state)
    { }

    public override uint NextUInt()
    {
        uint s0 = _s0, s1 = _s1, s2 = _s2, s3 = _s3;

        uint t = s0 - BitOperations.RotateLeft(s1, 27);

        _s0 = s1 ^ BitOperations.RotateLeft(s2, 17);
        _s1 = s2 + s3;
        _s2 = s3 + t;
        _s3 = t + s0;

        return _s3;
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
