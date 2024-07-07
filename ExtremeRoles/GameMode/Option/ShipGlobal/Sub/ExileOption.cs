using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;
using ExtremeRoles.Module.CustomOption.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public enum ExiledOption : int
{
	ConfirmExilMode,
	IsConfirmRole,
}

public enum ConfirmExileMode : byte
{
	Impostor,
	Crewmate,
	Neutral,
	AllTeam
}

public readonly struct ExileOption(in OptionCategory category)
{
	public readonly ConfirmExileMode Mode = (ConfirmExileMode)category.GetValue<int>((int)ExiledOption.ConfirmExilMode);
	public readonly bool IsConfirmRole = category.GetValue<bool>((int)ExiledOption.IsConfirmRole);

	public static void Create(in OptionCategoryFactory factory)
	{
		factory.CreateSelectionOption<ExiledOption, ConfirmExileMode>(ExiledOption.ConfirmExilMode);
		factory.CreateBoolOption(ExiledOption.IsConfirmRole, false);
	}
}
