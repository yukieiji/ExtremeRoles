using System;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class RaiseHandBehaviour : MonoBehaviour
{
	private SpriteRenderer? raiseHand;

	public RaiseHandBehaviour(IntPtr ptr) : base(ptr) { }

	public void Raise()
	{
		this.rendSetActive(true);
	}

	public void Down()
	{
		this.rendSetActive(false);
	}

	public void Initialize(PlayerVoteArea voteArea)
	{
		this.raiseHand = Instantiate(
			voteArea.Background, voteArea.LevelNumberText.transform);
		this.raiseHand.name = $"raisehand_{voteArea.TargetPlayerId}";
		this.raiseHand.sprite = Resources.Loader.CreateSpriteFromResources(
			Resources.Path.CaptainSpecialVoteCheck);
		this.raiseHand.transform.localPosition = new Vector3(7.25f, -0.5f, -3f);
		this.raiseHand.transform.localScale = new Vector3(1.0f, 3.5f, 1.0f);
		this.raiseHand.gameObject.layer = 5;
	}

	private void rendSetActive(bool active)
	{
		if (this.raiseHand != null)
		{
			this.raiseHand.gameObject.SetActive(active);
		}
	}
}
