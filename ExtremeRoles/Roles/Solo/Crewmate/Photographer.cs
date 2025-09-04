using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Hazel;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.Ability;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

public sealed class Photographer :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleAwake<RoleTypes>,
    IRoleReportHook
{
    public readonly struct PlayerPosInfo
    {
		public readonly string PlayerName;
		public readonly SystemTypes? Room;
		private readonly byte playerId;

		public PlayerPosInfo(
            NetworkedPlayerInfo player)
        {
			this.playerId = player.PlayerId;
            this.PlayerName = player.PlayerName;
			Player.TryGetPlayerRoom(player.Object, out var room);
			this.Room = room;
		}
		public PlayerPosInfo(NetworkedPlayerInfo player, SystemTypes? room)
		{
			this.playerId = player.PlayerId;
			this.PlayerName = player.PlayerName;
			this.Room = room;
		}

		public static PlayerPosInfo Deserialize(MessageReader reader)
		{
			byte playerId = reader.ReadByte();
			SystemTypes? room = (SystemTypes)reader.ReadByte();
			var playerInfo = GameData.Instance.GetPlayerById(playerId);

			if (room is SystemTypes.HeliSabotage)
			{
				room = null;
			}
			return new PlayerPosInfo(playerInfo, room);
		}

		public void Serialize(RPCOperator.RpcCaller caller)
		{
			caller.WritePackedInt(this.playerId);

			// 取り敢えずNullってる場合はヘリサボを突っ込む
			SystemTypes room = this.Room.HasValue ? this.Room.Value : SystemTypes.HeliSabotage;
			caller.WriteByte((byte)room);
		}

	}

	public readonly record struct PhotoNameGenerator(
		ExtremeRoleType TeamId, ExtremeRoleId RoleId, byte Indexer)
	{
		public override string ToString()
		{
			string roleNameTransKey =
				Enum.IsDefined((RoleTypes)this.RoleId) ?
				((RoleTypes)this.RoleId).ToString() : this.RoleId.ToString();

			// 適当な役職名とかを写真名にする
			string[] photoNameArr =
			[
				Tr.GetString(roleNameTransKey),
				Tr.GetString(this.TeamId.ToString()),
				randomStr[this.Indexer],
			];
			return string.Concat(photoNameArr.OrderBy(
				item => RandomGenerator.Instance.Next()));
		}

		public static PhotoNameGenerator Create()
		{
			int maxTeamId = Enum.GetValues(typeof(ExtremeRoleType)).Cast<int>().Max();
			ExtremeRoleType teamId = (ExtremeRoleType)RandomGenerator.Instance.Next(
				-1, maxTeamId + 1);
			int maxRoleId = Enum.GetValues(typeof(ExtremeRoleId)).Cast<int>().Max();
			ExtremeRoleId roleId = (ExtremeRoleId)RandomGenerator.Instance.Next(
				maxRoleId + 1);

			int indexer = RandomGenerator.Instance.Next(randomStr.Length);

			return new PhotoNameGenerator(teamId, roleId, (byte)indexer);
		}

		public static PhotoNameGenerator Deserialize(MessageReader reader)
		{
			ExtremeRoleType teamId = (ExtremeRoleType)reader.ReadPackedInt32();
			ExtremeRoleId roleId = (ExtremeRoleId)reader.ReadPackedInt32();
			byte indexer = reader.ReadByte();

			return new PhotoNameGenerator(teamId, roleId, indexer);
		}

		public void Serialize(RPCOperator.RpcCaller caller)
		{
			caller.WritePackedInt((int)this.TeamId);
			caller.WritePackedInt((int)this.RoleId);
			caller.WriteByte(this.Indexer);
		}

		private static readonly string[] randomStr =
		[
			"NoName",
			"NewFile",
			"NewPhoto",
			"sudo",
			"HelloWorld",
			"AmongUs",
			"yukieiji",
			"ExtremeRoles",
			"ExtremeSkins",
			"ExtremeHat",
			"ExtremeVisor",
			"ExtremeNamePlate",
			"ExR", "ExS","ExV",
			"ExH", "ExN"
		];
	}

	public sealed class PhotoSerializer : IStringSerializer
	{
		public StringSerializerType Type => StringSerializerType.PhotographerPhoto;

		public bool IsRpc { get; set; }

		private bool isUpgrade = false;
		private PhotoNameGenerator photoName;
		private IReadOnlyList<PlayerPosInfo> player;
		private DateTime takeTime;

		public PhotoSerializer(float range, bool isUpgrade)
		{
			this.isUpgrade = isUpgrade;
			this.photoName = PhotoNameGenerator.Create();
			this.takeTime = DateTime.UtcNow;

			var playerInfo = new List<PlayerPosInfo>(GameData.Instance.AllPlayers.Count);

			Vector3 photoCenter = PlayerControl.LocalPlayer.transform.position;

			foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
			{
				if (player == null ||
					player.IsDead ||
					player.Disconnected ||
					player.Object == null) { continue; }

				Vector3 position = player.Object.transform.position;
				if (range >= Vector2.Distance(photoCenter, position))
				{
					playerInfo.Add(new PlayerPosInfo(player));
				}
			}
			this.player = playerInfo;
		}

		public PhotoSerializer()
		{
			IsRpc = false;
			this.player = new List<PlayerPosInfo>();
		}

		public void Deserialize(MessageReader reader)
		{
			this.photoName = PhotoNameGenerator.Deserialize(reader);

			ulong ulongBinary = reader.ReadUInt64();
			this.takeTime = DateTime.FromBinary(
				unchecked((long)ulongBinary + long.MinValue));

			this.isUpgrade = reader.ReadBoolean();

			int playerNum = reader.ReadPackedInt32();
			var playerPos = new List<PlayerPosInfo>(playerNum);
			for (int i = 0; i < playerNum; ++i)
			{
				playerPos.Add(
					PlayerPosInfo.Deserialize(reader));
			}
			this.player = playerPos;
		}

		public void Serialize(RPCOperator.RpcCaller caller)
		{
			this.photoName.Serialize(caller);

			long binary = this.takeTime.ToBinary();
			caller.WriteUlong(unchecked((ulong)(binary - long.MinValue)));

			caller.WriteBoolean(this.isUpgrade);
			caller.WritePackedInt(this.player.Count);

			foreach (var player in this.player)
			{
				player.Serialize(caller);
			}
		}

		public override string ToString()
		{
			StringBuilder photoInfoBuilder = new StringBuilder();
			photoInfoBuilder.AppendLine(
				$"{Tr.GetString("takePhotoTime")} : {this.takeTime}");
			photoInfoBuilder.AppendLine(
				$"{Tr.GetString("photoName")} : {this.photoName.ToString()}");
			photoInfoBuilder.AppendLine("");
			if (this.player.Count <= 1)
			{
				photoInfoBuilder.AppendLine(
					Tr.GetString("onlyMeOnPhoto"));
			}
			else
			{
				foreach (PlayerPosInfo playerInfo in this.player)
				{
					string roomInfo = string.Empty;

					if (this.isUpgrade && playerInfo.Room != null)
					{
						roomInfo =
							TranslationController.Instance.GetString(
								playerInfo.Room.Value);
					}

					string photoPlayerInfo =
						roomInfo == string.Empty ?
						$"{playerInfo.PlayerName}" :
						$"{playerInfo.PlayerName}   {roomInfo}";

					photoInfoBuilder.AppendLine(photoPlayerInfo);
				}
			}
			return photoInfoBuilder.ToString().Trim('\r', '\n');
		}
	}


	private sealed class PhotoCamera
    {
        public bool IsUpgraded { get; set; } = false;
		public IReadOnlyList<PhotoSerializer> AllPhoto => this.film;

        private readonly float range;
        private readonly List<PhotoSerializer> film = new List<PhotoSerializer>();

        public PhotoCamera(in float range)
        {
            this.range = range;
            this.film.Clear();
        }
        public void Reset()
        {
            this.film.Clear();
        }

        public void TakePhoto()
        {
            this.film.Add(new PhotoSerializer(this.range, this.IsUpgraded));
        }
    }

    public enum PhotographerOption
    {
        AwakeTaskGage,
        UpgradePhotoTaskGage,
        EnableAllSendChat,
        UpgradeAllSendChatTaskGage,
        PhotoRange,
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
    private bool awakeHasOtherVision;

    private float upgradePhotoTaskGage;
    private bool enableAllSend;
    private float upgradeAllSendChatTaskGage;
    private bool isUpgradeChat;

    private readonly FullScreenFlasher flasher = new FullScreenFlasher(Color.white, 0.75f, 0.25f, 0.5f, 0.25f);
    public const float FlashTime = 1.0f;

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }
	private PhotoCamera photoCreater;
	public Photographer() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Photographer,
			ColorPalette.PhotographerVerdeSiena),
        false, true, false, false)
    { }
