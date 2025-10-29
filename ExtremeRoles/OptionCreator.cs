using System;
using Microsoft.Extensions.DependencyInjection;

using UnityEngine;

using ExtremeRoles.Compat;

using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;


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

    public static void Create(IServiceProvider provider)
    {
		// CustomRegion.Default = ServerManager.DefaultRegions;

		ClientOption.Create();

        Roles.ExtremeRoleManager.GameRole.Clear();

		var assembler = provider.GetRequiredService<OptionCategoryAssembler>();
		PresetOption.Create(assembler, CommonOption.PresetOption.ToString());

		using (var cate = assembler.CreateOptionCategory(
			CommonOption.RandomOption, color: DefaultOptionColor))
		{
			var builder = cate.Builder;
			var strongGen = builder.CreateBoolOption(
				RandomOptionKey.UseStrong,　true);
			builder.CreateSelectionOption(
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

        IRoleSelector.CreateRoleGlobalOption(assembler);
        IShipGlobalOption.Create(assembler);

		var factory = provider.GetRequiredService<AutoRoleOptionCategoryFactory>();
		Roles.ExtremeRoleManager.CreateNormalRoleOptions(factory);
        Roles.ExtremeRoleManager.CreateCombinationRoleOptions(factory);
        GhostRoles.ExtremeGhostRoleManager.CreateGhostRoleOption(factory);


		CompatModManager.Instance.CreateIntegrateOption(IntegrateOptionStartOffset);
    }
}
