#nullable enable

namespace ExtremeRoles.Extension.Manager;

public static class HudManagerExtension
{
	private static GridArrange? cachedArrange = null;

	public static void ReGridButtons(this HudManager? mng)
    {
		if (mng == null) { return; }

		if (cachedArrange == null)
		{
			cachedArrange = mng.UseButton.transform.parent.gameObject.GetComponent<GridArrange>();
		}
		cachedArrange.ArrangeChilds();
	}
}
