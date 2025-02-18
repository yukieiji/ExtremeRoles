using System;
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
	public Il2CppIEnumerator Compute(float deltaT);
}

public sealed class MeetingButtonPostionComputer(
	float time, UiElement element, float endOffset) : IMeetingButtonPostionComputer
{
	public Vector2 Anchor { private get; set; }
	public Vector2 Offset { private get; set; } = Vector2.zero;
	public float StartOffset { private get; set; }
	public UiElement Element { get; } = element;

	private readonly float time = time;
	private readonly Transform transform = element.transform;
	private readonly float endOffset = endOffset;

	private void deltaPos(float deltaT)
	{
		this.transform.localPosition = Vector2.Lerp(
			this.Anchor * this.StartOffset + this.Offset,
			this.Anchor * this.endOffset + this.Offset,
			Effects.ExpOut(deltaT));
	}

	public Il2CppIEnumerator Compute(float deltaT)
		=> Effects.Lerp(
			this.time,
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

	public IReadOnlyList<IMeetingButtonPostionComputer> Flatten(float startPos)
	{
		int secondCount = this.second.Count;

		var result = new List<MeetingButtonPostionComputer>(secondCount + this.first.Count);

		var firstOffset = secondCount > 0 ? Vector2.up * 0.65f : Vector2.zero;
		var secondOffset = secondCount > 0 ? Vector2.down * 0.65f : Vector2.zero;

		setUpComputer(this.first, result, firstOffset, startPos);
		setUpComputer(this.second, result, secondOffset, startPos);

		return result;
	}

	public void AddFirstRow(UiElement element)
		=> add(this.first, element);

	public void AddSecondRow(UiElement element)
		=> add(this.second, element);

	private static void setUpComputer(
		in List<MeetingButtonPostionComputer> setUpContainer,
		in List<MeetingButtonPostionComputer> result,
		Vector2 offset, float statPos)
	{
		foreach (var button in setUpContainer)
		{
			button.Offset = offset;
			button.StartOffset = statPos;
			result.Add(button);
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

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
public static class PlayerVoteAreaSelectPatch
{
	private static Dictionary<byte, UiElement> meetingAbilityButton =
		new Dictionary<byte, UiElement>();

	public static void Reset()
    {
		meetingAbilityButton.Clear();
    }

	public static bool Prefix(PlayerVoteArea __instance)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

		float startPos = __instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;
		List<MeetingButtonPostionComputer> button = [
			new MeetingButtonPostionComputer(0.25f, __instance.CancelButton, 1.3f),
			new MeetingButtonPostionComputer(0.35f, __instance.ConfirmButton,0.65f),
		];

		if (!OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			if (MonikaTrashSystem.TryGet(out var monika) &&
				monika.InvalidPlayer(localPlayer))
			{
				return false;
			}

			var (buttonRole, anotherButtonRole) = ExtremeRoleManager.GetInterfaceCastedLocalRole<
				IRoleMeetingButtonAbility>();

			if (buttonRole is not null || anotherButtonRole is not null)
            {
				return true; // TODO:Can use both role ability
            }
			else
			{
				return true;
			}
		}
		else if (
			localPlayer.PlayerId != system.Caller ||
			__instance.voteComplete ||
			__instance.Parent == null ||
			!__instance.Parent.Select((int)__instance.TargetPlayerId))
		{
			return false;
		}

		__instance.Buttons.SetActive(true);
		__instance.StartCoroutine(
			Effects.All(
				wrappedEffectsLerp(0.25f, (float t) =>
				{
					__instance.CancelButton.transform.localPosition = Vector2.Lerp(
						Vector2.right * startPos,
						Vector2.right * 1.3f,
						Effects.ExpOut(t));
				}),
				wrappedEffectsLerp(0.35f, (float t) =>
				{
					__instance.ConfirmButton.transform.localPosition = Vector2.Lerp(
						Vector2.right * startPos,
						Vector2.right * 0.65f,
						Effects.ExpOut(t));
				})
			)
		);

		var selectableElements = new Il2CppSystem.Collections.Generic.List<UiElement>();
		selectableElements.Add(__instance.CancelButton);
		selectableElements.Add(__instance.ConfirmButton);
		ControllerManager.Instance.OpenOverlayMenu(
			__instance.name,
			__instance.CancelButton,
			__instance.ConfirmButton, selectableElements, false);

		return false;
	}

	private static bool meetingButtonAbility(
		PlayerVoteArea instance,
		IRoleMeetingButtonAbility role)
	{
		byte target = instance.TargetPlayerId;

        if (instance.AmDead ||
			role.IsBlockMeetingButtonAbility(instance))
		{
			return true;
		}
		else if (
			instance.voteComplete ||
			instance.Parent == null ||
			!instance.Parent.Select((int)target))
		{
			return false;
		}

		if (!meetingAbilityButton.TryGetValue(target, out UiElement abilitybutton) ||
			abilitybutton == null)
		{
			UiElement newAbilitybutton = GameObject.Instantiate(
				instance.CancelButton, instance.ConfirmButton.transform.parent);
			var passiveButton = newAbilitybutton.GetComponent<PassiveButton>();
			passiveButton.OnClick.RemoveAllPersistentAndListeners();
			passiveButton.OnClick.AddListener(instance.Cancel);
			passiveButton.OnClick.AddListener(
				() => { newAbilitybutton.gameObject.SetActive(false); });
			passiveButton.OnClick.AddListener(role.CreateAbilityAction(instance));

			var render = newAbilitybutton.GetComponent<SpriteRenderer>();

			role.ButtonMod(instance, newAbilitybutton);
			role.SetSprite(render);

			meetingAbilityButton[target] = newAbilitybutton;
			abilitybutton = newAbilitybutton;
		}

		if (abilitybutton == null)
		{
			return true;
		}

		abilitybutton.gameObject.SetActive(true);
		instance.Buttons.SetActive(true);

		float startPos = instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;

		instance.StartCoroutine(
			Effects.All(
				wrappedEffectsLerp(0.25f, (float t) =>
				{
					instance.CancelButton.transform.localPosition = Vector2.Lerp(
						Vector2.right * startPos,
						Vector2.right * 1.3f,
						Effects.ExpOut(t));
				}),
				wrappedEffectsLerp(0.35f, (float t) =>
				{
					instance.ConfirmButton.transform.localPosition = Vector2.Lerp(
						Vector2.right * startPos,
						Vector2.right * 0.65f,
						Effects.ExpOut(t));
				}),
				wrappedEffectsLerp(0.45f, (float t) =>
				{
					abilitybutton.transform.localPosition = Vector2.Lerp(
						Vector2.right * startPos,
						Vector2.right * -0.01f,
						Effects.ExpOut(t));
				})
			)
		);

		Il2CppSystem.Collections.Generic.List<UiElement> selectableElements = new Il2CppSystem.Collections.Generic.List<UiElement>();
		selectableElements.Add(instance.CancelButton);
		selectableElements.Add(instance.ConfirmButton);
		selectableElements.Add(abilitybutton);

		ControllerManager.Instance.OpenOverlayMenu(
			instance.name,
			instance.CancelButton,
			instance.ConfirmButton, selectableElements, false);

		return false;

	}

	private static Il2CppIEnumerator wrappedEffectsLerp(float t, Delegate del)
		=> Effects.Lerp(t, (Il2CppActionFloat)(del));
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
