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
	bool isOnKill,
	float fontScale,
	IUnityObjectFactory unityObjectFactory) : IAbility, IExiledAnimationOverride
{
	public const int MaxExileIndex = 4;

	private readonly bool isOnKill = isOnKill;
	private readonly float fontScale = fontScale;
	private readonly IUnityObjectFactory unityObjectFactory = unityObjectFactory;

	public OverrideInfo? GetOverrideInfo(NetworkedPlayerInfo? exiledPlayer)
	{
		int index = getRandomIndex();
		return new OverrideInfo(exiledPlayer, Tr.GetString($"ScreamerExile{index}"));
	}

	public void SpawnScreamText(PlayerControl rolePlayer)
	{
		if (!this.isOnKill)
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
		string[] effectTags = ["b", "i", "u", "s", "mark", "uppercase", "lowercase", "smallcaps"];
		int index = RandomGenerator.Instance.Next(3);
		if (index == 2)
		{
			string tag = effectTags[RandomGenerator.Instance.Next(effectTags.Length)];
			formattedText = $"<{tag}>{formattedText}</{tag}>";
		}

		GameObject screamObj = this.unityObjectFactory.CreateGameObject("ScreamerScreamText");
		screamObj.transform.SetParent(targetBody.transform, false);
		screamObj.transform.localPosition = Vector3.zero;

		TextMeshPro textComponent = screamObj.AddComponent<TextMeshPro>();
		textComponent.alignment = TextAlignmentOptions.Center;
		textComponent.enableWordWrapping = false;
		textComponent.richText = true;
		textComponent.text = formattedText;

		// Random Z-axis rotation
		float randomAngle = RandomGenerator.Instance.Next(-180, 180);
		screamObj.transform.localEulerAngles = new Vector3(0f, 0f, randomAngle);

		// Scale based on screamImageScale
		screamObj.transform.localScale = new Vector3(this.fontScale, this.fontScale, 1.0f);

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
			if (i < text.Length - 1&&
				RandomGenerator.Instance.Next(100) < 25)
			{
				sb.AppendLine();
			}
		}
		return sb.ToString();
	}

	private static int getRandomIndex()
	{
		int rand = RandomGenerator.Instance.Next(100);
		return rand < 5 ? 0 : RandomGenerator.Instance.Next(1, MaxExileIndex + 1);
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
		// 100％でも大きかたので100％ => 0.5にしてそれでサイズ調整する感じに
		float scale = this.Loader.GetValue<Option, int>(Option.ScreamImageSize) / 200.0f;

		this.AbilityClass = new ScreamerAbilityHandler(
			isScreamOnKill,
			scale,
			new DefaultUnityObjectFactory());
	}
}
