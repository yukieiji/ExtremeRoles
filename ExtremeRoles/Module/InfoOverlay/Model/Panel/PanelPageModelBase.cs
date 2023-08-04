using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using System.Collections.Generic;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public abstract class PanelPageModelBase : IInfoOverlayPanelModel
{
	protected readonly record struct RoleInfo(string RoleName, string FullDec, int OptionId);

	public int PageNum => this.allPage.Count;

	public int CurPage
	{
		get => this.curPage;
		set
		{
			if (value < 0)
			{
				value = this.PageNum - 1;
			}
			this.curPage = value % this.PageNum;
		}
	}

	private List<RoleInfo> allPage = new List<RoleInfo>();

	private int curPage = 0;

	public (string, string) GetInfoText()
	{
		if (this.PageNum == 0)
		{
			CreateAllRoleText();
		}

		var info = this.allPage[this.curPage];

		string colorRoleName = info.RoleName;

		var option = OptionManager.Instance.GetIOption(
			info.OptionId + (int)RoleCommonOption.SpawnRate);
		string roleOptionStr = option.ToHudStringWithChildren();

		return (
			$"<size=150%>・{colorRoleName}</size>\n{info.FullDec}",
			$"<size=115%>・{colorRoleName}{Translation.GetString("roleOption")}</size>\n{roleOptionStr}"
		);
	}

	protected void AddPage(RoleInfo info)
	{
		this.allPage.Add(info);
	}

	protected abstract void CreateAllRoleText();
}
