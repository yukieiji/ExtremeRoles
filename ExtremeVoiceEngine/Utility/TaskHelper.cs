using System.Collections;
using System.Threading.Tasks;

namespace ExtremeVoiceEngine.Utility;

public static class TaskHelper
{
    public static IEnumerator CoRunWaitAsync(Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }
    }
}
