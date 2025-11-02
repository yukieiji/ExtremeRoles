using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.Old.OldSequentialOptionCategoryFactory;

namespace ExtremeRoles.Compat.Interface;

public interface IIntegrateOption
{
	public void CreateIntegrateOption(OptionFactory factory) { }
}
