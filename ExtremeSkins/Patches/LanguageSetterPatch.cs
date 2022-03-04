namespace ExtremeSkins.Patches
{
    public class LanguageSetterPatch
    {
        public static void Postfix()
        {
            ExtremeHatManager.UpdateTranslation();
        }
    }
}
