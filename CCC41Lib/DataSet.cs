using System.Numerics;

namespace CCC41Lib;

public class DataSet
{
    public Vector2 StartPosition { get; set; } = Vector2.Zero;
    public Vector2 TargetPosition { get; set; }
    public int TimeLimit { get; set; }

    public List<Vector2> Asteroids { get; set; } = new();
    public List<Vector2> ForbiddenAreas { get; set; } = new();

    public Vector2 BoundsMin { get; set; }
    public Vector2 BoundsMax { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public Vector2 GetGridPosition(Vector2 position) => position - BoundsMin;


}
