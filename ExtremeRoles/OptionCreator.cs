using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Compat;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;

namespace ExtremeRoles;

public static class OptionCreator
{
    private const int singleRoleOptionStartOffset = 256;
    private const int combRoleOptionStartOffset = 5000;
    private const int ghostRoleOptionStartOffset = 10000;
	private const int integrateOptionStartOffset = 15000;
    private const int maxPresetNum = 20;

    public static readonly string[] SpawnRate = new string[] {
        "0%", "10%", "20%", "30%", "40%",
        "50%", "60%", "70%", "80%", "90%", "100%" };

    public static readonly string[] Range = new string[] { "short", "middle", "long" };

    private static Color defaultOptionColor = new Color(204f / 255f, 204f / 255f, 0, 1f);

    public enum CommonOptionKey : int
    {
        PresetSelection = 0,

        UseStrongRandomGen,
        UsePrngAlgorithm,
    }

    public static void Create()
    {
        CustomRegion.Default = ServerManager.DefaultRegions;

        ClientOption.Create();

        Roles.ExtremeRoleManager.GameRole.Clear();

		var commonOptionFactory = new ColorSyncFactory(defaultOptionColor);

		commonOptionFactory.CreateIntOption(
			CommonOptionKey.PresetSelection,
			1, 1, maxPresetNum, 1,
			isHeader: true,
			format: OptionUnit.Preset);

		var strongGen = commonOptionFactory.CreateBoolOption(
			CommonOptionKey.UseStrongRandomGen,
			true);
		commonOptionFactory.CreateSelectionOption(
			CommonOptionKey.UsePrngAlgorithm,
			new string[]
			{
				"Pcg32XshRr", "Pcg64RxsMXs",
				"Xorshift64", "Xorshift128",
				"Xorshiro256StarStar",
				"Xorshiro512StarStar",
				"RomuMono", "RomuTrio", "RomuQuad",
				"Seiran128", "Shioi128", "JFT32",
			},
			strongGen, invert: true);

        IRoleSelector.CreateRoleGlobalOption();
        IShipGlobalOption.Create();

        Roles.ExtremeRoleManager.CreateNormalRoleOptions(
            singleRoleOptionStartOffset);

        Roles.ExtremeRoleManager.CreateCombinationRoleOptions(
            combRoleOptionStartOffset);

        GhostRoles.ExtremeGhostRoleManager.CreateGhostRoleOption(
            ghostRoleOptionStartOffset);


		CompatModManager.Instance.CreateIntegrateOption(integrateOptionStartOffset);
    }
}
