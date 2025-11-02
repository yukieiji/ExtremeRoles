using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using UnityEngine;
using ExtremeRoles.Module.CustomMonoBehaviour;
using UnityEngine.Video;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Thief : SingleRoleBase, IRoleAutoBuildAbility
{
    public enum ThiefOption
	{
		Range,
		SetTimeOffset,
		SetNum,
		PickUpTimeOffset,
		IsAddEffect
    }


    private NetworkedPlayerInfo? targetBody;
	private byte targetPlayerId = byte.MaxValue;
	private float activeRange;
	private bool isAddEffect;

	public ExtremeAbilityButton? Button { get; set; }

    public Thief() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.Thief),
        true, false, true, true)
    { }

	public static void AddEffect(byte deadBody)
	{
		DeadBody? body = GameSystem.GetDeadBody(deadBody);
		if (body == null) { return; }

		var effect = new GameObject("ThiefEffect");
		effect.transform.position = body.transform.position;
		effect.transform.SetParent(body.transform);
		effect.transform.localPosition = new Vector2(-0.25f, -0.15f);
		var player = effect.AddComponent<DlayableVideoPlayer>();

		var thum = UnityObjectLoader.LoadFromResources(ExtremeRoleId.Thief);
		player.SetThum(thum);

		var video = UnityObjectLoader.LoadFromResources<VideoClip, ExtremeRoleId>(
			ExtremeRoleId.Thief,
			ObjectPath.GetRoleVideoPath(ExtremeRoleId.Thief));
		player.SetVideo(video);
	}

    public void CreateAbility()
    {
        this.CreateActivatingAbilityCountButton(
            "steal",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Thief),
            checkAbility: CheckAbility,
            abilityOff: CleanUp,
            forceAbilityOff: ForceCleanUp);
    }

    public bool IsAbilityUse()
    {
        this.targetBody = Player.GetDeadBodyInfo(
            this.activeRange);
        return IRoleAbility.IsCommonUse() && this.targetBody != null;
    }

    public void ForceCleanUp()
    {
        this.targetBody = null;
    }

    public void CleanUp()
    {
		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
			ExtremeSystemType.ThiefMeetingTimeChange,
			x =>
			{
				x.Write((byte)ThiefMeetingTimeStealSystem.Ops.Set);
			});


		if (this.isAddEffect)
		{
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.ThiefAddDeadbodyEffect))
			{
				caller.WriteByte(this.targetPlayerId);
			}
			AddEffect(this.targetPlayerId);
		}
		this.targetPlayerId = byte.MaxValue;
	}

    public bool CheckAbility()
    {
        this.targetBody = Player.GetDeadBodyInfo(
            this.activeRange);

        bool result;

        if (this.targetBody == null)
        {
            result = false;
        }
        else
        {
            result = this.targetPlayerId == this.targetBody.PlayerId;
        }

        return result;
    }

    public bool UseAbility()
    {
        this.targetPlayerId = this.targetBody!.PlayerId;
		return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		IRoleAbility.CreateAbilityCountOption(
            factory, 2, 5, 2.0f);
		factory.CreateFloatOption(ThiefOption.Range, 0.1f, 1.8f, 3.6f, 0.1f);
		factory.CreateIntOption(ThiefOption.SetTimeOffset, 30, 10, 360, 5, format: OptionUnit.Second);
		factory.CreateIntOption(ThiefOption.SetNum, 5, 1, 10, 1);
		factory.CreateIntOption(ThiefOption.PickUpTimeOffset, 6, 1, 60, 1, format: OptionUnit.Second);
		factory.CreateBoolOption(ThiefOption.IsAddEffect, true);
	}

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;

		this.activeRange = cate.GetValue<ThiefOption, float>(ThiefOption.Range);
		this.isAddEffect = cate.GetValue<ThiefOption, bool>(ThiefOption.IsAddEffect);

		ExtremeSystemTypeManager.Instance.TryAdd(
			ExtremeSystemType.ThiefMeetingTimeChange,
			new ThiefMeetingTimeStealSystem(
				cate.GetValue<ThiefOption, int>(ThiefOption.SetNum),
				-cate.GetValue<ThiefOption, int>(ThiefOption.SetTimeOffset),
				cate.GetValue<ThiefOption, int>(ThiefOption.PickUpTimeOffset)));
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }
}
