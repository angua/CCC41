using System.Text;

namespace CCC41Lib;

public class Solver
{
    public string Solve(int level, List<string> lines)
    {
        return level switch
        {
            1 => SolveLevel1(lines),
            2 => SolveLevel2(lines),
            3 => SolveLevel3(lines),
            4 => SolveLevel4(lines),
            5 => SolveLevel5(lines),
            6 => SolveLevel6(lines),
            7 => SolveLevel7(lines),
            _ => throw new InvalidOperationException(($"Level {level} not supported."))
        };
    }

    private string SolveLevel1(List<string> lines)
    {
        var actualLines = lines.Skip(1);
        var fullResult = new StringBuilder();

        foreach (var line in actualLines)
        {
            var parts = line.Split(' ');

            var sum = parts.Select(int.Parse).Sum();

            var resultLine = sum.ToString();
            fullResult.AppendLine(resultLine);
        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }

    private string SolveLevel2(List<string> lines)
    {
        var actualLines = lines.Skip(1).ToList();
        var fullResult = new StringBuilder();

        foreach (var line in actualLines)
        {
            var parts = line.Split(' ');

            var position = parts.Select(int.Parse).Select(i => i > 0 ? 1 : (i < 0 ? -1 : 0)).Sum();
            var timeTaken = parts.Select(int.Parse).Select(i => i == 0 ? 1 : Math.Abs(i)).Sum();

            var resultLine = $"{position} {timeTaken}";
            fullResult.AppendLine(resultLine);
        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r') + Environment.NewLine;
    }

    private string SolveLevel3(List<string> lines)
    {
        var actualLines = lines.Skip(1).ToList();
        var fullResult = new StringBuilder();

        foreach (var line in actualLines)
        {
            var parts = line.Split(' ');

            var stationPos = int.Parse(parts[0]);
            var timeLimit = int.Parse(parts[1]);

            var sequence = new List<int>();
            sequence = CalculateSequence(stationPos, timeLimit);

            var resultLine = string.Join(' ', sequence.Select(i => i.ToString()));
            fullResult.AppendLine(resultLine);
        }

        return fullResult.ToString() + Environment.NewLine;
    }
    private List<int> CalculateSequence(int stationPos, int timeLimit)
    {
        var result = new List<int> { 0 };

        if (Math.Abs(stationPos) >= 9)
        {
            result.AddRange( [ 5, 4, 3, 2, 1 ]); // accel

            int additionalOnes = Math.Abs(stationPos) - 9;

            for (int i = 0; i < additionalOnes; i++)
            {
                result.Add(1);
            }

            result.AddRange( [ 2, 3, 4, 5 ]); // decel
        }
        else // pos < 9
        {
            var steps = new List<int> { 5, 4, 3, 2, 1, 2, 3, 4, 5 };
            var toRemove = 9 - Math.Abs(stationPos);

            for (int i = 0; i < toRemove; i++)
            {
                var currentMin = steps.Min();
                steps.RemoveAt(steps.IndexOf(currentMin));
            }

            result.AddRange(steps);
        }

        if (stationPos < 0)
        {
            for (int i = 0; i < result.Count; i++)
            {
                result[i] *= -1;
            }
        }

        result.Add(0);

        return result;
    }

    private string SolveLevel4(List<string> lines)
    {
        var actualLines = lines.Skip(1).ToList();
        var fullResult = new StringBuilder();

        for (int i = 0; i < actualLines.Count; i++)
        {
            string? line = actualLines[i];
            var parts = line.Split(' ');
        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }

    private string SolveLevel5(List<string> lines)
    {
        var actualLines = lines.Skip(1).ToList();
        var fullResult = new StringBuilder();

        for (int i = 0; i < actualLines.Count; i++)
        {
        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }

    private string SolveLevel6(List<string> lines)
    {
        var actualLines = lines.Skip(1).ToList();
        var fullResult = new StringBuilder();

        for (int i = 0; i < actualLines.Count; i++)
        {
            string? line = actualLines[i];
            var parts = line.Split(' ');

        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }

    private string SolveLevel7(List<string> lines)
    {
        var actualLines = lines.Skip(1).ToList();
        var fullResult = new StringBuilder();

        for (int i = 0; i < actualLines.Count; i++)
        {
            string? line = actualLines[i];
            var parts = line.Split(' ');

        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }

}