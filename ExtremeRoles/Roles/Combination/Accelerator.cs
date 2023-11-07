using UnityEngine;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Roles.Combination;

#nullable enable

public sealed class AcceleratorManager : FlexibleCombinationRoleManagerBase
{
    public AcceleratorManager() : base(new Accelerator(), 1)
    { }

}

public sealed class Accelerator :
    MultiAssignRoleBase,
    IRoleAbility,
    IRoleSpecialReset,
    IRoleUsableOverride
{
    public enum AcceleratorRpc : byte
    {
        Setup,
        End,
    }

    public override string RoleName =>
        string.Concat(this.roleNamePrefix, this.RawRoleName);

    public bool EnableUseButton { get; private set; } = true;

    public bool EnableVentButton { get; private set; } = true;


    private string roleNamePrefix;

	private GameObject? panelBody;

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }

	public Accelerator() : base(
        ExtremeRoleId.Accelerator,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Accelerator.ToString(),
        ColorPalette.MoverSafeColor,
        false, true, false, false,
        tab: OptionTab.Combination)
    { }
#pragma warning restore CS8618

	public static void Ability(ref MessageReader reader)
    {
		AcceleratorRpc rpcId = (AcceleratorRpc)reader.ReadByte();
        byte rolePlayerId = reader.ReadByte();

        var rolePlayer = Player.GetPlayerControlById(rolePlayerId);
        var role = ExtremeRoleManager.GetSafeCastedRole<Accelerator>(rolePlayerId);
        if (role == null || rolePlayer == null) { return; }

        float x = reader.ReadSingle();
        float y = reader.ReadSingle();

        rolePlayer.NetTransform.SnapTo(new Vector2(x, y));

    }

    private static void setupPanel(Accelerator accelerator, PlayerControl player)
    {
		accelerator.EnableUseButton = false;

		Vector2 pos = player.GetTruePosition();

		var obj = new GameObject("accelerate_panel");
		obj.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

		var trans = obj.AddComponent<AutoTransformer>();
		trans.Initialize(obj.transform, player.transform);
    }

    private static void removeConsole(Accelerator accelerator, PlayerControl player)
    {
		accelerator.EnableUseButton = true;
    }

    public void CreateAbility()
    {
        this.CreateReclickableCountAbilityButton(
            Translation.GetString("Moving"),
            Loader.CreateSpriteFromResources(
               Path.MoverMove),
            checkAbility: IsAbilityActive,
            abilityOff: this.CleanUp);
        if (this.IsCrewmate())
        {
            this.Button.SetLabelToCrewmate();
        }
    }

    public bool IsAbilityActive() =>
        CachedPlayerControl.LocalPlayer.PlayerControl.moveable;

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

		return true;
	}

    public void CleanUp()
    {
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        var imposterSetting = OptionManager.Instance.Get<bool>(
            GetManagerOptionId(CombinationRoleCommonOption.IsAssignImposter));

        CreateKillerOption(imposterSetting);

        this.CreateAbilityCountOption(
            parentOps, 3, 10, 30.0f);
    }

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();

        this.roleNamePrefix = this.CreateImpCrewPrefix();

        this.EnableVentButton = true;
        this.EnableUseButton = true;
    }

    public void AllReset(PlayerControl rolePlayer)
    {
    }

	private static void RpcAcceleratorPanelOps(Vector2 pos, bool isEnd)
	{
		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.AcceleratorAbility))
		{
			caller.WriteByte((byte)(isEnd ? AcceleratorRpc.Setup : AcceleratorRpc.End));
			caller.WriteFloat(pos.x);
			caller.WriteFloat(pos.y);
		}
	}
}
