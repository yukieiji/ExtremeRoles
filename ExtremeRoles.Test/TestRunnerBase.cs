using BepInEx.Logging;

namespace ExtremeRoles.Test;

internal abstract class TestRunnerBase
{
	public ManualLogSource Logger { protected get; set; }

	public abstract void Run();

	public abstract void Export();
}
