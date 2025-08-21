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
	private readonly List<(byte, byte)> swapList = [];
	private readonly Dictionary<byte, List<SpriteRenderer>> img = [];
	
	private Dictionary<byte, PlayerVoteArea>? pva;
	private Dictionary<byte, byte>? cache;

	private const float animeDuration = 1.5f;

	public static bool TryGet([NotNullWhen(true)] out VoteSwapSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.VoteSwapSystem, out system);

	private enum ImgType : byte
	{
		Source,
		Target,
	}

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
		this.pva = null;
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

		this.swapList.Add((source, target));

		if (!showImg || 
			(this.pva is null && MeetingHud.Instance == null))
		{
			return;
		}
		if (this.pva is null)
		{
			this.pva = MeetingHud.Instance.playerStates.ToDictionary(
				x => x.TargetPlayerId);
		}

		var color = Design.ToRGBA(colorUint);

		addImg(source, ImgType.Source, color);
		addImg(source, ImgType.Target, color);
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

	private void addImg(byte id, ImgType type, Color32 color)
	{
		if (this.pva == null ||
			!this.pva.TryGetValue(id, out var pva) ||
			pva == null)
		{
			return;
		}
		if (!this.img.TryGetValue(id, out var list) ||
			list is null)
		{
			list = [];
			this.img[id] = list;
		}
		var img = createImg(pva, list.Count);
		img.color = color;
		list.Add(img);
	}

	private static SpriteRenderer createImg(PlayerVoteArea pva, int index)
	{
		var img = UnityEngine.Object.Instantiate(
			pva.Background, pva.LevelNumberText.transform);
		img.name = $"swap_img_{pva.TargetPlayerId}_{index}";
		img.sprite = Resources.UnityObjectLoader.LoadSpriteFromResources(
			Resources.ObjectPath.CaptainSpecialVoteCheck);
		img.transform.localPosition = new Vector3(7.2f + 0.05f * index, -0.5f, -2.75f);
		img.transform.localScale = new Vector3(1.0f, 3.5f, 1.0f);
		img.gameObject.layer = 5;
		return img;
	}
}
