using System.Numerics;

namespace CCC41Lib;

internal class DataSet
{
    public Vector2 StartPosition { get; set; } = Vector2.Zero;
    public Vector2 StationPosition { get; set; }
    public int TimeLimit { get; set; }

    public List<Vector2> Asteroids { get; set; } = new();

}
