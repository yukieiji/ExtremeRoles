using System.Collections.Generic;
using System.Linq;

using Assets.CoreScripts;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Compat;

using OptionFactory = ExtremeRoles.Module.CustomOption.Factories.AutoParentSetFactory;
using ExtremeRoles.Module.Ability.Factory;

#nullable enable

namespace ExtremeRoles.GhostRoles.Crewmate;

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
    private Minigame? saboGame;
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
		OptionFactory factory)
    {
        GhostRoleAbilityFactory.CreateCountButtonOption(factory, 1, 5, 3.0f);
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {
        this.isOpen = false;
        Console? console;
        if (CompatModManager.Instance.TryGetModMap(out var modMap))
        {
            console = modMap!.GetConsole(this.saboTask);
        }
        else
        {
			string consoleName;
            switch (this.saboTask)
            {
                case TaskTypes.FixLights:
                    consoleName = getLightConsoleName();
                    break;
                case TaskTypes.RestoreOxy:
                case TaskTypes.StopCharles:
					IReadOnlyList<string> oxyConsole = getOxyConsole();
                    int oxyCount = oxyConsole.Count;
                    if (oxyCount == 0) { return; }
                    int oxyIndex = RandomGenerator.Instance.Next(oxyCount);
					consoleName = oxyConsole[oxyIndex];
                    break;
                case TaskTypes.ResetReactor:
                case TaskTypes.ResetSeismic:
					IReadOnlyList<string> handConsole = getHandConsole();
                    int handCount = handConsole.Count;
                    if (handCount == 0) { return; }
                    int seismicIndex = RandomGenerator.Instance.Next(handCount);
					consoleName = handConsole[seismicIndex];
                    break;
                default:
                    return;
            }
			console = findConsole(consoleName);
        }

        if (console == null || Camera.main == null)
        {
            return;
        }

        PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
        PlayerTask playerTask = console.FindTask(localPlayer);
        this.saboGame = MinigameSystem.Open(
            playerTask.GetMinigamePrefab(), playerTask, console);

        FastDestroyableSingleton<UnityTelemetry>.Instance.WriteUse(
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

        return IsCommonUse() && this.saboActive;
    }

	private string getLightConsoleName()
		=> Map.Id switch
		{
			0 or 1 or 3 => "SwitchConsole",
			2 => "panel_switches",
			4 => "task_lightssabotage",
			_ => string.Empty,
		};

    private IReadOnlyList<string> getOxyConsole()
		=> Map.Id switch
		{
			0 or 1 or 3 => new List<string> { "NoOxyConsole" },
			4 => new List<string> { "NoOxyConsoleLeft", "NoOxyConsoleRight" },
			_ => new List<string>(),
		};

    private IReadOnlyList<string> getHandConsole()
		=> Map.Id switch
		{
			0 or 3 => new List<string> { "UpperHandConsole", "LowerHandConsole" },
			1 => new List<string> { "LeftHandConsole", "RightHandConsole" },
			2 => new List<string> { "panel_hand_left", "panel_hand_right" },
			5 => new List<string> { "ResetReactorConsole" },
			_ => new List<string>(),
		};

    private void cleanUp()
    {
        if (this.isOpen && this.saboGame != null)
        {
            this.isOpen = false;
            this.saboGame.Close();
        }
    }

	private static Console? findConsole(string consoleName)
	{
		var console = Object.FindObjectsOfType<Console>();
		Console? conole = console.FirstOrDefault(x => x.gameObject.name.Contains(consoleName));
		return conole;
	}

}
