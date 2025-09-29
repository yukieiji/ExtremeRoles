using ExtremeRoles.Helper;

using System.Collections.Generic;

using ExtremeRoles.Module.CustomOption.Interfaces;
using System.Linq;
using System;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public abstract class RolePagePanelModelBase : IInfoOverlayPanelModel
{
	public bool ShowActiveOnly { private get; set; }
	protected readonly record struct RoleInfo(string RoleName, string FullDec, IOption Option);

	public int PageNum => this.curTarget.Count;

	public int CurPage
	{
		get => this.curPage;
		set
		{
			if (this.PageNum <= 0)
			{
				this.curPage = 0;
				return;
			}

			if (value < 0)
			{
				value = this.PageNum - 1;
			}
			this.curPage = value % this.PageNum;
		}
	}

	private IReadOnlyList<RoleInfo> curTarget => this.ShowActiveOnly ? curSettedRole : allPage;

	private readonly List<RoleInfo> curSettedRole = [];
	private readonly List<RoleInfo> allPage = [];

	private int curPage = 0;

	public void UpdateVisual()
	{
		this.curSettedRole.Clear();
	}

	public (string, string) GetInfoText()
	{
		if (this.PageNum == 0)
		{
			if (this.allPage.Count == 0)
			{
				CreateAllRoleText();
			}
			if (this.ShowActiveOnly)
			{
				RoleInfo? curShow = this.curTarget.Count == 0 ? null : this.curTarget[this.CurPage];
				createSettedRolePage();
				if (curShow.HasValue)
				{
					int newPage = this.curSettedRole.IndexOf(curShow.Value);
					this.CurPage = Math.Max(0, newPage);
				}
			}
		}
		if (this.curTarget.Count == 0)
		{
			return ("", "");
		}
		var info = this.curTarget[this.CurPage];

		string colorRoleName = info.RoleName;
		string roleOptionStr = IInfoOverlayPanelModel.ToHudStringWithChildren(info.Option);

		return (
			$"<size=150%>・{colorRoleName}</size>\n{info.FullDec}",
			$"<size=115%>・{colorRoleName}{Tr.GetString("roleOption")}</size>\n{roleOptionStr}"
		);
	}

	protected void AddPage(RoleInfo info)
	{
		this.allPage.Add(info);
	}

	protected abstract void CreateAllRoleText();

	private void createSettedRolePage()
	{
		this.curSettedRole.Clear();
		this.curSettedRole.Capacity = this.allPage.Count;
		foreach (var info in this.allPage)
		{
			var opt = info.Option;
			if (opt.IsEnable && !opt.Info.IsHidden)
			{
				this.curSettedRole.Add(info);
			}
		}
	}
}
