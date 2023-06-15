using ExtremeRoles.Performance;
using Innersloth.Assets;
using UnityEngine;
using ExtremeSkins.Module.Interface;
using ExtremeRoles.Module;

namespace ExtremeSkins.Module;

public sealed class AddressableAssetWrapper<T, W> : AddressableAsset<W>
	where T : CosmeticData
	where W : ScriptableObject
{
	private ICustomCosmicData<T, W> data;
	public AddressableAssetWrapper(ICustomCosmicData<T, W> data) : base()
	{
		this.data = data;
	}

	public override W GetAsset()
	{
		return this.data.GetViewData();
	}

	public override void Unload()
	{ }

	public override void Destroy()
	{ }

	public override AssetLoadState GetState() => AssetLoadState.Success;
}
