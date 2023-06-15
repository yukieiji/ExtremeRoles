using UnityEngine;

namespace ExtremeSkins.Module.Interface;

public interface ICustomCosmicData<T, C>
	where C : ScriptableObject
	where T : CosmeticData
{
    public T Data { get; }

    public string Author { get; }
    public string Name { get; }

    public string Id { get; }

	public C GetViewData();

    public T GetData();
}