#pragma warning restore CS8618

	public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "takePhoto",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.PhotographerPhotoCamera));
        this.Button.SetLabelToCrewmate();
    }

    public bool UseAbility()
    {
        this.photoCreater.TakePhoto();
        flasher.Flash();
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
		this.flasher.Hide();
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        this.photoCreater.Reset();
    }

    public void Update(PlayerControl rolePlayer)
    {
        float taskGage = Player.GetPlayerTaskGage(rolePlayer);

        if (!this.awakeRole)
        {
            if (taskGage >= this.awakeTaskGage && !this.awakeRole)
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

        if (taskGage >= this.upgradePhotoTaskGage &&
            !this.photoCreater.IsUpgraded)
        {
            this.photoCreater.IsUpgraded = true;
        }
        if (this.enableAllSend &&
            taskGage >= this.upgradeAllSendChatTaskGage &&
            !this.isUpgradeChat)
        {
            this.isUpgradeChat = true;
        }
    }

    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetColoredRoleName();
        }
        else
        {
            return Design.ColoredString(
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
            return Design.ColoredString(
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
            return Design.ColoredString(
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
            PhotographerOption.AwakeTaskGage,
            30, 0, 100, 10,
            format: OptionUnit.Percentage);

        factory.CreateIntOption(
            PhotographerOption.UpgradePhotoTaskGage,
            60, 0, 100, 10,
            format: OptionUnit.Percentage);

        var chatUpgradeOpt = factory.CreateBoolOption(
            PhotographerOption.EnableAllSendChat,
            false);

        factory.CreateIntOption(
            PhotographerOption.UpgradeAllSendChatTaskGage,
            80, 0, 100, 10,
            chatUpgradeOpt,
            format: OptionUnit.Percentage);

        factory.CreateFloatOption(
            PhotographerOption.PhotoRange,
            10.0f, 2.5f, 50f, 0.5f);

        IRoleAbility.CreateAbilityCountOption(
            factory, 5, 10);
    }

    protected override void RoleSpecificInit()
    {
        var loader = this.Loader;

        this.awakeTaskGage = loader.GetValue<PhotographerOption, int>(
            PhotographerOption.AwakeTaskGage) / 100.0f;
        this.upgradePhotoTaskGage = loader.GetValue<PhotographerOption, int>(
            PhotographerOption.UpgradePhotoTaskGage) / 100.0f;
		this.enableAllSend =
			loader.GetValue<PhotographerOption, bool>(
				PhotographerOption.EnableAllSendChat);
        this.upgradeAllSendChatTaskGage = loader.GetValue<PhotographerOption, int>(
            PhotographerOption.UpgradeAllSendChatTaskGage) / 100.0f;

        this.awakeHasOtherVision = this.HasOtherVision;

        if (this.awakeTaskGage <= 0.0f)
        {
            this.awakeRole = true;
            this.HasOtherVision = this.awakeHasOtherVision;
        }
        else
        {
            this.awakeRole = false;
            this.HasOtherVision = false;
        }

        this.isUpgradeChat = this.enableAllSend &&
            this.upgradeAllSendChatTaskGage <= 0.0f;

        this.photoCreater = new PhotoCamera(
			loader.GetValue<PhotographerOption, float>(
                PhotographerOption.PhotoRange));

        this.photoCreater.IsUpgraded = this.upgradePhotoTaskGage <= 0.0f;
    }

    private void sendPhotoInfo()
    {
        if (!this.IsAwake) { return; }

		foreach (var photo in this.photoCreater.AllPhoto)
		{
			photo.IsRpc = this.enableAllSend && this.isUpgradeChat;
			MeetingReporter.Instance.AddMeetingChatReport(photo);
		}
    }
}
