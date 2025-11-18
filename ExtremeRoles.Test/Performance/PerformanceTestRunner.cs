using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BepInEx.Logging;

namespace ExtremeRoles.Test.Performance;

public static class PerformanceTestRunner
{
    private const int Iterations = 10;

    public static IEnumerator RunComparison(IEnumerable<IPerformanceTest> testSteps)
    {
        var logger = ExtremeRolesTestPlugin.Instance.Log;
        var results = new Dictionary<string, List<(long time, long memory)>>();

        logger.LogInfo("------- START PERFORMANCE TEST -------");

        for (int i = 0; i < Iterations; i++)
        {
            foreach (var step in testSteps)
            {
				yield return step.Prepare();
                var stopwatch = new Stopwatch();
                GC.Collect();
                long memoryBefore = GC.GetTotalMemory(true);

                stopwatch.Start();
                yield return step.Run();
                stopwatch.Stop();

                long memoryAfter = GC.GetTotalMemory(true);

				string key = step.GetType().Name;
				if (!results.TryGetValue(key, out var result))
				{
					result = new List<(long time, long memory)>();
				}

				result.Add((stopwatch.ElapsedMilliseconds, memoryAfter - memoryBefore));
				results[key] = result;

				yield return step.CleanUp();
            }
        }

        logger.LogInfo("------- END PERFORMANCE TEST -------");
        logResults(logger, results);
    }

    private static void logResults(ManualLogSource logger, Dictionary<string, List<(long time, long memory)>> results)
    {
        var summaryData = new Dictionary<string, (double avgTime, double medianTime, long maxTime, double avgMem, double medianMem, long maxMem)>();

        foreach (var (testName, records) in results)
        {

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"--- Test Results: {testName} ---");
            sb.AppendLine("| Run | Time (ms) | Memory (bytes) |");
            sb.AppendLine("|-----|-----------|----------------|");

            for(int i = 0; i < records.Count; i++)
            {
                sb.AppendLine($"| {i+1, -3} | {records[i].time, -9} | {records[i].memory, -14} |");
            }

            var times = records.Select(r => r.time).OrderBy(t => t).ToList();
            var memories = records.Select(r => r.memory).OrderBy(m => m).ToList();

            double avgTime = times.Average();
			double medianTime = (times[Iterations / 2 - 1] + times[Iterations / 2]) / 2.0;
			long maxTime = times.Last();

			double avgMem = memories.Average();
			double medianMem = (memories[Iterations / 2 - 1] + memories[Iterations / 2]) / 2.0;
			long maxMem = memories.Last();

            summaryData[testName] = (avgTime, medianTime, maxTime, avgMem, medianMem, maxMem);

            sb.AppendLine();
            sb.AppendLine("--- Statistics ---");
            sb.AppendLine("| Metric | Average   | Median    | Max       |");
            sb.AppendLine("|--------|-----------|-----------|-----------|");
            sb.AppendLine($"| Time   | {avgTime,-9:F2} | {medianTime,-9:F2} | {maxTime,-9} |");
            sb.AppendLine($"| Memory | {avgMem,-9:F2} | {medianMem,-9:F2} | {maxMem,-9} |");

            logger.LogInfo(sb.ToString());
        }

        var summarySb = new StringBuilder();
        summarySb.AppendLine();
        summarySb.AppendLine("------- COMPARISON SUMMARY -------");
        summarySb.AppendLine("| Test Name      | Avg Time (ms) | Median Time (ms) | Max Time (ms) | Avg Memory (bytes) | Median Memory (bytes) | Max Memory (bytes) |");
        summarySb.AppendLine("|----------------|---------------|------------------|---------------|--------------------|-----------------------|--------------------|");

        foreach(var data in summaryData)
        {
            summarySb.AppendLine(
                $"| {data.Key, -14} | {data.Value.avgTime, -13:F2} | {data.Value.medianTime, -16:F2} | {data.Value.maxTime, -13} | {data.Value.avgMem, -18:F2} | {data.Value.medianMem, -21:F2} | {data.Value.maxMem, -18} |");
        }

        logger.LogInfo(summarySb.ToString());
    }
}
