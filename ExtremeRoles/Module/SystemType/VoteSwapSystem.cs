using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;

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

	public static VoteSwapSystem CreateOrGet()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<VoteSwapSystem>(ExtremeSystemType.VoteSwapSystem);

	public static bool TryGet([NotNullWhen(true)] out VoteSwapSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.VoteSwapSystem, out system);

	public enum ShowOps : byte
	{
		Hide,
		ShowOnlyCaller,
		ShowAll,
	}

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
		Dictionary<byte, PlayerVoteArea> pvaCache)
	{
		if (!TryGet(out var system))
		{
			return;
		}

		var swapInfo = system.getSwapInfo();
		var allAnime = new List<Il2CppIEnumerator>(swapInfo.Count * 2);
		var voteSwapedPos = new Dictionary<byte, Vector3>(pvaCache.Count);

		// 交換をシミュレートし、最終的なtとsの位置を計算
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

			var sTruePos = voteSwapedPos.TryGetValue(s, out var sSwaped) ? sSwaped : sPva.transform.localPosition;
			var tTruePos = voteSwapedPos.TryGetValue(t, out var tSwaped) ? tSwaped : tPva.transform.localPosition; 
			
			voteSwapedPos[s] = tTruePos;
			voteSwapedPos[t] = sTruePos;
		}

		//最終的な位置に向かってスワップ
		foreach (var (id, pos) in voteSwapedPos)
		{
			if (!pvaCache.TryGetValue(id, out var pva) ||
				pva.transform.localPosition == pos)
			{
				continue;
			}
			allAnime.Add(
				Effects.Slide3D(pva.transform, pva.transform.localPosition, pos, animeDuration));
		}
		instance.StartCoroutine(Effects.All(allAnime.ToArray()));
	}

	public static bool TryGetSwapTarget(byte source, out byte target)
	{
		if (!TryGet(out var system))
		{
			target = byte.MaxValue;
			return false;
		}
		var swapInfo = system.getSwapInfo();
		return swapInfo.TryGetValue(source, out target);
	}

	public void RpcSwapVote(byte source, byte target, ShowOps show)
	{
		uint color = show is ShowOps.Hide ? 0 : Design.FromRGBA(
			UnityEngine.Random.ColorHSV());

		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.VoteSwapSystem, x =>
			{
				x.Write(source);
				x.Write(target);
				x.Write((byte)show);
				x.WritePacked(color);
			});
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.MeetingStart)
		{
			this.swapList.Clear();
			this.img.Clear();

			this.cache = null;
			this.pva = MeetingHud.Instance.playerStates.ToDictionary(x => x.TargetPlayerId);
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte source = msgReader.ReadByte();
		byte target = msgReader.ReadByte();
		bool showImg = ((ShowOps)msgReader.ReadByte()) switch
		{
			ShowOps.ShowAll => true,
			ShowOps.ShowOnlyCaller => player.PlayerId == PlayerControl.LocalPlayer.PlayerId,
			_ => false,
		};
		uint color = msgReader.ReadPackedUInt32();
		swapVote(source, target, showImg, color);
	}

	private void swapVote(byte source, byte target, bool showImg, uint colorUint)
	{
		Logging.Debug($"Swap {source} to {target}");
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
		addImg(target, ImgType.Target, color);
	}

	private IReadOnlyDictionary<byte, int> swap(Dictionary<byte, int> voteInfo)
	{
		var swapInfo = getSwapInfo();

		var tempData = new Dictionary<byte, int>(voteInfo.Count);
		foreach (var (s, t) in swapInfo)
		{
			if (!voteInfo.TryGetValue(t, out int val))
			{
				val = 0;
			}
			tempData[s] = val;
		}

		Logging.Debug($"--- swaped vote info ---");
		var finalData = new Dictionary<byte, int>(tempData.Count);
		foreach (var (t, v) in tempData)
		{
			if (v > 0)
			{
				Logging.Debug($"Vote to {t}, Num:{v}");
				finalData[t] = v;
			}
		}

		return finalData;
	}

	private IReadOnlyDictionary<byte, byte> getSwapInfo()
	{
		if (this.pva is null)
		{
			return new Dictionary<byte, byte>();
		}

		if (this.cache is null)
		{
			this.cache = this.pva.Keys.ToDictionary(key => key, key => key);

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
		var img = createImg(pva, list.Count, type);
		img.color = color;
		list.Add(img);
	}

	private static SpriteRenderer createImg(PlayerVoteArea pva, int index, ImgType type)
	{
		var img = UnityEngine.Object.Instantiate(
			pva.Background, pva.LevelNumberText.transform);
		img.name = $"swap_img_{pva.TargetPlayerId}_{index}";
		img.sprite = UnityObjectLoader.LoadFromResources<Sprite>(
			ObjectPath.CommonTextureAsset,
			string.Format(
				ObjectPath.CommonImagePathFormat,
				type is ImgType.Source ? 
					ObjectPath.VoteSwapSource :
					ObjectPath.VoteSwapTarget));
		img.transform.localPosition = new Vector3(6.5f + 0.05f * index, -0.5f, -2.75f);
		img.transform.localScale = new Vector3(0.5f, 3.0f, 1.0f);
		img.gameObject.layer = 5;
		return img;
	}
}
