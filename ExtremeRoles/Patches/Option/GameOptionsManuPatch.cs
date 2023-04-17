using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using HarmonyLib;


using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Patches.Option;

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
public static class GameOptionsMenuStartPatch
{
    private static List<(StringNames, Action<OptionBehaviour>)> hooks = 
        new List<(StringNames, Action<OptionBehaviour>)>();

    public static void AddHook(StringNames targetOption, Action<OptionBehaviour> hook)
    {
        hooks.Add((targetOption, hook));
    }

    public static void Postfix(GameOptionsMenu __instance)
    {
        // SliderInnner => GameGroup => Game Settings => PlayerOptionsMenu
        GameObject playerOptMenuObj = __instance.transform.parent.parent.parent.gameObject;

        if (playerOptMenuObj.GetComponent<ExtremeOptionMenu>() != null) { return; }

        // Adapt task count for main options
        modifiedDefaultGameOptions(__instance);
        // AddHook
        addHookOptionValueChange(__instance.Children);

        playerOptMenuObj.AddComponent<ExtremeOptionMenu>();
    }

    private static void addHookOptionValueChange(
        Il2CppReferenceArray<OptionBehaviour> child)
    {
        foreach (var (name, hook) in hooks)
        {
            if (!child.tryGetOption(name, out OptionBehaviour opt)) { return; }

            // まず初期化
            hook.Invoke(opt);

            //ほんでで追加
            opt.OnValueChanged += hook;
        }
    }

    private static void changeValueRange(
        Il2CppReferenceArray<OptionBehaviour> child,
        StringNames name, float minValue, float maxValue)
    {
        if (!child.tryGetOption(name, out OptionBehaviour opt)) { return; }

        NumberOption numOpt = opt.TryCast<NumberOption>();
        if (!numOpt) { return; }
        numOpt.ValidRange = new FloatRange(minValue, maxValue);
    }

    private static void modifiedDefaultGameOptions(GameOptionsMenu instance)
    {
        Il2CppReferenceArray<OptionBehaviour> child = instance.Children;

        if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ||
            FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name == "custom")
        {
            changeValueRange(child, StringNames.GameNumImpostors, 0f, GameSystem.MaxImposterNum);
        }

        changeValueRange(child, StringNames.GameCommonTasks, 0f, 4f );
        changeValueRange(child, StringNames.GameShortTasks , 0f, 23f);
        changeValueRange(child, StringNames.GameLongTasks  , 0f, 15f);
    }

    private static OptionBehaviour tryGetOption(
        this Il2CppReferenceArray<OptionBehaviour> child,
        StringNames name, out OptionBehaviour optionBehaviour)
    {
        optionBehaviour = child.FirstOrDefault(x => x.Title == name);

        return optionBehaviour;
    }
}
