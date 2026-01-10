using System;
using System.Security.Cryptography;
using ExtremeRoles.Helper;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Core.PRNG;

public sealed class SeedInfo : IDisposable
{
	private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    public int CreateInt()
    {
        byte[] bs = new byte[4];
		//Int32と同じサイズのバイト配列にランダムな値を設定する
		this.rng.GetBytes(bs);

        Logging.Debug($"Int32 SeedValue:{string.Join("", bs)}");

        //RNGCryptoServiceProviderで得たbit列をInt32型に変換してシード値とする。
        return BitConverter.ToInt32(bs, 0);
    }

    public uint CreateUint()
    {
        byte[] bs = new byte[4];
		//Int32と同じサイズのバイト配列にランダムな値を設定する
		this.rng.GetBytes(bs);
        Logging.Debug($"Int32 SeedValue:{string.Join("", bs)}");

        //RNGCryptoServiceProviderで得たbit列をUInt32型に変換してシード値とする。
        return BitConverter.ToUInt32(bs, 0);
    }


    public ulong CreateULong()
    {
        byte[] bs = new byte[8];
		//Int64と同じサイズのバイト配列にランダムな値を設定する
		this.rng.GetBytes(bs);

        Logging.Debug($"UInt64 Seed:{string.Join("", bs)}");

        //RNGCryptoServiceProviderで得たbit列をUInt64型に変換してシード値とする。
        return BitConverter.ToUInt64(bs, 0);
    }

    public int CreateNormal()
    {
        return ((int)DateTime.Now.Ticks & 0x0000FFFF) + SystemInfo.processorFrequency;
    }

	public void Dispose()
	{
		this.rng.Dispose();
	}
}
