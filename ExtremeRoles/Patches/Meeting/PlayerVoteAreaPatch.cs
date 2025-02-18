using System;
using System.Collections;
using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;



using Il2CppActionFloat = Il2CppSystem.Action<float>;
using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Extension.Il2Cpp;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Linq;

namespace ExtremeRoles.Patches.Meeting;

public static class NamePlateHelper
{
	public static bool NameplateChange = true;

	public static void UpdateNameplate(
		PlayerVoteArea pva, byte playerId = byte.MaxValue)
	{
		var playerInfo = GameData.Instance.GetPlayerById(
			playerId != byte.MaxValue ?
			playerId : pva.TargetPlayerId);
		if (playerInfo == null) { return; }

		var cache = ShipStatus.Instance.CosmeticsCache;
		string id = playerInfo.DefaultOutfit.NamePlateId;
		if (ClientOption.Instance.HideNamePlate.Value ||
			!cache.nameplates.TryGetValue(id, out var np) ||
			np == null)
		{
			np = cache.nameplates["nameplate_NoPlate"];
		}
		if (np == null) { return; }
		pva.Background.sprite = np.GetAsset().Image;
	}
}


[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class PlayerVoteAreaCosmetics
{
	public static void Postfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
	{
		NamePlateHelper.UpdateNameplate(
			__instance, playerInfo.PlayerId);
	}
}

public interface IMeetingButtonPostionComputer
{
	public UiElement Element { get; }
	public float Time { get; }
	public Il2CppIEnumerator Compute();
}

public sealed class MeetingButtonPostionComputer(
	float time, UiElement element, float endOffset) : IMeetingButtonPostionComputer
{
	public Vector2 Anchor { private get; set; }
	public Vector2 Offset { private get; set; } = Vector2.zero;
	public float StartOffset { private get; set; }
	public UiElement Element { get; } = element;
	public float Time { get; } = time;

	private readonly Transform transform = element.transform;
	private readonly float endOffset = endOffset;

	private void deltaPos(float deltaT)
	{
		this.transform.localPosition = Vector2.Lerp(
			this.Anchor * this.StartOffset + this.Offset,
			this.Anchor * this.endOffset + this.Offset,
			Effects.ExpOut(deltaT));
	}

	public Il2CppIEnumerator Compute()
		=> Effects.Lerp(
			this.Time,
			(Il2CppActionFloat)(deltaPos));
}

public sealed class MeetingButtonGroup
{
	private readonly List<MeetingButtonPostionComputer> first = new(2);
	private readonly List<MeetingButtonPostionComputer> second = new();

	public MeetingButtonGroup(PlayerVoteArea __instance)
	{
		this.AddFirstRow(__instance.CancelButton);
		this.AddFirstRow(__instance.ConfirmButton);
	}

	public void ResetFirst()
	{
		if (this.first.Count <= 2)
		{
			return;
		}
		this.first.RemoveRange(3, this.first.Count + 1 - 3);
	}

	public void ResetSecond()
		=> this.second.Clear();

	public IEnumerable<IMeetingButtonPostionComputer> Flatten(float startPos)
	{
		int secondCount = this.second.Count;

		var result = new List<MeetingButtonPostionComputer>(secondCount + this.first.Count);

		var firstOffset = secondCount > 0 ? Vector2.up * 0.65f : Vector2.zero;
		var secondOffset = secondCount > 0 ? Vector2.down * 0.65f : Vector2.zero;

		foreach (var buttn in setUpComputer(this.first, firstOffset, startPos))
		{
			yield return buttn;
		}
		foreach (var buttn in setUpComputer(this.second, secondOffset, startPos))
		{
			yield return buttn;
		}
	}

	public IEnumerable<IMeetingButtonPostionComputer> DefaultFlatten(float startPos)
		=> setUpComputer(this.first.GetRange(0, 2), Vector2.up, startPos);

	public void AddFirstRow(UiElement element)
		=> add(this.first, element);

	public void AddSecondRow(UiElement element)
		=> add(this.second, element);

	private static IEnumerable<IMeetingButtonPostionComputer> setUpComputer(
		IEnumerable<MeetingButtonPostionComputer> setUpContainer,
		Vector2 offset, float statPos)
	{
		foreach (var button in setUpContainer)
		{
			button.Offset = offset;
			button.StartOffset = statPos;
			yield return button;
		}
	}

	private static void add(in List<MeetingButtonPostionComputer> groups, UiElement element)
	{
		int size = groups.Count;
		float time = size + 0.25f;
		float endOffset = (size * 0.65f) - 1.3f;
		if (endOffset <= 0.0f)
		{
			endOffset = -0.01f;
		}
		groups.Add(new MeetingButtonPostionComputer(time, element, endOffset));
	}
}

[Il2CppRegister]
public sealed class ExtremeMeetingButton(IntPtr ptr) : MonoBehaviour(ptr)
{
	private sealed class MeetingButtonProp(PlayerVoteArea pva)
	{
		private readonly PlayerVoteArea pva = pva;
		public MeetingButtonGroup Group { get; } = new MeetingButtonGroup(pva);
		private readonly Dictionary<ExtremeRoleId, UiElement> cache = new Dictionary<ExtremeRoleId, UiElement>(2);

		public bool IsRecrateButtn(
			ExtremeRoleId id,
			IRoleMeetingButtonAbility buttonRole,
			out UiElement? button)
		{
			if (!this.cache.TryGetValue(id, out button) ||
				button == null)
			{
				UiElement newAbilitybutton = Instantiate(
					this.pva.CancelButton,
					this.pva.ConfirmButton.transform.parent);
				var passiveButton = newAbilitybutton.GetComponent<PassiveButton>();
				passiveButton.OnClick.RemoveAllPersistentAndListeners();
				passiveButton.OnClick.AddListener(this.pva.Cancel);
				passiveButton.OnClick.AddListener(
					() => { newAbilitybutton.gameObject.SetActive(false); });
				passiveButton.OnClick.AddListener(
					buttonRole.CreateAbilityAction(this.pva));

				var render = newAbilitybutton.GetComponent<SpriteRenderer>();

				buttonRole.ButtonMod(this.pva, newAbilitybutton);
				buttonRole.SetSprite(render);

				this.cache[id] = newAbilitybutton;
				button = newAbilitybutton;
				return false;
			}
			return true;
		}
	}

	private readonly Dictionary<byte, MeetingButtonProp> meetingButton = new Dictionary<byte, MeetingButtonProp>(PlayerCache.AllPlayerControl.Count);

	public bool TryGetMeetingButton(
		PlayerVoteArea pva,
		out IEnumerable<IMeetingButtonPostionComputer>? result)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		byte targetPlayerId = pva.TargetPlayerId;
		result = null;

		if (!this.meetingButton.TryGetValue(targetPlayerId, out var button))
		{
			button = new MeetingButtonProp(pva);
			this.meetingButton[targetPlayerId] = button;
		}

		float startPos = pva.AnimateButtonsFromLeft ? 0.2f : 1.95f;

		if (OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			result = button.Group.DefaultFlatten(startPos);
			return system.Caller == localPlayer.PlayerId;
		}

		var singleRole = ExtremeRoleManager.GetLocalPlayerRole();
		if (MonikaTrashSystem.TryGet(out var monika) &&
			monika.InvalidPlayer(localPlayer))
		{
			result = null;
			return false;
		}

		var role = ExtremeRoleManager.GetLocalPlayerRole();
		var multiRole = role as MultiAssignRoleBase;

		if (role is IRoleMeetingButtonAbility buttonRole &&
			multiRole?.AnotherRole is IRoleMeetingButtonAbility anotherButtonRole &&
			isOkRoleAbilityButton(pva, buttonRole) &&
			isOkRoleAbilityButton(pva, anotherButtonRole))
		{
			if (button.IsRecrateButtn(role.Id, buttonRole, out var element1))
			{
				button.Group.ResetSecond();
			}
			button.Group.AddSecondRow(element1);
			if (button.IsRecrateButtn(multiRole.AnotherRole.Id, anotherButtonRole, out var element2))
			{
				button.Group.ResetSecond();
			}
			button.Group.AddSecondRow(element2);
			result = button.Group.Flatten(startPos);
		}
		else if (
			role is IRoleMeetingButtonAbility mainButtonRole &&
			isOkRoleAbilityButton(pva, mainButtonRole))
		{
			if (button.IsRecrateButtn(role.Id, mainButtonRole, out var element1))
			{
				button.Group.ResetFirst();
			}
			button.Group.AddFirstRow(element1);
			result = button.Group.Flatten(startPos);
		}
		else if (
			multiRole?.AnotherRole is IRoleMeetingButtonAbility subButtonRole &&
			isOkRoleAbilityButton(pva, subButtonRole))
		{
			if (button.IsRecrateButtn(multiRole.AnotherRole.Id, subButtonRole, out var element1))
			{
				button.Group.ResetFirst();
			}
			button.Group.AddFirstRow(element1);
			result = button.Group.Flatten(startPos);
		}
		else
		{
			result = null;
		}
		return true;
	}

	private bool isOkRoleAbilityButton(
		PlayerVoteArea pva,
		IRoleMeetingButtonAbility buttonRole)
		=> !(pva.AmDead || buttonRole.IsBlockMeetingButtonAbility(pva) || pva.voteComplete || !pva.Parent.Select((int)pva.TargetPlayerId));
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
public static class PlayerVoteAreaSelectPatch
{

	public static bool Prefix(PlayerVoteArea __instance)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

		float startPos = __instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;

		var button = __instance.gameObject.TryAddComponent<ExtremeMeetingButton>();

		if (!button.TryGetMeetingButton(__instance, out var buttonEnumerable) ||
			__instance.voteComplete ||
			__instance.Parent == null ||
			!__instance.Parent.Select((int)__instance.TargetPlayerId))
		{
			return false;
		}

		if (buttonEnumerable is null)
		{
			return true;
		}

		__instance.Buttons.SetActive(true);
		__instance.StartCoroutine(
			buttonCompute(buttonEnumerable).WrapToIl2Cpp()
		);

		var selectableElements = new Il2CppSystem.Collections.Generic.List<UiElement>();
		foreach (var btn in buttonEnumerable)
		{
			selectableElements.Add(btn.Element);
		}
		ControllerManager.Instance.OpenOverlayMenu(
			__instance.name,
			__instance.CancelButton,
			__instance.ConfirmButton, selectableElements, false);

		return false;
	}

	private static IEnumerator buttonCompute(IEnumerable<IMeetingButtonPostionComputer> buttons)
	{
		foreach (var button in buttons.OrderByDescending(x => x.Time))
		{
			yield return button.Compute();
		}
	}
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class PlayerVoteAreaSetCosmeticsPatch
{
	public static void Postfix(PlayerVoteArea __instance)
	{
		if (ExtremeGameModeManager.Instance.ShipOption.Meeting.IsFixedVoteAreaPlayerLevel)
        {
			__instance.LevelNumberText.text = "99";
		}
	}
}


[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetDead))]
public static class PlayerVoteAreaSetDeadPatch
{
	public static bool Prefix(
		PlayerVoteArea __instance,
		[HarmonyArgument(0)] bool didReport,
		[HarmonyArgument(1)] bool isDead,
		[HarmonyArgument(2)] bool isGuardian = false)
	{
		if (!OnemanMeetingSystemManager.IsActive)
		{
			return true;
		}

		__instance.AmDead = false;
		__instance.DidReport = didReport;
		__instance.Megaphone.enabled = didReport;
		__instance.Overlay.gameObject.SetActive(false);
		__instance.XMark.gameObject.SetActive(false);

		return false;
	}

	public static void Postfix(
		PlayerVoteArea __instance,
		[HarmonyArgument(0)] bool didReport,
		[HarmonyArgument(1)] bool isDead,
		[HarmonyArgument(2)] bool isGuardian = false)
        {
		if (ExtremeGameModeManager.Instance.ShipOption.GhostRole.IsRemoveAngleIcon)
		{
			__instance.GAIcon.gameObject.SetActive(false);
		}
		else
		{
			bool isGhostRole = isGuardian ||
				ExtremeGhostRoleManager.GameRole.ContainsKey(__instance.TargetPlayerId);

			__instance.GAIcon.gameObject.SetActive(isGhostRole);
		}
	}
}
