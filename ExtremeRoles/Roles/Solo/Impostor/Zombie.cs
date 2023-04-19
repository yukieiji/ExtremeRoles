using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Video;
using Hazel;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Zombie : 
    SingleRoleBase,
    IRoleAbility,
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

    private Il2CppReferenceArray<Collider2D> buffer;
    private ContactFilter2D filter = default(ContactFilter2D);

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

        player.SetThum(Loader.CreateSpriteFromResources(
            Path.ZombieMagicCircle));
        player.SetVideo(Loader.GetUnityObjectFromResources<VideoClip>(
            Path.VideoAsset, string.Format(
                Path.VideoAssetPlaceHolder, Path.ZombieMagicCircleVideo)));
        player.SetTimer(activeTime);
    }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            Translation.GetString("featMagicCircle"),
            Loader.CreateSpriteFromResources(
                Path.ZombieMagicCircleButton),
            IsActivate,
            SetMagicCircle,
             () => { });

        if (this.Button?.Behavior is not AbilityCountBehavior countBehavior)
        {
            return;
        }

        PlainShipRoom[] allRooms = CachedShipStatus.Instance.AllRooms;
        var useRoom = (
            from room in allRooms
            where room != null && room.RoomId != SystemTypes.Hallway
            orderby RandomGenerator.Instance.Next()
            select (room.RoomId, room)
        ).Take(countBehavior.AbilityCount);

        this.setRooms = new Dictionary<SystemTypes, Arrow>();
        foreach (var(roomId, room) in useRoom)
        {
            var arrow = new Arrow(Palette.ImpostorRed * 0.5f);
            arrow.UpdateTarget(room.transform.position);
            this.setRooms.Add(roomId, arrow);
        }

    }

    public bool IsActivate()
        => this.curPos == CachedPlayerControl.LocalPlayer.PlayerControl.transform.position;

    public bool UseAbility()
    {
        this.curPos = CachedPlayerControl.LocalPlayer.PlayerControl.transform.position;

        if (!tryGetPlayerInRoom(out SystemTypes room) ||
            !this.setRooms.ContainsKey(room)) { return false; }

        this.targetRoom = room; 
        return true;
    }

    public bool IsAbilityUse()
        => this.IsCommonUse() &&
           tryGetPlayerInRoom(out SystemTypes room) &&
           this.setRooms.ContainsKey(room);

    public void SetMagicCircle()
    {
        var arrow = this.setRooms[this.targetRoom];

        Vector2 pos = arrow.Target;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.ZombieRpc))
        {
            caller.WriteByte((byte)ZombieRpcOps.SetMagicCircle);
            caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
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

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
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
            CachedShipStatus.Instance == null ||
            !CachedShipStatus.Instance.enabled;
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
            FastDestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(false);
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
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                    Camera.main.transform, false);
                this.resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
                this.resurrectText.enableWordWrapping = false;
            }

            this.resurrectText.gameObject.SetActive(true);
            this.resurrectTimer -= Time.fixedDeltaTime;
            this.resurrectText.text = string.Format(
                Translation.GetString("resurrectText"),
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
                Palette.White, Translation.GetString(RoleTypes.Crewmate.ToString()));
        }
    }
    public override string GetFullDescription()
    {
        if (IsAwake)
        {
            return Translation.GetString(
                $"{this.Id}FullDescription");
        }
        else
        {
            return Translation.GetString(
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
                $"{this.GetColoredRoleName()}: {Translation.GetString("crewImportantText")}");
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
                CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
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
        IOption parentOps)
    {
        CreateIntOption(
            ZombieOption.AwakeKillCount,
            1, 0, 3, 1,
            parentOps,
            format: OptionUnit.Shot);

        this.CreateAbilityCountOption(parentOps, 1, 3, 3f);

        CreateFloatOption(
            ZombieOption.ShowMagicCircleTime,
            10.0f, 0.0f, 30.0f, 0.5f,
            parentOps,
            format: OptionUnit.Second);

        CreateIntOption(
            ZombieOption.ResurrectKillCount,
            2, 0, 3, 1,
            parentOps,
            format: OptionUnit.Shot);

        CreateFloatOption(
            ZombieOption.ResurrectDelayTime,
            5.0f, 4.0f, 60.0f, 0.1f,
            parentOps, format: OptionUnit.Second);
        CreateBoolOption(
            ZombieOption.CanResurrectOnExil,
            false, parentOps);
    }

    protected override void RoleSpecificInit()
    {
        var allOpt = OptionHolder.AllOption;

        this.killCount = 0;

        this.awakeKillCount = allOpt[
            GetRoleOptionId(ZombieOption.AwakeKillCount)].GetValue();
        this.resurrectKillCount = allOpt[
            GetRoleOptionId(ZombieOption.ResurrectKillCount)].GetValue();

        this.showMagicCircleTime = allOpt[
            GetRoleOptionId(ZombieOption.ShowMagicCircleTime)].GetValue();
        this.resurrectTimer = allOpt[
            GetRoleOptionId(ZombieOption.ResurrectDelayTime)].GetValue();
        this.canResurrectOnExil = allOpt[
            GetRoleOptionId(ZombieOption.CanResurrectOnExil)].GetValue();

        this.awakeHasOtherVision = this.HasOtherVision;
        this.canResurrect = false;
        this.isResurrected = false;
        this.activateResurrectTimer = false;

        this.cachedColider = null;
        this.buffer = null;

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

        var allPlayer = GameData.Instance.AllPlayers;
        ShipStatus ship = CachedShipStatus.Instance;

        List<Vector2> randomPos = new List<Vector2>();

        if (ExtremeRolesPlugin.Compat.IsModMap)
        {
            randomPos = ExtremeRolesPlugin.Compat.ModMap.GetSpawnPos(
                playerId);
        }
        else
        {
            switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                ByteOptionNames.MapId))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    Vector2 baseVec = Vector2.up;
                    baseVec = baseVec.Rotate(
                        (float)(playerId - 1) * (360f / (float)allPlayer.Count));
                    Vector2 offset = baseVec * ship.SpawnRadius + new Vector2(0f, 0.3636f);
                    randomPos.Add(ship.InitialSpawnCenter + offset);
                    randomPos.Add(ship.MeetingSpawnCenter + offset);
                    break;
                case 4:
                    randomPos.AddRange(GameSystem.GetAirShipRandomSpawn());
                    break;
                default:
                    break;
            }
        }

        Player.RpcUncheckSnap(playerId, randomPos[
            RandomGenerator.Instance.Next(randomPos.Count)]);

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.ZombieRpc))
        {
            caller.WriteByte((byte)ZombieRpcOps.UseResurrect);
            caller.WriteByte(playerId);
        }
        UseResurrect(this);

        FastDestroyableSingleton<HudManager>.Instance.Chat.chatBubPool.ReclaimAll();
        if (this.resurrectText != null)
        {
            this.resurrectText.gameObject.SetActive(false);
        }
    }

    private void updateReviveState(bool isReduceAfter)
    {
        if (this.killCount >= this.resurrectKillCount &&
            this.Button.Behavior is AbilityCountBehavior behavior &&
            behavior.AbilityCount <= (isReduceAfter ? 1 : 0) &&
            !this.canResurrect)
        {
            this.canResurrect = true;
            this.isResurrected = false;
        }
    }

    private bool tryGetPlayerInRoom(out SystemTypes playerRoom)
    {
        playerRoom = SystemTypes.Hallway;

        if (this.cachedColider == null)
        {
            this.cachedColider = CachedPlayerControl.LocalPlayer.PlayerControl.GetComponent<Collider2D>();
        }
        if (this.buffer == null)
        {
            this.buffer = new Il2CppReferenceArray<Collider2D>(10);
        }

        foreach (PlainShipRoom room in CachedShipStatus.Instance.AllRooms)
        {
            if (room == null || !room.roomArea) { continue; }

            int hitCount = room.roomArea.OverlapCollider(this.filter, this.buffer);
            if (isHit(this.cachedColider, buffer, hitCount))
            {
                playerRoom = room.RoomId;
                return true;
            }
        }

        return false;
    }

    private static bool isHit(
        Collider2D playerCollinder,
        Collider2D[] buffer,
        int hitCount)
    {
        for (int i = 0; i < hitCount; i++)
        {
            if (buffer[i] == playerCollinder)
            {
                return true;
            }
        }
        return false;
    }
}
