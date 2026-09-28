using System.Text;
using TMPro;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class ScreamerAbilityHandler(
	bool isScreamOnKill,
	float screamImageScale,
	IUnityObjectFactory unityObjectFactory) : IAbility, IExiledAnimationOverride
{
	private readonly bool isScreamOnKill = isScreamOnKill;
	private readonly float screamImageScale = screamImageScale;
	private readonly IUnityObjectFactory unityObjectFactory = unityObjectFactory;

	public OverrideInfo? GetOverrideInfo(NetworkedPlayerInfo? exiledPlayer)
	{
		int index = getRandomIndex();
		return new OverrideInfo(exiledPlayer, Tr.GetString($"ScreamerExile{index}"));
	}

	public void SpawnScreamText(PlayerControl rolePlayer)
	{
		if (!this.isScreamOnKill)
		{
			return;
		}

		DeadBody? targetBody = GameSystem.GetDeadBody(rolePlayer.PlayerId);
		if (targetBody == null)
		{
			return;
		}

		int textIndex = getRandomIndex();
		string rawText = Tr.GetString($"ScreamerExile{textIndex}");
		string formattedText = insertRandomLineBreaks(rawText);

		// Rich text effect tags
		string[] effectTags = new[] { "shake", "wave" };
		string tag = effectTags[RandomGenerator.Instance.Next(effectTags.Length)];
		formattedText = $"<{tag}>{formattedText}</{tag}>";

		GameObject screamObj = this.unityObjectFactory.CreateGameObject("ScreamerScreamText");
		screamObj.transform.SetParent(targetBody.transform, false);
		screamObj.transform.localPosition = Vector3.zero;

		// Random Z-axis rotation
		float randomAngle = RandomGenerator.Instance.Next(-180, 180);
		screamObj.transform.localEulerAngles = new Vector3(0f, 0f, randomAngle);

		// Scale based on screamImageScale
		float fontScale = this.screamImageScale;
		screamObj.transform.localScale = new Vector3(fontScale, fontScale, 1.0f);

		TextMeshPro textComponent = screamObj.AddComponent<TextMeshPro>();
		textComponent.alignment = TextAlignmentOptions.Center;
		textComponent.enableWordWrapping = false;
		textComponent.richText = true;
		textComponent.text = formattedText;

		// Random color
		textComponent.color = new Color(
			RandomGenerator.Instance.Next(10000) / 10000.0f,
			RandomGenerator.Instance.Next(10000) / 10000.0f,
			RandomGenerator.Instance.Next(10000) / 10000.0f,
			1.0f);

		// Random font styles
		FontStyles styles = FontStyles.Normal;
		if (RandomGenerator.Instance.Next(2) == 0) { styles |= FontStyles.Bold; }
		if (RandomGenerator.Instance.Next(2) == 0) { styles |= FontStyles.Italic; }
		if (RandomGenerator.Instance.Next(2) == 0) { styles |= FontStyles.Underline; }
		if (RandomGenerator.Instance.Next(2) == 0) { styles |= FontStyles.Strikethrough; }
		textComponent.fontStyle = styles;

		// Outline properties
		textComponent.outlineWidth = RandomGenerator.Instance.Next(100, 500) / 1000.0f;
		textComponent.outlineColor = new Color32(
			(byte)RandomGenerator.Instance.Next(256),
			(byte)RandomGenerator.Instance.Next(256),
			(byte)RandomGenerator.Instance.Next(256),
			255);
	}

	private static string insertRandomLineBreaks(string text)
	{
		if (string.IsNullOrEmpty(text) || text.Length <= 1)
		{
			return text;
		}

		var sb = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			sb.Append(text[i]);
			if (i < text.Length - 1 && text[i] != '\n')
			{
				if (RandomGenerator.Instance.Next(100) < 30)
				{
					sb.Append('\n');
				}
			}
		}
		return sb.ToString();
	}

	private static int getRandomIndex()
	{
		int rand = RandomGenerator.Instance.Next(100);
		return rand < 5 ? 4 : (rand - 5) % 4;
	}
}

public sealed class Screamer : SingleRoleBase
{
	public enum Option
	{
		IsScreamOnKill,
		ScreamImageSize,
	}

	public Screamer() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Screamer,
			ColorPalette.ScreamerColor))
	{
	}

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer,
		PlayerControl killerPlayer)
	{
		if (this.AbilityClass is ScreamerAbilityHandler handler)
		{
			handler.SpawnScreamText(rolePlayer);
		}
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(
			Option.IsScreamOnKill,
			true);
		factory.CreateIntOption(
			Option.ScreamImageSize,
			100, 10, 1000, 10,
			format: OptionUnit.Percentage);
	}

	protected override void RoleSpecificInit()
	{
		bool isScreamOnKill = this.Loader.GetValue<Option, bool>(Option.IsScreamOnKill);
		float scale = this.Loader.GetValue<Option, int>(Option.ScreamImageSize) / 100.0f;

		this.AbilityClass = new ScreamerAbilityHandler(
			isScreamOnKill,
			scale,
			new DefaultUnityObjectFactory());
	}
}
