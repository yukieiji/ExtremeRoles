using HarmonyLib;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(HashRandom), nameof(HashRandom.FastNext))]
    public static class HashRandomFastNextPatch
    {
        public static bool Prefix(
            ref int __result,
            [HarmonyArgument(0)] int maxInt)
        {
            if (OptionHolder.AllOption[
                (int)OptionHolder.CommonOptionKey.UseStrongRandomGen].GetValue())
            {
                __result = RandomGenerator.Instance.Next(maxInt);
                return false;
            }
            return true;
        }
    }
}
