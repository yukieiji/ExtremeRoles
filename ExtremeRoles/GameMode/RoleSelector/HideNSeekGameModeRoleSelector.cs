using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;


namespace ExtremeRoles.GameMode.RoleSelector;

public sealed class HideNSeekGameModeRoleSelector : IRoleSelector
{
    public bool IsAdjustImpostorNum => true;

    public bool CanUseXion => true;

	public bool IsVanillaRoleToMultiAssign => true;

    public IEnumerable<ExtremeRoleId> UseNormalRoleId
    {
        get
        {
            foreach (ExtremeRoleId id in getUseNormalId())
            {
                yield return id;
            }
        }
    }
    public IEnumerable<CombinationRoleType> UseCombRoleType
    {
		get
		{
			foreach (CombinationRoleType id in getUseCombRoleType())
			{
				yield return id;
			}
		}
	}
    public IEnumerable<ExtremeGhostRoleId> UseGhostRoleId
    {
        get
        {
            yield break;
        }
    }

	private readonly HashSet<int> roleCategoryGroup = new HashSet<int>();

	public HideNSeekGameModeRoleSelector()
    {
		var gen = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IRoleParentOptionIdGenerator>();
		foreach (ExtremeRoleId id in getUseNormalId())
		{
			this.roleCategoryGroup.Add(gen.Get(id));
		}
    }

	public bool IsValidCategory(int categoryId)
		=> this.roleCategoryGroup.Contains(categoryId);

	private static ExtremeRoleId[] getUseNormalId() =>
		[
            ExtremeRoleId.SpecialCrew,
            ExtremeRoleId.Neet,
            ExtremeRoleId.Watchdog,
            ExtremeRoleId.Supervisor,
            ExtremeRoleId.Survivor,
            ExtremeRoleId.Resurrecter,
            ExtremeRoleId.Teleporter,

            ExtremeRoleId.BountyHunter,
            ExtremeRoleId.Bomber,
            ExtremeRoleId.LastWolf,
            ExtremeRoleId.Hypnotist,
            ExtremeRoleId.Slime,
			ExtremeRoleId.Terorist,
			ExtremeRoleId.Raider,
			ExtremeRoleId.Hijacker,
			ExtremeRoleId.TimeBreaker,

			ExtremeRoleId.IronMate,
			ExtremeRoleId.Heretic,
		];
	private CombinationRoleType[] getUseCombRoleType() =>
		[
			CombinationRoleType.Accelerator,
		];
}
