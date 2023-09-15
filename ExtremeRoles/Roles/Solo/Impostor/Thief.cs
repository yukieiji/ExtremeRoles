using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using UnityEngine;
using ExtremeRoles.Module.CustomMonoBehaviour;
using UnityEngine.Video;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Thief : SingleRoleBase, IRoleAbility
{
    public enum ThiefOption
	{
		Range,
        SetNum,
		SetTimeOffset,
		PickUpTimeOffset,
		IsAddEffect
    }


    private GameData.PlayerInfo? targetBody;
	private byte targetPlayerId = byte.MaxValue;
	private float activeRange;
	private bool isAddEffect;

	public ExtremeAbilityButton? Button { get; set; }

    public Thief() : base(
        ExtremeRoleId.Thief,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Thief.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

	public static void AddEffect(byte deadBody)
	{
		DeadBody? body = GameSystem.GetDeadBody(deadBody);
		if (body == null) { return; }

		var effect = new GameObject("ThiefEffect");
		effect.transform.position = body.transform.position;
		effect.transform.SetParent(body.transform);
		var player = effect.AddComponent<DlayableVideoPlayer>();

		player.SetThum(Loader.CreateSpriteFromResources(
			Path.TheifMagicCircle));
		player.SetVideo(Loader.GetUnityObjectFromResources<VideoClip>(
			Path.VideoAsset, string.Format(
				Path.VideoAssetPlaceHolder, Path.TheifMagicCircleVideo)));
	}

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "evolve",
            Loader.CreateSpriteFromResources(
                Path.TheifMagicCircle),
            checkAbility: CheckAbility,
            abilityOff: CleanUp,
            forceAbilityOff: ForceCleanUp);
    }

    public bool IsAbilityUse()
    {
        this.targetBody = Player.GetDeadBodyInfo(
            this.activeRange);
        return this.IsCommonUse() && this.targetBody != null;
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


		if (!this.isAddEffect) { return; }

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.ThiefAddDeadbodyEffect))
		{
			caller.WriteByte(this.targetPlayerId);
		}
		AddEffect(this.targetPlayerId);
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
        IOptionInfo parentOps)
    {
		this.CreateAbilityCountOption(
            parentOps, 2, 5, 2.0f);
		CreateFloatOption(ThiefOption.Range, 0.1f, 1.8f, 3.6f, 0.1f, parentOps);
		CreateIntOption(ThiefOption.SetNum, 5, 1, 10, 1, parentOps);
		CreateIntOption(ThiefOption.SetTimeOffset, 30, 10, 360, 5, parentOps);
		CreateIntOption(ThiefOption.PickUpTimeOffset, 6, 1, 60, 1, parentOps);
		CreateBoolOption(ThiefOption.IsAddEffect, true, parentOps);
	}

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();

        var allOption = OptionManager.Instance;

		this.activeRange = allOption.GetValue<float>(GetRoleOptionId(ThiefOption.Range));
		this.isAddEffect = allOption.GetValue<bool>(GetRoleOptionId(ThiefOption.IsAddEffect));

		ExtremeSystemTypeManager.Instance.TryAdd(
			ExtremeSystemType.ThiefMeetingTimeChange,
			new ThiefMeetingTimeStealSystem(
				allOption.GetValue<int>(GetRoleOptionId(ThiefOption.SetNum)),
				-allOption.GetValue<int>(GetRoleOptionId(ThiefOption.SetTimeOffset)),
				allOption.GetValue<int>(GetRoleOptionId(ThiefOption.PickUpTimeOffset))));
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
    {
        return;
    }
}
