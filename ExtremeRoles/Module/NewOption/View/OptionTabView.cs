using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.NewOption.View;

public sealed class OptionTabView(OptionTab tab, in OptionGroupView[] optionGroupView)
{
	private OptionTab tab = tab;
	private readonly OptionGroupView[] allOptionGroupView = optionGroupView;

	private const float posOffsetInit = 2.75f;

	public void Update()
	{
		float pos = posOffsetInit;
		foreach (OptionGroupView view in allOptionGroupView)
		{
			view.Update(in pos);
		}
		// this.scroller.ContentYBounds.max = -4.0f + pos * 0.5f;

	}
}
