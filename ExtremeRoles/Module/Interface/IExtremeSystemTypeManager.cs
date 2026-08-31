using System;
using System.Diagnostics.CodeAnalysis;
using Hazel;
using ExtremeRoles.Module.SystemType;

using Il2CppByteArry = Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>;

#nullable enable

namespace ExtremeRoles.Module.Interface;

public interface IExtremeSystemTypeManager : IAmongUs.ISystemType
{
	public bool IsActiveSpecialSabotage { get; }

	public bool ExistSystem(ExtremeSystemType type);

	public bool TryGet(ExtremeSystemType systemType, [NotNullWhen(true)] out IExtremeSystemType? system);
	public T CreateOrGet<T>(ExtremeSystemType systemType) where T : class, IExtremeSystemType, new();
	public T CreateOrGet<T>(ExtremeSystemType systemType, Func<T> construnctFunc) where T : class, IExtremeSystemType;
	public bool TryGet<T>(ExtremeSystemType systemType, [NotNullWhen(true)] out T? system) where T : class, IExtremeSystemType;
	public bool TryAdd(ExtremeSystemType systemType, IExtremeSystemType system);

	public void Reset(PlayerControl? player, byte amount);
	public void RemoveSystem();
	public void UpdateSystem(Il2CppByteArry data);
}
