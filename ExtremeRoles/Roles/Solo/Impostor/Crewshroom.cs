using AmongUs.GameOptions;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

public sealed class Crewshroom : SingleRoleBase, IRoleAbility
{
	public ExtremeAbilityButton Button { get; set; }

	public enum Option
	{
		DelaySecond
	}

#pragma warning disable CS8618
	public Crewshroom() : base(
		ExtremeRoleId.Crewshroom,
		ExtremeRoleType.Impostor,
		ExtremeRoleId.Crewshroom.ToString(),
		Palette.ImpostorRed,
		true, false, true, true)
	{ }
#pragma warning restore CS8618

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			Translation.GetString("CrewshroomSet"),
			Loader.CreateSpriteFromResources(
			   Path.CrewshroomSet));
	}

	public override string GetIntroDescription()
		=> GameOptionsManager.Instance.CurrentGameOptions.GetByte(
			ByteOptionNames.MapId) switch
		{
			5 => Translation.GetString($"{this.Id}FungleIntroDescription"),
			_ => base.GetIntroDescription()
		};

	public bool IsAbilityUse() => this.IsCommonUse();

	public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
	{
		return;
	}

	public void ResetOnMeetingStart()
	{
		return;
	}

	public bool UseAbility()
	{
		PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
		Vector2 setPos = localPlayer.GetTruePosition();

		ModedMushroomSystem.RpcSetModMushroom(setPos);
		return true;
	}

	protected override void CreateSpecificOption(
		IOptionInfo parentOps)
	{
		this.CreateAbilityCountOption(parentOps, 3, 50);
		CreateFloatOption(Option.DelaySecond, 5.0f, 0.5f, 30.0f, 0.5f, parentOps, format:OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		ExtremeSystemTypeManager.Instance.TryAdd(
			ModedMushroomSystem.Type,
			new ModedMushroomSystem(
				OptionManager.Instance.GetValue<float>(
					GetRoleOptionId(Option.DelaySecond))));
	}
}
