using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomMonoBehaviour;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Artist : SingleRoleBase, IRoleAbility, IRoleUpdate
{
    public enum AliceOption
    {
        CanUseSabotage,
        RevartCommonTaskNum,
        RevartLongTaskNum,
        RevartNormalTaskNum,
    }

    public ExtremeAbilityButton? Button { get; set; }
	private float area = 0.0f;
	private ArtistLineDrawer? drawer;

	private float winArea = 0.0f;

    public Artist(): base(
        ExtremeRoleId.Artist,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Artist.ToString(),
        ColorPalette.AliceGold,
        true, false, true, true)
    { }

	public void Update(PlayerControl rolePlayer)
	{
		if (rolePlayer == null ||
			rolePlayer.Data == null ||
			this.IsWin)
		{
			return;
		}

		if (this.drawer != null)
		{
			this.IsWin = this.area + this.drawer.Area >= this.winArea;

			if (MeetingHud.Instance != null ||
				ExileController.Instance != null)
			{
				// 解除処理
			}
		}
	}

	public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "shipBroken", Loader.CreateSpriteFromResources(
                Path.AliceShipBroken));
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    public bool IsAbilityUse() => this.IsCommonUse();

	public bool UseAbility()
    {
		// 解除処理
		// 追加処理


        return true;
    }

    public static void ShipBroken(
        byte callerId, byte targetPlayerId, List<int> addTaskId)
    {

    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {

        this.CreateAbilityCountOption(
            parentOps, 2, 100);

    }

    protected override void RoleSpecificInit()
    {
        var allOption = OptionManager.Instance;

		this.area = 0.0f;
        this.RoleAbilityInit();
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }
}
