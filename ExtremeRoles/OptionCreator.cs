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
    private const int maxPresetNum = 20;

    public static readonly string[] Range = [ "short", "middle", "long" ];

    private static Color defaultOptionColor => new Color(204f / 255f, 204f / 255f, 0, 1f);

    public enum PresetOptionKey : int
    {
        PresetSelection = 0,
    }

	public enum RandomOptionKey : int
	{
		UseStrong = 0,
		Algorithm,
	}

	public enum CommonOption : int
	{
		Preset,
		RandomOption
	}

    public static void Create()
    {
        CustomRegion.Default = ServerManager.DefaultRegions;

        ClientOption.Create();

        Roles.ExtremeRoleManager.GameRole.Clear();

		using (var commonOptionFactory = NewOptionManager.CreateOptionCategory(
			CommonOption.Preset, color: defaultOptionColor))
		{
			commonOptionFactory.CreateIntOption(
				PresetOptionKey.PresetSelection,
				1, 1, maxPresetNum, 1,
				format: OptionUnit.Preset);
		}

		using (var commonOptionFactory = NewOptionManager.CreateOptionCategory(
			CommonOption.RandomOption, color: defaultOptionColor))
		{
			var strongGen = commonOptionFactory.CreateBoolOption(
				RandomOptionKey.UseStrong,　true);
			commonOptionFactory.CreateSelectionOption(
				RandomOptionKey.Algorithm,
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

        Roles.ExtremeRoleManager.CreateNormalRoleOptions();
        Roles.ExtremeRoleManager.CreateCombinationRoleOptions();
        GhostRoles.ExtremeGhostRoleManager.CreateGhostRoleOption();


		CompatModManager.Instance.CreateIntegrateOption(IntegrateOptionStartOffset);
    }
}
