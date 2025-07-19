using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.Ability;

using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Faker : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum FakerDummyOps : byte
	{
		DeadBody,
		Player,
	}
	public enum Option
	{
		SeeDummyMerlin
	}

	public ExtremeAbilityButton Button { get; set; }

	private Sprite deadBodyDummy;
	private Sprite playerDummy;

	private string deadBodyDummyStr;
	private string playerDummyStr;

	public Faker() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.Faker),
		true, false, true, true)
	{ }

	public void CreateAbility()
	{
		this.deadBodyDummy = UnityObjectLoader.LoadFromResources(
			ExtremeRoleId.Faker,
			ObjectPath.FakerDummyDeadBody);
		this.playerDummy = UnityObjectLoader.LoadFromResources(
			ExtremeRoleId.Faker,
			ObjectPath.FakerDummyPlayer);

		this.deadBodyDummyStr = Tr.GetString("dummyDeadBody");
		this.playerDummyStr = Tr.GetString("dummyPlayer");

		this.CreateNormalAbilityButton(
			"dummyDeadBody",
			this.deadBodyDummy);
	}

    public bool IsAbilityUse()
    {
        bool isPlayerDummy = Key.IsShift();

		this.Button.Behavior.SetGraphic(
			isPlayerDummy ? this.playerDummyStr : this.deadBodyDummyStr,
			isPlayerDummy ? this.playerDummy : this.deadBodyDummy);

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

		var allPlayer = GameData.Instance.AllPlayers;

        bool isPlayerMode = Key.IsShift();
		bool excludeImp = Key.IsControlDown();
		bool excludeMe = Key.IsAltDown();

		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;

		bool contine;
		byte targetPlayerId;

		do
		{
			int index = Random.RandomRange(0, allPlayer.Count);
			var player = allPlayer[index];
			targetPlayerId = player.PlayerId;

			contine = player.IsDead || player.Disconnected;
			if (!contine && excludeImp)
			{
				contine = ExtremeRoleManager.GameRole[targetPlayerId].IsImpostor();
			}
			else if (!contine && excludeMe)
			{
				contine = localPlayerId == targetPlayerId;
			}

		} while (contine);

		byte ops = isPlayerMode ? (byte)FakerDummyOps.Player : (byte)FakerDummyOps.DeadBody;

		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.FakerDummy, (writer) =>
			{
				writer.Write(localPlayerId);
				writer.Write(targetPlayerId);
				writer.Write(ops);
			});

		return true;
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateCommonAbilityOption(
			factory);
		factory.CreateBoolOption(
			Option.SeeDummyMerlin,
			true);
	}

	protected override void RoleSpecificInit()
	{
		ExtremeSystemTypeManager.Instance.TryAdd(
			ExtremeSystemType.FakerDummy,
			new FakerDummySystem(
				this.Loader.GetValue<Option, bool>(Option.SeeDummyMerlin)));
	}
}
