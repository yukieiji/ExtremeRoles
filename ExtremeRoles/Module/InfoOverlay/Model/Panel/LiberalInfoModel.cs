using System;
using System.Text;

using ExtremeRoles.Helper;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Compat;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.Option.ShipGlobal;


using ExtremeRoles.Roles;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using ExtremeRoles.Module.CustomOption.Interfaces;



namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class LiberalInfoModel : IInfoOverlayPanelModel
{
	private StringBuilder printOption = new StringBuilder();

	public (string, string) GetInfoText()
	{
		this.printOption.Clear();
		var liberalSetting = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptipnLoader>();


		this.printOption.AppendLine();
		addHudString(this.printOption, liberalSetting.GlobalOption);

		this.printOption.AppendLine();
		addHudString(this.printOption, liberalSetting.LeaderOption);

		this.printOption.AppendLine();

		return (
			Tr.GetString("liberalGlobalInfo"),
			$"<size=135%>{Tr.GetString("gameOption")}</size>\n\n{this.printOption}"
		);
	}

	private static void addHudString(StringBuilder builder, IReadOnlyList<IOption> options)
	{
		foreach (var target in options)
		{
			IInfoOverlayPanelModel.AddHudStringWithChildren(builder, target, 0);
		}
	}

	public void UpdateVisual()
	{

	}
}
