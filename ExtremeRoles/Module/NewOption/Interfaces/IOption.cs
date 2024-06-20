
namespace ExtremeRoles.Module.NewOption.Interfaces;

public interface IOption
{
	public IOptionInfo Info { get; }
	public IOptionRelation Relation { get; }

	public string Title { get; }
	public string ValueString { get; }

	public int Range { get; }
	public int Selection { get; set; }

	public bool IsEnable { get; }
	public bool IsActiveAndEnable { get; }

	public void SwitchPreset();
}
