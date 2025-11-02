
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.ModeSwitcher;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ButtonGraphic = ExtremeRoles.Module.Ability.Behavior.ButtonGraphic;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Umbrer : SingleRoleBase, IRoleAutoBuildAbility, IRoleSpecialSetUp, IRoleUpdate
{
    private sealed class InfectedContainer
    {
        public HashSet<byte> FirstStage => this.firstStage;
        public HashSet<byte> FinalStage => this.finalStage;

        private HashSet<byte> firstStage = new HashSet<byte>();
        private HashSet<byte> finalStage = new HashSet<byte>();

        public InfectedContainer()
        {
            this.firstStage.Clear();
            this.finalStage.Clear();
        }

        public void Fetch(SingleRoleBase umbrer)
        {
            foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
            {
                if (umbrer.IsSameTeam(role))
                {
                    this.firstStage.Add(playerId);
                }
            }
        }

        public bool IsAllPlayerInfected()
        {
            if (this.firstStage.Count <= 0) { return false; }

            foreach (NetworkedPlayerInfo player in
                GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (player == null || player.Object == null ||
					player.IsDead || player.Disconnected) { continue; }

                if (!this.firstStage.Contains(player.PlayerId))
                {
                    return false;
                }

            }
            return true;
        }

        public bool IsContain(byte playerId) =>
            this.firstStage.Contains(playerId) || this.finalStage.Contains(playerId);

        public bool IsFirstStage(byte playerId) =>
            this.firstStage.Contains(playerId);

        public bool IsFinalStage(byte playerId) =>
            this.finalStage.Contains(playerId);

        public void Update()
        {
            removeToHashSet(ref this.firstStage);
            removeToHashSet(ref this.finalStage);
        }

        private void removeToHashSet(ref HashSet<byte> cont)
        {
            List<byte> remove = new List<byte>();

            foreach (byte playerId in this.firstStage)
            {

                NetworkedPlayerInfo player = GameData.Instance.GetPlayerById(playerId);

                if (player == null ||
                    player.IsDead ||
                    player.Disconnected)
                {
                    remove.Add(playerId);
                }
            }
            foreach (byte playerId in remove)
            {
                cont.Remove(playerId);
            }
        }

    }

    public enum UmbrerMode : byte
    {
        Feat,
        Upgrage,
    }

    public enum UmbrerOption
    {
        Range,
        UpgradeVirusTime,
        InfectRange,
        KeepUpgradedVirus
    }

    public ExtremeAbilityButton Button
    {
        get => this.madmateAbilityButton;
        set
        {
            this.madmateAbilityButton = value;
        }
    }

    private ExtremeAbilityButton madmateAbilityButton;
    private InfectedContainer container;
    private PlayerControl tmpTarget;
    private PlayerControl target;
    private float range;
    private float infectRange;
    private float maxTimer;
    private bool isFetch = false;
    private Dictionary<byte, float> timer;
    private Dictionary<byte, PoolablePlayer> playerIcon;
    private GridArrange grid;

    public bool isUpgradeVirus;
    private GraphicAndActiveTimeSwitcher<UmbrerMode> mode;

    public Umbrer() : base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Umbrer,
			ColorPalette.UmbrerRed),
        false, false, false, false)
    { }

    public void CreateAbility()
    {
        var cate = this.Loader;
        var featVirusMode = new GraphicAndActiveTimeMode<UmbrerMode>(
			UmbrerMode.Feat,
				new ButtonGraphic(
					Tr.GetString("featVirus"),
					Resources.UnityObjectLoader.LoadSpriteFromResources(
						ObjectPath.UmbrerFeatVirus)),
				cate.GetValue<RoleAbilityCommonOption, float >(
					RoleAbilityCommonOption.AbilityActiveTime)
		);

        this.CreateNormalActivatingAbilityButton(
            featVirusMode.Graphic.Text, featVirusMode.Graphic.Img,
            IsAbilityCheck, CleanUp, ForceCleanUp);

        this.mode = new GraphicAndActiveTimeSwitcher<UmbrerMode>(
			this.Button.Behavior,
			featVirusMode,
			new(
				UmbrerMode.Upgrage,
				new ButtonGraphic(
					Tr.GetString("upgradeVirus"),
					Resources.UnityObjectLoader.LoadSpriteFromResources(
					ObjectPath.UmbrerUpgradeVirus)),
				cate.GetValue<UmbrerOption, float>(
					UmbrerOption.UpgradeVirusTime))
			);
    }

    public bool UseAbility()
    {
        this.target = this.tmpTarget;
        return true;
    }

    public void IntroBeginSetUp()
    {
        return;
    }

    public void IntroEndSetUp()
    {
        GameObject bottomLeft = new GameObject("BottomLeft");
        bottomLeft.transform.SetParent(
            HudManager.Instance.UseButton.transform.parent.parent);
        AspectPosition aspectPosition = bottomLeft.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        aspectPosition.anchorPoint = new Vector2(0.5f, 0.5f);
        aspectPosition.DistanceFromEdge = new Vector3(0.375f, 0.35f);
        aspectPosition.AdjustPosition();

        this.grid = bottomLeft.AddComponent<GridArrange>();
        this.grid.CellSize = new Vector2(0.5f, 0.75f);
        this.grid.MaxColumns = 14;
        this.grid.Alignment = GridArrange.StartAlign.Right;
        this.grid.cells = new();

        this.playerIcon = Helper.Player.CreatePlayerIcon(
            bottomLeft.transform, Vector3.one * 0.275f);
        this.updateShowIcon(true);
    }


    public bool IsAbilityCheck()
        => Helper.Player.IsPlayerInRangeAndDrawOutLine(
            PlayerControl.LocalPlayer,
            this.target, this, this.range);

    public void CleanUp()
    {
        if (this.container.IsFirstStage(this.target.PlayerId))
        {
            this.container.FinalStage.Add(this.target.PlayerId);
            this.timer.Add(this.target.PlayerId, this.maxTimer);
        }
        else
        {
            this.container.FirstStage.Add(this.target.PlayerId);
        }
        this.target = null;
    }

    public void ForceCleanUp()
    {
        this.target = null;
    }

    public bool IsAbilityUse()
    {
        this.tmpTarget = Helper.Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer, this, this.range);

        bool isUpgrade = this.container.IsFirstStage(
            this.tmpTarget == null ? byte.MaxValue : this.tmpTarget.PlayerId);

        this.mode.Switch(
            isUpgrade ? UmbrerMode.Upgrage : UmbrerMode.Feat);

        if (this.tmpTarget == null) { return false; }

        return IRoleAbility.IsCommonUse() && !this.container.IsFinalStage(this.tmpTarget.PlayerId);
    }

    public void ResetOnMeetingStart()
    {
        foreach (var (_, poolPlayer) in this.playerIcon)
        {
            poolPlayer.gameObject.SetActive(false);
        }
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

        if (!isFetch)
        {
            this.isFetch = true;
            this.container.Fetch(this);
        }

        if (this.container.IsAllPlayerInfected())
        {
            this.IsWin = true;
            ExtremeRolesPlugin.ShipState.RpcRoleIsWin(rolePlayer.PlayerId);
            return;
        }

        HashSet<byte> remove = new HashSet<byte> ();

            foreach (byte playerId in this.container.FinalStage)
            {
                this.timer[playerId] = this.timer[playerId] - Time.deltaTime;
                if (this.timer[playerId] <= 0.0f ||
                    isInfectOtherPlayer(
                        Helper.Player.GetPlayerControlById(playerId)))
                {
                    remove.Add(playerId);
                }
            }

        foreach (byte playerId in remove)
        {
            this.container.FinalStage.Remove(playerId);
        }

        this.container.Update();
        this.updateShowIcon();
    }

    public override bool IsSameTeam(SingleRoleBase targetRole)
    {
        if (this.Core.Id == targetRole.Core.Id)
        {
            if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
            {
                return true;
            }
            else
            {
                return this.IsSameControlId(targetRole);
            }
        }
        else
        {
            return base.IsSameTeam(targetRole);
        }
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateFloatOption(
            UmbrerOption.Range,
            1.0f, 0.1f, 4.0f, 0.1f);

        IRoleAbility.CreateCommonAbilityOption(
           factory, 3.0f);

        factory.CreateFloatOption(
            UmbrerOption.UpgradeVirusTime,
            3.5f, 0.5f, 10.0f, 0.1f,
            format: OptionUnit.Second);

        factory.CreateFloatOption(
            UmbrerOption.InfectRange,
            1.4f, 0.1f, 3.6f, 0.1f);

        factory.CreateFloatOption(
            UmbrerOption.KeepUpgradedVirus,
            10.0f, 2.5f, 360.0f, 0.5f,
            format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
        this.container = new InfectedContainer();

        this.timer = new Dictionary<byte, float>();
        this.playerIcon = new Dictionary<byte, PoolablePlayer>();

        var cate = this.Loader;

        this.range = cate.GetValue<UmbrerOption, float>(UmbrerOption.Range);
        this.infectRange = cate.GetValue<UmbrerOption, float>(UmbrerOption.InfectRange);
        this.maxTimer = cate.GetValue<UmbrerOption, float>(UmbrerOption.KeepUpgradedVirus);

        this.isFetch = false;
    }
    private bool isInfectOtherPlayer(PlayerControl sourcePlayer)
    {
        if (sourcePlayer == null) { return false; }
        Vector2 pos = sourcePlayer.GetTruePosition();
        byte sourcePlayerId = sourcePlayer.PlayerId;
        byte rolePlayerId = PlayerControl.LocalPlayer.PlayerId;

        foreach (NetworkedPlayerInfo playerInfo in
                GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            if (playerInfo == null) { continue; }

            if (!playerInfo.Disconnected &&
                !playerInfo.IsDead &&
                playerInfo.Object != null &&
                !playerInfo.Object.inVent &&
                (playerInfo.PlayerId != sourcePlayerId || playerInfo.PlayerId != rolePlayerId))
            {
                PlayerControl @object = playerInfo.Object;
                if (@object)
                {
                    Vector2 vector = @object.GetTruePosition() - pos;
                    float magnitude = vector.magnitude;
                    if (magnitude <= this.infectRange &&
                        !PhysicsHelpers.AnyNonTriggersBetween(
                            pos, vector.normalized,
                            magnitude, Constants.ShipAndObjectsMask))
                    {
                        this.container.FirstStage.Add(playerInfo.PlayerId);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void updateShowIcon(bool update = false)
    {
        foreach (var (playerId, poolPlayer) in this.playerIcon)
        {
            NetworkedPlayerInfo player = GameData.Instance.GetPlayerById(playerId);

            if (this.container.IsContain(playerId) ||
                player == null ||
                player.IsDead ||
                player.Disconnected)
            {
                poolPlayer.gameObject.SetActive(false);
                update = true;
            }
            else
            {
                poolPlayer.gameObject.SetActive(true);
            }
        }
        if (update)
        {
            this.grid.ArrangeChilds();
        }
    }
}
