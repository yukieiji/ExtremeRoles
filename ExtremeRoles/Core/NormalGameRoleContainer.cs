using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Roles.API;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Core;

public class NormalGameRoleContainer : INomalGameRoleContainer
{
	public IReadOnlyDictionary<byte, SingleRoleBase> All => ExtremeRoleManager.GameRole;

	public void Dispose()
	{
	}

	public (T, T) GetInterfaceCastedLocalRole<T>() where T : class
		=> ExtremeRoleManager.GetInterfaceCastedLocalRole<T>();

	public (T, T) GetInterfaceCastedRole<T>(byte playerId) where T : class
		=> ExtremeRoleManager.GetInterfaceCastedRole<T>(playerId);

	public SingleRoleBase GetLocalPlayerRole()
		=> ExtremeRoleManager.GetLocalPlayerRole();

	public bool GetLocalRoleCastedStatusFlag<T>(Func<T, bool> statusFlag) where T : class
		=> ExtremeRoleManager.GetLocalRoleCastedStatusFlag(statusFlag);

	public bool GetRoleCastedStatusFlag<T>(SingleRoleBase role, Func<T, bool> statusFlag) where T : class
		=> ExtremeRoleManager.GetRoleCastedStatusFlag(role, statusFlag);

	public T GetSafeCastedLocalPlayerRole<T>() where T : SingleRoleBase
		=> ExtremeRoleManager.GetSafeCastedLocalPlayerRole<T>();

	public T GetSafeCastedRole<T>(byte playerId) where T : SingleRoleBase
		=> ExtremeRoleManager.GetSafeCastedRole<T>(playerId);

	public bool TryGetRole(byte playerId, [NotNullWhen(true)] out SingleRoleBase role)
		=> ExtremeRoleManager.TryGetRole(playerId, out role);

	public bool TryGetSafeCastedLocalRole<T>([NotNullWhen(true)] out T role) where T : SingleRoleBase
		=> ExtremeRoleManager.TryGetSafeCastedLocalRole(out role);

	public bool TryGetSafeCastedRole<T>(byte playerId, [NotNullWhen(true)] out T role) where T : SingleRoleBase
		=> ExtremeRoleManager.TryGetSafeCastedRole(playerId, out role);
}
