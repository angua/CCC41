using System.Numerics;
using System.Text;

namespace CCC41Lib;

public partial class Solver
{
    public Dictionary<Vector2, HashSet<(int PaceX, int PaceY, int WaitX, int WaitY)>> Visited { get; set; } = new();
    public Dictionary<int, Stack<MoveState>> MoveStates { get; set; } = new();
    public Stack<MoveState> BestStates { get; set; } = new();
    public MoveState? Current { get; set; }
    public MoveState? FinalState { get; set; } = null;
    public bool BatchDone { get; set; } = false;
    public int MinCost { get; set; }


    public void Solve(int level, DataSet dataSet)
    {
        switch (level)
        {
            case 1:
                SolveLevel1(dataSet);
                break;
            case 2:
                SolveLevel1(dataSet);
                break;
            case 3:
                SolveLevel3(dataSet);
                break;
            case 4:
                SolveLevel3(dataSet);
                break;
            case 5:
                SolveLevel6(dataSet);
                break;
            case 6:
                SolveLevel6(dataSet);
                break;
            case 7:
                SolveLevel7(dataSet);
                break;

            default:
                break;
        }

    }


    private void SolveLevel1(DataSet dataSet)
    {
        dataSet.TimeUsed = CalculateTime(dataSet.XSequence);
        var timedPositionsX = GetTimedPositions(0, dataSet.XSequence);
        dataSet.TimedPositions = timedPositionsX.Select(p => (p.Key, new Vector2(p.Value, 0))).ToDictionary();
        var maxTime = dataSet.TimedPositions.Max(p => p.Key);
        dataSet.TargetPosition = dataSet.TimedPositions[maxTime];

        SetBounds(dataSet);
    }

    private void SolveLevel3(DataSet dataSet)
    {
        (dataSet.XSequence, dataSet.YSequence) = GetSequences(dataSet.StartPosition, dataSet.TargetPosition);
        dataSet.TimedPositions = GetTimedPositions(dataSet.StartPosition, dataSet.XSequence, dataSet.YSequence);
        dataSet.TimeUsed = CalculateTime(dataSet.XSequence);
    }

    private void SolveLevel7(DataSet dataSet)
    {
        SetupLevel7(dataSet);

        FinalState = null;
        while (MoveStates.Count > 0)
        {
            CheckBatchDone();
            PrepareNextStepLevel7();
            NextStepLevel7(dataSet);

            if (FinalState != null)
            {
                break;
            }
        }

        CreateSolutionLevel7(dataSet);
    }


    public void SetupLevel7(DataSet dataSet)
    {
        var start = new MoveState()
        {
            Position = dataSet.StartPosition,
        };

        var timeX = CalculateBestTime((int)(dataSet.TargetPosition.X - dataSet.StartPosition.X));
        var timeY = CalculateBestTime((int)(dataSet.TargetPosition.Y - dataSet.StartPosition.Y));
        start.Cost = Math.Max(timeX, timeY);

        MoveStates = new();
        var startStates = new Stack<MoveState>();
        startStates.Push(start);
        MoveStates.Add(start.Cost, startStates);

        Visited = new();
        BestStates.Clear();
        Current = null;
        FinalState = null;
        MinCost = -1;
    }

    public void CheckBatchDone()
    {
        if (BestStates.Count == 0)
        {
            MoveStates.Remove(MinCost);
            BatchDone = true;
        }
        else
        {
            BatchDone = false;
        }
    }

    public void PrepareNextStepLevel7()
    {
        if (MoveStates.Count > 0)
        {
            MinCost = MoveStates.Min(s => s.Key);
            BestStates = MoveStates[MinCost];

            Current = BestStates.Pop();
        }
    }

