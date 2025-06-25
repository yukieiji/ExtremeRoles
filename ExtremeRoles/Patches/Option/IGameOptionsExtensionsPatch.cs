using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;

using ExtremeRoles.Extension.Strings;
using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Compat;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.Option.ShipGlobal;



namespace ExtremeRoles.Patches.Option;

// FIXME : どうもPatchが動作していないので無効化しておく
/*
[HarmonyPatch(
    typeof(IGameOptionsExtensions),
    nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
public static class IGameOptionsExtensionsNumImpostorsPatch
{
    public static bool Prefix(ref int __result)
    {
        if (ExtremeGameModeManager.Instance.RoleSelector.IsAdjustImpostorNum) { return true; }

        __result = Math.Clamp(
            GameOptionsManager.Instance.CurrentGameOptions.NumImpostors,
            0, GameData.Instance.PlayerCount);
        return false;
    }
}
*/