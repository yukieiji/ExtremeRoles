using Innersloth.Assets;

using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem;

using ExtremeRoles.Module;

namespace ExtremeSkins.Module;

[Il2CppRegister]
public sealed class HatAddressableAsset : AddressableAsset<HatViewData>
{
	private CustomHatProvider data;

#pragma warning disable CS8618
	public HatAddressableAsset(System.IntPtr ptr) : base(ptr)
	{ }

	public HatAddressableAsset() : base(
		ClassInjector.DerivedConstructorPointer<HatAddressableAsset>())
	{
		ClassInjector.DerivedConstructorBody(this);
	}
#pragma warning restore CS8618

	public static AddressableAsset<HatViewData> CreateAsset(CustomHatProvider data)
	{
		var asset = new HatAddressableAsset();
		asset.Init(data);
		return asset.Cast<AddressableAsset<HatViewData>>();
	}

	[HideFromIl2Cpp]
	public void Init(CustomHatProvider data)
	{
		this.data = data;
	}

	public override HatViewData GetAsset()
	{
		return this.data.GetViewData();
	}
	public override void LoadAsync(
		Action? onSuccessCb = null,
		Action? onErrorcb = null,
		Action? onFinishedcb = null)
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
	}

	public override void Destroy()
	{ }

	public override AssetLoadState GetState() => AssetLoadState.Success;
}

[Il2CppRegister]
public sealed class NamePlateAddressableAsset : AddressableAsset<NamePlateViewData>
{

	private CustomNamePlate data;

#pragma warning disable CS8618
	public NamePlateAddressableAsset(System.IntPtr ptr) : base(ptr)
	{ }

	public NamePlateAddressableAsset() : base(
		ClassInjector.DerivedConstructorPointer<NamePlateAddressableAsset>())
	{
		ClassInjector.DerivedConstructorBody(this);
	}
#pragma warning restore CS8618

	public static AddressableAsset<NamePlateViewData> CreateAsset(CustomNamePlate data)
	{
		var asset = new NamePlateAddressableAsset();
		asset.Init(data);
		return asset.Cast<AddressableAsset<NamePlateViewData>>();
	}

	[HideFromIl2Cpp]
	public void Init(CustomNamePlate data)
	{
		this.data = data;
	}

	public override NamePlateViewData GetAsset()
	{
		return this.data.GetViewData();
	}
	public override void LoadAsync(
		Action? onSuccessCb = null,
		Action? onErrorcb = null,
		Action? onFinishedcb = null)
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
	}

	public override void Destroy()
	{ }

	public override AssetLoadState GetState() => AssetLoadState.Success;
}

[Il2CppRegister]
public sealed class VisorAddressableAsset : AddressableAsset<VisorViewData>
{

	private CustomVisor data;

#pragma warning disable CS8618
	public VisorAddressableAsset(System.IntPtr ptr) : base(ptr)
	{ }

	public VisorAddressableAsset() : base(
		ClassInjector.DerivedConstructorPointer<VisorAddressableAsset>())
	{
		ClassInjector.DerivedConstructorBody(this);
	}
#pragma warning restore CS8618

	public static AddressableAsset<VisorViewData> CreateAsset(CustomVisor data)
	{
		var asset = new VisorAddressableAsset();
		asset.Init(data);
		return asset.Cast<AddressableAsset<VisorViewData>>();
	}

	[HideFromIl2Cpp]
	public void Init(CustomVisor data)
	{
		this.data = data;
	}

	public override VisorViewData GetAsset()
	{
		return this.data.GetViewData();
	}
	public override void LoadAsync(
		Action? onSuccessCb = null,
		Action? onErrorcb = null,
		Action? onFinishedcb = null)
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
	}

	public override void Destroy()
	{ }

	public override AssetLoadState GetState() => AssetLoadState.Success;
}
