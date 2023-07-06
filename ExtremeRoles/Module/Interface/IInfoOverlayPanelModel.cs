namespace ExtremeRoles.Module.Interface;

public interface IInfoOverlayPanelModel
{
	public string Title { get; }

	public (string, string) GetInfoText();
}
