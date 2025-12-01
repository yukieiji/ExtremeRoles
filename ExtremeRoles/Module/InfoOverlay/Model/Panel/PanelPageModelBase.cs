using System;
using System.Collections.Generic;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.Interface;

public interface IOptionToStringHelper
{
	public bool IsActive { get; }

	public string ToString();
}

#nullable enable

public abstract class RolePagePanelModelBase : IInfoOverlayPanelModel
{
	public sealed class DefaultOptionToString(IOption option) : IOptionToStringHelper
	{
		private readonly IOption option = option;

		public bool IsActive => !option.Info.IsHidden && option.IsChangeDefault;

		public override string ToString()
			=> IInfoOverlayPanelModel.ToHudStringWithChildren(option);
	}


	public bool ShowActiveOnly
	{
		set
		{
			this.prevShow = this.curTarget.Count == 0 ? null : this.curTarget[this.CurPage];
			this.showActiveOnly = value;
		}
	}
	protected readonly record struct RoleInfo(string RoleName, string FullDec, IOptionToStringHelper Option);

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

	private List<RoleInfo> curTarget => this.showActiveOnly ? curSettedRole : allPage;

	private readonly List<RoleInfo> curSettedRole = [];
	private readonly List<RoleInfo> allPage = [];

	private int curPage = 0;
	private bool showActiveOnly = false;
	private RoleInfo? prevShow;

	// 何回も呼ばれるのでここではクリアだけしておく
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
			if (this.showActiveOnly)
			{
				createSettedRolePage();
			}
		}
		if (this.curTarget.Count == 0)
		{
			return ("", "");
		}

		if (this.prevShow.HasValue)
		{
			int newPage = this.curTarget.IndexOf(this.prevShow.Value);
			this.CurPage = Math.Max(0, newPage);
			this.prevShow = null;
		}

		var info = this.curTarget[this.CurPage];

		string colorRoleName = info.RoleName;
		string roleOptionStr = info.Option.ToString();

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
			if (info.Option.IsActive)
			{
				this.curSettedRole.Add(info);
			}
		}
	}
}
