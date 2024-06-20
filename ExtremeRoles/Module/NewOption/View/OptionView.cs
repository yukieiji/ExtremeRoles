using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Module.NewOption.View;

public sealed class OptionView(
	int groupId,
	in OptionTabView tab,
	in IOptionInfo option)
{
	private readonly IOptionInfo option = option;
	private readonly OptionTabView tab = tab;

	public void Update()
	{
		// SelectionUpdate
		this.tab.Update();
		// Resync
	}
}
