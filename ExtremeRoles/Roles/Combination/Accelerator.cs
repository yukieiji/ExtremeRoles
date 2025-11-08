using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using Hazel;
using UnityEngine;

namespace ExtremeRoles.Roles.Combination;

#nullable enable

public sealed class AcceleratorManager : FlexibleCombinationRoleManagerBase
{
    public AcceleratorManager() : base(
		CombinationRoleType.Accelerator,
		new AcceleratorRole(), 1)
    { }

}

public sealed class AcceleratorStatus : IStatusModel, IUsableOverrideStatus
{
	public bool EnableUseButton { get; set; } = true;

	public bool EnableVentButton { get; set; } = true;
}

public sealed class AcceleratorRole :
    MultiAssignRoleBase,
    IRoleAutoBuildAbility,
    IRoleSpecialReset
{
    public enum AcceleratorRpc : byte
    {
        Setup,
        End,
    }

	public enum Option
	{
		Speed,
		UseOtherPlayer
	}

    public override string RoleName =>
        string.Concat(this.roleNamePrefix, this.Core.Name);

	private float speed;
	private bool canUseOtherPlayer = false;
    private string roleNamePrefix;

	private AutoTransformerWithFixedFirstPoint? transformer;
	public override IStatusModel? Status  => status;
	private AcceleratorStatus? status;

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }

	public AcceleratorRole() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Accelerator,
			ColorPalette.AcceleratorBiancoPeria),
        false, true, false, false,
        tab: OptionTab.CombinationTab)
    { }
#pragma warning restore CS8618

	public static void Ability(ref MessageReader reader)
    {
		AcceleratorRpc rpcId = (AcceleratorRpc)reader.ReadByte();
        byte rolePlayerId = reader.ReadByte();

        var rolePlayer = Player.GetPlayerControlById(rolePlayerId);
        if (rolePlayer == null ||
			!ExtremeRoleManager.TryGetSafeCastedRole<AcceleratorRole>(rolePlayerId, out var role))
		{
			return;
		}

        float x = reader.ReadSingle();
        float y = reader.ReadSingle();

        rolePlayer.NetTransform.SnapTo(new Vector2(x, y));
		var pos = rolePlayer.GetTruePosition();

		switch (rpcId)
		{
			case AcceleratorRpc.Setup:
				setupPanel(role, rolePlayer);
				break;
			case AcceleratorRpc.End:
				endPanel(role, pos);
				break;
			default:
				break;
		}

    }

    private static void setupPanel(AcceleratorRole accelerator, PlayerControl player)
    {
		if (accelerator.status == null)
		{
			return;
		}

		accelerator.status.EnableUseButton = false;
		accelerator.status.EnableVentButton = false;

		Vector2 pos = player.GetTruePosition();

		var obj = new GameObject("accelerate_panel");
		var firstPoint = new Vector3(pos.x, pos.y, pos.y / 100.0f);
		obj.transform.position = firstPoint;

		var rend = obj.AddComponent<SpriteRenderer>();
		rend.sprite = UnityObjectLoader.LoadFromResources(
			ExtremeRoleId.Accelerator,
			ObjectPath.AcceleratorAcceleratePanel);

		accelerator.transformer = obj.AddComponent<AutoTransformerWithFixedFirstPoint>();
		accelerator.transformer.Initialize(firstPoint, player.transform, rend);
    }

    private static void endPanel(AcceleratorRole accelerator, Vector2 endPos)
    {
		if (accelerator.transformer == null ||
			accelerator.status == null)
		{
			return;
		}

		accelerator.status.EnableUseButton = true;
		accelerator.status.EnableVentButton = true;

		accelerator.transformer.Fixed(
			new Vector3(endPos.x, endPos.y, endPos.y / 100.0f));
		GameObject obj = accelerator.transformer.gameObject;
		Vector2 vec = accelerator.transformer.Vector;
		Object.Destroy(accelerator.transformer);
		accelerator.transformer = null;

		var panel = obj.AddComponent<AcceleratorPanel>();
		panel.Initialize(vec, accelerator.speed);
	}

    public void CreateAbility()
    {
        this.CreateReclickableCountAbilityButton(
            Tr.GetString("AccelerateSet"),
            UnityObjectLoader.LoadFromResources(ExtremeRoleId.Accelerator),
            checkAbility: IsAbilityActive,
            abilityOff: this.CleanUp);
        if (this.IsCrewmate())
        {
            this.Button?.SetLabelToCrewmate();
        }
    }

    public bool IsAbilityActive() =>
        PlayerControl.LocalPlayer.moveable;

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
		acceleratorPanelOps(PlayerControl.LocalPlayer, false);
		return true;
	}

    public void CleanUp()
    {
		acceleratorPanelOps(PlayerControl.LocalPlayer, true);
	}

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
		if (this.transformer == null) { return; }

		Vector2 playerPos = rolePlayer.GetTruePosition();
		endPanel(this, playerPos);
	}

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        var imposterSetting = factory.Get((int)CombinationRoleCommonOption.IsAssignImposter);
        CreateKillerOption(factory, new ParentActive(imposterSetting));

        IRoleAbility.CreateAbilityCountOption(
            factory, 3, 10, 30.0f);
		factory.CreateFloatOption(
			Option.Speed, 1.0f, 0.1f,
			3.0f, 0.1f);
		factory.CreateBoolOption(
			Option.UseOtherPlayer,
			true);
    }

    protected override void RoleSpecificInit()
    {
        this.roleNamePrefix = this.CreateImpCrewPrefix();

		var loader = this.Loader;
		this.canUseOtherPlayer = loader.GetValue<Option, bool>(
			Option.UseOtherPlayer);
		this.speed = loader.GetValue<Option, float>(
			Option.Speed);

		this.status = new AcceleratorStatus();
    }

    public void AllReset(PlayerControl rolePlayer)
    {
		if (this.transformer == null) { return; }

		Vector2 playerPos = rolePlayer.GetTruePosition();
		endPanel(this, playerPos);
    }

	private void acceleratorPanelOps(PlayerControl playerControl, bool isEnd)
	{
		Vector2 pos = playerControl.transform.position;

		if (this.canUseOtherPlayer)
		{
			using (var caller = RPCOperator.CreateCaller(
				RPCOperator.Command.AcceleratorAbility))
			{
				caller.WriteByte((byte)(isEnd ? AcceleratorRpc.End : AcceleratorRpc.Setup));
				caller.WriteByte(playerControl.PlayerId);
				caller.WriteFloat(pos.x);
				caller.WriteFloat(pos.y);
			}
		}

		if (isEnd)
		{
			var endPos = playerControl.GetTruePosition();
			endPanel(this, endPos);
		}
		else
		{
			setupPanel(this, playerControl);
		}
	}
}
