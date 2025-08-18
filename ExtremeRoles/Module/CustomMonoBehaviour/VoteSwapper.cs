using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class VoteSwapper : MonoBehaviour
{
	private readonly List<SpriteRenderer> vote = [];
	
	private FloatRange? range;
	private Transform? target;
	
	private int maxVotesBeforeSmoosh = 7;
	private float counterY = -0.16f;
	private float adjustRate = 4f;

	public VoteSwapper(IntPtr ptr) : base(ptr)
	{
	}

	public void Awake()
	{
		if (!this.gameObject.TryGetComponent<VoteSpreader>(out var voteSpreader))
		{
			return;
		}
		this.range = voteSpreader.VoteRange;
		this.maxVotesBeforeSmoosh = voteSpreader.maxVotesBeforeSmoosh;
		this.counterY = voteSpreader.CounterY;
		this.adjustRate = voteSpreader.adjustRate;
	}

	public void Update()
	{
		if (this.range == null || this.target == null)
		{
			return;
		}

		int num = this.vote.Count((SpriteRenderer v) => v.transform.localScale.magnitude > 0.0001f);

		for (int i = 0; i < num; i++)
		{
			var spriteRenderer = this.vote[i];

			// 追加: 親オブジェクトの変更と座標の維持
			if (spriteRenderer.transform.parent != this.target)
			{
				// 親オブジェクトから切り離した際のワールド座標を保持
				var currentWorldPosition = spriteRenderer.transform.position;
				var currentWorldRotation = spriteRenderer.transform.rotation;

				// 新しい親を設定
				spriteRenderer.transform.parent = this.target;

				// ワールド座標を再適用して、位置がジャンプしないようにする
				spriteRenderer.transform.position = currentWorldPosition;
				spriteRenderer.transform.rotation = currentWorldRotation;
			}

			Vector2 vector = new Vector3(0f, this.counterY);
			vector.x = this.range.SpreadToEdges(i, Mathf.Max(num, this.maxVotesBeforeSmoosh));

			// ワールド座標で目標位置を計算
			var localPos = new Vector3(vector.x, vector.y, (float)(-(float)i) / 50f);
			var targetWorldPosition = this.target.TransformPoint(localPos);

			// ワールド座標でLerpを使ってスムーズに移動
			spriteRenderer.transform.position = Vector3.Lerp(spriteRenderer.transform.position, targetWorldPosition, Time.deltaTime * this.adjustRate);
		}
	}

	public void Add(SpriteRenderer vote, Transform parent, Transform target)
	{
		if (this.range == null)
		{
			return;
		}

		this.target = target;
		vote.transform.SetParent(parent);

		vote.transform.localPosition = new Vector3(this.range.max, this.counterY, 0f);
		this.vote.Add(vote);
	}
}
