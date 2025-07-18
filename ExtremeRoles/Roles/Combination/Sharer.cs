using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Combination;

public sealed class SharerManager : FlexibleCombinationRoleManagerBase
{
    public SharerManager() : base(
		CombinationRoleType.Sharer,
        new Sharer(), 2, false)
    { }

}

public sealed class Sharer : MultiAssignRoleBase, IRoleMurderPlayerHook, IRoleResetMeeting, IRoleUpdate
{
    public enum SharerOption
    {
        SharerTellKill
    }

    private TextPopUpper textPopUp;
    private SharerContainer sameSharer;
    private bool sharerTellKill;


    public class SharerContainer
    {
        private Dictionary<byte, Arrow> arrow = new Dictionary<byte, Arrow>();
        private Dictionary<byte, PlayerControl> sharer = new Dictionary<byte, PlayerControl>();

        public SharerContainer(List<byte> sharerPlayerId)
        {
            foreach (byte playerId in sharerPlayerId)
            {
                this.arrow.Add(playerId, new Arrow(Palette.ImpostorRed));
                this.sharer.Add(playerId, Player.GetPlayerControlById(playerId));
            }
        }

        public void Clear()
        {
            this.arrow.Clear();
            this.sharer.Clear();
        }

        public bool Contains(byte playerId) => arrow.ContainsKey(playerId);

        public void SetActive(bool active)
        {
            foreach (var sharerArrow in this.arrow.Values)
            {
                if (sharerArrow != null)
                {
                    sharerArrow.SetActive(active);
                }
            }
        }

        public void Update()
        {
            foreach (var (playerId, sharerArrow) in this.arrow)
            {
                if (sharerArrow != null)
                {
                    sharerArrow.UpdateTarget(
                        this.sharer[playerId].GetTruePosition());
                    sharerArrow.Update();
                }
            }
        }

    }


    public Sharer() : base(
        ExtremeRoleId.Sharer,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Sharer.ToString(),
        Palette.ImpostorRed,
        true, false,
        true, true,
        tab: OptionTab.CombinationTab)
    { }

    public void HookMuderPlayer(
        PlayerControl source,
        PlayerControl target)
    {
        if (this.sharerTellKill &&
            this.sameSharer.Contains(source.PlayerId) &&
            this.textPopUp != null)
        {
            this.textPopUp.AddText(
				Tr.GetString(
					"sharerKill",
                    source.Data.DefaultOutfit.PlayerName));
        }
    }

    public void Update(
        PlayerControl rolePlayer)
    {
        if (Minigame.Instance != null ||
            ShipStatus.Instance == null ||
            GameData.Instance == null ||
            MeetingHud.Instance != null)
        {
            return;
        }
        if (this.sameSharer != null)
        {
            this.sameSharer.Update();
        }
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        if (this.sameSharer != null)
        {
            this.sameSharer.SetActive(true);
        }
    }

    public void ResetOnMeetingStart()
    {
        if (this.textPopUp != null)
        {
            this.textPopUp.Clear();
        }
        if (this.sameSharer != null)
        {
            this.sameSharer.SetActive(false);
        }
    }

    public override string GetFullDescription()
    {
		var player = new List<string>();
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            if (playerControl == null || playerControl.Data == null ||
                !ExtremeRoleManager.TryGetRole(playerControl.PlayerId, out var role) ||
				!this.IsSameControlId(role))
            {
                continue;
            }
			player.Add(playerControl.Data.PlayerName);
        }
		return $"{base.GetFullDescription()}\n{Tr.GetString("curSharer")}:{string.Join(",", player)}";
	}

    public override string GetIntroDescription()
    {
        string baseString = base.GetIntroDescription();
        baseString += Design.ColoedString(
            Palette.ImpostorRed, "\n▽ ");

        List<byte> sharer = getAliveSameSharer();

        sharer.Remove(PlayerControl.LocalPlayer.PlayerId);

        this.sameSharer = new SharerContainer(sharer);

        byte firstSharer = sharer[0];
        sharer.RemoveAt(0);

        baseString += Player.GetPlayerControlById(
            firstSharer).Data.PlayerName;
        if (sharer.Count != 0)
        {
            for (int i = 0; i < sharer.Count; ++i)
            {

                if (i == 0)
                {
                    baseString += Tr.GetString("andFirst");
                }
                else
                {
                    baseString += Tr.GetString("and");
                }
                baseString += Player.GetPlayerControlById(
                    sharer[i]).Data.PlayerName;

            }
        }

        return string.Concat(
            baseString,
            Tr.GetString("SharerIntoPlus"),
            Design.ColoedString(Palette.ImpostorRed, " ▽"));
    }


    public override string GetRoleTag() => "▽";

    public override string GetRolePlayerNameTag(
        SingleRoleBase targetRole, byte targetPlayerId)
    {
        if (targetRole.Core.Id == ExtremeRoleId.Sharer &&
            this.IsSameControlId(targetRole))
        {
            return Design.ColoedString(
                Palette.ImpostorRed,
                $" {GetRoleTag()}");
        }

        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }

    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {
        if (targetRole.Core.Id == ExtremeRoleId.Sharer &&
            this.IsSameControlId(targetRole))
        {
            return Palette.ImpostorRed;
        }

        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

    public override bool IsSameTeam(SingleRoleBase targetRole)
    {
        if (targetRole.Core.Id == ExtremeRoleId.Sharer &&
            this.IsSameControlId(targetRole))
        {
            return true;
        }
        else
        {
            return base.IsSameTeam(targetRole);
        }
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            SharerOption.SharerTellKill,
            true);
    }

    protected override void RoleSpecificInit()
    {

        this.sharerTellKill = this.Loader.GetValue<SharerOption, bool>(
            SharerOption.SharerTellKill);

        if (this.sharerTellKill)
        {
            this.textPopUp = new TextPopUpper(
                3, 10.0f,
                new Vector3(-3.75f, -2.5f, -250.0f),
                TMPro.TextAlignmentOptions.BottomLeft);
        }
        if (this.sameSharer != null)
        {
            this.sameSharer.Clear();
        }
    }

    private List<byte> getAliveSameSharer()
    {
        List<byte> alive = [];

        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            if (playerControl == null ||
				playerControl.Data == null ||
				playerControl.Data.IsDead ||
				playerControl.Data.Disconnected ||
				!ExtremeRoleManager.TryGetRole(playerControl.PlayerId, out var role) ||
				!this.IsSameControlId(role))
            {
                continue;
            }
			alive.Add(playerControl.PlayerId);
		}

        return alive;
    }

}
