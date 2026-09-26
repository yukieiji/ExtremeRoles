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
			var useButton = mng.UseButton;
			if (useButton == null || useButton.transform == null || useButton.transform.parent == null)
			{
				return;
			}
			cachedArrange = useButton.transform.parent.gameObject.GetComponent<GridArrange>();
		}
		if (cachedArrange != null)
		{
			cachedArrange.ArrangeChilds();
		}
	}
}
