using System;
using System.Linq;

using UnityEngine;
using Hazel;
using AmongUs.GameOptions;

using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Ship;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Carpenter : SingleRoleBase, IRoleAbility, IRoleAwake<RoleTypes>
    {
        public sealed class CarpenterAbilityButton : RoleAbilityButtonBase
        {
            public int CurAbilityNum
            {
                get => this.abilityNum;
            }

            private int abilityNum = 0;
            private bool isVentRemove;
            private int ventRemoveScrewNum;
            private int cameraSetScrewNum;
            private float ventRemoveStopTime;
            private float cameraRemoveStopTime;
            private string cameraSetString;
            private string ventRemoveString;
            private Sprite cameraSetSprite;
            private Sprite ventRemoveSprite;
            private Func<bool> ventModeCheck;

            private TMPro.TextMeshPro abilityCountText = null;

            private Action baseCleanUp;
            private Action reduceCountAction;

            public CarpenterAbilityButton(
                Func<bool> ability,
                Func<bool> canUse,
                Sprite cameraSetSprite,
                Sprite ventRemoveSprite,
                Action abilityCleanUp,
                Func<bool> abilityCheck,
                Func<bool> isVentMode,
                int ventRemoveScrewNum,
                int cameraSetScrewNum,
                float ventRemoveStopTime,
                float cameraRemoveStopTime,
                KeyCode hotkey = KeyCode.F
                ) : base(
                    "",
                    ability,
                    canUse,
                    cameraSetSprite,
                    abilityCleanUp, abilityCheck, hotkey)
            {

                var cooldownTimerText = this.GetCoolDownText();

                this.abilityCountText = GameObject.Instantiate(
                    cooldownTimerText, cooldownTimerText.transform.parent);
                updateAbilityCountText();
                this.abilityCountText.enableWordWrapping = false;
                this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
                this.abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);

                this.ventModeCheck = isVentMode;

                this.ventRemoveString = Translation.GetString("ventSeal");
                this.cameraSetString = Translation.GetString("cameraSet");
                this.SetButtonText(this.cameraSetString);

                this.ventRemoveScrewNum = ventRemoveScrewNum;
                this.cameraSetScrewNum = cameraSetScrewNum;
                
                this.ventRemoveStopTime = ventRemoveStopTime;
                this.cameraRemoveStopTime = cameraRemoveStopTime;

                this.cameraSetSprite = cameraSetSprite;
                this.ventRemoveSprite = ventRemoveSprite;
                
                this.isVentRemove = false;

                this.reduceCountAction = this.reduceAbilityCount();

                if (HasCleanUp())
                {
                    this.baseCleanUp = new Action(this.AbilityCleanUp);
                    this.AbilityCleanUp += this.reduceCountAction;
                }
                else
                {
                    this.baseCleanUp = null;
                    this.AbilityCleanUp = this.reduceCountAction;
                }
            }

            public void UpdateAbilityCount(int newCount)
            {
                this.abilityNum = newCount;
                this.updateAbilityCountText();
                if (this.State == AbilityState.None)
                {
                    this.SetStatus(AbilityState.CoolDown);
                }
            }

            public override void ForceAbilityOff()
            {
                this.SetStatus(AbilityState.Ready);
                this.baseCleanUp?.Invoke();
            }

            protected override bool IsEnable() =>
                this.CanUse.Invoke() && this.abilityNum > 0 && screwCheck();

            protected override void UpdateAbility()
            {
                this.isVentRemove = this.ventModeCheck();
                if (this.isVentRemove)
                {
                    this.SetButtonImg(this.ventRemoveSprite);
                    this.SetButtonText(this.ventRemoveString);
                    this.SetAbilityActiveTime(this.ventRemoveStopTime);
                }
                else
                {
                    this.SetButtonImg(this.cameraSetSprite);
                    this.SetButtonText(this.cameraSetString);
                    this.SetAbilityActiveTime(this.cameraRemoveStopTime);
                }
                if (this.abilityNum <= 0)
                {
                    this.SetStatus(AbilityState.None);
                }
            }

            protected override void DoClick()
            {
                if (this.IsEnable() &&
                    this.Timer <= 0f &&
                    this.IsAbilityReady() &&
                    this.UseAbility.Invoke())
                {
                    if (this.HasCleanUp())
                    {
                        this.SetStatus(AbilityState.Activating);
                    }
                    else
                    {
                        this.reduceCountAction.Invoke();
                        this.ResetCoolTimer();
                    }
                }
            }

            private Action reduceAbilityCount()
            {
                return () =>
                {
                    this.abilityNum = this.isVentRemove ?
                    this.abilityNum - this.ventRemoveScrewNum : this.abilityNum - this.cameraSetScrewNum;
                    updateAbilityCountText();
                };
            }

            private void updateAbilityCountText()
            {
                if (this.abilityCountText == null) { return; }

                this.abilityCountText.text = string.Format(
                    Translation.GetString("carpenterScrewNum"),
                        this.abilityNum,
                        this.isVentRemove ? this.ventRemoveScrewNum : this.cameraSetScrewNum);
            }

            private bool screwCheck()
            {
                return
                (
                    (this.abilityNum - this.ventRemoveScrewNum >= 0 && this.isVentRemove) ||
                    (this.abilityNum - this.cameraSetScrewNum >= 0 && !this.isVentRemove)
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

        public RoleAbilityButtonBase Button
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
        private RoleAbilityButtonBase abilityButton;

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
            Vent vent = ShipStatus.Instance?.AllVents.FirstOrDefault((x) => x != null && x.Id == ventId);
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
            vent.GetComponent<PowerTools.SpriteAnim>()?.Stop();

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
            if (targetVent.Right?.Id == unlinkVent.Id)
            {
                targetVent.Right = null;
            }
            else if (targetVent.Center?.Id == unlinkVent.Id)
            {
                targetVent.Center = null;
            }
            else if (targetVent.Left?.Id == unlinkVent.Id)
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
                    this.HasOtherVison = this.awakeHasOtherVision;
                    this.Button.SetButtonShow(true);
                }
            }
        }
        public void CreateAbility()
        {

            var allOpt = OptionHolder.AllOption;

            this.Button = new CarpenterAbilityButton(
                UseAbility,
                IsAbilityUse,
                Resources.Loader.CreateSpriteFromResources(
                    Resources.Path.CarpenterSetCamera),
                Resources.Loader.CreateSpriteFromResources(
                    Resources.Path.CarpenterVentSeal),
                CleanUp,
                IsAbilityCheck,
                IsVentMode,
                (int)allOpt[GetRoleOptionId(CarpenterOption.RemoveVentScrew)].GetValue(),
                (int)allOpt[GetRoleOptionId(CarpenterOption.SetCameraScrew)].GetValue(),
                (float)allOpt[GetRoleOptionId(CarpenterOption.RemoveVentStopTime)].GetValue(),
                (float)allOpt[GetRoleOptionId(CarpenterOption.SetCameraStopTime)].GetValue());
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


        public void RoleAbilityResetOnMeetingStart()
        {
            return;     
        }

        public void RoleAbilityResetOnMeetingEnd()
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
            IOption parentOps)
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
            this.awakeTaskGage = (float)OptionHolder.AllOption[
                GetRoleOptionId(CarpenterOption.AwakeTaskGage)].GetValue() / 100.0f;
            
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

        private void createAbilityOption(IOption parentOps)
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
            ((IntCustomOption)OptionHolder.AllOption[
                GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityCount)]).SetOptionUnit(OptionUnit.ScrewNum);
        }

        private void abilityInit()
        {
            if (this.Button == null) { return; }

            var allOps = OptionHolder.AllOption;
            this.Button.SetCoolTime(
                allOps[GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue());
            this.Button.SetAbilityActiveTime(1.0f);

            var button = this.Button as CarpenterAbilityButton;

            if (button != null)
            {
                button.UpdateAbilityCount(
                    allOps[GetRoleOptionId(RoleAbilityCommonOption.AbilityCount)].GetValue());
            }

            this.Button.ResetCoolTimer();
        }

    }
}
