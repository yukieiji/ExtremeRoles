using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Video;
using Hazel;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;

using Il2CppObject = Il2CppSystem.Object;
using SystemArray = System.Array;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Zombie :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleAwake<RoleTypes>,
    IRoleOnRevive
{
    public override bool IsAssignGhostRole
    {
        get => false;
    }

    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.awakeRole;
        }
    }

    public RoleTypes NoneAwakeRole => RoleTypes.Impostor;

    public ExtremeAbilityButton Button { get; set; }

    public enum ZombieOption
    {
        AwakeKillCount,
        ResurrectKillCount,
        ShowMagicCircleTime,
        ResurrectDelayTime,
        CanResurrectOnExil,
    }

    public enum ZombieRpcOps : byte
    {
        UseResurrect,
        SetMagicCircle
    }

    private bool awakeRole;
    private bool awakeHasOtherVision;
    private int awakeKillCount;
    private int resurrectKillCount;

    private int killCount;

    private bool canResurrect;
    private bool canResurrectOnExil;
    private bool isResurrected;

    private bool activateResurrectTimer;
    private float resurrectTimer;
    private float showMagicCircleTime;

    private Vector3 curPos;

    private TMPro.TextMeshPro resurrectText;
    private Dictionary<SystemTypes, Arrow> setRooms;
    private SystemTypes targetRoom;

    private Collider2D cachedColider = null;

    public Zombie() : base(
        ExtremeRoleId.Zombie,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Zombie.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

    public static void RpcAbility(ref MessageReader reader)
    {
        ZombieRpcOps ops = (ZombieRpcOps)reader.ReadByte();
        byte zombiePlayerId = reader.ReadByte();

        switch (ops)
        {
            case ZombieRpcOps.UseResurrect:
                Zombie zombie = ExtremeRoleManager.GetSafeCastedRole<Zombie>(
                    zombiePlayerId);
                if (zombie == null) { return; }
                UseResurrect(zombie);
                break;
            case ZombieRpcOps.SetMagicCircle:
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float timer = reader.ReadSingle();
                setMagicCircle(new Vector2(x, y), timer);
                break;
            default:
                break;
        }
    }

    public static void UseResurrect(Zombie zombie)
    {
        zombie.isResurrected = true;
        zombie.activateResurrectTimer = false;
    }
    private static void setMagicCircle(Vector2 pos, float activeTime)
    {
        GameObject circle = new GameObject("MagicCircle");
        circle.SetActive(true);
        circle.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

        var player = circle.AddComponent<DlayableVideoPlayer>();

		var thum = UnityObjectLoader.LoadFromResources(
			ExtremeRoleId.Zombie,
			ObjectPath.GetRoleImgPath(ExtremeRoleId.Zombie, ObjectPath.MapIcon));
		player.SetThum(thum);

		var video = UnityObjectLoader.LoadFromResources<VideoClip, ExtremeRoleId>(
			ExtremeRoleId.Zombie,
			ObjectPath.GetRoleVideoPath(ExtremeRoleId.Zombie));
		player.SetVideo(video);

        player.SetTimer(activeTime);
    }

    public void CreateAbility()
    {
        this.CreateActivatingAbilityCountButton(
            Tr.GetString("featMagicCircle"),
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Zombie),
            IsActivate,
            SetMagicCircle,
             () => { });

        if (this.Button?.Behavior is not ICountBehavior countBehavior)
        {
            return;
        }

        PlainShipRoom[] allRooms = ShipStatus.Instance.AllRooms;
        var useRoom = (
            from room in allRooms
            where room != null && room.RoomId != SystemTypes.Hallway
            orderby RandomGenerator.Instance.Next()
            select (room.RoomId, room)
        ).Take(countBehavior.AbilityCount);

        this.setRooms = new Dictionary<SystemTypes, Arrow>();
        foreach (var(roomId, room) in useRoom)
        {
            var arrow = new Arrow(new Color32(255, 25, 25, 200));
            arrow.UpdateTarget(room.roomArea.bounds.center);
            this.setRooms.Add(roomId, arrow);
        }

    }

    public bool IsActivate()
        => this.curPos == PlayerControl.LocalPlayer.transform.position;

    public bool UseAbility()
    {
        this.curPos = PlayerControl.LocalPlayer.transform.position;

        if (!tryGetPlayerInRoom(out SystemTypes? room) ||
			!room.HasValue ||
            !this.setRooms.ContainsKey(room.Value)) { return false; }

        this.targetRoom = room.Value;
        return true;
    }

    public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse() &&
			tryGetPlayerInRoom(out SystemTypes? room) &&
			room.HasValue &&
			this.setRooms.ContainsKey(room.Value);

    public void SetMagicCircle()
    {
        var arrow = this.setRooms[this.targetRoom];

        Vector2 pos = arrow.Target;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.ZombieRpc))
        {
            caller.WriteByte((byte)ZombieRpcOps.SetMagicCircle);
            caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
            caller.WriteFloat(pos.x);
            caller.WriteFloat(pos.y);
            caller.WriteFloat(this.showMagicCircleTime);
        }
        setMagicCircle(pos, this.showMagicCircleTime);

        arrow.Clear();
        this.setRooms.Remove(this.targetRoom);
        updateReviveState(true);
    }

    public void ResetOnMeetingStart()
    {
        if (this.resurrectText != null)
        {
            this.resurrectText.gameObject.SetActive(false);
        }
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ReviveAction(PlayerControl player)
    {

    }

    public string GetFakeOptionString() => "";

    public void Update(PlayerControl rolePlayer)
    {

        bool isDead = rolePlayer.Data.IsDead;
        bool isNotTaskPhase =
            MeetingHud.Instance ||
            ExileController.Instance ||
            ShipStatus.Instance == null ||
            !ShipStatus.Instance.enabled;
        bool isNotAwake = !this.IsAwake;
        bool isDeActivateArrow = isDead || isNotTaskPhase || isNotAwake;

        foreach (var arrow in this.setRooms.Values)
        {
            arrow.SetActive(!isDeActivateArrow);
            arrow.Update();
        }

        if (isNotAwake)
        {
            this.Button?.SetButtonShow(false);
            return;
        }

        if (isDead && this.infoBlock())
        {
            HudManager.Instance.Chat.gameObject.SetActive(false);
        }

        if (!rolePlayer.moveable || isNotTaskPhase)
        {
            return;
        }

        if (this.isResurrected) { return; }

        if (rolePlayer.Data.IsDead &&
            this.activateResurrectTimer &&
            this.canResurrect)
        {
            if (this.resurrectText == null)
            {
                this.resurrectText = Object.Instantiate(
                    HudManager.Instance.KillButton.cooldownTimerText,
                    Camera.main.transform, false);
                this.resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
                this.resurrectText.enableWordWrapping = false;
            }

            this.resurrectText.gameObject.SetActive(true);
            this.resurrectTimer -= Time.deltaTime;
            this.resurrectText.text = string.Format(
                Tr.GetString("resurrectText"),
                Mathf.CeilToInt(this.resurrectTimer));

            if (this.resurrectTimer <= 0.0f)
            {
                this.activateResurrectTimer = false;
                revive(rolePlayer);
            }
        }
    }

    public override bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        if ((!this.awakeRole ||
            (!this.canResurrect && !this.isResurrected)))
        {
            ++this.killCount;

            if (this.killCount >= this.awakeKillCount && !this.awakeRole)
            {
                this.awakeRole = true;
                this.HasOtherVision = this.awakeHasOtherVision;
                this.Button?.SetButtonShow(true);
            }

            updateReviveState(false);
        }
        return true;
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
                Palette.ImpostorRed, Tr.GetString(RoleTypes.Impostor.ToString()));
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
                $"{RoleTypes.Impostor}FullDescription");
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
            return string.Concat(new string[]
            {
                TranslationController.Instance.GetString(
                   StringNames.ImpostorTask,
                   SystemArray.Empty<Il2CppObject>()),
                "\r\n",
                Palette.ImpostorRed.ToTextColor(),
                TranslationController.Instance.GetString(
                    StringNames.FakeTasks,
                    SystemArray.Empty<Il2CppObject>()),
                "</color>"
            });
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
            return Palette.ImpostorRed;
        }
    }

    public override void ExiledAction(
        PlayerControl rolePlayer)
    {

        if (this.isResurrected) { return; }

        // 追放でオフ時は以下の処理を行わない
        if (!this.canResurrectOnExil) { return; }

        if (this.canResurrect)
        {
            this.activateResurrectTimer = true;
        }
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer,
        PlayerControl killerPlayer)
    {
        if (this.isResurrected) { return; }

        if (this.canResurrect)
        {
            this.activateResurrectTimer = true;
        }
    }

    public override bool IsBlockShowMeetingRoleInfo() => this.infoBlock();

    public override bool IsBlockShowPlayingRoleInfo() => this.infoBlock();


    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            ZombieOption.AwakeKillCount,
            1, 0, 3, 1,
            format: OptionUnit.Shot);

        IRoleAbility.CreateAbilityCountOption(factory, 1, 3, 3f);

        factory.CreateIntOption(
            ZombieOption.ResurrectKillCount,
            2, 0, 3, 1,
            format: OptionUnit.Shot);

        factory.CreateFloatOption(
            ZombieOption.ShowMagicCircleTime,
            10.0f, 0.0f, 30.0f, 0.5f,
            format: OptionUnit.Second);

        factory.CreateFloatOption(
            ZombieOption.ResurrectDelayTime,
            5.0f, 4.0f, 60.0f, 0.1f,
            format: OptionUnit.Second);
        factory.CreateBoolOption(
            ZombieOption.CanResurrectOnExil,
            false);
    }

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;

        this.killCount = 0;

        this.awakeKillCount = cate.GetValue<ZombieOption, int>(
            ZombieOption.AwakeKillCount);
        this.resurrectKillCount = cate.GetValue<ZombieOption, int>(
            ZombieOption.ResurrectKillCount);

        this.showMagicCircleTime = cate.GetValue<ZombieOption, float>(
            ZombieOption.ShowMagicCircleTime);
        this.resurrectTimer = cate.GetValue<ZombieOption, float>(
            ZombieOption.ResurrectDelayTime);
        this.canResurrectOnExil = cate.GetValue<ZombieOption, bool>(
            ZombieOption.CanResurrectOnExil);

        this.awakeHasOtherVision = this.HasOtherVision;
        this.canResurrect = false;
        this.isResurrected = false;
        this.activateResurrectTimer = false;

        this.cachedColider = null;

        if (this.awakeKillCount <= 0)
        {
            this.awakeRole = true;
            this.HasOtherVision = this.awakeHasOtherVision;
        }
        else
        {
            this.awakeRole = false;
            this.HasOtherVision = false;
        }
    }

    private bool infoBlock()
    {
        // ・詳細
        // 復活を使用後に死亡 => 常に見える
        // 非復活可能状態でキル、死亡後復活出来ない => 常に見える
        // 非復活可能状態でキル、死亡後復活出来る => 復活できるまで見えない
        // 非復活可能状態で追放、死亡後復活できる => 見えない
        // 非復活可能状態で追放、死亡後復活出来ない => 常に見える
        // 復活可能状態で死亡か追放 => 見えない

        if (this.isResurrected)
        {
            return false;
        }
        else
        {
            return this.activateResurrectTimer;
        }
    }

    private void revive(PlayerControl rolePlayer)
    {
        if (rolePlayer == null) { return; }

        byte playerId = rolePlayer.PlayerId;

        Player.RpcUncheckRevive(playerId);

        if (rolePlayer.Data == null ||
            rolePlayer.Data.IsDead ||
            rolePlayer.Data.Disconnected) { return; }

		List<Vector2> randomPos = new List<Vector2>();

		Map.AddSpawnPoint(randomPos, playerId);

        Player.RpcUncheckSnap(playerId, randomPos[
            RandomGenerator.Instance.Next(randomPos.Count)]);

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.ZombieRpc))
        {
            caller.WriteByte((byte)ZombieRpcOps.UseResurrect);
            caller.WriteByte(playerId);
        }
        UseResurrect(this);

        HudManager.Instance.Chat.chatBubblePool.ReclaimAll();
        if (this.resurrectText != null)
        {
            this.resurrectText.gameObject.SetActive(false);
        }
    }

    private void updateReviveState(bool isReduceAfter)
    {
        if (this.killCount >= this.resurrectKillCount &&
            this.Button.Behavior is ICountBehavior behavior &&
            behavior.AbilityCount <= (isReduceAfter ? 1 : 0) &&
            !this.canResurrect)
        {
            this.canResurrect = true;
            this.isResurrected = false;
        }
    }

    private bool tryGetPlayerInRoom(out SystemTypes? playerRoom)
    {
        if (this.cachedColider == null)
        {
            this.cachedColider = PlayerControl.LocalPlayer.GetComponent<Collider2D>();
        }

        return Player.TryGetPlayerColiderRoom(this.cachedColider, out playerRoom);
    }
}
