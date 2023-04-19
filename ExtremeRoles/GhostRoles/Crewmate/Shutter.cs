using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityFactory;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using static ExtremeRoles.Roles.Solo.Crewmate.Photographer;

namespace ExtremeRoles.GhostRoles.Crewmate;

public sealed class Shutter : GhostRoleBase
{
    private struct GhostPhoto
    {
        private List<PlayerPosInfo> player;
        private DateTime takeTime;
        private int rate;

        public GhostPhoto(int rate, float range, ContactFilter2D filter)
        {
            this.rate = rate;
            this.takeTime = DateTime.UtcNow;
            this.player = new List<PlayerPosInfo>();

            Vector3 photoCenter = CachedPlayerControl.LocalPlayer.PlayerControl.transform.position;

            foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (player == null ||
                    player.IsDead ||
                    player.Disconnected ||
                    player.Object == null) { continue; }

                Vector3 position = player.Object.transform.position;
                if (range >= Vector2.Distance(photoCenter, position))
                {
                    this.player.Add(new PlayerPosInfo(player, filter));
                }

            }
        }

        public override string ToString()
        {
            StringBuilder photoInfoBuilder = new StringBuilder();
            photoInfoBuilder.AppendLine(
                $"{Translation.GetString("takePhotoTime")} : {this.takeTime}");
            photoInfoBuilder.AppendLine(
                $"{Translation.GetString("photoName")} : {
                    Photo.GetRandomPhotoName()}");
            photoInfoBuilder.AppendLine("");
            if (this.player.Count <= 0)
            {
                return string.Empty;
            }
            else
            {
                foreach (PlayerPosInfo playerInfo in this.player)
                {
                    string addInfo =
                        this.rate < RandomGenerator.Instance.Next(100) ?
                        Translation.GetString("smokingThisName") : 
                        playerInfo.PlayerName;

                    photoInfoBuilder.AppendLine(addInfo);
                }
            }
            return photoInfoBuilder.ToString().Trim('\r', '\n');
        }
    }

    private sealed class GhostPhotoCamera
    {
        public bool IsUpgraded = false;

        private const string separateLine = "---------------------------------";
        private float range;
        private int rate;
        private List<GhostPhoto> film = new List<GhostPhoto>();
        private ContactFilter2D filter;

        public GhostPhotoCamera(float range, int rate)
        {
            this.range = range;
            this.rate = rate;

            this.filter = default(ContactFilter2D);
            this.filter.layerMask = Constants.PlayersOnlyMask;
            this.filter.useLayerMask = true;
            this.filter.useTriggers = false;

            this.film.Clear();
        }
        public void Reset()
        {
            this.film.Clear();
        }

        public void TakePhoto()
        {
            this.film.Add(new GhostPhoto(this.rate, this.range, this.filter));
        }
        public override string ToString()
        {
            if (this.film.Count == 0) { return string.Empty; }

            StringBuilder builder = new StringBuilder();

            foreach (GhostPhoto photo in this.film)
            {
                string photoInfo = photo.ToString();

                if (string.IsNullOrEmpty(photoInfo)) { continue; }

                builder.AppendLine(separateLine);
                builder.AppendLine(photoInfo);
                builder.AppendLine(separateLine);
            }

            return builder.ToString().Trim('\r', '\n');
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

    private GhostPhotoCamera photoCreater;
    private SpriteRenderer flash;

    public Shutter() : base(
        true,
        ExtremeRoleType.Crewmate,
        ExtremeGhostRoleId.Shutter,
        ExtremeGhostRoleId.Shutter.ToString(),
        ColorPalette.PhotographerVerdeSiena)
    { }

    public override void CreateAbility()
    {
        this.Button = GhostRoleAbilityFactory.CreateCountAbility(
            AbilityType.ShutterTakePhoto,
            Resources.Loader.CreateSpriteFromResources(
                Resources.Path.PhotographerPhotoCamera),
            this.isReportAbility(),
            () => true,
            this.isAbilityUse,
            this.UseAbility,
            null, true,
            null, null, null,
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
        this.photoCreater = new GhostPhotoCamera(
            (float)OptionHolder.AllOption[
                GetRoleOptionId(ShutterOption.PhotoRange)].GetValue(),
            (int)OptionHolder.AllOption[
                GetRoleOptionId(ShutterOption.RightPlayerNameRate)].GetValue());
    }

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {
        string photoInfo = this.photoCreater.ToString();

        if (string.IsNullOrEmpty(photoInfo)) { return; }

        MeetingReporter.RpcAddMeetingChatReport(
            string.Format(
                Translation.GetString("ShutterTakePhotoReport"),
                photoInfo));
    }

    protected override void CreateSpecificOption(
        IOption parentOps)
    {
        CreateFloatOption(
            ShutterOption.PhotoRange,
            5.0f, 2.5f, 25f, 0.5f,
            parentOps);
        CreateIntOption(
            ShutterOption.RightPlayerNameRate,
            50, 25, 100, 5,
            parentOps,
            format: OptionUnit.Percentage);

        CreateCountButtonOption(
            parentOps, 3, 10);
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {
        var hudManager = FastDestroyableSingleton<HudManager>.Instance;

        if (this.flash == null)
        {
            this.flash = UnityEngine.Object.Instantiate(
                 hudManager.FullScreen,
                 hudManager.transform);
            this.flash.transform.localPosition = new Vector3(0f, 0f, 20f);
            this.flash.gameObject.SetActive(true);
        }

        this.flash.enabled = true;

        this.photoCreater.TakePhoto();

        hudManager.StartCoroutine(
            Effects.Lerp(FlashTime, new Action<float>((p) =>
            {
                if (this.flash == null) { return; }
                if (p < 0.25f)
                {
                    this.flash.color = new Color(
                        255f, 255f, 255f, Mathf.Clamp01(p * 5 * 0.75f));

                }
                else if (p >= 0.5f)
                {
                    this.flash.color = new Color(
                        255f, 255f, 255f, Mathf.Clamp01((1 - p) * 5 * 0.75f));
                }
                if (p == FlashTime)
                {
                    this.flash.enabled = false;
                }
            }))
        );
    }

    private bool isAbilityUse() => this.IsCommonUse();
}
