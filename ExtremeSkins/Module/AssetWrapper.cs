using ExtremeRoles.Performance;
using Innersloth.Assets;
using UnityEngine;
using ExtremeSkins.Module.Interface;
using ExtremeRoles.Module;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem;

namespace ExtremeSkins.Module;

[Il2CppRegister]
public sealed class HatAddressableAset : AddressableAsset<HatViewData>
{
	private CustomHat data;

	public HatAddressableAset(System.IntPtr ptr) : base(ptr)
	{ }

	public HatAddressableAset() : base(
		ClassInjector.DerivedConstructorPointer<HatAddressableAset>())
	{
		ClassInjector.DerivedConstructorBody(this);
	}

	public void Init(CustomHat data)
	{
		this.data = data;
	}

	public override HatViewData GetAsset()
	{
		return this.data.GetViewData();
	}
	public override void LoadAsync(
		Action onSuccessCb = null,
		Action onErrorcb = null,
		Action onFinishedcb = null)
	{
		if (onSuccessCb != null)
		{
			onSuccessCb.Invoke();
		}
		if (onFinishedcb != null)
		{
			onFinishedcb.Invoke();
		}
	}

	public override void Unload()
	{ }

	public override void Destroy()
	{ }

	public override AssetLoadState GetState() => AssetLoadState.Success;
}
