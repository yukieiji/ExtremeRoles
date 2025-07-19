using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using AmongUs.GameOptions;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;




using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

public sealed class Psychic :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleAwake<RoleTypes>,
    IRoleReportHook
{

	public sealed class TeamCounter
	{
		private List<ExtremeRoleId> ids = new List<ExtremeRoleId>(GameData.Instance.AllPlayers.Count);
		private int countNum = 0;

		public TeamCounter() {}
		public TeamCounter(int countNum, List<ExtremeRoleId> ids)
		{
			this.countNum = countNum;
			this.ids = ids;
		}

		public void Add(ExtremeRoleId id)
		{
			++this.countNum;
			this.ids.Add(id);
		}

		public void AddToStringBuilder(StringBuilder builder, bool includeRoleId)
		{
			builder.AppendLine(
				Tr.GetString(
					"PsychicPsychicStrForAliveNum",
					this.countNum));

			if (includeRoleId && this.ids.Count != 0)
			{
				builder.AppendLine(Tr.GetString("PsychicPsychicStrAliveRole"));
				foreach (var roleId in this.ids)
				{
					var castedRoleId = (RoleTypes)roleId;
					string transKey =
						Enum.IsDefined(castedRoleId) ?
						castedRoleId.ToString() : roleId.ToString();

					builder.AppendLine(
						Tr.GetString(transKey));
				}
			}
		}

		public static TeamCounter Deserialize(MessageReader reader, bool includeRoleId)
		{
			int count = reader.ReadPackedInt32();

			List<ExtremeRoleId> ids = new List<ExtremeRoleId>();
			if (includeRoleId)
			{
				int readNum = reader.ReadPackedInt32();
				ids.Capacity = readNum;
				ids.Add((ExtremeRoleId)reader.ReadPackedInt32());
			}
			return new TeamCounter(count, ids);
		}

		public void Serialize(RPCOperator.RpcCaller caller, bool includeRoleId)
		{
			caller.WritePackedInt(this.countNum);

			if (!includeRoleId) { return; }

			caller.WritePackedInt(this.ids.Count);
			foreach (var id in this.ids)
			{
				caller.WritePackedInt((int)id);
			}
		}
	}

	public sealed class AlivePlayerCounter
	{
		private readonly IReadOnlyDictionary<ExtremeRoleType, TeamCounter> teamCount;

		public AlivePlayerCounter(Dictionary<ExtremeRoleType, TeamCounter> counts)
		{
			this.teamCount = counts;
		}

		public AlivePlayerCounter()
		{
			this.teamCount = new Dictionary<ExtremeRoleType, TeamCounter>()
			{
				{ ExtremeRoleType.Neutral, new TeamCounter() },
				{ ExtremeRoleType.Impostor, new TeamCounter() }
			};

			foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
			{
				if (player == null ||
					player.IsDead ||
					player.Disconnected ||
					!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) ||
					!this.teamCount.TryGetValue(role!.Core.Team, out TeamCounter? counter) ||
					counter is null)
				{
					continue;
				}

				ExtremeRoleId id = role!.Core.Id;
				if (role is MultiAssignRoleBase multiRole &&
					multiRole.AnotherRole != null)
				{
					if (role!.IsNeutral() &&
						multiRole.AnotherRole.Core.Id == ExtremeRoleId.Servant)
					{
						id = ExtremeRoleId.Servant;
					}
					else if (role!.IsVanillaRole())
					{
						id = multiRole.AnotherRole.Core.Id;
					}
				}
				else if (role is VanillaRoleWrapper vanillaRole)
				{
					id = (ExtremeRoleId)vanillaRole.VanilaRoleId;
				}

				counter.Add(id);
			}
		}

		public string ToString(bool includeRoleId)
		{
			var builder = new StringBuilder();
			foreach (var (team, counter) in this.teamCount)
			{
				builder.Append($"{Tr.GetString("PsychicPsychicStrTeam")}{Tr.GetString(team.ToString())}");
				counter.AddToStringBuilder(builder, includeRoleId);
			}
			return builder.ToString();
		}

		public void Serialize(RPCOperator.RpcCaller caller, bool includeRoleId)
		{
			foreach (var count in this.teamCount.Values)
			{
				count.Serialize(caller, includeRoleId);
			}
		}

		public static AlivePlayerCounter Deserialize(MessageReader reader, bool includeRoleId)
		{
			var result = new Dictionary<ExtremeRoleType, TeamCounter>(2);
			foreach (var team in
				new ExtremeRoleType[] { ExtremeRoleType.Impostor, ExtremeRoleType.Neutral })
			{
				result.Add(team, TeamCounter.Deserialize(reader, includeRoleId));
			}
			return new AlivePlayerCounter(result);
		}
	}

    public enum PsychicOption
    {
        AwakeTaskGage,
		AwakeDeadPlayerNum,
        IsUpgradeAbility,
		UpgradeTaskGage,
		UpgradeDeadPlayerNum,
	}
    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.awakeRole;
        }
    }

    public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;
    private bool awakeRole;
    private float awakeTaskGage;
	private int awakeDeadPlayerNum;

    private bool awakeHasOtherVision;

	private bool isUpgraded;
	private bool enableUpgrade;
	private float upgradeTaskGage;
	private int upgradeDeadPlayerNum;

	private List<AlivePlayerCounter>? counters;
	private Vector2? startPos;
	private TextPopUpper? popUpper;

	private const string splitter = "------------";

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }

	public Psychic() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Psychic,
			ColorPalette.PsychicSyentyietu),
        false, true, false, false)
    { }