    public void NextStepLevel7(DataSet dataSet)
    {
        if (Current == null)
        {
            return;
        }

        var dir = dataSet.TargetPosition - Current.Position;
        var newPacesX = Current.TimeLeftX > 0 ? [Current.PaceX] : NextPaces(Current.PaceX, (int)dir.X);
        var newPacesY = Current.TimeLeftY > 0 ? [Current.PaceY] : NextPaces(Current.PaceY, (int)dir.Y);

        foreach (int paceX in newPacesX)
        {
            foreach (var paceY in newPacesY)
            {
                var waitX = Current.TimeLeftX > 0 ? Current.TimeLeftX : GetTimeForStep(paceX);
                var waitY = Current.TimeLeftY > 0 ? Current.TimeLeftY : GetTimeForStep(paceY);

                var wait = Math.Min(waitX, waitY);
                var timeLeftX = waitX - wait;
                var timeLeftY = waitY - wait;

                var dirX = timeLeftX == 0 ? Math.Sign(paceX) : 0;
                var dirY = timeLeftY == 0 ? Math.Sign(paceY) : 0;

                var newPos = new Vector2(Current.Position.X + dirX, Current.Position.Y + dirY);

                if (newPos.X < dataSet.BoundsMin.X ||
                    newPos.X > dataSet.BoundsMax.X ||
                    newPos.Y < dataSet.BoundsMin.Y ||
                    newPos.Y > dataSet.BoundsMax.Y ||
                    dataSet.ForbiddenAreas.Contains(newPos))
                {
                    continue;
                }

                var moveState = new MoveState()
                {
                    Position = newPos,
                    PaceX = paceX,
                    PaceY = paceY,
                    TimeLeftX = timeLeftX,
                    TimeLeftY = timeLeftY,
                    Time = Current.Time + wait
                };

                if (moveState.Position == dataSet.TargetPosition)
                {
                    if ((moveState.PaceX == 5 || moveState.PaceX == -5 || moveState.PaceX == 0) &&
                        (moveState.PaceY == 5 || moveState.PaceY == -5 || moveState.PaceY == 0))
                    {
                        // we found our solution!
                        FinalState = moveState;
                    }
                }

                if (!Visited.TryGetValue(newPos, out var movesHere))
                {
                    movesHere = new HashSet<(int, int, int, int)>();
                    Visited[newPos] = movesHere;
                }
                if (!movesHere.Contains((moveState.PaceX, moveState.PaceY, moveState.TimeLeftX, moveState.TimeLeftY)))
                {
                    movesHere.Add((moveState.PaceX, moveState.PaceY, moveState.TimeLeftX, moveState.TimeLeftY));

                    // estimate remaining time
                    var timeX = CalculateBestTime((int)(dataSet.TargetPosition.Y - newPos.Y));
                    var timeY = CalculateBestTime((int)(dataSet.TargetPosition.X - newPos.X));
                    moveState.TimeToReach = Math.Max(timeX, timeY);
                    moveState.Cost = moveState.Time + moveState.TimeToReach;

                    if (!MoveStates.TryGetValue(moveState.Cost, out var movesWithCost))
                    {
                        movesWithCost = new Stack<MoveState>();
                        MoveStates.Add(moveState.Cost, movesWithCost);
                    }
                    movesWithCost.Push(moveState);

                    moveState.AddMoveX = Current.TimeLeftX == 0 ? moveState.PaceX : null;
                    moveState.AddMoveY = Current.TimeLeftY == 0 ? moveState.PaceY : null;

                    moveState.Previous = Current;
                }
            }
        }
    }

    public void CreateSolutionLevel7(DataSet dataSet)
    {
        // create steps in order
        var xSteps = new List<int>();
        var ySteps = new List<int>();
        var positions = new List<Vector2>();

        if (FinalState != null)
        {
            var currentState = FinalState;
            dataSet.TimedPositions.Clear();
            while (true)
            {
                dataSet.TimedPositions.Add(currentState.Time, currentState.Position);
                if (currentState.AddMoveX != null)
                {
                    xSteps.Add(currentState.AddMoveX.Value);
                }
                if (currentState.AddMoveY != null)
                {
                    ySteps.Add(currentState.AddMoveY.Value);
                }
                if (currentState.Previous != null)
                {
                    currentState = currentState.Previous;
                }
                else
                {
                    break;
                }
            }
            xSteps.Reverse();
            ySteps.Reverse();
            positions.Reverse();

            // might have to stand still in one direction until the last movement in the other one is finished
            for (int xLeft = 0; xLeft < FinalState.TimeLeftX; xLeft++)
            {
                ySteps.Add(0);
            }
            for (int yLeft = 0; yLeft < FinalState.TimeLeftY; yLeft++)
            {
                xSteps.Add(0);
            }

            xSteps.Insert(0, 0);
            ySteps.Insert(0, 0);
            xSteps.Add(0);
            ySteps.Add(0);
        }

        dataSet.XSequence = xSteps;
        dataSet.YSequence = ySteps;
        dataSet.TimeUsed = Math.Max(CalculateTime(dataSet.XSequence), CalculateTime(dataSet.YSequence));
        //GetTimedPositions(dataSet.StartPosition, dataSet.XSequence, dataSet.YSequence);
        Validate(7, dataSet);
    }


