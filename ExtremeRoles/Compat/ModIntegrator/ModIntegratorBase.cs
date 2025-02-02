using HarmonyLib;

using ExtremeRoles.Compat.Interface;

namespace ExtremeRoles.Compat.ModIntegrator;

#nullable enable

public abstract class ModIntegratorBase
{
    public SemanticVersioning.Version Version { get; }
	public string Name { get; }
// Harmonyクラスは消されるとパッチ周りが消えるのでとりあえずメンバ変数として保持
#pragma warning disable IDE0052 // 読み取られていないプライベート メンバーを削除
	private readonly Harmony patch;
#pragma warning restore IDE0052 // 読み取られていないプライベート メンバーを削除

	internal ModIntegratorBase(IInitializer initializer)
	{
		this.patch = initializer.Patch;
		this.Version = initializer.Version;
		this.Name = initializer.Name;
	}
}
