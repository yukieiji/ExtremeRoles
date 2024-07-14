namespace ExtremeRoles.Module;

#nullable enable

public class NullableSingleton<T> where T : class, new()
{
	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new T();
			}
			return instance;
		}
	}
	public static bool IsExist => instance != null;

	private static T? instance = null;

	public static void TryDestroy()
	{
		if (instance != null)
		{
			instance = null;
		}
	}

	public void Destroy()
	{
		instance = null;
	}
}