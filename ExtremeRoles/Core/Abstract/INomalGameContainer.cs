using ExtremeRoles.Roles.API;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ExtremeRoles.Core.Abstract;

public interface INomalGameRoleContainer : IDisposable
{
	public IReadOnlyDictionary<byte, SingleRoleBase> All { get; }

	public SingleRoleBase GetLocalPlayerRole();

	// TryGet系列：ここでTrueの場合、取得した役職はNullではない！！！！
	public bool TryGetRole(byte playerId, [NotNullWhen(true)] out SingleRoleBase? role);

	public bool TryGetSafeCastedRole<T>(byte playerId, [NotNullWhen(true)] out T? role) where T : SingleRoleBase;

	public bool TryGetSafeCastedLocalRole<T>([NotNullWhen(true)] out T? role) where T : SingleRoleBase;


	public T? GetSafeCastedRole<T>(byte playerId) where T : SingleRoleBase;

	public T? GetSafeCastedLocalPlayerRole<T>() where T : SingleRoleBase;

	public bool GetRoleCastedStatusFlag<T>(SingleRoleBase role, Func<T, bool> statusFlag) where T : class; 

	public bool GetLocalRoleCastedStatusFlag<T>(Func<T, bool> statusFlag)
		where T : class;

	public (T?, T?) GetInterfaceCastedLocalRole<T>() where T : class;

	public (T?, T?) GetInterfaceCastedRole<T>(byte playerId) where T : class;
}