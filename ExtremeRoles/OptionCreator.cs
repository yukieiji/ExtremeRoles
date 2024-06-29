using UnityEngine;

using ExtremeRoles.Compat;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.Implemented;



namespace ExtremeRoles;

public static class OptionCreator
{
	public const int IntegrateOptionStartOffset = 40000;

    public static readonly string[] Range = [ "short", "middle", "long" ];

    public static Color DefaultOptionColor => new Color(204f / 255f, 204f / 255f, 0, 1f);

    public enum PresetOptionKey : int
    {
        Selection = 0,
    }

	public enum RandomOptionKey : int
	{
		UseStrong = 0,
		Algorithm,
	}

	public enum CommonOption : int
	{
		PresetOption,
		RandomOption
	}

    public static void Create()
    {
        CustomRegion.Default = ServerManager.DefaultRegions;

        ClientOption.Create();

        Roles.ExtremeRoleManager.GameRole.Clear();

		PresetOption.Create(CommonOption.PresetOption.ToString());

		using (var commonOptionFactory = OptionManager.CreateOptionCategory(
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
