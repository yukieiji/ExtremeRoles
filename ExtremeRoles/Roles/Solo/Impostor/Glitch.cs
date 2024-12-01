using System;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API;

using UnityObject = UnityEngine.Object;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Glitch : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum Ops : byte
	{
		DeadBody,
		Player,
	}

	public ExtremeAbilityButton Button { get; set; }

	private ExtremeAbilityButton createFake;

	private Sprite deadBodyDummy;
	private Sprite playerDummy;

	private string deadBodyDummyStr;
	private string playerDummyStr;

	public Glitch() : base(
		ExtremeRoleId.Faker,
		ExtremeRoleType.Impostor,
		ExtremeRoleId.Faker.ToString(),
		Palette.ImpostorRed,
		true, false, true, true)
	{ }

	public void CreateAbility()
	{
		this.deadBodyDummy = Resources.UnityObjectLoader.LoadSpriteFromResources(
			ObjectPath.FakerDummyDeadBody, 115f);
		this.playerDummy = Resources.UnityObjectLoader.LoadSpriteFromResources(
			ObjectPath.FakerDummyPlayer, 115f);

		this.deadBodyDummyStr = Tr.GetString("dummyDeadBody");
		this.playerDummyStr = Tr.GetString("dummyPlayer");

		this.CreateNormalAbilityButton(
			"dummyDeadBody",
			this.deadBodyDummy);
	}

	public bool IsAbilityUse()
	{
		return IRoleAbility.IsCommonUse();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
	{
		return;
	}

	public void ResetOnMeetingStart()
	{
		return;
	}

	public bool UseAbility()
	{

		return true;
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateCommonAbilityOption(
			factory);
	}

	protected override void RoleSpecificInit()
	{

	}
}
