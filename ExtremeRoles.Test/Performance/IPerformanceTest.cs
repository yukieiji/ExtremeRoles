namespace ExtremeRoles.Test.Performance;

public interface IPerformanceTest
{
	public IEnumerator Prepare();

	public IEnumerator Run();

	public IEnumerator CleanUp();
}
