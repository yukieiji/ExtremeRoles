
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Compat;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomOption.Factory.Old;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Cracker : SingleRoleBase, IRoleAutoBuildAbility
{
    public sealed class CrackTrace : IMeetingResetObject
    {
        private SpriteRenderer image;
        private GameObject body;

        public CrackTrace(Vector3 pos)
        {
            this.body = new GameObject("CrackTrace");
            this.image = this.body.AddComponent<SpriteRenderer>();
            this.image.sprite = Resources.UnityObjectLoader.LoadSpriteFromResources(
			   ObjectPath.CrackerCrackTrace, 300f);

            this.body.transform.position = pos;

            if (CompatModManager.Instance.TryGetModMap(out var modMap))
            {
				modMap.AddCustomComponent(
                    this.body, Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
            }
        }

        public void Clear()
        {
            Object.Destroy(this.image);
            Object.Destroy(this.body);
        }
    }
    public enum CrackerOption
    {
        RemoveDeadBody,
        CanCrackDistance,
    }

    public bool IsRemoveDeadBody;
    private float crackDistance;
    private byte targetDeadBodyId;

    public ExtremeAbilityButton? Button { get; set; }
	private ResetObjectSystem? system;

    public Cracker() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.Cracker),
        true, false, true, true)
    { }

    public static void CrackDeadBody(
        byte rolePlayerId, byte targetPlayerId)
    {
        var role = ExtremeRoleManager.GetSafeCastedRole<Cracker>(rolePlayerId);
        if (role == null || role.system is null) { return; }

        DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < array.Length; ++i)
        {
            if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId != targetPlayerId)
            {
                continue;
            }
			if (role.IsRemoveDeadBody)
			{
				var gameObj = array[i].gameObject;
				role.system.Add(new CrackTrace(gameObj.transform.position));
				Object.Destroy(gameObj);
			}
			else
			{
				array[i].GetComponentInChildren<BoxCollider2D>().enabled = false;
			}
		}
    }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "crack",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
			   ObjectPath.CrackerCrack));
    }

    public bool IsAbilityUse()
    {
        this.targetDeadBodyId = byte.MaxValue;
        NetworkedPlayerInfo info = Player.GetDeadBodyInfo(
            this.crackDistance);

        if (info != null)
        {
            this.targetDeadBodyId = info.PlayerId;
        }

        return IRoleAbility.IsCommonUse() && this.targetDeadBodyId != byte.MaxValue;
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
        byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.CrackerCrackDeadBody))
        {
            caller.WriteByte(localPlayerId);
            caller.WriteByte(this.targetDeadBodyId);
        }
        CrackDeadBody(localPlayerId, this.targetDeadBodyId);
        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(
            factory, 2, 5);

        factory.CreateFloatOption(
            CrackerOption.CanCrackDistance,
            1.0f, 1.0f, 5.0f, 0.5f);

        factory.CreateBoolOption(
            CrackerOption.RemoveDeadBody,
            false);
    }

    protected override void RoleSpecificInit()
    {
		var cate = this.Loader;
        this.crackDistance = cate.GetValue<CrackerOption, float>(
            CrackerOption.CanCrackDistance);
        this.IsRemoveDeadBody = cate.GetValue<CrackerOption, bool>(
            CrackerOption.RemoveDeadBody);

		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet<ResetObjectSystem>(
			ExtremeSystemType.ResetObjectSystem);
    }
}