    private int CalculateBestTime(int targetPos)
    {
        var absTargetPos = Math.Abs(targetPos);

        if (absTargetPos >= 9)
        {
            return 20 + absTargetPos;
        }
        else
        {
            var steps = new List<int> { 5, 4, 3, 2, 1, 2, 3, 4, 5 };
            var toRemove = 9 - Math.Abs(targetPos);

            for (int i = 0; i < toRemove; i++)
            {
                var currentMin = steps.Min();
                steps.RemoveAt(steps.IndexOf(currentMin));
            }

            return steps.Sum();
        }

    }

    public static void SetBounds(DataSet dataSet)
    {
        var minX = Math.Min(dataSet.StartPosition.X, dataSet.TargetPosition.X);
        var maxX = Math.Max(dataSet.StartPosition.X, dataSet.TargetPosition.X);
        var minY = Math.Min(dataSet.StartPosition.Y, dataSet.TargetPosition.Y);
        var maxY = Math.Max(dataSet.StartPosition.Y, dataSet.TargetPosition.Y);

        if (dataSet.Asteroids.Count > 0)
        {
            var asteroidMinX = dataSet.Asteroids.Min(a => a.X);
            var asteroidMaxX = dataSet.Asteroids.Max(a => a.X);
            var asteroidMinY = dataSet.Asteroids.Min(a => a.Y);
            var asteroidMaxY = dataSet.Asteroids.Max(a => a.Y);

            minX = Math.Min(minX, asteroidMinX);
            maxX = Math.Max(maxX, asteroidMaxX);
            minY = Math.Min(minY, asteroidMinY);
            maxY = Math.Max(maxY, asteroidMaxY);
        }

        dataSet.BoundsMin = new Vector2(minX - 5, minY - 5);
        dataSet.BoundsMax = new Vector2(maxX + 5, maxY + 5);
        dataSet.Width = (int)(dataSet.BoundsMax.X - dataSet.BoundsMin.X + 1);
        dataSet.Height = (int)(dataSet.BoundsMax.Y - dataSet.BoundsMin.Y + 1);
    }

