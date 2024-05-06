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
		foreach (var hat in CosmicStorage<CustomHat>.GetAll())
		{
			if (hat.Data == null)
			{
				continue;
			}
			hat.Data.name = Translation.GetString(hat.Name);
		}
#endif
#if WITHNAMEPLATE
		foreach (var np in CosmicStorage<CustomNamePlate>.GetAll())
		{
			if (np.Data == null)
			{
				continue;
			}
			np.Data.name = Translation.GetString(np.Name);
		}

#endif
#if WITHVISOR
		foreach (var vi in CosmicStorage<CustomVisor>.GetAll())
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
