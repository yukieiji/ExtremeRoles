using ExtremeRoles.Compat;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using Hazel;
using UnityEngine;
using UnityHelper = ExtremeRoles.Helper.Unity;


#nullable enable

namespace ExtremeRoles.Roles.Combination;

public sealed class MoverManager : FlexibleCombinationRoleManagerBase
{
    public MoverManager() : base(
		CombinationRoleType.Mover,
		new Mover(), 1)
    { }

}

public sealed class MoverStatus : IStatusModel, IUsableOverrideStatus
{
	public bool EnableUseButton { get; set; } = true;

	public bool EnableVentButton => true;
}

public sealed class Mover :
    MultiAssignRoleBase,
    IRoleAutoBuildAbility,
    IRoleSpecialReset
{
    public enum MoverRpc : byte
    {
        Move,
        Reset,
    }

    public override string RoleName =>
        string.Concat(this.roleNamePrefix, this.Core.Name);

    public ExtremeAbilityButton? Button { get; set; }


    private sealed class ConsoleData
    {
        public Console Console { get; init; }
        public GameObject Object => this.Console.gameObject;
        private readonly Transform parent;

        public ConsoleData(Console console)
        {
            this.Console = console;
            this.parent = this.Console.transform.parent;
            disableBehavioures(this.parent.gameObject);
        }
        public void PickUp(Transform trans)
        {
            this.Console.Image.enabled = false;
            this.Console.transform.position = trans.position;
            this.Console.transform.SetParent(trans);
        }
        public void Put(Vector2 pos)
        {
			this.Console.transform.SetParent(this.parent);
            this.Console.Image.enabled = true;
            this.Console.transform.position =
                new Vector3(pos.x, pos.y, pos.y / 1000.0f);

        }
    }

	public override IStatusModel? Status => status;
	private MoverStatus? status;

	private Console? targetConsole;

    private ConsoleData? hasConsole;

    private string roleNamePrefix = string.Empty;

    public Mover() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Mover,
			ColorPalette.MoverSafeColor),
        false, true, false, false,
        tab: OptionTab.CombinationTab)
    { }

    public static void Ability(ref MessageReader reader)
    {
        MoverRpc rpcId = (MoverRpc)reader.ReadByte();
        byte rolePlayerId = reader.ReadByte();

        var rolePlayer = Player.GetPlayerControlById(rolePlayerId);
        if (rolePlayer == null ||
			!ExtremeRoleManager.TryGetSafeCastedRole<Mover>(rolePlayerId, out var role))
        {
            return;
        }

        float x = reader.ReadSingle();
        float y = reader.ReadSingle();

        rolePlayer.NetTransform.SnapTo(new Vector2(x, y));

        switch (rpcId)
        {
            case MoverRpc.Move:
                int index = reader.ReadPackedInt32();
                pickUpConsole(role, rolePlayer, index);
                break;
            case MoverRpc.Reset:
                removeConsole(role, rolePlayer);
                break;
            default:
                break;
        }
    }

    private static void pickUpConsole(Mover mover, PlayerControl player, int index)
    {
        Console console = ShipStatus.Instance.AllConsoles[index];

        if (console == null)
		{
			return;
		}

		if (mover.status != null)
		{
			mover.status.EnableUseButton = false;
		}

		UnityHelper.SetColliderActive(console.gameObject, false);
        setColliderTriggerOn(console.gameObject);
        disableBehavioures(console.gameObject);

        mover.hasConsole = new ConsoleData(console);

        if (mover.hasConsole.Console.GetComponent<Vent>() != null)
        {
            mover.hasConsole.Object.AddComponent<VentInPlayerPosSyncer>();
        }

        mover.hasConsole.PickUp(player.transform);
    }

    private static void removeConsole(Mover mover, PlayerControl player)
    {
		if (mover.status != null)
		{
			mover.status.EnableUseButton = true;
		}

        if (mover.hasConsole == null)
		{
			return;
		}

        if (mover.hasConsole.Console.TryGetComponent<VentInPlayerPosSyncer>(out var syncer))
        {
            Object.Destroy(syncer);
        }
		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			modMap.AddCustomComponent(
				mover.hasConsole.Console.gameObject,
				Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
		}
		UnityHelper.SetColliderActive(mover.hasConsole.Object, true);

        mover.hasConsole.Put(player.GetTruePosition());
		mover.hasConsole = null;
    }

    public void CreateAbility()
    {
        this.CreateReclickableCountAbilityButton(
			Tr.GetString("Moving"),
			Resources.UnityObjectLoader.LoadSpriteFromResources(
			   ObjectPath.MoverMove),
            checkAbility: IsAbilityActive,
            abilityOff: this.CleanUp);
        if (this.IsCrewmate())
        {
            this.Button?.SetLabelToCrewmate();
        }
    }

    public bool IsAbilityActive() =>
        PlayerControl.LocalPlayer.moveable;

    public bool IsAbilityUse()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        this.targetConsole = Player.GetClosestConsole(
            localPlayer, localPlayer.MaxReportDistance);

        if (this.targetConsole == null) { return false; }

        return
            IRoleAbility.IsCommonUse() &&
            this.targetConsole.Image != null &&
            GameSystem.IsValidConsole(localPlayer, this.targetConsole);
    }

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
        PlayerControl player = PlayerControl.LocalPlayer;

        for (int i = 0; i < ShipStatus.Instance.AllConsoles.Length; ++i)
        {
            Console console = ShipStatus.Instance.AllConsoles[i];
            if (console != this.targetConsole) { continue; }

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.MoverAbility))
            {
                caller.WriteByte((byte)MoverRpc.Move);
                caller.WriteByte(player.PlayerId);
                caller.WriteFloat(player.transform.position.x);
                caller.WriteFloat(player.transform.position.y);
                caller.WritePackedInt(i);
            }
            pickUpConsole(this, player, i);
            return true;
        }
        return false;
    }

    public void CleanUp()
    {
        PlayerControl player = PlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.MoverAbility))
        {
            caller.WriteByte((byte)MoverRpc.Reset);
            caller.WriteByte(player.PlayerId);
            caller.WriteFloat(player.transform.position.x);
            caller.WriteFloat(player.transform.position.y);
        }
        removeConsole(this, player);
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        removeConsole(this, rolePlayer);
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
	{
		IRoleAbility.CreateAbilityCountOption(categoryScope.Builder, 3, 10, 30.0f);
    }

    protected override void RoleSpecificInit()
    {
        this.roleNamePrefix = this.CreateImpCrewPrefix();

		this.status = new MoverStatus();
    }

    public void AllReset(PlayerControl rolePlayer)
    {
        removeConsole(this, rolePlayer);
    }

    public static void setColliderTriggerOn(GameObject obj)
    {
        colliderTriggerOn<Collider2D>(obj);
        colliderTriggerOn<PolygonCollider2D>(obj);
        colliderTriggerOn<BoxCollider2D>(obj);
        colliderTriggerOn<CircleCollider2D>(obj);
    }

    private static void colliderTriggerOn<T>(GameObject obj) where T : Collider2D
    {
        if (obj.TryGetComponent<T>(out var comp))
        {
            comp.isTrigger = true;
        }
    }

    private static void disableBehavioures(GameObject obj)
    {
        disableBehaviour<HoverAnimBehaviour>(obj);
    }

    private static void disableBehaviour<T>(GameObject obj) where T : MonoBehaviour
    {
        if (obj.TryGetComponent<T>(out var comp))
        {
            comp.enabled = false;
        }
    }
}
