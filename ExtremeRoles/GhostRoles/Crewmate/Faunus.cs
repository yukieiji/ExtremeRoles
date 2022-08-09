using Assets.CoreScripts;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public Faunus() : base(
            true,
            ExtremeRoleType.Crewmate,
            ExtremeGhostRoleId.Faunus,
            ExtremeGhostRoleId.Faunus.ToString(),
            ColorPalette.FaunusAntiquewhite)
        { }

        public override void CreateAbility()
        {
            this.Button = new AbilityCountButton(
                GhostRoleAbilityManager.AbilityType.FaunusOpenSaboConsole,
                this.UseAbility,
                this.isPreCheck,
                this.isAbilityUse,
                Resources.Loader.CreateSpriteFromResources(
                    Resources.Path.MaintainerRepair),
                this.DefaultButtonOffset,
                abilityCleanUp: cleanUp);
            this.ButtonInit();
            this.Button.SetLabelToCrewmate();
        }

        public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

        public override void Initialize()
        {
            this.saboActive = false;
        }

        public override void ReseOnMeetingEnd()
        {
            return;
        }

        public override void ReseOnMeetingStart()
        {
            Object.Destroy(this.saboGame);
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateCountButtonOption(
                parentOps, 1, 5, 3.0f);
        }

        protected override void UseAbility(MessageWriter writer)
        {

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

            this.saboGame = Object.Instantiate(
                playerTask.GetMinigamePrefab(),
                Camera.main.transform, false);
            this.saboGame.transform.SetParent(Camera.main.transform, false);
            this.saboGame.transform.localPosition = new Vector3(0f, 0f, -50f);
            this.saboGame.Console = console;
            this.saboGame.Begin(playerTask);
            FastDestroyableSingleton<Telemetry>.Instance.WriteUse(
                localPlayer.PlayerId,
                playerTask.TaskType,
                console.transform.position);

            var abilityCountButton = this.Button as AbilityCountButton;
            if (abilityCountButton != null)
            {
                abilityCountButton.UpdateAbilityCount(
                    abilityCountButton.CurAbilityNum - 1);
            }
        }

        private bool isPreCheck() => this.saboActive;

        private bool isAbilityUse()
        {
            this.saboActive = false;

            foreach (PlayerTask task in 
                CachedPlayerControl.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
            {

                switch (task?.TaskType)
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
            switch (PlayerControl.GameOptions.MapId)
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
            switch (PlayerControl.GameOptions.MapId)
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
            switch (PlayerControl.GameOptions.MapId)
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
            var abilityCountButton = this.Button as AbilityCountButton;
            if (abilityCountButton != null)
            {
                abilityCountButton.UpdateAbilityCount(
                    abilityCountButton.CurAbilityNum + 1);
            }

            if (this.saboGame != null)
            {
                this.saboGame.Close();
            }

        }

    }
}
