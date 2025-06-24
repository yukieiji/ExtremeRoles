using UnityEngine;


using Il2CppActionFloat = Il2CppSystem.Action<float>;
using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace ExtremeRoles.Module.Meeting;

public interface IPlayerVoteAreaButtonPostionComputer
{
	public UiElement Element { get; }
	public Il2CppIEnumerator Compute();
}

public sealed class PlayerVoteAreaButtonPostionComputer(
	float time, UiElement element, float endOffset) : IPlayerVoteAreaButtonPostionComputer
{
	public Vector2 Anchor { private get; set; } = Vector2.right;
	public Vector2 Offset { private get; set; } = Vector2.zero;
	public float StartOffset { private get; set; }
	public UiElement Element { get; } = element;

	private readonly float time = time;
	private readonly float endOffset = endOffset;

	public Il2CppIEnumerator Compute()
		=> Effects.Lerp(this.time, (Il2CppActionFloat)this.deltaPos);

	private void deltaPos(float deltaT)
	{
		this.Element.transform.localPosition = Vector2.Lerp(
			this.Anchor * this.StartOffset + this.Offset,
			this.Anchor * this.endOffset + this.Offset,
			Effects.ExpOut(deltaT));
	}
}