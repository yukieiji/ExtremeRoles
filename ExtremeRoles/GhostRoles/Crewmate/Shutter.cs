using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;



using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;

using static ExtremeRoles.Roles.Solo.Crewmate.Photographer;
using ExtremeRoles.Module.Ability.Factory;


#nullable enable

namespace ExtremeRoles.GhostRoles.Crewmate;

public sealed class Shutter : GhostRoleBase
{
	public sealed class GhostPhotoSerializer : IStringSerializer
	{
		public StringSerializerType Type => StringSerializerType.ShutterPhoto;

		public bool IsRpc { get; set; } = true;

		private IReadOnlyList<(PlayerPosInfo, bool)> player;
		private DateTime takeTime;
		private PhotoNameGenerator photoName;

		public GhostPhotoSerializer()
		{
			this.player = new List<(PlayerPosInfo, bool)>();
		}

		public GhostPhotoSerializer(in int rate, in float range)
        {
			this.photoName = PhotoNameGenerator.Create();
			this.takeTime = DateTime.UtcNow;
            var playerPoses = new List<(PlayerPosInfo, bool)>();

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
					playerPoses.Add((
						new PlayerPosInfo(player),
						rate < RandomGenerator.Instance.Next(101)));
                }
            }
			this.player = playerPoses;
        }

		public void Deserialize(MessageReader reader)
		{
			this.photoName = PhotoNameGenerator.Deserialize(reader);

			ulong ulongBinary = reader.ReadUInt64();
			this.takeTime = DateTime.FromBinary(
				unchecked((long)ulongBinary + long.MinValue));

			int playerNum = reader.ReadPackedInt32();
			var playerPos = new List<(PlayerPosInfo, bool)>(playerNum);
			for (int i = 0; i < playerNum; ++i)
			{
				bool isSmokey = reader.ReadBoolean();
				var playerInfo = PlayerPosInfo.Deserialize(reader);
				playerPos.Add((playerInfo, isSmokey));
			}
			this.player = playerPos;
		}

		public void Serialize(RPCOperator.RpcCaller caller)
		{
			this.photoName.Serialize(caller);

			long binary = this.takeTime.ToBinary();
			caller.WriteUlong(unchecked((ulong)(binary - long.MinValue)));

			caller.WritePackedInt(this.player.Count);
			foreach (var (player, isSmokey) in this.player)
			{
				caller.WriteBoolean(isSmokey);
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
            if (this.player.Count <= 0)
            {
                return string.Empty;
            }
            else
            {
                foreach (var (playerInfo, isSmokey) in this.player)
                {
                    string addInfo =
						isSmokey ?
                        Tr.GetString("smokingThisName") :
                        playerInfo.PlayerName;

                    photoInfoBuilder.AppendLine(addInfo);
                }
            }
            return photoInfoBuilder.ToString().Trim('\r', '\n');
        }
    }

	private sealed class GhostPhotoCamera
	{
		public IReadOnlyList<GhostPhotoSerializer> AllPhoto => this.film;

		private readonly float range;
		private readonly int rate;
		private readonly List<GhostPhotoSerializer> film =
			new List<GhostPhotoSerializer>();

		public GhostPhotoCamera(float range, int rate)
		{
			this.range = range;
			this.rate = rate;

			this.film.Clear();
		}
		public void Reset()
		{
			this.film.Clear();
		}

		public void TakePhoto()
		{
			this.film.Add(
				new GhostPhotoSerializer(this.rate, this.range));
		}
	}

    public enum ShutterRpcOps : byte
    {
        TakePhoto,
        SharePhoto
    }

    public enum ShutterOption
    {
        PhotoRange,
        RightPlayerNameRate
    }

    private readonly FullScreenFlasher flasher = new FullScreenFlasher(Color.white, 0.75f, 0.25f, 0.5f, 0.25f);
#pragma warning disable CS8618
	private GhostPhotoCamera photoCreater;
	public Shutter() : base(
        true,
        ExtremeRoleType.Crewmate,
        ExtremeGhostRoleId.Shutter,
        ExtremeGhostRoleId.Shutter.ToString(),
        ColorPalette.PhotographerVerdeSiena)
    { }
#pragma warning restore CS8618

	public override void CreateAbility()
    {
        this.Button = GhostRoleAbilityFactory.CreateCountAbility(
            AbilityType.ShutterTakePhoto,
            Resources.UnityObjectLoader.LoadSpriteFromResources(
                Resources.ObjectPath.PhotographerPhotoCamera),
            this.IsReportAbility(),
            () => true,
            this.isAbilityUse,
            this.UseAbility,
            null, true,
            null, null,
            KeyCode.F);
        this.ButtonInit();
        this.Button.SetLabelToCrewmate();
    }

    public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Photographer
    };

    public override void Initialize()
    {
		var loader = this.Loader;

        this.photoCreater = new GhostPhotoCamera(
			loader.GetValue<ShutterOption, float>(ShutterOption.PhotoRange),
			loader.GetValue<ShutterOption, int>(ShutterOption.RightPlayerNameRate));
    }

    protected override void OnMeetingEndHook()
    {
        return;
    }

	protected override void OnMeetingStartHook()
	{
		this.flasher.Hide();
		foreach (var photo in this.photoCreater.AllPhoto)
		{
			photo.IsRpc = true;
			MeetingReporter.Instance.AddMeetingChatReport(photo);
		}
	}

    protected override void CreateSpecificOption(OptionFactory factory)
    {
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 3, 10);
		factory.CreateFloatOption(
            ShutterOption.PhotoRange,
            7.5f, 0.5f, 25f, 0.5f);

		factory.CreateIntOption(
            ShutterOption.RightPlayerNameRate,
            50, 25, 100, 5,
            format: OptionUnit.Percentage);
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {
        this.photoCreater.TakePhoto();
        flasher.Flash();
    }

    private bool isAbilityUse() => IsCommonUse();
}
