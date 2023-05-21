using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles;

public static class OptionCreator
{
    private const int singleRoleOptionStartOffset = 256;
    private const int combRoleOptionStartOffset = 5000;
    private const int ghostRoleOptionStartOffset = 10000;
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

        new IntCustomOption(
            (int)CommonOptionKey.PresetSelection, Design.ColoedString(
                defaultOptionColor,
                CommonOptionKey.PresetSelection.ToString()),
            1, 1, maxPresetNum, 1, null, true,
            format: OptionUnit.Preset);

        var strongGen = new BoolCustomOption(
            (int)CommonOptionKey.UseStrongRandomGen, Design.ColoedString(
                defaultOptionColor,
                CommonOptionKey.UseStrongRandomGen.ToString()), true);
        new SelectionCustomOption(
            (int)CommonOptionKey.UsePrngAlgorithm, Design.ColoedString(
                defaultOptionColor,
                CommonOptionKey.UsePrngAlgorithm.ToString()),
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
    }
}
