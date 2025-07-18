namespace ExtremeRoles.Roles.API.Systems;

public static class Common
{
	public static bool IsForceInfoBlockRole(SingleRoleBase role)
		=> role.IsImpostor() || isForceInfoBlockRoleIds(role.Core.Id);

	public static bool IsForceInfoBlockRoleWithoutAssassin(SingleRoleBase role)
	{
		var id = role.Core.Id;
		return 
			(role.IsImpostor() && id is not ExtremeRoleId.Assassin) ||
			isForceInfoBlockRoleIds(id);
	}
		

	private static bool isForceInfoBlockRoleIds(ExtremeRoleId checkId)
		=> checkId is ExtremeRoleId.Totocalcio or ExtremeRoleId.Madmate or ExtremeRoleId.Doll or ExtremeRoleId.Heretic;
}
