namespace ExtremeRoles.Module.Interface;

public interface IInfoOverlayPanelPageModel : IInfoOverlayPanelModel
{
				protected sealed record RoleInfo(string RoleName, string FullDec, int OptionId);

				public int PageNum { get; }

				public int CurPage { get; set; }
}
