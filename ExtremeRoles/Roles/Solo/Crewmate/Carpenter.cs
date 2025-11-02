using System;
using System.Linq;

using UnityEngine;
using Hazel;
using TMPro;
using AmongUs.GameOptions;

using ExtremeRoles.Compat;
using ExtremeRoles.Extension.Vector;
using ExtremeRoles.Extension.VentModule;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.ModeSwitcher;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Carpenter : SingleRoleBase, IRoleAbility, IRoleAwake<RoleTypes>
{
    public enum CarpenterAbilityMode
    {
        RemoveVent,
        SetCamera
    }

    public sealed class CarpenterAbilityBehavior : BehaviorBase, IActivatingBehavior
    {
        public int AbilityCount { get; private set; }
		public float ActiveTime { get; set; }

		public bool CanAbilityActiving => this.abilityCheck.Invoke();

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
            GraphicAndActiveTimeMode<CarpenterAbilityMode> ventMode,
            GraphicAndActiveTimeMode<CarpenterAbilityMode> cameraMode,
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

            this.switcher = new GraphicAndActiveTimeSwitcher<CarpenterAbilityMode>(
				this, ventMode, cameraMode);
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
            this.abilityCountText.text = Tr.GetString(
				"carpenterScrewNum",
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
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Carpenter,
			ColorPalette.CarpenterBrown),
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
        if (!ShipStatus.Instance) { return; }

        Vent vent = ShipStatus.Instance.AllVents.FirstOrDefault(
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

		var ventRenderer = vent.myRend;

		if (vent.myAnim != null)
		{
			vent.myAnim.Stop();
			vent.myAnim.enabled = false;
		}

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

        byte mapId = Map.Id;

        if (mapId == 2 || mapId == 4)
        {
            camera.transform.localRotation = new Quaternion(0, 0, 1, 1);
        }
        if (CompatModManager.Instance.TryGetModMap(out var modMap))
        {
			modMap.SetUpNewCamera(camera);
        }

        var allCameras = ShipStatus.Instance.AllCameras.ToList();
        camera.gameObject.SetActive(true);
        allCameras.Add(camera);
        ShipStatus.Instance.AllCameras = allCameras.ToArray();
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

        var loader = this.Loader;

        this.Button = new ExtremeAbilityButton(
            new CarpenterAbilityBehavior(
                ventMode: new(
					mode: CarpenterAbilityMode.RemoveVent,
					graphic: new (
						Tr.GetString("ventSeal"),
						Resources.UnityObjectLoader.LoadSpriteFromResources(
							ObjectPath.CarpenterVentSeal)),
					time: loader.GetValue<CarpenterOption, float>(
						CarpenterOption.RemoveVentStopTime)
					),
                cameraMode: new(
					mode: CarpenterAbilityMode.SetCamera,
					graphic: new(
						Tr.GetString("cameraSet"),
						Resources.UnityObjectLoader.LoadSpriteFromResources(
							ObjectPath.CarpenterSetCamera)),
					time: loader.GetValue<CarpenterOption, float>(
						CarpenterOption.SetCameraStopTime)
					),
                ventRemoveScrewNum: loader.GetValue<CarpenterOption, int>(
					CarpenterOption.RemoveVentScrew),
                cameraSetScrewNum: loader.GetValue<CarpenterOption, int>(
					CarpenterOption.SetCameraScrew),
                setCountStart: UseAbility,
                canUse: IsAbilityUse,
                abilityCheck: IsAbilityCheck,
                updateMapObj: CleanUp,
                ventRemoveModeCheck: IsVentMode),
            new RoleButtonActivator(),
			KeyCode.F);

		this.RoleAbilityInit();
		this.Button.SetLabelToCrewmate();
    }

    public bool UseAbility()
    {
        if (this.targetVent != null)
        {

            var ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation].TryCast<VentilationSystem>();

            if (!PlayerControl.LocalPlayer.Data.IsDead &&
                ventilationSystem != null &&
                ventilationSystem.IsImpostorInsideVent(this.targetVent.Id))
            {
                VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, this.targetVent.Id);
                return false;
            }
        }
        this.prevPos = PlayerControl.LocalPlayer.GetTruePosition();
        return true;
    }

    public bool IsAbilityUse()
	{
		byte mapId = Map.Id;

		return
			this.IsAwake &&
			IRoleAbility.IsCommonUse() &&
			!(
				// Miraとファングルはカメラ設置できない, TODO:ファングルはカメラ設置できるか調査
				this.targetVent == null && (mapId == 1 || mapId == 5)
			)
			&&
			!(
				this.targetVent == null &&
				CompatModManager.Instance.TryGetModMap(out var modMap) &&
				!modMap.CanPlaceCamera
			);
	}

    public bool IsAbilityCheck() =>
        this.prevPos.IsCloseTo(PlayerControl.LocalPlayer.GetTruePosition(), 0.1f);

    public bool IsVentMode()
    {
        this.targetVent = null;

        ShipStatus ship = ShipStatus.Instance;

        if (ship == null || !ship.enabled) { return false; }

        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();

        foreach (Vent vent in ship.AllVents)
        {
            if (vent.IsModed() && !vent.gameObject.active)
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
            var pos = PlayerControl.LocalPlayer.GetTruePosition();
            byte roomId;
            try
            {
                roomId = (byte)HudManager.Instance.roomTracker.LastRoom.RoomId;
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

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
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
        OldAutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            CarpenterOption.AwakeTaskGage,
            70, 0, 100, 10,
            format: OptionUnit.Percentage);
        createAbilityOption(factory);
    }

    protected override void RoleSpecificInit()
    {
        this.targetVent = null;
        this.awakeTaskGage = this.Loader.GetValue<CarpenterOption, int>(
            CarpenterOption.AwakeTaskGage) / 100.0f;

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
    }

    private void createAbilityOption(OldAutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateFloatOption(
            RoleAbilityCommonOption.AbilityCoolTime,
            15.0f, 2.0f, 60.0f, 0.5f,
            format: OptionUnit.Second);
        factory.CreateIntOption(
            RoleAbilityCommonOption.AbilityCount,
            15, 5, 100, 1,
            format: OptionUnit.ScrewNum);
        factory.CreateIntOption(
            CarpenterOption.RemoveVentScrew,
            10, 1, 20, 1,
            format: OptionUnit.ScrewNum);
        factory.CreateFloatOption(
            CarpenterOption.RemoveVentStopTime,
            5.0f, 2.0f, 15.0f, 0.5f,
            format: OptionUnit.Second);
        factory.CreateIntOption(
            CarpenterOption.SetCameraScrew,
            5, 1, 10, 1,
            format: OptionUnit.ScrewNum);
        factory.CreateFloatOption(
            CarpenterOption.SetCameraStopTime,
            2.5f, 1.0f, 5.0f, 0.5f,
            format: OptionUnit.Second);
    }

    public void RoleAbilityInit()
    {
        if (this.Button == null) { return; }

        var loader = this.Loader;
        this.Button.Behavior.SetCoolTime(
            loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime));

        if (this.Button.Behavior is CarpenterAbilityBehavior behavior)
        {
            behavior.SetAbilityCount(
                loader.GetValue<RoleAbilityCommonOption, int>(RoleAbilityCommonOption.AbilityCount));
        }
        this.Button.OnMeetingEnd();
    }
}