#pragma warning restore CS8618

	public void CreateAbility()
    {
        this.CreateActivatingAbilityCountButton(
			Tr.GetString("PsychicPsychic"),
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.PsychicPsychic),
			CheckAbility,
			CleanUp,
			ForceAbilityOff);
        this.Button.SetLabelToCrewmate();

		if (this.Button?.Behavior is ICountBehavior behavior)
		{
			this.counters = new List<AlivePlayerCounter>(behavior.AbilityCount);
		}
	}

	public void ForceAbilityOff()
	{
		this.startPos = null;
	}

	public void CleanUp()
	{
		this.ForceAbilityOff();
		this.counters?.Add(new AlivePlayerCounter());

		this.popUpper?.AddText(Tr.GetString("PsychicPsychicEnd"));
	}

	public bool CheckAbility()
		=> this.startPos == PlayerControl.LocalPlayer.GetTruePosition() &&
		IRoleAbility.IsCommonUse();

	public bool UseAbility()
	{
		this.startPos = PlayerControl.LocalPlayer.GetTruePosition();
		this.popUpper?.AddText(Tr.GetString("PsychicPsychicStart"));

		return true;
	}

    public bool IsAbilityUse()
        => this.IsAwake && IRoleAbility.IsCommonUse();

    public string GetFakeOptionString() => "";

    public void HookReportButton(
        PlayerControl rolePlayer, NetworkedPlayerInfo reporter)
    {
        sendPhotoInfo();
    }

    public void HookBodyReport(
        PlayerControl rolePlayer,
        NetworkedPlayerInfo reporter,
        NetworkedPlayerInfo reportBody)
    {
        sendPhotoInfo();
    }

    public void ResetOnMeetingStart()
    {
		this.popUpper?.Clear();
	}

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
		this.counters?.Clear();
    }

    public void Update(PlayerControl rolePlayer)
    {
        float taskGage = Player.GetPlayerTaskGage(rolePlayer);
		int deadPlayerNum = 0;

		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (player == null || player.IsDead || player.Disconnected)
			{
				++deadPlayerNum;
			}
		}

        if (!this.awakeRole)
        {
            if (taskGage >= this.awakeTaskGage &&
				deadPlayerNum >= this.awakeDeadPlayerNum &&
				!this.awakeRole)
            {
                this.awakeRole = true;
                this.HasOtherVision = this.awakeHasOtherVision;
                this.Button.SetButtonShow(true);
            }
            else
            {
                this.Button.SetButtonShow(false);
            }
        }

		this.isUpgraded =
			this.awakeRole && this.enableUpgrade &&
			this.upgradeTaskGage <= taskGage && this.upgradeDeadPlayerNum <= deadPlayerNum;
	}

    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetColoredRoleName();
        }
        else
        {
            return Design.ColoedString(
                Palette.White, Tr.GetString(RoleTypes.Crewmate.ToString()));
        }
    }
    public override string GetFullDescription()
    {
        if (IsAwake)
        {
            return Tr.GetString(
                $"{this.Core.Id}FullDescription");
        }
        else
        {
            return Tr.GetString(
                $"{RoleTypes.Crewmate}FullDescription");
        }
    }

    public override string GetImportantText(bool isContainFakeTask = true)
    {
        if (IsAwake)
        {
            return base.GetImportantText(isContainFakeTask);

        }
        else
        {
            return Design.ColoedString(
                Palette.White,
                $"{this.GetColoredRoleName()}: {Tr.GetString("crewImportantText")}");
        }
    }

    public override string GetIntroDescription()
    {
        if (IsAwake)
        {
            return base.GetIntroDescription();
        }
        else
        {
            return Design.ColoedString(
                Palette.CrewmateBlue,
                PlayerControl.LocalPlayer.Data.Role.Blurb);
        }
    }

    public override Color GetNameColor(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetNameColor(isTruthColor);
        }
        else
        {
            return Palette.White;
        }
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            PsychicOption.AwakeTaskGage,
            30, 0, 100, 10,
            format: OptionUnit.Percentage);
		factory.CreateIntOption(
		   PsychicOption.AwakeDeadPlayerNum,
		   2, 0, 7, 1);

        IRoleAbility.CreateAbilityCountOption(
            factory, 1, 5, 3.0f);

		var isUpgradeOpt = factory.CreateBoolOption(
			PsychicOption.IsUpgradeAbility,
			false);
		factory.CreateIntOption(
			PsychicOption.UpgradeTaskGage,
			70, 0, 100, 10,
			isUpgradeOpt,
			format: OptionUnit.Percentage);
		factory.CreateIntOption(
		   PsychicOption.UpgradeDeadPlayerNum,
		   5, 0, 15, 1, isUpgradeOpt);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		this.awakeTaskGage = loader.GetValue<PsychicOption, int>(
			PsychicOption.AwakeTaskGage) / 100.0f;
		this.awakeDeadPlayerNum = loader.GetValue<PsychicOption, int>(
			PsychicOption.AwakeDeadPlayerNum);

		this.upgradeTaskGage = loader.GetValue<PsychicOption, int>(
			PsychicOption.UpgradeTaskGage) / 100.0f;
		this.upgradeDeadPlayerNum = loader.GetValue<PsychicOption, int>(
			PsychicOption.UpgradeDeadPlayerNum);
		this.enableUpgrade = loader.GetValue<PsychicOption, bool>(
			PsychicOption.IsUpgradeAbility);

		int maxPlayerNum = PlayerCache.AllPlayerControl.Count - 1;

		this.awakeDeadPlayerNum = Mathf.Clamp(
			this.awakeDeadPlayerNum, 0, maxPlayerNum);
		this.awakeHasOtherVision = this.HasOtherVision;

		this.upgradeDeadPlayerNum = Mathf.Clamp(
			this.upgradeDeadPlayerNum, 0, maxPlayerNum);

		if (this.awakeTaskGage <= 0.0f && this.awakeDeadPlayerNum == 0)
		{
			this.awakeRole = true;
			this.HasOtherVision = this.awakeHasOtherVision;
		}
		else
		{
			this.awakeRole = false;
			this.HasOtherVision = false;
		}

		this.popUpper = new TextPopUpper(
			3, 2.5f, new Vector3(-3.75f, -2.5f, -250.0f),
			TMPro.TextAlignmentOptions.BottomLeft);
	}

    private void sendPhotoInfo()
    {
        if (!this.IsAwake || this.counters is null || this.counters.Count == 0) { return; }


		StringBuilder builder = new StringBuilder(
			Tr.GetString("PsychicPsychicResult"));
		builder.AppendLine();
		foreach (var counter in this.counters)
		{
			string text = counter.ToString(this.isUpgraded);

			if (string.IsNullOrEmpty(text)) { continue; }

			builder
				.AppendLine(splitter)
				.Append(text)
				.AppendLine(splitter);
		}
		string chatText = builder.ToString();

		HudManager hud = HudManager.Instance;

		if (chatText == string.Empty ||
			!AmongUsClient.Instance.AmClient ||
			hud == null) { return; }

        MeetingReporter.Instance.AddMeetingChatReport(chatText);
    }
}
