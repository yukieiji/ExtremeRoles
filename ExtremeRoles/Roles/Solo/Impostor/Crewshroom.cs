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




using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

public sealed class Crewshroom : SingleRoleBase, IRoleAutoBuildAbility
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
			Resources.Loader.CreateSpriteFromResources(
			   Path.CrewshroomSet));
	}

	public override string GetIntroDescription()
		=> Map.Id switch
		{
			5 => Translation.GetString($"{this.Id}FungleIntroDescription"),
			_ => base.GetIntroDescription()
		};

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		return;
	}

	public void ResetOnMeetingStart()
	{
		return;
	}

	public bool UseAbility()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		Vector2 setPos = localPlayer.GetTruePosition();

		ModedMushroomSystem.RpcSetModMushroom(setPos);
		return true;
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 3, 50);
		factory.CreateFloatOption(
			Option.DelaySecond, 5.0f, 0.5f, 30.0f, 0.5f, format:OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		ExtremeSystemTypeManager.Instance.TryAdd(
			ModedMushroomSystem.Type,
			new ModedMushroomSystem(
				this.Loader.GetValue<Option, float>(
					Option.DelaySecond)));
	}
}
