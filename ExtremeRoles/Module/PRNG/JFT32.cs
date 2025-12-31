using System.Numerics;

#nullable enable

namespace ExtremeRoles.Module.PRNG;

public sealed class JFT32 : RNG32Base
{
	/*
        以下のURLの実装を元に実装
         https://github.com/Shiroechi/Litdex.Security.RNG/blob/main/Source/Security/RNG/PRNG/JSF32.cs
        
    */

	private uint _s0, _s1, _s2, _s3;

	public JFT32(SeedInfo seed)
	{
		do
		{
			_s0 = seed.CreateUint();
			_s1 = seed.CreateUint();
			_s2 = seed.CreateUint();
			_s3 = seed.CreateUint();
		}
		while ((_s0 | _s1 | _s2 | _s3) == 0); // at least one value must be non-zero
	}

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
}
