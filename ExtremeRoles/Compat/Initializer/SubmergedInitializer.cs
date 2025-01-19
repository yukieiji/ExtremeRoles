using System;

using BepInEx;
using HarmonyLib;


using ExtremeRoles.Compat.ModIntegrator;

#nullable enable

namespace ExtremeRoles.Compat.Initializer;

public sealed class SubmergedInitializer(PluginInfo plugin) : InitializerBase<SubmergedIntegrator>(plugin)
{
	public Type? SubmarineOxygenSystem { get; private set; }

	protected override void PatchAll(Harmony harmony)
	{
		var wrapUpAndSpawn = GetMethod("SubmergedExileController", "WrapUpAndSpawn");
		ExileController? controller = null;
		var wrapUpAndSpawnPrefix =
			 SymbolExtensions.GetMethodInfo(() => Patches.SubmergedExileControllerWrapUpAndSpawnPatch.Prefix());
		var wrapUpAndSpawnPostfix =
			 SymbolExtensions.GetMethodInfo(() => Patches.SubmergedExileControllerWrapUpAndSpawnPatch.Postfix(controller));

		var displayPrespawnStepPatchesPostfix = GetMethod(
			"DisplayPrespawnStepPatches", "CustomPrespawnStep");
		// ref渡しができないため・・・・
		System.Collections.IEnumerator? enumerator = null;
#pragma warning disable CS8601
		var displayPrespawnStepPatchesPostfixPrefix = SymbolExtensions.GetMethodInfo(
			() => Patches.DisplayPrespawnStepPatchesCustomPrespawnStepPatch.Prefix(ref enumerator));
#pragma warning restore CS8601

		var onDestroy = GetMethod("SubmarineSelectSpawn", "OnDestroy");
		var onDestroyPrefix = SymbolExtensions.GetMethodInfo(() => Patches.SubmarineSelectOnDestroyPatch.Prefix());

		var hudManagerUpdatePatch = GetClass("ChangeFloorButtonPatches");
		var hudManagerUpdatePatchPostfix = GetMethod(hudManagerUpdatePatch, "HudUpdatePatch");
		Patches.HudManagerUpdatePatchPostfixPatch.SetType(hudManagerUpdatePatch);
		object? instance = null;
		var hubManagerUpdatePatchPostfixPatch =
			SymbolExtensions.GetMethodInfo(() => Patches.HudManagerUpdatePatchPostfixPatch.Postfix(instance));

		string deteriorateFunction = nameof(ExtremeRoles.Module.Interface.IAmongUs.ISystemType.Deteriorate);

		this.SubmarineOxygenSystem = GetClass("SubmarineOxygenSystem");
		var submarineOxygenSystemDetoriorate = GetMethod(this.SubmarineOxygenSystem, deteriorateFunction);
		Patches.SubmarineOxygenSystemDetorioratePatch.SetType(this.SubmarineOxygenSystem);
		var submarineOxygenSystemDetorioratePostfixPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineOxygenSystemDetorioratePatch.Postfix(instance));


		var submarineSpawnInSystem = GetClass("SubmarineSpawnInSystem");
		var submarineSpawnInSystemDetoriorate = GetMethod(submarineSpawnInSystem, deteriorateFunction);
		Patches.SubmarineSpawnInSystemDetorioratePatch.SetType(submarineSpawnInSystem);
		var submarineSpawnInSystemDetorioratePostfixPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineSpawnInSystemDetorioratePatch.Postfix(instance));

		var submarineSurvillanceMinigame = GetClass("SubmarineSurvillanceMinigame");
		var submarineSurvillanceMinigameSystemUpdate = GetMethod(submarineSurvillanceMinigame, "Update");
		Patches.SubmarineSurvillanceMinigamePatch.SetType(submarineSurvillanceMinigame);
		Minigame? minigame = null;
		var submarineSurvillanceMinigameSystemUpdatePrefixPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineSurvillanceMinigamePatch.Prefix(minigame));
		var submarineSurvillanceMinigameSystemUpdatePostfixPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineSurvillanceMinigamePatch.Postfix(minigame));

		bool upperSelected = false;
		var submarineSelectSpawnCoSelectLevel = GetMethod("SubmarineSelectSpawn", "CoSelectLevel");
		var submarineSelectSpawnCoSelectLevelPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineSelectSpawnCoSelectLevelPatch.Prefix(ref upperSelected));

		// 会議終了時のリセット処理を呼び出せるように
		harmony.Patch(wrapUpAndSpawn,
			new HarmonyMethod(wrapUpAndSpawnPrefix),
			new HarmonyMethod(wrapUpAndSpawnPostfix));

		// アサシン会議発動するとスポーン画面が出ないように
		harmony.Patch(displayPrespawnStepPatchesPostfix,
			new HarmonyMethod(displayPrespawnStepPatchesPostfixPrefix));

		// キルクール周りが上書きされているのでそれの調整
		harmony.Patch(onDestroy,
			new HarmonyMethod(onDestroyPrefix));

		// フロアの階層変更ボタンの位置を変えるパッチ
		harmony.Patch(hudManagerUpdatePatchPostfix,
			postfix: new HarmonyMethod(hubManagerUpdatePatchPostfixPatch));

		// 酸素枯渇発動時アサシンは常にマスクを持つパッチ
		harmony.Patch(submarineOxygenSystemDetoriorate,
			postfix: new HarmonyMethod(submarineOxygenSystemDetorioratePostfixPatch));

		// アサシン会議時の暗転を防ぐパッチ
		harmony.Patch(submarineSpawnInSystemDetoriorate,
			postfix: new HarmonyMethod(submarineSpawnInSystemDetorioratePostfixPatch));

		// サブマージドのセキュリティカメラの制限をつけるパッチ
		harmony.Patch(submarineSurvillanceMinigameSystemUpdate,
			new HarmonyMethod(submarineSurvillanceMinigameSystemUpdatePrefixPatch),
			new HarmonyMethod(submarineSurvillanceMinigameSystemUpdatePostfixPatch));

		// ランダムスポーンを無効化する用
		harmony.Patch(submarineSelectSpawnCoSelectLevel,
			new HarmonyMethod(submarineSelectSpawnCoSelectLevelPatch));
	}
}
