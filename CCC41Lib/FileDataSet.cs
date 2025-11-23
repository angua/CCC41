using System.Numerics;

namespace CCC41Lib;

public class FileDataSet
{
    public string FilePath = string.Empty;

    public FileDataSet(int level, string file)
    {
        Level = level;
        DataSets = Parse(Level, file);
        FilePath = file;
    }

    public FileDataSet(int level, List<string> lines)
    {
        Level = level;
        DataSets = Parse(Level, lines);
    }

    public int Level { get; set; }

    public int DataSetsCount => DataSets.Count;

    public List<DataSet> DataSets { get; set; } = new();

    public List<DataSet> Parse(int level, string inputfilename)
    {
        var lines = File.ReadAllLines(inputfilename).ToList();
        return Parse(level, lines);
    }

    private static List<DataSet> Parse(int level, List<string> lines)
    {
        var dataSets = new List<DataSet>();
        var actualLines = lines.Skip(1).ToList();

        switch (level)
        {
            case 1:
                foreach (var line in actualLines)
                {
                    var data = new DataSet();
                    var parts = line.Split(' ');
                    data.XSequence = parts.Select(p => int.Parse(p)).ToList();
                    dataSets.Add(data);
                }
                break;

            case 2:
                foreach (var line in actualLines)
                {
                    var data = new DataSet();
                    var parts = line.Split(' ');
                    data.XSequence = parts.Select(p => int.Parse(p)).ToList();
                    dataSets.Add(data);
                }
                break;

            case 3:
                foreach (var line in actualLines)
                {
                    var data = new DataSet();
                    var parts = line.Split(' ');
                    data.TargetPosition = new Vector2(int.Parse(parts[0]), 0);
                    data.TimeLimit = int.Parse(parts[1]);
                    Solver.SetBounds(data);
                    dataSets.Add(data);
                }
                break;

            case 4:
                foreach (var line in actualLines)
                {
                    var data = new DataSet();
                    var parts = line.Split(new char[] { ' ', ',' });
                    data.TargetPosition = new Vector2(int.Parse(parts[0]), int.Parse(parts[1]));
                    data.TimeLimit = int.Parse(parts[2]);
                    Solver.SetBounds(data);
                    dataSets.Add(data);
                }
                break;

            case 5:
                ParseLevel5(dataSets, actualLines);
                break;

            case 6:
                ParseLevel5(dataSets, actualLines);
                break;

            case 7:
                ParseLevel7(dataSets, actualLines);
                break;

            default:

                break;
        }
        return dataSets;
    }

    private static void ParseLevel7(List<DataSet> dataSets, List<string> actualLines)
    {
        for (int i = 0; i < actualLines.Count; i++)
        {
            var dataSet = new DataSet();
            dataSet.ForbiddenAreas = new List<Vector2>();

            string? line = actualLines[i];
            var parts = line.Split(new char[] { ' ', ',' });
            dataSet.TargetPosition = new Vector2(int.Parse(parts[0]), int.Parse(parts[1]));
            dataSet.TimeLimit = int.Parse(parts[2]);

            ++i;
            var asteroidCount = actualLines[i];
            ++i;

            var asteroidLine = actualLines[i];
            var asteroids = asteroidLine.Split(" ");
            foreach (var asteroid in asteroids)
            {
                var coordsParts = asteroid.Split(",");
                var asteroidPosition = new Vector2(int.Parse(coordsParts[0]), int.Parse(coordsParts[1]));
                dataSet.Asteroids.Add(asteroidPosition);

                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        dataSet.ForbiddenAreas.Add(new Vector2(asteroidPosition.X + x, asteroidPosition.Y + y));
                    }
                }
            }
            Solver.SetBounds(dataSet);
            dataSets.Add(dataSet);

        }
    }

    private static void ParseLevel5(List<DataSet> dataSets, List<string> actualLines)
    {
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

            var data = new DataSet()
            {
                TargetPosition = new Vector2(stationPosX, stationPosY),
                Asteroids = [new Vector2(asteroidX, asteroidY)],
                TimeLimit = timeLimit,
                ForbiddenAreas = forbidden
            };

            var minX = Math.Min(data.StartPosition.X, Math.Min(stationPosX, asteroidX)) - 5;
            var maxX = Math.Max(data.StartPosition.X, Math.Max(stationPosX, asteroidX)) + 5;
            var minY = Math.Min(data.StartPosition.Y, Math.Min(stationPosY, asteroidY)) - 5;
            var maxY = Math.Max(data.StartPosition.Y, Math.Max(stationPosY, asteroidY)) + 5;

            data.BoundsMin = new Vector2(minX, minY);
            data.BoundsMax = new Vector2(maxX, maxY);
            data.Width = (int)(data.BoundsMax.X - data.BoundsMin.X + 1);
            data.Height = (int)(data.BoundsMax.Y - data.BoundsMin.Y + 1);

            dataSets.Add(data);
        }
    }
}
