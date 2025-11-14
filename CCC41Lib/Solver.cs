using System.Numerics;
using System.Text;

namespace CCC41Lib;

public partial class Solver
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
            result.AddRange([5, 4, 3, 2, 1]); // accel

            int additionalOnes = Math.Abs(stationPos) - 9;

            for (int i = 0; i < additionalOnes; i++)
            {
                result.Add(1);
            }

            result.AddRange([2, 3, 4, 5]); // decel
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
            var parts = line.Split(new char[] { ' ', ',' });

            var stationPosX = int.Parse(parts[0]);
            var stationPosY = int.Parse(parts[1]);
            var timeLimit = int.Parse(parts[2]);

            var sequenceX = CalculateSequence(stationPosX, timeLimit);
            var sequenceY = CalculateSequence(stationPosY, timeLimit);

            var resultLine = string.Join(' ', sequenceX.Select(i => i.ToString()));
            fullResult.AppendLine(resultLine);

            resultLine = string.Join(' ', sequenceY.Select(i => i.ToString()));
            fullResult.AppendLine(resultLine);

            if (i < actualLines.Count - 1)
            {
                fullResult.AppendLine();
            }
        }

        return fullResult.ToString();
    }


    private string SolveLevel5(List<string> lines)
    {
        var actualLines = lines.Skip(1).ToList();
        var fullResult = new StringBuilder();

        for (int i = 0; i < actualLines.Count; i++)
        {
            string? line = actualLines[i];
            var parts = line.Split(new char[] { ' ', ',' });

            var stationPosX = int.Parse(parts[0]);
            var stationPosY = int.Parse(parts[1]);
            var timeLimit = int.Parse(parts[2]);

            ++i;
            var asteroidLine = actualLines[i];
            parts = asteroidLine.Split(new char[] { ' ', ',' });

            var asteroidX = int.Parse(parts[0]);
            var asteroidY = int.Parse(parts[1]);

            var forbidden = new List<Vector2>();
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    forbidden.Add(new Vector2(asteroidX + x, asteroidY + y));
                }
            }

            var minX = Math.Min(0, stationPosX) - 5;
            var minY = Math.Min(0, stationPosY) - 5;
            var maxX = Math.Max(0, stationPosX) + 5;
            var maxY = Math.Max(0, stationPosY) + 5;



            var bestPath = GetShortestPath(Vector2.Zero, new Vector2(stationPosX, stationPosY), new Vector2(minX, minY), new Vector2(maxX, maxY), forbidden);

            var directions = new List<Vector2>();
            for (int s = 0; s < bestPath.Count - 1; s++)
            {
                directions.Add(bestPath[s + 1] - bestPath[s]);
            }

            var xSteps = new List<int>();
            xSteps.Add(0);
            var ySteps = new List<int>();
            ySteps.Add(0);

            var index = 0;

            while (index < directions.Count)
            {
                var current = directions[index];

                var testIndex = index;

                while (true)
                {
                    testIndex++;
                    if (testIndex == directions.Count || directions[testIndex] != directions[index])
                    {
                        break;
                    }
                }

                var stepCount = testIndex - index;


                var movesX = new List<int>();
                if (current.X != 0)
                {
                    movesX = CalculateMovementSequence(stepCount);
                    if (current.X < 0)
                    {
                        for (int s = 0; s < movesX.Count; s++)
                        {
                            movesX[s] *= -1;
                        }
                    }
                }

                var movesY = new List<int>();
                if (current.Y != 0)
                {
                    movesY = CalculateMovementSequence(stepCount);
                    if (current.Y < 0)
                    {
                        for (int s = 0; s < movesY.Count; s++)
                        {
                            movesY[s] *= -1;
                        }
                    }
                }

                if (current.X == 0)
                {
                    var timeSum = Math.Abs(movesY.Sum());
                    for (int t = 0; t < timeSum; t++)
                    {
                        movesX.Add(0);
                    }
                }

                if (current.Y == 0)
                {
                    var timeSum = Math.Abs(movesX.Sum());
                    for (int t = 0; t < timeSum; t++)
                    {
                        movesY.Add(0);
                    }
                }

                xSteps.AddRange(movesX);
                ySteps.AddRange(movesY);

                index = testIndex;

            }
            xSteps.Add(0);
            ySteps.Add(0);


            var resultLine = string.Join(' ', xSteps.Select(i => i.ToString()));
            fullResult.AppendLine(resultLine);

            resultLine = string.Join(' ', ySteps.Select(i => i.ToString()));
            fullResult.AppendLine(resultLine);

            if (i < actualLines.Count - 1)
            {
                fullResult.AppendLine();
            }
        }

        return fullResult.ToString();
    }



    private List<int> CalculateMovementSequence(int stationPos)
    {
        var result = new List<int>();

        if (Math.Abs(stationPos) >= 9)
        {
            result.AddRange([5, 4, 3, 2, 1]); // accel

            int additionalOnes = Math.Abs(stationPos) - 9;

            for (int i = 0; i < additionalOnes; i++)
            {
                result.Add(1);
            }

            result.AddRange([2, 3, 4, 5]); // decel
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
        return result;
    }


    private List<Vector2> GetShortestPath(Vector2 startPos, Vector2 endPos, Vector2 minPos, Vector2 maxPos, List<Vector2> forbidden)
    {
        var available = new Queue<Step>();
        var visited = new HashSet<Vector2>();

        var startStep = new Step()
        {
            Position = startPos
        };

        available.Enqueue(startStep);
        visited.Add(startPos);

        var directRoute = endPos - startPos;

        var xDirections = directRoute.X > 0 ? new List<int> { 1, 0, -1 } : new List<int> { -1, 0, 1 };
        var yDirections = directRoute.Y > 0 ? new List<int> { 1, 0, -1 } : new List<int> { -1, 0, 1 };

        var directions = new List<Vector2>();
        foreach (var x in xDirections)
        {
            foreach (var y in yDirections)
            {
                directions.Add(new Vector2(x, y));
            }
        }

        Step? lastStep = null;

        while (available.Count > 0)
        {
            var current = available.Dequeue();

            foreach (var dir in directions)
            {
                var moveDiff = current.Move - dir;

                if (Math.Abs(moveDiff.X) > 1 || Math.Abs(moveDiff.Y) > 1)
                {
                    continue;
                }

                var newPos = current.Position + dir;

                if (visited.Contains(newPos) || forbidden.Contains(newPos) ||
                    newPos.X < minPos.X || newPos.Y < minPos.Y || newPos.X > maxPos.X || newPos.Y > maxPos.Y)
                {
                    continue;
                }

                var newStep = new Step()
                {
                    Position = newPos,
                    Previous = current,
                    Move = dir
                };

                if (newPos == endPos)
                {
                    lastStep = newStep;
                    break;
                }

                available.Enqueue(newStep);
                visited.Add(newPos);
            }
        }

        var positions = new List<Vector2>();
        var currentstep = lastStep;
        positions.Add(currentstep.Position);


        while (currentstep.Previous != null)
        {
            currentstep = currentstep.Previous;
            positions.Add(currentstep.Position);
        }

        positions.Reverse();
        return positions;
    }

    private class Step
    {
        public Vector2 Position { get; set; }
        public Step? Previous { get; set; }
        public Vector2 Move { get; set; }
    }

    private string SolveLevel6(List<string> lines)
    {
        var actualLines = lines.Skip(1).ToList();
        var fullResult = new StringBuilder();

        for (int i = 0; i < actualLines.Count; i++)
        {
            string? line = actualLines[i];
            var parts = line.Split(new char[] { ' ', ',' });

            var stationPosX = int.Parse(parts[0]);
            var stationPosY = int.Parse(parts[1]);
            var timeLimit = int.Parse(parts[2]);

            ++i;
            var asteroidLine = actualLines[i];
            parts = asteroidLine.Split(new char[] { ' ', ',' });

            var asteroidX = int.Parse(parts[0]);
            var asteroidY = int.Parse(parts[1]);

            var forbidden = new List<Vector2>();
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    forbidden.Add(new Vector2(asteroidX + x, asteroidY + y));
                }
            }

            var minX = Math.Min(0, stationPosX) - 5;
            var minY = Math.Min(0, stationPosY) - 5;
            var maxX = Math.Max(0, stationPosX) + 5;
            var maxY = Math.Max(0, stationPosY) + 5;



            var bestPath = GetShortestPath(Vector2.Zero, new Vector2(stationPosX, stationPosY), new Vector2(minX, minY), new Vector2(maxX, maxY), forbidden);

            var directions = new List<Vector2>();
            for (int s = 0; s < bestPath.Count - 1; s++)
            {
                directions.Add(bestPath[s + 1] - bestPath[s]);
            }

            var xSteps = new List<int>();
            xSteps.Add(0);
            var ySteps = new List<int>();
            ySteps.Add(0);

            var index = 0;

            while (index < directions.Count)
            {
                var current = directions[index];

                var testIndex = index;

                while (true)
                {
                    testIndex++;
                    if (testIndex == directions.Count || directions[testIndex] != directions[index])
                    {
                        break;
                    }
                }

                var stepCount = testIndex - index;


                var movesX = new List<int>();
                if (current.X != 0)
                {
                    movesX = CalculateMovementSequence(stepCount);
                    if (current.X < 0)
                    {
                        for (int s = 0; s < movesX.Count; s++)
                        {
                            movesX[s] *= -1;
                        }
                    }
                }

                var movesY = new List<int>();
                if (current.Y != 0)
                {
                    movesY = CalculateMovementSequence(stepCount);
                    if (current.Y < 0)
                    {
                        for (int s = 0; s < movesY.Count; s++)
                        {
                            movesY[s] *= -1;
                        }
                    }
                }

                if (current.X == 0)
                {
                    var timeSum = Math.Abs(movesY.Sum());
                    for (int t = 0; t < timeSum; t++)
                    {
                        movesX.Add(0);
                    }
                }

                if (current.Y == 0)
                {
                    var timeSum = Math.Abs(movesX.Sum());
                    for (int t = 0; t < timeSum; t++)
                    {
                        movesY.Add(0);
                    }
                }

                xSteps.AddRange(movesX);
                ySteps.AddRange(movesY);

                index = testIndex;

            }
            xSteps.Add(0);
            ySteps.Add(0);

            OptimizePath(xSteps, ySteps);


            var resultLine = string.Join(' ', xSteps.Select(i => i.ToString()));
            fullResult.AppendLine(resultLine);

            resultLine = string.Join(' ', ySteps.Select(i => i.ToString()));
            fullResult.AppendLine(resultLine);

            if (i < actualLines.Count - 1)
            {
                fullResult.AppendLine();
            }
        }

        return fullResult.ToString();
    }

    private void OptimizePath(List<int> xSteps, List<int> ySteps)
    {
        while (true)
        {
            var changed = false;

            for (int i = 1; i < Math.Max(xSteps.Count - 1, ySteps.Count - 1); ++i)
            {
                if (i >= xSteps.Count - 1 || i >= ySteps.Count - 1)
                {
                    continue;
                }

                var prevPair = (xSteps[i - 1], ySteps[i - 1]);
                var nextPair = (xSteps[i + 1], ySteps[i + 1]);
                var thisPair = (xSteps[i], ySteps[i]);

                if (thisPair == (0, 0))
                {
                    continue;
                }

                if (thisPair.Item1 > 0 && thisPair.Item2 > 0)
                {
                    if (thisPair == prevPair && thisPair == nextPair && thisPair.Item1 > 1)
                    {
                        xSteps[i]--;
                        ySteps[i]--;
                        changed = true;
                    }
                    else if (thisPair == (prevPair.Item1 + 1, prevPair.Item2 + 1) && thisPair == nextPair && thisPair.Item1 > 1)
                    {
                        xSteps[i]--;
                        ySteps[i]--;
                        changed = true;
                    }
                }
                else // negative
                {
                    if (thisPair == prevPair && thisPair == nextPair && thisPair.Item1 < -1)
                    {
                        xSteps[i]++;
                        ySteps[i]++;
                        changed = true;
                    }
                    else if (thisPair == (prevPair.Item1 - 1, prevPair.Item2 - 1) && thisPair == nextPair && thisPair.Item1 < -1)
                    {
                        xSteps[i]++;
                        ySteps[i]++;
                        changed = true;
                    }
                }
            }

            if (!changed)
            {
                break;
            } 
        }
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