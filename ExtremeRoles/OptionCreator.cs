using UnityEngine;

using ExtremeRoles.Compat;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
// using ExtremeRoles.Module.CustomOption.Factories;
using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles;

public static class OptionCreator
{
	public const int IntegrateOptionStartOffset = 15000;

	private const int singleRoleOptionStartOffset = 256;
    private const int combRoleOptionStartOffset = 5000;
    private const int ghostRoleOptionStartOffset = 10000;
    private const int maxPresetNum = 20;

    public static readonly string[] SpawnRate = [
        "0%", "10%", "20%", "30%", "40%",
        "50%", "60%", "70%", "80%", "90%", "100%" ];

    public static readonly string[] Range = [ "short", "middle", "long" ];

    private static Color defaultOptionColor => new Color(204f / 255f, 204f / 255f, 0, 1f);

    public enum CommonOptionKey : int
    {
        PresetSelection = 0,

		UseRaiseHand,

        UseStrongRandomGen,
        UsePrngAlgorithm,
    }

    public static void Create()
    {
        CustomRegion.Default = ServerManager.DefaultRegions;

        ClientOption.Create();

        Roles.ExtremeRoleManager.GameRole.Clear();

		var optionMng = NewOptionManager.Instance;
		using (var commonOptionFactory = optionMng.CreateColorSyncOptionGroup(
			"CommonOption", defaultOptionColor))
		{
			commonOptionFactory.CreateIntOption(
				CommonOptionKey.PresetSelection,
				1, 1, maxPresetNum, 1,
				format: OptionUnit.Preset);
			commonOptionFactory.CreateBoolOption(
				CommonOptionKey.UseRaiseHand,　false);
		}

		using (var commonOptionFactory = optionMng.CreateColorSyncOptionGroup(
			"RandomOption", defaultOptionColor))
		{
			var strongGen = commonOptionFactory.CreateBoolOption(
				CommonOptionKey.UseStrongRandomGen,　true);
			commonOptionFactory.CreateSelectionOption(
				CommonOptionKey.UsePrngAlgorithm,
				[
					"Pcg32XshRr", "Pcg64RxsMXs",
					"Xorshift64", "Xorshift128",
					"Xorshiro256StarStar",
					"Xorshiro512StarStar",
					"RomuMono", "RomuTrio", "RomuQuad",
					"Seiran128", "Shioi128", "JFT32",
				],
				strongGen, invert: true);
		}

        IRoleSelector.CreateRoleGlobalOption();
        IShipGlobalOption.Create();

        Roles.ExtremeRoleManager.CreateNormalRoleOptions(
            singleRoleOptionStartOffset);

        Roles.ExtremeRoleManager.CreateCombinationRoleOptions(
            combRoleOptionStartOffset);

        GhostRoles.ExtremeGhostRoleManager.CreateGhostRoleOption(
            ghostRoleOptionStartOffset);


		CompatModManager.Instance.CreateIntegrateOption(IntegrateOptionStartOffset);
    }
}
