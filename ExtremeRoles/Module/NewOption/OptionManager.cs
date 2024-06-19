using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.NewOption;

public sealed class OptionManager
{
	public readonly static OptionManager Instance = new ();

	private readonly Dictionary<OptionTab, OptionTabContainer> options = new ();

	private OptionManager()
	{
		foreach (var tab in Enum.GetValues<OptionTab>())
		{
			options.Add(tab, new OptionTabContainer(tab));
		}
	}
}
