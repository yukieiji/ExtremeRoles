using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Module.Interface;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class VoteSwapSystem : IExtremeSystemType
{
	private sealed class VoteSwapper
	{
		public enum VoteSwapResult
		{
			Succes,
			Override,
			Fail
		}

		private readonly Dictionary<byte, byte> swapList = [];
		private readonly Dictionary<byte, Guid?> swapId = [];

		public VoteSwapResult AddSwap(byte start, byte end, out Guid? id)
		{
			// 循環チェック
			if (createsCycle(start, end))
			{
				id = null;
				return VoteSwapResult.Fail;
			}

			// 既存の出発地があれば上書き
			if (!this.swapId.TryGetValue(start, out id))
			{
				id = Guid.NewGuid();
			}

			this.swapId[start] = id;
			var result = this.swapList.ContainsKey(start) ? VoteSwapResult.Override : VoteSwapResult.Succes;
			this.swapList[start] = end;
			return result;
		}

		public void Clear()
			=> this.swapList.Clear();

		public IReadOnlyList<(byte, byte)> FindShortestRoutes()
		{
			var shortestRoutes = new List<(byte, byte)>();
			var visitedStarts = new HashSet<byte>();

			// すべての出発地をループ
			foreach (byte start in swapList.Keys)
			{
				// すでに処理済みのルートの出発地はスキップ
				if (visitedStarts.Contains(start))
				{
					continue;
				}

				// ルートの終端（目的地）を見つける
				byte current = start;
				while (swapList.ContainsKey(current))
				{
					visitedStarts.Add(current);
					current = swapList[current];
				}

				// 最短ルートとして追加
				shortestRoutes.Add((start, current));
			}

			return shortestRoutes;
		}

		private bool createsCycle(byte start, byte end)
		{
			// 目的地がすでに他の場所の出発地になっているか確認
			byte current = end;
			while (swapList.TryGetValue(current, out byte curTarget))
			{
				if (curTarget == start)
				{
					return true; // 循環を発見
				}
				current = swapList[current];
			}
			return false;
		}
	}

	private readonly VoteSwapper swapper = new VoteSwapper();

	private readonly record struct Img(SpriteRenderer Start, SpriteRenderer Target);
	private readonly Dictionary<Guid, Img> img = [];
	private Dictionary<byte, PlayerVoteArea>? pva;

	public IReadOnlyList<(byte, byte)> Result
	{
		get
		{
			if (this.result is null)
			{
				this.result = this.swapper.FindShortestRoutes();
			}
			return this.result;
		}
	}
	private IReadOnlyList<(byte, byte)>? result;

	public static bool TryGet([NotNullWhen(true)] out VoteSwapSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.VoteSwapSystem, out system);

	public static void SwapVote()
	{

	}

	public static bool TryGetSwapSource(byte target, out byte source)
	{
		source = byte.MaxValue;
		if (!TryGet(out var system))
		{
			return false;
		}
		foreach (var (s, t) in system.Result)
		{
			if (t == target)
			{
				source = s;
				return true;
			}
		}
		return false;
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is not ResetTiming.ExiledEnd)
		{
			return;
		}
		this.swapper.Clear();
		this.result = null;
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

		var result = this.swapper.AddSwap(source, target, out var id);
		if (result is VoteSwapper.VoteSwapResult.Fail || 
			!showImg || 
			!id.HasValue || 
			!(
				this.pva.TryGetValue(source, out var sourcePva) &&
				this.pva.TryGetValue(target, out var targetPva)
			))
		{
			return;
		}
		

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
}
