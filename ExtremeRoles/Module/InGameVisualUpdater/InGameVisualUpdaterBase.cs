using UnityEngine;

using TMPro;

namespace ExtremeRoles.Module.InGameVisualUpdater;

public abstract class InGameVisualUpdaterBase(PlayerControl target)
{
	public const string RoleInfoObjectName = "Info";
	public const float InfoScale = 0.25f;

	protected bool IsVisual => this.Owner.Visible;
	protected string PlayerName => this.Owner.CurrentOutfit.PlayerName;
	protected NetworkedPlayerInfo Data => this.Owner.Data;
	protected TextMeshPro NameText => this.Owner.cosmetics.nameText;
	public byte PlayerId { get; } = target.PlayerId;

	public TextMeshPro Info
	{
		get
		{
			if (field == null)
			{
				var cosmetics = this.Owner.cosmetics;
				field = Object.Instantiate(
					cosmetics.nameText,
					cosmetics.nameText.transform.parent);
				field.fontSize *= 0.75f;
				field.gameObject.name = RoleInfoObjectName;
			}
			return field;
		}
	}

	protected PlayerControl Owner { get; } = target;

	public abstract void Update();

	protected bool IsCommActive(PlayerControl target)
	{
		return PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(target);
	}

	protected void SetNameText(string text)
	{
		this.Owner.cosmetics.SetName(text);
	}

	protected void SetNameColor(Color color)
	{
		this.Owner.cosmetics.SetNameColor(color);
	}
}
