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

        for (int i = 0; i < actualLines.Count; i++)
        {
            string? line = actualLines[i];
            var parts = line.Split(' ');

        }

        return fullResult.ToString().TrimEnd('\n').TrimEnd('\r');
    }

    private string SolveLevel3(List<string> lines)
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