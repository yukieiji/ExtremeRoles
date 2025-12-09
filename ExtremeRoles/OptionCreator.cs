using UnityEngine;

using ExtremeRoles.Compat;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.OLDS;
using ExtremeRoles.Module.CustomOption.Factory;



namespace ExtremeRoles;

public static class OptionCreator
{
	public const int IntegrateOptionStartOffset = 40000;

    public static readonly string[] Range = ["SettingShort", "SettingMedium", "SettingLong"];

    public static Color DefaultOptionColor => new Color(204f / 255f, 204f / 255f, 0, 1f);

    public enum PresetOptionKey
    {
        Selection = 0,
    }

	public enum RandomOptionKey
	{
		UseStrong = 0,
		Algorithm,
	}

	public enum CommonOption
	{
		PresetOption,
		RandomOption
	}

	public static bool IsCommonOption(int id)
		=> (CommonOption)id is CommonOption.PresetOption or CommonOption.RandomOption;

    public static void Create()
    {
		// CustomRegion.Default = ServerManager.DefaultRegions;

		ClientOption.Create();

        Roles.ExtremeRoleManager.GameRole.Clear();

		PresetOption.Create(CommonOption.PresetOption.ToString());

		using (var commonOptionFactory = OptionCategoryAssembler.CreateOptionCategory(
			CommonOption.RandomOption, color: DefaultOptionColor))
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
				], new InvertActive(strongGen));
		}

        IRoleSelector.CreateRoleGlobalOption();
        IShipGlobalOption.Create();

        Roles.ExtremeRoleManager.CreateNormalRoleOptions();
        Roles.ExtremeRoleManager.CreateCombinationRoleOptions();
        GhostRoles.ExtremeGhostRoleManager.CreateGhostRoleOption();


		CompatModManager.Instance.CreateIntegrateOption(IntegrateOptionStartOffset);
    }
}
