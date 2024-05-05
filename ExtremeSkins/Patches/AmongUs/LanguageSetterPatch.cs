using HarmonyLib;

using ExtremeSkins.Helper;
using ExtremeSkins.Module;

namespace ExtremeSkins.Patches.AmongUs;

[HarmonyPatch(typeof(LanguageSetter), nameof(LanguageSetter.SetLanguage))]
public static class LanguageSetterPatch
{
    public static void Postfix()
    {
#if WITHHAT
		foreach (var hat in SkinContainer<CustomHat>.GetValues())
		{
			if (hat.Data == null)
			{
				continue;
			}
			hat.Data.name = Translation.GetString(hat.Name);
		}
#endif
#if WITHNAMEPLATE
		foreach (var np in SkinContainer<CustomNamePlate>.GetValues())
		{
			if (np.Data == null)
			{
				continue;
			}
			np.Data.name = Translation.GetString(np.Name);
		}

#endif
#if WITHVISOR
		foreach (var vi in SkinContainer<CustomVisor>.GetValues())
		{
			if (vi.Data == null)
			{
				continue;
			}
			vi.Data.name = Translation.GetString(vi.Name);
		}
#endif
	}
}
