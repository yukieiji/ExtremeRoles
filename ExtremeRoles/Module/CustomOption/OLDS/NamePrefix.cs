using ExtremeRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.CustomOption.OLDS;

public static class NamePrefix
{
	public static string SidekickOptionPrefix => ExtremeRoleId.Sidekick.ToString();
	public static string DetectiveApprenticeOptionPrefix => ExtremeRoleId.DetectiveApprentice.ToString();
}
