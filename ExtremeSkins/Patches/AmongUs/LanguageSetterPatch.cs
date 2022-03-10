namespace ExtremeSkins.Patches.AmongUs
{
    public class LanguageSetterPatch
    {
        public static void Postfix()
        {
            ExtremeHatManager.UpdateTranslation();
        }
    }
}
