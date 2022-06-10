namespace ExtremeRoles.Compat.Patches
{
    public static class SubmergedExileControllerWrapUpAndSpawnPatch
    {
        public static void Prefix(ExileController __instance)
        {
            ExtremeRoles.Patches.Controller.ExileControllerWrapUpPatch.WrapUpPrefix(
                __instance);
        }

        public static void Postfix(ExileController __instance)
        {
            ExtremeRoles.Patches.Controller.ExileControllerReEnableGameplayPatch.ReEnablePostfix();
            ExtremeRoles.Patches.Controller.ExileControllerWrapUpPatch.WrapUpPostfix(
                __instance.exiled);
        }
    }
}
