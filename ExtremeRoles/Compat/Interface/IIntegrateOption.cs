using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.Old.SequentialOptionCategoryFactory;

namespace ExtremeRoles.Compat.Interface;

public interface IIntegrateOption
{
	public void CreateIntegrateOption(OptionFactory factory) { }
}
