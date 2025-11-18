using System.Collections;

namespace ExtremeRoles.Test.Performance;

public sealed class EmptyTestStep : IPerformanceTest
{
	public IEnumerator CleanUp()
	{
		yield break;
	}

	public IEnumerator Prepare()
	{
		yield break;
	}

	public IEnumerator Run()
    {
        yield break;
    }
}
