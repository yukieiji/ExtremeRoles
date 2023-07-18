using ExtremeRoles.Performance;
using Innersloth.Assets;
using UnityEngine;
using ExtremeSkins.Module.Interface;
using ExtremeRoles.Module;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem;

namespace ExtremeSkins.Module;

[Il2CppRegister]
public sealed class HatAddressableAsset : AddressableAsset<HatViewData>
{
	private CustomHat data;

	public HatAddressableAsset(System.IntPtr ptr) : base(ptr)
	{ }

	public HatAddressableAsset() : base(
		ClassInjector.DerivedConstructorPointer<HatAddressableAsset>())
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
	{
		this.data.Release();
	}

	public override void Destroy()
	{ }

	public override AssetLoadState GetState() => AssetLoadState.Success;
}

[Il2CppRegister]
public sealed class NamePlateAddressableAsset : AddressableAsset<NamePlateViewData>
{
	private CustomNamePlate data;

	public NamePlateAddressableAsset(System.IntPtr ptr) : base(ptr)
	{ }

	public NamePlateAddressableAsset() : base(
		ClassInjector.DerivedConstructorPointer<NamePlateAddressableAsset>())
	{
		ClassInjector.DerivedConstructorBody(this);
	}

	public void Init(CustomNamePlate data)
	{
		this.data = data;
	}

	public override NamePlateViewData GetAsset()
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
	{
		this.data.Release();
	}

	public override void Destroy()
	{ }

	public override AssetLoadState GetState() => AssetLoadState.Success;
}
