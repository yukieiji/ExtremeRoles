using ExtremeRoles.Module.Interface;
using Hazel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class VoteSwapSystem : IExtremeSystemType
{

	private readonly record struct Img(SpriteRenderer Start, SpriteRenderer Target);
	private readonly List<(byte, byte)> swapList = [];
	private readonly List<Img> img = [];
	private Dictionary<byte, PlayerVoteArea>? pva;

	private Dictionary<byte, byte>? cache;

	public static bool TryGet([NotNullWhen(true)] out VoteSwapSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.VoteSwapSystem, out system);

	public static IReadOnlyDictionary<byte, int> Swap(Dictionary<byte, int> voteInfo)
	{
		if (!TryGet(out var system))
		{
			return voteInfo;
		}

		return system.swap(voteInfo);
	}

	public static bool TryGetSwapSource(byte target, out byte source)
	{
		if (!TryGet(out var system) || system.cache is null)
		{
			source = byte.MaxValue;
			return false;
		}
		return system.cache.TryGetValue(target, out source);
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is not ResetTiming.ExiledEnd)
		{
			return;
		}
		
		this.swapList.Clear();
		this.img.Clear();

		this.cache = null;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte source = msgReader.ReadByte();
		byte target = msgReader.ReadByte();
		bool showImg = msgReader.ReadBoolean();
		swapVote(source, target, showImg);
	}

	private void swapVote(byte source, byte target, bool showImg)
	{
		if (this.pva is null)
		{
			return;
		}

		this.swapList.Add((source, target));

		/* 画像の処理 (後で追加)
		var imgId = id.Value;
		if (!this.img.TryGetValue(imgId, out var img))
		{
			var sourceImg = new SpriteRenderer();
			var targetImg = new SpriteRenderer();

			img = new Img(sourceImg, targetImg);
		}
		
		img.Start.transform.SetParent(sourcePva.transform);
		img.Target.transform.SetParent(targetPva.transform);
		
		this.img[imgId] = img;
		*/
	}

	private Dictionary<byte, int> swap(Dictionary<byte, int> voteInfo)
	{
		if (this.cache is null)
		{
			this.cache = voteInfo.Keys.ToDictionary(key => key, key => key);

			// 2. 各スワップ操作をシミュレートし、マップの値を更新していく
			foreach (var (s, t) in this.swapList)
			{
				if (this.cache.ContainsKey(s) && 
					this.cache.ContainsKey(t))
				{
					// key1の位置とkey2の位置にある「値の出所（元のキー）」を交換する
					(this.cache[s], this.cache[t]) = (this.cache[t], this.cache[s]);
				}
			}
		}

		var finalData = new Dictionary<byte, int>(voteInfo.Count);
		foreach (var (s, t) in this.cache)
		{
			finalData[s] = voteInfo[t];
		}

		return finalData;
	}
}
