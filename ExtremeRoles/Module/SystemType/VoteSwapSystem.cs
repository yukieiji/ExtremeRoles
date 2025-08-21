using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;

using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;


#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class VoteSwapSystem : IExtremeSystemType
{

	private readonly record struct Img(SpriteRenderer Start, SpriteRenderer Target);
	private readonly List<(byte, byte)> swapList = [];
	private readonly List<Img> img = [];
	private Dictionary<byte, PlayerVoteArea>? pva;

	private Dictionary<byte, byte>? cache;
	private const float animeDuration = 1.5f;

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

	public static void AnimateSwap(
		MeetingHud instance,
		Dictionary<byte, int> voteInfo,
		Dictionary<byte, PlayerVoteArea> pvaCache)
	{
		if (!TryGet(out var system))
		{
			return;
		}

		var swapInfo = system.getSwapInfo(voteInfo);
		var allAnime = new List<Il2CppIEnumerator>(swapInfo.Count * 2);

		foreach (var (s, t) in swapInfo)
		{
			if (s == t ||
				!pvaCache.TryGetValue(s, out var sPva) ||
				!pvaCache.TryGetValue(t, out var tPva) ||
				sPva == null ||
				tPva == null)
			{
				continue;
			}

			var sAnime = Effects.Slide3D(
				sPva.transform,
				sPva.transform.localPosition,
				tPva.transform.localPosition, animeDuration);
			var tAnime = Effects.Slide3D(
				tPva.transform,
				tPva.transform.localPosition,
				sPva.transform.localPosition, animeDuration);
			allAnime.Add(sAnime);
			allAnime.Add(tAnime);
		}
		instance.StartCoroutine(Effects.All(allAnime.ToArray()));
	}

	public void RpcSwapVote(byte source, byte target, bool isShowImg)
	{
		uint color = isShowImg ? Design.FromRGBA(
			UnityEngine.Random.ColorHSV()) : 0;

		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.VoteSwapSystem, x =>
			{
				x.Write(source);
				x.Write(target);
				x.Write(isShowImg);
				x.WritePacked(color);
			});
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
		uint color = msgReader.ReadPackedUInt32();
		swapVote(source, target, showImg, color);
	}

	private void swapVote(byte source, byte target, bool showImg, uint colorUint)
	{
		if (this.pva is null)
		{
			return;
		}

		this.swapList.Add((source, target));

		/* 画像の処理 (後で追加)

		var color = Design.ToRGBA(colorUint);
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

	private IReadOnlyDictionary<byte, int> swap(Dictionary<byte, int> voteInfo)
	{
		var swapInfo = getSwapInfo(voteInfo);

		var finalData = new Dictionary<byte, int>(voteInfo.Count);
		foreach (var (s, t) in swapInfo)
		{
			finalData[s] = voteInfo[t];
		}

		return finalData;
	}

	private IReadOnlyDictionary<byte, byte> getSwapInfo(Dictionary<byte, int> voteInfo)
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
		return this.cache;
	}
}
