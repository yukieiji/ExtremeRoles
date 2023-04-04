using System.Collections.Generic;
using System.Linq;

using Assets.CoreScripts;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityFactory;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.GhostRoles.Crewmate
{
    public sealed class Faunus : GhostRoleBase
    {
        public enum SaboType
        {
            None,
            FixLight,
            StopCharles,
            ResetSeismic,
            ResetReactor,
            RestoreOxy,
        }

        private TaskTypes saboTask;
        private bool saboActive;
        private Minigame saboGame;
        private bool isOpen = false;

        public Faunus() : base(
            true,
            ExtremeRoleType.Crewmate,
            ExtremeGhostRoleId.Faunus,
            ExtremeGhostRoleId.Faunus.ToString(),
            ColorPalette.FaunusAntiquewhite)
        { }

        public override void CreateAbility()
        {
            this.Button = GhostRoleAbilityFactory.CreateCountAbility(
                AbilityType.FaunusOpenSaboConsole,
                Resources.Loader.CreateSpriteFromResources(
                    Resources.Path.MaintainerRepair),
                this.isReportAbility(),
                this.isPreCheck,
                this.isAbilityUse,
                this.UseAbility,
                null, true,
                isAbilityActive,
                cleanUp, cleanUp,
                KeyCode.F);
            this.ButtonInit();
            this.Button.SetLabelToCrewmate();
        }

        public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

        public override void Initialize()
        {
            this.saboActive = false;
            this.isOpen = false;
        }

        protected override void OnMeetingEndHook()
        {
            this.isOpen = false;
        }

        protected override void OnMeetingStartHook()
        {
            this.saboGame = null;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateCountButtonOption(
                parentOps, 1, 5, 3.0f);
        }

        protected override void UseAbility(RPCOperator.RpcCaller caller)
        {
            this.isOpen = false;
            Console console;
            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                console = ExtremeRolesPlugin.Compat.ModMap.GetConsole(this.saboTask);
            }
            else
            {
                switch (this.saboTask)
                {
                    case TaskTypes.FixLights:
                        console = getLightConsole();
                        break;
                    case TaskTypes.RestoreOxy:
                    case TaskTypes.StopCharles:
                        List<Console> oxyConsole = getOxyConsole();
                        int oxyCount = oxyConsole.Count;
                        if (oxyCount == 0) { return; }
                        int oxyIndex = RandomGenerator.Instance.Next(oxyCount);
                        console = oxyConsole[oxyIndex];
                        break;
                    case TaskTypes.ResetReactor:
                    case TaskTypes.ResetSeismic:
                        List<Console> handConsole = getHandConsole();
                        int handCount = handConsole.Count;
                        if (handCount == 0) { return; }
                        int seismicIndex = RandomGenerator.Instance.Next(handCount);
                        console = handConsole[seismicIndex];
                        break;
                    default:
                        return;
                }
            }

            if (console == null || Camera.main == null)
            {
                return;
            }

            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
            PlayerTask playerTask = console.FindTask(localPlayer);
            this.saboGame = GameSystem.OpenMinigame(
                playerTask.GetMinigamePrefab(), playerTask, console);

            FastDestroyableSingleton<Telemetry>.Instance.WriteUse(
                localPlayer.PlayerId,
                playerTask.TaskType,
                console.transform.position);
            this.isOpen = true;
        }

        private bool isPreCheck() => this.saboActive;

        private bool isAbilityActive()
        {
            this.isOpen = this.saboGame != null;
            return this.isOpen;
        }
        private bool isAbilityUse()
        {
            this.saboActive = false;

            foreach (PlayerTask task in 
                CachedPlayerControl.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
            {
                if (!task) { continue; }

                switch (task.TaskType)
                {
                    case TaskTypes.FixLights:
                    case TaskTypes.RestoreOxy:
                    case TaskTypes.ResetReactor:
                    case TaskTypes.ResetSeismic:
                    case TaskTypes.StopCharles:
                        this.saboActive = true;
                        this.saboTask = task.TaskType;
                        break;
                    default:
                        break;
                }
                if (this.saboActive) { break; }
            }

            return this.IsCommonUse() && this.saboActive;
        }

        private Console getLightConsole()
        {
            var console = Object.FindObjectsOfType<Console>();
            switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                ByteOptionNames.MapId))
            {
                // 0 = Skeld
                // 1 = Mira HQ
                // 2 = Polus
                // 3 = Dleks - deactivated
                // 4 = Airship
                case 0:
                case 1:
                case 3:
                    return console.FirstOrDefault(
                        x => x.gameObject.name.Contains("SwitchConsole"));
                case 2:
                    return console.FirstOrDefault(
                        x => x.gameObject.name.Contains("panel_switches"));
                case 4:
                    return console.FirstOrDefault(
                        x => x.gameObject.name.Contains("task_lightssabotage"));
                default:
                    return null;
            }
        }

        private List<Console> getOxyConsole()
        {
            var console = Object.FindObjectsOfType<Console>().ToList();
            switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                ByteOptionNames.MapId))
            {
                // 0 = Skeld
                // 1 = Mira HQ
                // 2 = Polus
                // 3 = Dleks - deactivated
                // 4 = Airship
                case 0:
                case 1:
                case 3:
                    return console.FindAll(
                        x => x.gameObject.name.Contains("NoOxyConsole"));
                case 4:
                    // AirShipは昇降が酸素コンソール
                    List<Console> res = new List<Console>(2);
                    Console leftConsole = console.FirstOrDefault(
                        x => x.gameObject.name.Contains("NoOxyConsoleLeft"));
                    if (leftConsole != null)
                    {
                        res.Add(leftConsole);
                    }
                    Console rightConsole = console.FirstOrDefault(
                        x => x.gameObject.name.Contains("NoOxyConsoleRight"));
                    if (rightConsole != null)
                    {
                        res.Add(rightConsole);
                    }
                    return res;
                default:
                    return new List<Console> ();
            }
        }
        private List<Console> getHandConsole()
        {
            var console = Object.FindObjectsOfType<Console>().ToList();
            switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                ByteOptionNames.MapId))
            {
                // 0 = Skeld
                // 1 = Mira HQ
                // 2 = Polus
                // 3 = Dleks - deactivated
                // 4 = Airship
                case 0:
                case 3:
                    List<Console> skeldsHand = new List<Console>(2);
                    Console upperConsole = console.FirstOrDefault(
                        x => x.gameObject.name.Contains("UpperHandConsole"));
                    if (upperConsole != null)
                    {
                        skeldsHand.Add(upperConsole);
                    }
                    Console lowerConsole = console.FirstOrDefault(
                        x => x.gameObject.name.Contains("LowerHandConsole"));
                    if (lowerConsole != null)
                    {
                        skeldsHand.Add(lowerConsole);
                    }
                    return skeldsHand;
                case 1:
                    List<Console> miraHqHand = new List<Console>(2);
                    Console leftConsole = console.FirstOrDefault(
                        x => x.gameObject.name.Contains("LeftHandConsole"));
                    if (leftConsole != null)
                    {
                        miraHqHand.Add(leftConsole);
                    }
                    Console rightConsole = console.FirstOrDefault(
                        x => x.gameObject.name.Contains("RightHandConsole"));
                    if (rightConsole != null)
                    {
                        miraHqHand.Add(rightConsole);
                    }
                    return miraHqHand;
                case 2:
                    // ポーラスは耐震がハンドコンソール
                    List<Console> polusConsole = new List<Console>(2);
                    Console leftPanel = console.FirstOrDefault(
                        x => x.gameObject.name.Contains("panel_hand_left"));
                    if (leftPanel != null)
                    {
                        polusConsole.Add(leftPanel);
                    }
                    Console rightPanel = console.FirstOrDefault(
                        x => x.gameObject.name.Contains("panel_hand_right"));
                    if (rightPanel != null)
                    {
                        polusConsole.Add(rightPanel);
                    }
                    return polusConsole;
                default:
                    return new List<Console>();
            }
        }
        private void cleanUp()
        {
            if (this.isOpen && this.saboGame != null)
            {
                this.isOpen = false;
                this.saboGame.Close();
            }
        }

    }
}
