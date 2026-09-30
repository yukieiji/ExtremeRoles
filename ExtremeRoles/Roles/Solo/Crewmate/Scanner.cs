using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Extension.Vector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class ScannerRole :
	SingleRoleBase,
	IRoleAbility,
	IRoleUpdate,
	IRoleResetMeeting
{
	public ExtremeAbilityButton? Button { get; set; }

	private ModdedShapeShifterMinigameWrapper? minigameWrapper;
	private SystemTypes? selectedRoom;
	private bool isScanning;
	private Vector2 prevPlayerPos;
	private static Vector2 defaultPos => new Vector2(100.0f, 100.0f);

	private TextPopUpper? textPopUp;
	private TMPro.TextMeshPro? abilityText;

	public ScannerRole() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Scanner,
			ColorPalette.ScannerCyan))
	{ }

	public void CreateAbility()
	{
		// var img = UnityObjectLoader.LoadFromResources(ExtremeRoleId.Scanner);
		var img = UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton);
		string name = Tr.GetString("scannerScan");

		var beha = new ChargingAndReclickCountBehavior(
			name, img,
			(isCharge, _) =>
			{
				if (isCharge)
				{
					return IRoleAbility.IsCommonUseWithMinigame();
				}
				return IsAbilityUse();
			},
			openUI,
			(_) => UseAbility(),
			isCharge: () => this.minigameWrapper is not null && this.minigameWrapper.IsOpen,
			abilityOff: repose,
			reduceOnCharge: false);

		if (beha is IChargingBehavior charging)
		{
			charging.ChargeTime = float.MaxValue;
		}

		this.Button = new ExtremeAbilityButton(
			beha,
			new RoleButtonActivator(),
			KeyCode.F);
		((IRoleAbility)this).RoleAbilityInit();
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse();

	public bool UseAbility()
	{
		if (!this.selectedRoom.HasValue)
		{
			return false;
		}

		this.isScanning = true;
		this.prevPlayerPos = PlayerControl.LocalPlayer.GetTruePosition();
		return true;
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsTaskPhase ||
			Minigame.Instance != null ||
			!rolePlayer.CanMove)
		{
			cancelScan();
			return;
		}

		if (this.abilityText == null)
		{
			this.abilityText = Object.Instantiate(
				HudManager.Instance.KillButton.cooldownTimerText,
				Camera.main.transform, false);
			this.abilityText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
			this.abilityText.enableWordWrapping = false;
		}

		if (!this.isScanning)
		{
			this.abilityText.gameObject.SetActive(false);
			return;
		}

		var curPos = rolePlayer.GetTruePosition();

		if (this.prevPlayerPos.IsCloseTo(defaultPos, 0.01f))
		{
			this.prevPlayerPos = curPos;
		}

		if (this.prevPlayerPos.IsNotCloseTo(curPos))
		{
			cancelScan();
			return;
		}

		this.prevPlayerPos = curPos;
		this.abilityText.text = Tr.GetString("scanningText");
		this.abilityText.color = new Color(0f, 0.8f, 0f);
		this.abilityText.gameObject.SetActive(true);
	}

	public void ResetOnMeetingStart()
	{
		this.textPopUp?.Clear();
		if (this.abilityText != null)
		{
			this.abilityText.gameObject.SetActive(false);
		}
		this.minigameWrapper?.Reset();
		this.isScanning = false;
		this.selectedRoom = null;
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{

	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 3, 10, 5);
	}

	protected override void RoleSpecificInit()
	{
		this.textPopUp = new TextPopUpper(
			3, 5.0f,
			new Vector3(-3.75f, -2.5f, -250.0f),
			TMPro.TextAlignmentOptions.BottomLeft);
		this.prevPlayerPos = defaultPos;
		this.isScanning = false;
		this.selectedRoom = null;
	}

	private bool openUI()
	{
		this.minigameWrapper ??= new ModdedShapeShifterMinigameWrapper();
		if (this.minigameWrapper.IsOpen)
		{
			return true;
		}
		return this.minigameWrapper.Open<ScannerScanSelectorMinigame>(
			minigame => minigame.OnSelectRoom = this.overrideSelectRoom);
	}

	private void overrideSelectRoom(SystemTypes roomId)
	{
		if (this.Button is null ||
			!this.Button.Transform.TryGetComponent<PassiveButton>(out var button))
		{
			return;
		}
		this.selectedRoom = roomId;
		button.OnClick.Invoke();
	}

	private void cancelScan()
	{
		this.isScanning = false;
		this.selectedRoom = null;
		if (this.abilityText != null)
		{
			this.abilityText.gameObject.SetActive(false);
		}
		this.Button?.Behavior.ForceAbilityOff();
	}

	private void repose()
	{
		if (this.abilityText != null)
		{
			this.abilityText.gameObject.SetActive(false);
		}

		if (this.isScanning && this.selectedRoom.HasValue)
		{
			performScan(this.selectedRoom.Value);
		}

		this.isScanning = false;
		this.selectedRoom = null;
	}

	private void performScan(SystemTypes room)
	{
		var foundPlayerNames = new List<string>();

		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player.IsInValid() || !player.IsAlive())
			{
				continue;
			}

			if (Player.TryGetPlayerRoom(player, out var playerRoom) &&
				playerRoom == room)
			{
				foundPlayerNames.Add(player.Data.PlayerName);
			}
		}

		string roomName = TranslationController.Instance.GetString(room);
		string resultText;

		if (foundPlayerNames.Count > 0)
		{
			string namesList = string.Join("\n", foundPlayerNames.Select(name => $"・{name}"));
			resultText = string.Format(Tr.GetString("scanResultFound"), roomName, namesList);
		}
		else
		{
			resultText = string.Format(Tr.GetString("scanResultEmpty"), roomName);
		}

		this.textPopUp ??= new TextPopUpper(
			3, 5.0f,
			new Vector3(-3.75f, -2.5f, -250.0f),
			TMPro.TextAlignmentOptions.BottomLeft);
		this.textPopUp.AddText(resultText);
	}
}
