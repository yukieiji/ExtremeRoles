using System.Collections;

namespace ExtremeRoles.Test.Performance
{
    public sealed class EmptyTestStep : IPerformanceTest
    {
        public IEnumerator Run()
        {
            yield break;
        }
    }
}
