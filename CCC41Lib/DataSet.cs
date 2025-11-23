using System.Numerics;

namespace CCC41Lib;

public class DataSet
{
    public Vector2 StartPosition { get; set; } = Vector2.Zero;
    public Vector2 TargetPosition { get; set; }
    public int TimeLimit { get; set; }

    public List<Vector2> Asteroids { get; set; } = new();
    public HashSet<Vector2> ForbiddenAreas { get; set; } = new();

    public Vector2 BoundsMin { get; set; }
    public Vector2 BoundsMax { get; set; }

    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;

    public List<int> XSequence { get; set; } = new();
    public List<int> YSequence { get; set; } = new();
    public string XSequenceString => string.Join(' ', XSequence);
    public string YSequenceString => string.Join(' ', YSequence);
    public Dictionary<int, Vector2> TimedPositions { get; set; } = new();
    public int TimeUsed { get; set; }
    public bool Valid { get; set; }
    public List<string> ErrorText { get; set; } = new();

    public Vector2 GetGridPosition(Vector2 position) => position - BoundsMin;


}