    private List<int> CalculateSequence(int targetPos)
    {
        var result = new List<int> { 0 };

        if (Math.Abs(targetPos) >= 9)
        {
            result.AddRange([5, 4, 3, 2, 1]); // accel

            int additionalOnes = Math.Abs(targetPos) - 9;

            for (int i = 0; i < additionalOnes; i++)
            {
                result.Add(1);
            }

            result.AddRange([2, 3, 4, 5]); // decel
        }
        else // pos < 9
        {
            var steps = new List<int> { 5, 4, 3, 2, 1, 2, 3, 4, 5 };
            var toRemove = 9 - Math.Abs(targetPos);

            for (int i = 0; i < toRemove; i++)
            {
                var currentMin = steps.Min();
                steps.RemoveAt(steps.IndexOf(currentMin));
            }

            result.AddRange(steps);
        }

        if (targetPos < 0)
        {
            for (int i = 0; i < result.Count; i++)
            {
                result[i] *= -1;
            }
        }

        result.Add(0);

        return result;
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

    private List<int> CalculateMovementSequence(int startPos, int targetPos)
    {
        var dir = targetPos - startPos;
        var sequence = CalculateMovementSequence(dir);

        if (dir < 0)
        {
            sequence = sequence.Select(x => -1 * x).ToList();
        }
        return sequence;
    }


    private List<int> CalculateMovementSequence(int targetPos)
    {
        var absTargetPos = Math.Abs(targetPos);
        var result = new List<int>(absTargetPos);

        if (Math.Abs(targetPos) >= 9)
        {
            result.AddRange([5, 4, 3, 2, 1]); // accel

            int additionalOnes = absTargetPos - 9;

            for (int i = 0; i < additionalOnes; i++)
            {
                result.Add(1);
            }

            result.AddRange([2, 3, 4, 5]); // decel
        }
        else // pos < 9
        {
            var steps = new List<int>(9) { 5, 4, 3, 2, 1, 2, 3, 4, 5 };
            var toRemove = 9 - Math.Abs(targetPos);

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
        var fullResult = new StringBuilder();
        var fileDataSet = new FileDataSet(6, lines);

        for (var i = 0; i < fileDataSet.DataSets.Count; i++)
        {
            var data = fileDataSet.DataSets[i];

            var result = SolveLevel6(data);

            foreach (var str in result)
            {
                fullResult.AppendLine(str);
            }

            // Console.WriteLine($"Time: , limit: {data.TimeLimit}");
            //var validText = totalTime <= data.TimeLimit ? "Valid" : "INVALID!!!!! Taking too long!!!!";
            //Console.WriteLine(validText);

            if (i < fileDataSet.DataSets.Count - 1)
            {
                fullResult.AppendLine();
            }
        }

        return fullResult.ToString();
    }

    private List<string> SolveLevel6(DataSet data)
    {
        var result = new List<string>();

        // 1 second for standing still at the beginning of the sequence
        var xSequence = new List<int>()
        {
            0
        };
        var ySequence = new List<int>()
        {
            0
        };

        var startPos = data.StartPosition;
        var targetPos = data.TargetPosition;

        var xSteps = new List<int>();
        var ySteps = new List<int>();

        TryOrthogonalPath(data, startPos, targetPos, xSteps, ySteps, data.TimeLimit - 2);

        // nope, need another solution here
        if (xSteps.Count < 1)
        {
            var dist = data.TargetPosition - data.StartPosition;

            var testPos = data.StartPosition;
            var startPath = new List<int>();

            var moveInXDir = Math.Abs(dist.X) > Math.Abs(dist.Y);

            // target is more ore less on a line in one orthogonal direction with the start position, but is blocked by the asteroid
            // need to deviate from the direct path
            for (int c = 1; c < 7; c++)
            {
                testPos = moveInXDir ? new Vector2(data.StartPosition.X, data.StartPosition.Y + c) :
                                            new Vector2(data.StartPosition.X + c, data.StartPosition.Y);
                startPath = moveInXDir ? CalculateMovementSequence((int)data.StartPosition.Y, (int)testPos.Y) :
                                                CalculateMovementSequence((int)data.StartPosition.X, (int)testPos.X);
                var startTime = CalculateTime(startPath);

                TryOrthogonalPath(data, testPos, targetPos, xSteps, ySteps, data.TimeLimit - startTime - 2);

                if (xSteps.Count > 0)
                {
                    // solution found
                    break;
                }

                testPos = moveInXDir ? new Vector2(data.StartPosition.X, data.StartPosition.Y - c) :
                                            new Vector2(data.StartPosition.X - c, data.StartPosition.Y);
                startPath = moveInXDir ? CalculateMovementSequence((int)data.StartPosition.Y, (int)testPos.Y) :
                                                CalculateMovementSequence((int)data.StartPosition.X, (int)testPos.X);
                startTime = CalculateTime(startPath);

                TryOrthogonalPath(data, testPos, targetPos, xSteps, ySteps, data.TimeLimit - startTime - 2);

                if (xSteps.Count > 0)
                {
                    // solution found
                    break;
                }
            }

            // add the start path at the beginning before the steps
            while (startPath.Count > 0)
            {
                var last = startPath.Last();
                var lastTime = GetTimeForStep(last);
                if (moveInXDir)
                {
                    // move in y direction
                    ySteps.Insert(0, last);
                    for (int t = 0; t < lastTime; t++)
                    {
                        // wait in x direction
                        xSteps.Insert(0, 0);
                    }
                }
                else
                {
                    xSteps.Insert(0, last);
                    for (int t = 0; t < lastTime; t++)
                    {
                        ySteps.Insert(0, 0);
                    }
                }
                startPath.RemoveAt(startPath.Count - 1);
            }

        }

        xSequence.AddRange(xSteps);
        ySequence.AddRange(ySteps);

        xSequence.Add(0);
        ySequence.Add(0);


        data.XSequence = xSequence;
        data.YSequence = ySequence;

        data.TimedPositions = GetTimedPositions(data.StartPosition, data.XSequence, data.YSequence);

        data.TimeUsed = Math.Max(CalculateTime(data.XSequence), CalculateTime(data.YSequence));


        var resultLine = string.Join(' ', xSteps);
        Console.WriteLine(resultLine);
        result.Add(resultLine);

        resultLine = string.Join(' ', ySteps);
        Console.WriteLine(resultLine);
        result.Add(resultLine);

        Validate(6, data);



        return result;
    }

    private void Validate(int level, DataSet data)
    {
        var valid = true;
        data.ErrorText.Clear();

        if (!ValidateSequence(data, data.XSequence))
        {
            valid = false;
        }
        if (!ValidateSequence(data, data.YSequence))
        {
            valid = false;
        }
        if (data.TimeUsed > data.TimeLimit)
        {
            valid = false;
            data.ErrorText.Add("Taking too long.");
        }
        if (data.TimedPositions.Any(p => data.ForbiddenAreas.Contains(p.Value)))
        {
            valid = false;
            data.ErrorText.Add("Hitting asteroid.");
        }

        data.Valid = valid;
    }

    private bool ValidateSequence(DataSet data, List<int> sequence)
    {
        var valid = true;
        if (sequence.Count == 0)
        {
            valid = false;
            data.ErrorText.Add("Sequence contains no elements.");
        }
        else
        {
            if (sequence.First() != 0)
            {
                valid = false;
                data.ErrorText.Add("Sequence must start with 0.");
            }
            if (sequence.Last() != 0)
            {
                valid = false;
                data.ErrorText.Add("Sequence must end with 0.");
            }
            for (int i = 0; i < sequence.Count - 1; i++)
            {
                var current = sequence[i];
                var next = sequence[i + 1];

                if (current < -5 || current > 5)
                {
                    valid = false;
                    data.ErrorText.Add($"Invalid pace value {current}.");
                }

                if (!IsValidNextPace(current, next))
                {
                    valid = false;
                    data.ErrorText.Add($"Invalid pace {next} after {current}.");
                }
            }
        }

        return valid;
    }

    private bool IsValidNextPace(int current, int next)
    {
        return ValidNextPaces(current).Contains(next);
    }

    private List<int> ValidNextPaces(int current)
    {
        return current switch
        {
            -5 => [0, -5, -4],
            -4 => [-5, -4, -3],
            -3 => [-4, -3, -2],
            -2 => [-3, -2, -1],
            -1 => [-2, -1],
            0 => [0, -5, 5],
            1 => [1, 2],
            2 => [1, 2, 3],
            3 => [2, 3, 4],
            4 => [3, 4, 5],
            5 => [0, 5, 4],
            _ => [],
        };
    }

    private List<int> NextPaces(int current, int dir)
    {
        return current switch
        {
            -5 => dir < 0 ? [-4, -5, 0] : [0, -5, -4],
            -4 => dir < 0 ? [-3, -4, -5] : [-5, -4, -3],
            -3 => dir < 0 ? [-2, -3, -4] : [-4, -3, -2],
            -2 => dir < 0 ? [-1, -2, -3] : [-3, -2, -1],
            -1 => dir < 0 ? [-1, -2] : [-2, -1],
            0 => dir < 0 ? [-5, 0, 5] : (dir > 0 ? [5, 0, -5] : [0, 5, -5]),
            1 => dir > 0 ? [1, 2] : [2, 1],
            2 => dir > 0 ? [1, 2, 3] : [3, 2, 1],
            3 => dir > 0 ? [2, 3, 4] : [4, 3, 2],
            4 => dir > 0 ? [3, 4, 5] : [5, 4, 3],
            5 => dir > 0 ? [4, 5, 0] : [0, 5, 4],
            _ => [],
        };
    }


    private void TryOrthogonalPath(DataSet data, Vector2 startPos, Vector2 targetPos, List<int> xSteps, List<int> ySteps, int timeLimit)
    {
        // try orthogonal paths along x and y coordinates
        // x first then y
        var startLineBlockedY = LineBlockedY(data, (int)startPos.X, (int)startPos.Y, (int)targetPos.Y);
        var targetLineBlockedX = LineBlockedX(data, (int)targetPos.Y, (int)startPos.X, (int)targetPos.X);
        var firstYRectangleBlocked = startLineBlockedY || targetLineBlockedX;

        // y first then x
        var startLineBlockedX = LineBlockedX(data, (int)startPos.Y, (int)startPos.X, (int)targetPos.X);
        var targetLineBlockedY = LineBlockedY(data, (int)targetPos.X, (int)startPos.Y, (int)targetPos.Y);
        var firstXRectangleBlocked = startLineBlockedX || targetLineBlockedY;

        var xLine = CalculateMovementSequence((int)startPos.X, (int)targetPos.X);
        var yLine = CalculateMovementSequence((int)startPos.Y, (int)targetPos.Y);

        var xTime = CalculateTime(xLine);
        var yTime = CalculateTime(yLine);

        if (!firstYRectangleBlocked)
        {
            // first go to y position of target, remaining at x position of start
            if (xTime + yTime <= timeLimit)
            {
                // time limit is long enough to do movements one after the other
                // move in Y
                ySteps.AddRange(yLine);
                for (int x = 0; x < xTime; x++)
                {
                    // then stand still in y direction while traveling in x direction
                    ySteps.Add(0);
                }

                for (int y = 0; y < yTime; y++)
                {
                    // stand still in x direction while traveling in y direction
                    xSteps.Add(0);
                }
                // move in X
                xSteps.AddRange(xLine);
            }
            else
            {
                // start movement in second direction earlier
                var startTime = timeLimit - xTime;

                var xTemp = new List<int>();
                var yTemp = new List<int>();

                for (int y = 0; y < startTime; y++)
                {
                    // stand still in x direction while traveling in y direction
                    xTemp.Add(0);
                }
                xTemp.AddRange(xLine);

                yTemp.AddRange(yLine);
                for (int x = 0; x < xTime - yTime + startTime; x++)
                {
                    // stand still in y direction while traveling in x direction
                    yTemp.Add(0);
                }

                var timedPositions = GetTimedPositions(startPos, xTemp, yTemp);

                if (!timedPositions.Any(p => data.ForbiddenAreas.Contains(p.Value)))
                {
                    // found solution
                    xSteps.AddRange(xTemp);
                    ySteps.AddRange(yTemp);
                }
            }
        }
        // need to finnd another way
        if (xSteps.Count < 1 && !firstXRectangleBlocked)
        {
            // first go to X position of target, remaining at Y position of start
            if (xTime + yTime <= timeLimit)
            {
                // time limit is long enough to do movements one after the other
                xSteps.AddRange(xLine);
                for (int y = 0; y < yTime; y++)
                {
                    // stand still in x direction while traveling in y direction
                    xSteps.Add(0);
                }

                for (int x = 0; x < xTime; x++)
                {
                    // stand still in y direction while traveling in x direction
                    ySteps.Add(0);
                }
                ySteps.AddRange(yLine);
            }
            else
            {
                // start movement in second direction earlier
                var startTime = timeLimit - yTime;

                var xTemp = new List<int>();
                var yTemp = new List<int>();


                xTemp.AddRange(xLine);
                for (int y = 0; y < yTime - xTime + startTime; y++)
                {
                    // stand still in x direction while traveling in y direction
                    xTemp.Add(0);
                }

                for (int x = 0; x < startTime; x++)
                {
                    // stand still in y direction while traveling in x direction
                    yTemp.Add(0);
                }
                yTemp.AddRange(yLine);

                var timedPositions = GetTimedPositions(startPos, xTemp, yTemp);

                if (!timedPositions.Any(p => data.ForbiddenAreas.Contains(p.Value)))
                {
                    // found solution
                    xSteps.AddRange(xTemp);
                    ySteps.AddRange(yTemp);
                }

            }
        }
    }

    private Dictionary<int, Vector2> GetTimedPositions(Vector2 startPosition, List<int> xSequence, List<int> ySequence)
    {
        var positions = new Dictionary<int, Vector2>();

        var xPositions = GetTimedPositions((int)startPosition.X, xSequence);
        var yPositions = GetTimedPositions((int)startPosition.Y, ySequence);

        var currentX = startPosition.X;
        var currentY = startPosition.Y;
        var currentTime = 0;

        while (xPositions.Count > 0 && yPositions.Count > 0)
        {
            var minXTime = xPositions.Min(k => k.Key);
            var minYTime = yPositions.Min(k => k.Key);

            if (minXTime < minYTime)
            {
                currentX = xPositions[minXTime];
                xPositions.Remove(minXTime);
                currentTime = minXTime;
            }
            else if (minYTime < minXTime)
            {
                currentY = yPositions[minYTime];
                yPositions.Remove(minYTime);
                currentTime = minYTime;
            }
            else
            {
                currentX = xPositions[minXTime];
                xPositions.Remove(minXTime);
                currentY = yPositions[minYTime];
                yPositions.Remove(minYTime);
                currentTime = minXTime;
            }
            positions.Add(currentTime, new Vector2(currentX, currentY));
        }

        while (xPositions.Count > 0)
        {
            var minXTime = xPositions.Min(k => k.Key);
            currentX = xPositions[minXTime];
            xPositions.Remove(minXTime);
            currentTime = minXTime;
            positions.Add(currentTime, new Vector2(currentX, currentY));
        }
        while (yPositions.Count > 0)
        {
            var minYTime = yPositions.Min(k => k.Key);
            currentY = yPositions[minYTime];
            xPositions.Remove(minYTime);
            currentTime = minYTime;
            positions.Add(currentTime, new Vector2(currentX, currentY));
        }

        return positions;
    }

    // time and position at this time in one dimension
    // space ship reaches the next position at the end of the pace waiting time (in the last second)
    private Dictionary<int, int> GetTimedPositions(int start, List<int> sequence)
    {
        var positions = new Dictionary<int, int>();
        positions.Add(0, start);

        var time = 0;
        var position = start;
        foreach (var step in sequence)
        {
            time += GetTimeForStep(step);
            var dir = Math.Sign(step);
            position += dir;
            positions.Add(time, position);
        }
        return positions;
    }

    private bool LineBlockedY(DataSet data, int x, int startY, int endY)
    {
        var line = endY - startY;
        var dir = Math.Sign(line);

        var yPos = startY;
        while (yPos != endY)
        {
            if (data.ForbiddenAreas.Contains(new Vector2(x, yPos)))
            {
                return true;
            }
            yPos += dir;
        }
        return data.ForbiddenAreas.Contains(new Vector2(x, endY));
    }

    private bool LineBlockedX(DataSet data, int y, int startX, int endX)
    {
        var line = endX - startX;
        var dir = Math.Sign(line);

        var xPos = startX;
        while (xPos != endX)
        {
            if (data.ForbiddenAreas.Contains(new Vector2(xPos, y)))
            {
                return true;
            }
            xPos += dir;
        }
        return data.ForbiddenAreas.Contains(new Vector2(endX, y));
    }



    private (List<int> sequenceX, List<int> sequenceY) GetSequences(Vector2 start, Vector2 end)
    {
        var dir = end - start;
        var sequenceX = CalculateMovementSequence((int)(dir.X));
        if (dir.X < 0)
        {
            sequenceX = sequenceX.Select(x => -1 * x).ToList();
        }

        var sequenceY = CalculateMovementSequence((int)(end.Y - start.Y));
        if (dir.Y < 0)
        {
            sequenceX = sequenceX.Select(x => -1 * x).ToList();
        }

        // stand still when finished in the faster direction until the slower direction is done moving
        var timeX = CalculateTime(sequenceX);
        var timeY = CalculateTime(sequenceY);

        var timeDiff = timeX - timeY;

        if (timeDiff > 0)
        {
            // X takes longer
            for (int i = 0; i < Math.Abs(timeDiff); i++)
            {
                sequenceY.Add(0);
            }
        }
        else if (timeDiff < 0)
        {
            // X takes longer
            for (int i = 0; i < Math.Abs(timeDiff); i++)
            {
                sequenceY.Add(0);
            }
        }
        return (sequenceX, sequenceY);
    }

    /// <summary>
    /// Calculate time used for a sequence of paces
    /// </summary>
    /// <param name="sequence"></param>
    /// <returns></returns>
    private int CalculateTime(List<int> sequence)
    {
        var sum = 0;
        foreach (var step in sequence)
        {
            sum += GetTimeForStep(step);
        }
        return sum;
    }

    private bool CanReach(MoveState newState, DataSet data)
    {
        if (!CanReach(newState.Position.X, data.TargetPosition.X, newState.SpeedX, data.TimeLimit - newState.Time))
        {
            return false;
        }
        if (!CanReach(newState.Position.Y, data.TargetPosition.Y, newState.SpeedY, data.TimeLimit - newState.Time))
        {
            return false;
        }
        return true;
    }

    private int TimeToReach(MoveState state, DataSet data)
    {
        return Math.Max(TimeToReach((int)state.Position.X, (int)data.TargetPosition.X, state.SpeedX),
                        TimeToReach((int)state.Position.Y, (int)data.TargetPosition.Y, state.SpeedY));
    }
    private int TimeToReachX(MoveState state, DataSet data)
    {
        return TimeToReach((int)state.Position.X, (int)data.TargetPosition.X, state.SpeedX);
    }

    private int TimeToReachY(MoveState state, DataSet data)
    {
        return TimeToReach((int)state.Position.Y, (int)data.TargetPosition.Y, state.SpeedY);
    }


    private int TimeToReach(int current, int target, int speed)
    {
        var dir = target - current;
        var dist = (int)Math.Abs(dir);
        var absSpeed = Math.Abs(speed);

        var timeUsed = 0;

        if (dir * speed < 0)
        {
            // wrong direction, need to turn around
            // eg decelerating from speed 5: pace(speed 4) + pace(speed 3) + pace(speed 2) + pace(speed 1)
            //  + 1 second for standing still at the end of th deceleration
            timeUsed = 1;
            for (int i = 1; i < absSpeed; i++)
            {
                timeUsed += GetPace(absSpeed);

            }
            // still moves into wrong direction while slowing down, distance increases
            dist += absSpeed - 1;
            absSpeed = 0;
        }

        var maxSpeed = 5;

        // can reach target without overshooting
        // eg at speed 4 we need 3m to slow down: pace(speed 3) + pace(speed2) + pace(speed1)
        if (dist + 1 >= absSpeed)
        {
            // from the target, move back towards the current position and increase speed until the distance is filled or it matches the spaceship's speed
            var currentSpeed = 0;
            while (currentSpeed < absSpeed && dist > 0)
            {
                currentSpeed++;
                timeUsed += GetPace(currentSpeed);
                dist--;
            }

            while (dist > 0)
            {
                // symmetrically incease speed from spaceship and current position unless max speed is reached
                if (currentSpeed < maxSpeed)
                {
                    currentSpeed++;
                    if (dist > 1)
                    {
                        timeUsed += 2 * GetPace(currentSpeed);
                        dist -= 2;
                    }
                    else
                    {
                        timeUsed += GetPace(currentSpeed);
                        dist -= 1;
                    }
                }
                else
                {
                    timeUsed += dist * GetPace(maxSpeed);
                    dist = 0;
                }
            }

        }
        else
        {
            // we overshoot, so we have to stop and turn back
            // decelerate
            for (int i = 1; i < absSpeed; i++)
            {
                timeUsed += GetPace(absSpeed);
            }
            // 1 second for standing still
            timeUsed += 1;

            var newDist = absSpeed - 1 - dist;
            timeUsed += TimeToReach(0, newDist, 0);
        }
        return timeUsed;
    }


    private bool CanReach(float current, float target, int speed, int timeLimit)
    {
        var dir = target - current;
        var dist = (int)Math.Abs(dir);
        var absSpeed = Math.Abs(speed);

        var timeUsed = 0;

        if (dir * speed < 0)
        {
            // wrong direction, need to turn around
            //  + 1 second for standing still
            timeUsed = 1;
            for (int i = 1; i < absSpeed; i++)
            {
                timeUsed += GetPace(absSpeed);

            }
            // still moves into wrong direction while slowing down
            dist += absSpeed - 1;
            absSpeed = 0;
            if (timeUsed > timeLimit)
            {
                return false;
            }
        }

        var maxSpeed = 5;

        // need the first meters to accelerate to max speed ( maxSpeed - absSpeed - 1)
        // need the last meters to decelerate (maxSpeed - 1 meters, 4s + 3s + 2s + 1s + 1s for standing still = 11
        if (dist >= 8 - absSpeed)
        {
            for (int i = absSpeed; i < maxSpeed; i++)
            {
                timeUsed += GetPace(i);
            }
            timeUsed += dist - 8 + 11;
            if (timeUsed > timeLimit)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns the time it takes for a single pace step to complete
    /// </summary>
    /// <param name="pace"></param>
    /// <returns></returns>
    private int GetTimeForStep(int pace)
    {
        if (pace == 0)
        {
            return 1;
        }
        return Math.Abs(pace);
    }

    private int GetPace(int speed)
    {
        if (speed == 0)
        {
            return 0;
        }
        if (speed > 0)
        {
            return 6 - speed;
        }
        return -6 - speed;
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

    public string CreateOutput(FileDataSet fileDataSet)
    {
        var fullResult = new StringBuilder();

        switch (fileDataSet.Level)
        {
            case 1:
                for (var i = 0; i < fileDataSet.DataSets.Count; i++)
                {
                    var dataset = fileDataSet.DataSets[i];
                    fullResult.Append(dataset.TimeUsed);
                }
                break;

            case 2:
                for (var i = 0; i < fileDataSet.DataSets.Count; i++)
                {
                    var dataset = fileDataSet.DataSets[i];
                    fullResult.Append($"{dataset.TargetPosition.X} {dataset.TimeUsed}");
                }
                break;

            case 3:
                for (var i = 0; i < fileDataSet.DataSets.Count; i++)
                {
                    var dataset = fileDataSet.DataSets[i];
                    fullResult.Append(dataset.XSequence);
                }
                break;

            case 4:
                for (var i = 0; i < fileDataSet.DataSets.Count; i++)
                {
                    var dataset = fileDataSet.DataSets[i];
                    fullResult.Append(dataset.XSequence);
                    fullResult.Append(dataset.YSequence);

                    if (i < fileDataSet.DataSets.Count - 1)
                    {
                        fullResult.AppendLine();
                    }
                }
                break;

            case 6:
                for (var i = 0; i < fileDataSet.DataSets.Count; i++)
                {
                    var dataset = fileDataSet.DataSets[i];

                    fullResult.Append(dataset.XSequenceString);
                    fullResult.Append(dataset.YSequenceString);
                    if (i < fileDataSet.DataSets.Count - 1)
                    {
                        fullResult.AppendLine();
                    }
                }
                break;

            case 7:
                for (var i = 0; i < fileDataSet.DataSets.Count; i++)
                {
                    var dataset = fileDataSet.DataSets[i];

                    fullResult.Append(dataset.XSequenceString);
                    fullResult.Append(dataset.YSequenceString);
                    if (i < fileDataSet.DataSets.Count - 1)
                    {
                        fullResult.AppendLine();
                    }
                }
                break;


            default:
                break;
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