using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.SequentialOptionCategoryFactory;

namespace ExtremeRoles.Compat.Interface;

public interface IIntegrateOption
{
	public void CreateIntegrateOption(OptionFactory factory) { }
}
