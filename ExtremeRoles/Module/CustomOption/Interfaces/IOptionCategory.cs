using System.Text;

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionCategory
{
	public int Id { get; }
	public bool IsDirty { get; set; }
	public IOptionCategoryViweInfo View { get; }
	public IOptionLoader Loader { get; }

	public void AddHudString(in StringBuilder builder);
}
