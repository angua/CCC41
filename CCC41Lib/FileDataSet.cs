using System.Numerics;

namespace CCC41Lib;

public class FileDataSet
{
    private string _file = string.Empty;

    public FileDataSet(int level, string file)
    {
        Level = level;
        DataSets = Parse(Level, file);
        _file = file;
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
        if (level == 6)
        {
            var actualLines = lines.Skip(1).ToList();
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

        return dataSets;
    }
}
