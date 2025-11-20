using System.Diagnostics;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Threading.Channels;

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



    private List<int> CalculateMovementSequence(int targetPos)
    {
        var result = new List<int>();

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
            Console.WriteLine($"data {i / 2}");
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

            var data = new DataSet()
            {
                TargetPosition = new Vector2(stationPosX, stationPosY),
                Asteroids = [new Vector2(asteroidX, asteroidY)],
                TimeLimit = timeLimit
            };

            var corners = new List<Vector2>()
            {
                new Vector2(asteroidX - 3, asteroidY - 3),
                new Vector2(asteroidX - 3, asteroidY + 3),
                new Vector2(asteroidX + 3, asteroidY - 3),
                new Vector2(asteroidX + 3, asteroidY + 3),
            };

            // closest corner to start position
            var cornersByDistance = corners.OrderBy(c => (c - data.StartPosition).LengthSquared());

            var firstCorner = cornersByDistance.First();
            var adjacentCorners = corners.Where(c => c.X == firstCorner.X || c.Y == firstCorner.Y);


            // closest corner to target position
            var adjacentCornersByDistance = adjacentCorners.OrderBy(c => (data.TargetPosition - c).LengthSquared());
            var secondCorner = adjacentCornersByDistance.First();

            var (xSteps, ySteps) = GetSequences(data.TargetPosition, firstCorner);

            var (middleSequenceX, middleSequenceY) = firstCorner != secondCorner ?
                                                    GetSequences(firstCorner, secondCorner) :
                                                    (new List<int>(), new List<int>());

            var (endSequenceX, endSequenceY) = GetSequences(secondCorner, data.TargetPosition);

            xSteps.AddRange(middleSequenceX);
            xSteps.AddRange(endSequenceX);
            ySteps.AddRange(middleSequenceY);
            ySteps.AddRange(endSequenceY);

            xSteps.Insert(0, 0);
            xSteps.Add(0);
            ySteps.Insert(0, 0);
            ySteps.Add(0);

            var xTime = CalculateTime(xSteps);
            var yTime = CalculateTime(ySteps);
            var totalTime = Math.Max(xTime, yTime);


            /*
            var minX = Math.Min(0, stationPosX) - 3;
            var minY = Math.Min(0, stationPosY) - 3;
            var maxX = Math.Max(0, stationPosX) + 3;
            var maxY = Math.Max(0, stationPosY) + 3;






            var state = new MoveState()
            {
                Position = data.StartPosition,
                TimeLeftX = 0,
                TimeLeftY = 0,
                AddMoveX = "0",
                AddMoveY = "0",
            };
            state.TimeToReachX = TimeToReachX(state, data);
            state.TimeToReachY = TimeToReachY(state, data);
            state.TotalTimeToReach = TimeToReach(state, data);

            // total time it would take at this position and speed, movestates at this time
            var moveStates = new Dictionary<int, List<MoveState>>();
            moveStates.Add(state.TotalTimeToReach, [state]);

            MoveState? finalState = null;

            var speedDiffs = new List<int>
            {
                1, 0, -1
            };
            var minSpeed = -5;
            var maxSpeed = 5;


            while (moveStates.Count > 0)
            {
                if (finalState != null)
                {
                    break;
                }

                var minTime = moveStates.Min(m => m.Key);
                var currentMoves = moveStates[minTime];

                var current = currentMoves.First();

                currentMoves.Remove(current);

                if (currentMoves.Count == 0)
                {
                    moveStates.Remove(minTime);
                }


                var newStates = new List<MoveState>();

                // at the start of the next second, the space ship gets to move on to the next move state and position
                var newSpeedsX = current.TimeLeftX == 0 ? speedDiffs.Select(d => current.SpeedX + d) : [current.SpeedX];
                var newSpeedsY = current.TimeLeftY == 0 ? speedDiffs.Select(d => current.SpeedY + d) : [current.SpeedY];
                foreach (var newSpeedX in newSpeedsX)
                {
                    if (newSpeedX < minSpeed || newSpeedX > maxSpeed)
                    {
                        continue;
                    }

                    foreach (var newSpeedY in newSpeedsY)
                    {
                        if (newSpeedY < minSpeed || newSpeedY > maxSpeed)
                        {
                            continue;
                        }


                        var newState = new MoveState()
                        {
                            SpeedX = newSpeedX,
                            PaceX = GetPace(newSpeedX),
                            SpeedY = newSpeedY,
                            PaceY = GetPace(newSpeedY),
                            Previous = current
                        };
                        newState.AddMoveX = current.TimeLeftX == 0 ? newState.PaceX.ToString() : string.Empty;
                        newState.AddMoveY = current.TimeLeftY == 0 ? newState.PaceY.ToString() : string.Empty;
                        newState.TimeLeftX = current.TimeLeftX > 0 ? current.TimeLeftX : GetTimeForStep(newState.PaceX);
                        newState.TimeLeftY = current.TimeLeftY > 0 ? current.TimeLeftY : GetTimeForStep(newState.PaceY);
                        newStates.Add(newState);
                    }
                }

                // now let the new states sit on their current position until the smaller waiting time is over and then move to new position
                foreach (var newState in newStates)
                {
                    var waitTime = Math.Min(newState.TimeLeftX, newState.TimeLeftY);
                    newState.TimeLeftX -= waitTime;
                    newState.TimeLeftY -= waitTime;
                    newState.Time = current.Time + waitTime;

                    var dirX = newState.TimeLeftX == 0 ? Math.Sign(newState.SpeedX) : 0;
                    var dirY = newState.TimeLeftY == 0 ? Math.Sign(newState.SpeedY) : 0;

                    var newPos = new Vector2(current.Position.X + dirX, current.Position.Y + dirY);

                    if (newPos.X < minX || newPos.X > maxX || newPos.Y < minY || newPos.Y > maxY || forbidden.Contains(newPos))
                    {
                        continue;
                    }
                    newState.Position = newPos;

                    newState.TimeToReachX = TimeToReachX(newState, data) + newState.TimeLeftX;
                    newState.TimeToReachY = TimeToReachY(newState, data) + newState.TimeLeftY;
                    newState.TotalTimeToReach = newState.Time + Math.Max(newState.TimeToReachX, newState.TimeToReachY);

                    if (newState.Position == data.StationPosition)
                    {
                        if (newState.SpeedX <= 1 && newState.SpeedY <= 1)
                        {
                            // we found our solution!
                            finalState = newState;
                            break;
                        }

                    }

                    if (newState.Time > data.TimeLimit)
                    {
                        continue;
                    }

                    if (!CanReach(newState, data))
                    {
                        continue;
                    }

                    if (!moveStates.TryGetValue(newState.TotalTimeToReach, out var movesList))
                    {
                        movesList = new List<MoveState>();
                        moveStates[newState.TotalTimeToReach] = movesList;
                    }
                    movesList.Add(newState);


                }

            }



            // create steps in order
            var xSteps = new List<string>();
            var xSpeeds = new List<int>();
            var ySteps = new List<string>();
            var ySpeeds = new List<int>();

            var positions = new List<Vector2>();

            var currentState = finalState;
            while (true)
            {
                if (!string.IsNullOrEmpty(currentState.AddMoveX))
                {
                    xSteps.Add(currentState.AddMoveX);
                    xSpeeds.Add(currentState.SpeedX);
                }
                if (!string.IsNullOrEmpty(currentState.AddMoveY))
                {
                    ySteps.Add(currentState.AddMoveY);
                    ySpeeds.Add(currentState.SpeedY);
                }
                positions.Add(currentState.Position);
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
            xSpeeds.Reverse();
            ySpeeds.Reverse();
            positions.Reverse();

            // might have to stand still in one direction until the last movement in the other one is finished
            for (int xLeft = 0; xLeft < finalState.TimeLeftX; xLeft++)
            {
                ySteps.Add("0");
                ySpeeds.Add(0);
            }
            for (int yLeft = 0; yLeft < finalState.TimeLeftY; yLeft++)
            {
                xSteps.Add("0");
                xSpeeds.Add(0);
            }


            xSteps.Add("0");
            ySteps.Add("0");
            */
            /*
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

            */


            var resultLine = string.Join(' ', xSteps);
            Console.WriteLine(resultLine);
            fullResult.AppendLine(resultLine);

            resultLine = string.Join(' ', ySteps);
            Console.WriteLine(resultLine);
            fullResult.AppendLine(resultLine);

            Console.WriteLine($"Time: {totalTime}, limit: {data.TimeLimit}");
            var validText = totalTime <= data.TimeLimit ? "Valid" : "INVALID!!!!! Taking too long!!!!";
            Console.WriteLine(validText);

            if (i < actualLines.Count - 1)
            {
                fullResult.AppendLine();
            }
        }

        return fullResult.ToString();
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
        if (dir.X < 0)
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