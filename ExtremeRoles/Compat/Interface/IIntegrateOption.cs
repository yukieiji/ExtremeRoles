using OptionFactory = ExtremeRoles.Core.Service.CustomOption.Factory.SequentialOptionCategoryFactory;

namespace ExtremeRoles.Compat.Interface;

public interface IIntegrateOption
{
	public void CreateIntegrateOption(OptionFactory factory) { }
}
