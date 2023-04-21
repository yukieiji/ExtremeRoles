using System;
using System.Linq;

using UnityEngine;
using Hazel;
using TMPro;
using AmongUs.GameOptions;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.AbilityModeSwitcher;
using ExtremeRoles.Module.ButtonAutoActivator;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Extension.Ship;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Carpenter : SingleRoleBase, IRoleAbility, IRoleAwake<RoleTypes>
{
    public enum CarpenterAbilityMode
    {
        RemoveVent,
        SetCamera
    }

    public sealed class CarpenterAbilityBehavior : AbilityBehaviorBase
    {
        public int AbilityCount { get; private set; }

        private bool isUpdate;

        private TextMeshPro abilityCountText;
        private bool isVentRemoveMode = true;

        private Func<bool> setCountStart;
        private Func<bool> canUse;
        private Func<bool> abilityCheck;
        private Action updateMapObj;
        private Func<bool> ventRemoveModeCheck;
        
        private int ventRemoveScrewNum;
        private int cameraSetScrewNum;

        private GraphicAndActiveTimeSwitcher<CarpenterAbilityMode> switcher;

        public CarpenterAbilityBehavior(
            GraphicAndActiveTimeMode ventMode,
            GraphicAndActiveTimeMode cameraMode,
            int ventRemoveScrewNum,
            int cameraSetScrewNum,
            Func<bool> setCountStart,
            Func<bool> canUse,
            Func<bool> abilityCheck,
            Action updateMapObj,
            Func<bool> ventRemoveModeCheck) : base(
                ventMode.Graphic.Text,
                ventMode.Graphic.Img)
        {
            this.ventRemoveScrewNum = ventRemoveScrewNum;
            this.cameraSetScrewNum = cameraSetScrewNum;

            this.setCountStart = setCountStart;
            this.canUse = canUse;
            this.abilityCheck = abilityCheck;
            this.updateMapObj = updateMapObj;
            this.ventRemoveModeCheck = ventRemoveModeCheck;

            this.switcher = new GraphicAndActiveTimeSwitcher<CarpenterAbilityMode>(this);
            this.switcher.Add(CarpenterAbilityMode.RemoveVent, ventMode);
            this.switcher.Add(CarpenterAbilityMode.SetCamera, cameraMode);
        }

        public void SetAbilityCount(int newAbilityNum)
        {
            this.AbilityCount = newAbilityNum;
            this.isUpdate = true;
            updateAbilityCountText();
        }

        public override void AbilityOff()
        {
            this.AbilityCount = this.isVentRemoveMode ?
                this.AbilityCount - this.ventRemoveScrewNum : 
                this.AbilityCount - this.cameraSetScrewNum;
            updateAbilityCountText();
            this.updateMapObj.Invoke();
        }

        public override void ForceAbilityOff()
        { }

        public override void Initialize(ActionButton button)
        {
            var coolTimerText = button.cooldownTimerText;

            this.abilityCountText = UnityEngine.Object.Instantiate(
                coolTimerText, coolTimerText.transform.parent);
            this.abilityCountText.enableWordWrapping = false;
            this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
            this.abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
            updateAbilityCountText();
        }

        public override bool IsCanAbilityActiving() => this.abilityCheck.Invoke();

        public override bool IsUse() =>
            this.canUse.Invoke() && 
            this.AbilityCount > 0 && screwCheck();

        public override bool TryUseAbility(
            float timer, AbilityState curState, out AbilityState newState)
        {
            newState = curState;

            if (timer > 0 ||
                curState != AbilityState.Ready ||
                (this.AbilityCount <= 0 || !screwCheck()))
            {
                return false;
            }

            if (!this.setCountStart.Invoke())
            {
                return false;
            }

            newState = this.ActiveTime <= 0.0f ?
                AbilityState.CoolDown : AbilityState.Activating;

            return true;
        }

        public override AbilityState Update(AbilityState curState)
        {
            if (curState == AbilityState.Activating)
            {
                return curState;
            }
            else
            {
                this.isVentRemoveMode = this.ventRemoveModeCheck.Invoke();

                this.switcher.Switch(this.isVentRemoveMode ?
                    CarpenterAbilityMode.RemoveVent : CarpenterAbilityMode.SetCamera);
                updateAbilityCountText();
            }

            if (this.isUpdate)
            {
                this.isUpdate = false;
                return AbilityState.CoolDown;
            }

            return this.AbilityCount > 0 ? curState : AbilityState.None;
        }

        private void updateAbilityCountText()
        {
            this.abilityCountText.text = string.Format(
                Translation.GetString("carpenterScrewNum"),
                this.AbilityCount,
                this.isVentRemoveMode ? this.ventRemoveScrewNum : this.cameraSetScrewNum);
        }

        private bool screwCheck()
        {
            return
            (
                (this.AbilityCount - this.ventRemoveScrewNum >= 0 && this.isVentRemoveMode) ||
                (this.AbilityCount - this.cameraSetScrewNum >= 0 && !this.isVentRemoveMode)
            );
        }
    }

    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.awakeRole;
        }
    }

    public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

    public ExtremeAbilityButton Button
    { 
        get => this.abilityButton;
        set
        {
            this.abilityButton = value;
        }
    }

    public enum CarpenterOption
    {
        AwakeTaskGage,
        RemoveVentScrew,
        RemoveVentStopTime,
        SetCameraScrew,
        SetCameraStopTime
    }

    public enum AbilityType : byte
    {
        RemoveVent,
        SetCamera
    }

    private bool awakeRole = false;
    private float awakeTaskGage;
    private Vent targetVent;
    private Vector2 prevPos;
    private ExtremeAbilityButton abilityButton;

    private bool awakeHasOtherVision;

    private static int cameraNum = 0;
    public Carpenter() : base(
        ExtremeRoleId.Carpenter,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Carpenter.ToString(),
        ColorPalette.CarpenterBrown,
        false, true, false, false)
    { }

    public static void UpdateMapObject(ref MessageReader reader)
    {
        byte abilityType = reader.ReadByte();

        switch ((AbilityType)abilityType)
        {
            case AbilityType.RemoveVent:
                int ventId = reader.ReadInt32();
                removeVent(ventId);
                break;
            case AbilityType.SetCamera:
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                SystemTypes roomType = (SystemTypes)reader.ReadByte();
                setCamera(x, y, roomType);
                break;
            default:
                break;
        }
    }

    private static void removeVent(int ventId)
    {
        if (!CachedShipStatus.Instance) { return; }

        Vent vent = CachedShipStatus.Instance.AllVents.FirstOrDefault(
            (x) => x != null && x.Id == ventId);
        if (vent == null) { return; }

        Vent rightVent = vent.Right;
        if (rightVent != null)
        {
            unlinkVent(rightVent, vent);
        }
        Vent centerVent = vent.Center;
        if (centerVent != null)
        {
            unlinkVent(centerVent, vent);
        }
        Vent leftVent = vent.Left;
        if (leftVent != null)
        {
            unlinkVent(leftVent, vent);
        }

        vent.EnterVentAnim = null;
        vent.ExitVentAnim = null;

        var anim = vent.GetComponent<PowerTools.SpriteAnim>();
        if (anim)
        {
            anim.Stop();
        }

        var ventRenderer = vent.GetComponent<SpriteRenderer>();
        ventRenderer.sprite = null;
        vent.myRend = ventRenderer;
    }

    private static void setCamera(float x, float y, SystemTypes roomType)
    {
        var referenceCamera = UnityEngine.Object.FindObjectOfType<SurvCamera>();
        if (referenceCamera == null) { return; }

        var camera = UnityEngine.Object.Instantiate<SurvCamera>(referenceCamera);
        camera.transform.position = new Vector3(x, y, referenceCamera.transform.position.z - 1f);
        camera.CamName = $"CarpenterSecurityCameraNo{cameraNum}";
        cameraNum++;
        camera.Offset = new Vector3(0f, 0f, camera.Offset.z);

        StringNames newName = StringNames.ExitButton;

        switch (roomType)
        {
            case SystemTypes.Hallway:
                newName = StringNames.Hallway;
                break;
            case SystemTypes.Storage:
                newName = StringNames.Storage;
                break;
            case SystemTypes.Cafeteria:
                newName = StringNames.Cafeteria;
                break;
            case SystemTypes.Reactor:
                newName = StringNames.Reactor;
                break;
            case SystemTypes.UpperEngine:
                newName = StringNames.UpperEngine;
                break;
            case SystemTypes.Nav:
                newName = StringNames.Nav;
                break;
            case SystemTypes.Admin:
                newName = StringNames.Admin;
                break;
            case SystemTypes.Electrical:
                newName = StringNames.Electrical;
                break;
            case SystemTypes.LifeSupp:
                newName = StringNames.LifeSupp;
                break;
            case SystemTypes.Shields:
                newName = StringNames.Shields;
                break;
            case SystemTypes.MedBay:
                newName = StringNames.MedBay;
                break;
            case SystemTypes.Security:
                newName = StringNames.Security;
                break;
            case SystemTypes.Weapons:
                newName = StringNames.Weapons;
                break;
            case SystemTypes.LowerEngine:
                newName = StringNames.LowerEngine;
                break;
            case SystemTypes.Comms:
                newName = StringNames.Comms;
                break;
            case SystemTypes.Decontamination:
                newName = StringNames.Decontamination;
                break;
            case SystemTypes.Launchpad:
                newName = StringNames.Launchpad;
                break;
            case SystemTypes.LockerRoom:
                newName = StringNames.LockerRoom;
                break;
            case SystemTypes.Laboratory:
                newName = StringNames.Laboratory;
                break;
            case SystemTypes.Balcony:
                newName = StringNames.Balcony;
                break;
            case SystemTypes.Office:
                newName = StringNames.Office;
                break;
            case SystemTypes.Greenhouse:
                newName = StringNames.Greenhouse;
                break;
            case SystemTypes.Dropship:
                newName = StringNames.Dropship;
                break;
            case SystemTypes.Decontamination2:
                newName = StringNames.Decontamination2;
                break;
            case SystemTypes.Outside:
                newName = StringNames.Outside;
                break;
            case SystemTypes.Specimens:
                newName = StringNames.Specimens;
                break;
            case SystemTypes.BoilerRoom:
                newName = StringNames.BoilerRoom;
                break;
            case SystemTypes.VaultRoom:
                newName = StringNames.VaultRoom;
                break;
            case SystemTypes.Cockpit:
                newName = StringNames.Cockpit;
                break;
            case SystemTypes.Armory:
                newName = StringNames.Armory;
                break;
            case SystemTypes.Kitchen:
                newName = StringNames.Kitchen;
                break;
            case SystemTypes.ViewingDeck:
                newName = StringNames.ViewingDeck;
                break;
            case SystemTypes.HallOfPortraits:
                newName = StringNames.HallOfPortraits; 
                break;
            case SystemTypes.CargoBay:
                newName = StringNames.CargoBay;
                break;
            case SystemTypes.Ventilation:
                newName = StringNames.Ventilation;
                break;
            case SystemTypes.Showers:
                newName = StringNames.Showers;
                break;
            case SystemTypes.Engine:
                newName = StringNames.Engine;
                break;
            case SystemTypes.Brig:
                newName = StringNames.Brig;
                break;
            case SystemTypes.MeetingRoom:
                newName = StringNames.MeetingRoom;
                break;
            case SystemTypes.Records:
                newName = StringNames.Records;
                break;
            case SystemTypes.Lounge:
                newName = StringNames.Lounge;
                break;
            case SystemTypes.GapRoom:
                newName = StringNames.GapRoom;
                break;
            case SystemTypes.MainHall:
                newName = StringNames.MainHall;
                break;
            case SystemTypes.Medical:
                newName = StringNames.Medical;
                break;
            default:
                break;
        }
        camera.NewName = newName;

        byte mapId = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
            ByteOptionNames.MapId);

        if (mapId == 2 || mapId == 4)
        {
            camera.transform.localRotation = new Quaternion(0, 0, 1, 1);
        }
        if (ExtremeRolesPlugin.Compat.IsModMap)
        {
            ExtremeRolesPlugin.Compat.ModMap.SetUpNewCamera(camera);
        }

        var allCameras = CachedShipStatus.Instance.AllCameras.ToList();
        camera.gameObject.SetActive(true);
        allCameras.Add(camera);
        CachedShipStatus.Instance.AllCameras = allCameras.ToArray();
    }

    private static void unlinkVent(Vent targetVent, Vent unlinkVent)
    {
        if (targetVent.Right &&
            targetVent.Right.Id == unlinkVent.Id)
        {
            targetVent.Right = null;
        }
        else if (
            targetVent.Center &&
            targetVent.Center.Id == unlinkVent.Id)
        {
            targetVent.Center = null;
        }
        else if (
            targetVent.Left &&
            targetVent.Left.Id == unlinkVent.Id)
        {
            targetVent.Left = null;
        }
    }

    public string GetFakeOptionString() => "";

    public void Update(PlayerControl rolePlayer)
    {
        if (!this.awakeRole)
        {
            if (this.Button != null)
            {
                this.Button.SetButtonShow(false);
            }
            if (Player.GetPlayerTaskGage(rolePlayer) >= this.awakeTaskGage)
            {
                this.awakeRole = true;
                this.HasOtherVision = this.awakeHasOtherVision;
                this.Button.SetButtonShow(true);
            }
        }
    }
    public void CreateAbility()
    {

        var allOpt = AllOptionHolder.Instance;

        this.Button = new ExtremeAbilityButton(
            new CarpenterAbilityBehavior(
                ventMode: new GraphicAndActiveTimeMode()
                {
                    Graphic = new ButtonGraphic(
                        Translation.GetString("ventSeal"),
                        Loader.CreateSpriteFromResources(
                            Path.CarpenterVentSeal)),
                    Time = allOpt.GetValue<float>(
                        GetRoleOptionId(CarpenterOption.RemoveVentStopTime))
                },
                cameraMode: new GraphicAndActiveTimeMode()
                {
                    Graphic = new ButtonGraphic(
                        Translation.GetString("cameraSet"),
                        Loader.CreateSpriteFromResources(
                            Path.CarpenterSetCamera)),
                    Time = allOpt.GetValue<float>(
                        GetRoleOptionId(CarpenterOption.SetCameraStopTime))
                },
                ventRemoveScrewNum: allOpt.GetValue<int>(
                    GetRoleOptionId(CarpenterOption.RemoveVentScrew)),
                cameraSetScrewNum: allOpt.GetValue<int>(
                    GetRoleOptionId(CarpenterOption.SetCameraScrew)),
                setCountStart: UseAbility,
                canUse: IsAbilityUse,
                abilityCheck: IsAbilityCheck,
                updateMapObj: CleanUp,
                ventRemoveModeCheck: IsVentMode),
            new RoleButtonActivator(),
            KeyCode.F);
        abilityInit();
        this.Button.SetLabelToCrewmate();
    }

    public bool UseAbility()
    {
        if (this.targetVent != null)
        {

            var ventilationSystem = CachedShipStatus.Systems[SystemTypes.Ventilation].TryCast<VentilationSystem>();

            if (!PlayerControl.LocalPlayer.Data.IsDead && 
                ventilationSystem != null && 
                ventilationSystem.IsImpostorInsideVent(this.targetVent.Id))
            {
                VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, this.targetVent.Id);
                return false;
            }
        }
        this.prevPos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();
        return true;
    }

    public bool IsAbilityUse() => 
        this.IsAwake &&
        this.IsCommonUse() && 
        !(GameOptionsManager.Instance.CurrentGameOptions.GetByte(
            ByteOptionNames.MapId) == 1 && this.targetVent == null) &&
        !(this.targetVent == null && 
            ExtremeRolesPlugin.Compat.IsModMap && 
            !ExtremeRolesPlugin.Compat.ModMap.CanPlaceCamera);

    public bool IsAbilityCheck() => 
        this.prevPos == CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();

    public bool IsVentMode()
    {
        this.targetVent = null;

        ShipStatus ship = CachedShipStatus.Instance;

        if (ship == null || !ship.enabled) { return false; }

        Vector2 truePosition = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();
        
        foreach (Vent vent in ship.AllVents)
        {
            if (vent == null) { continue; }
            if (ship.IsCustomVent(vent.Id) &&
                !vent.gameObject.active)
            {
                continue;
            }
            float distance = Vector2.Distance(vent.transform.position, truePosition);
            if (distance <= vent.UsableDistance &&
                vent.myRend.sprite != null)
            {
                this.targetVent = vent;
                return true;
            }
        }
        return false;
    }

    public void CleanUp()
    {
        if (this.targetVent != null)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.CarpenterUseAbility))
            {
                caller.WriteByte((byte)AbilityType.RemoveVent);
                caller.WriteInt(this.targetVent.Id);
            }
            removeVent(this.targetVent.Id);
        }
        else
        {
            var pos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();
            byte roomId;
            try
            {
                roomId = (byte)FastDestroyableSingleton<HudManager>.Instance.roomTracker.LastRoom.RoomId;
            }
            catch
            {
                roomId = byte.MaxValue;
            }
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.CarpenterUseAbility))
            {
                caller.WriteByte((byte)AbilityType.SetCamera);
                caller.WriteFloat(pos.x);
                caller.WriteFloat(pos.y);
                caller.WriteByte(roomId);
            }
            setCamera(pos.x, pos.y, (SystemTypes)roomId);
        }
        this.targetVent = null;
    }


    public void ResetOnMeetingStart()
    {
        return;     
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        this.targetVent = null;
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

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateIntOption(
            CarpenterOption.AwakeTaskGage,
            70, 0, 100, 10,
            parentOps,
            format: OptionUnit.Percentage);
        createAbilityOption(parentOps);
    }

    protected override void RoleSpecificInit()
    {
        this.targetVent = null;
        this.awakeTaskGage = AllOptionHolder.Instance.GetValue<int>(
            GetRoleOptionId(CarpenterOption.AwakeTaskGage)) / 100.0f;
        
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
        abilityInit();
    }

    private void createAbilityOption(IOptionInfo parentOps)
    {
        CreateFloatOption(
            RoleAbilityCommonOption.AbilityCoolTime,
            15.0f, 2.0f, 60.0f, 0.5f,
            parentOps, format: OptionUnit.Second);
        CreateIntOption(
            RoleAbilityCommonOption.AbilityCount,
            15, 5, 100, 1,
            parentOps, format: OptionUnit.Shot);
        CreateIntOption(
            CarpenterOption.RemoveVentScrew,
            10, 1, 20, 1,
            parentOps, format: OptionUnit.ScrewNum);
        CreateFloatOption(
            CarpenterOption.RemoveVentStopTime,
            5.0f, 2.0f, 15.0f, 0.5f,
            parentOps, format: OptionUnit.Second);
        CreateIntOption(
            CarpenterOption.SetCameraScrew,
            5, 1, 10, 1,
            parentOps, format: OptionUnit.ScrewNum);
        CreateFloatOption(
            CarpenterOption.SetCameraStopTime,
            2.5f, 1.0f, 5.0f, 0.5f,
            parentOps, format: OptionUnit.Second);
        ((IntCustomOption)AllOptionHolder.Instance.Get<int>( 
            GetRoleOptionId(
                RoleAbilityCommonOption.AbilityCount),
            AllOptionHolder.ValueType.Int)).SetOptionUnit(OptionUnit.ScrewNum);
    }

    private void abilityInit()
    {
        if (this.Button == null) { return; }

        var allOps = AllOptionHolder.Instance;
        this.Button.Behavior.SetCoolTime(
            allOps.GetValue<float>(GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)));

        if (this.Button.Behavior is CarpenterAbilityBehavior behavior)
        {
            behavior.SetAbilityCount(
                allOps.GetValue<int>(GetRoleOptionId(RoleAbilityCommonOption.AbilityCount)));
        }
        this.Button.OnMeetingEnd();
    }
}
