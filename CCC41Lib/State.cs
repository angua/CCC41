using System.Diagnostics;
using System.Numerics;

namespace CCC41Lib;

[DebuggerDisplay("{Position} (Distance: {DistanceToStation}, Cost: {Cost})")]
internal class State
{
    private Lazy<int> _cost;

    public State()
    {
        _cost = new Lazy<int>(() => CalculateCost(StepsX) + CalculateCost(StepsY));
    }

    public Vector2 Position { get; set; }

    public List<int> StepsX { get; set; } = new();

    public List<int> StepsY { get; set; } = new();

    public float DistanceToStation { get; set; }

    public int Cost => _cost.Value;

    private static int CalculateCost(List<int> paces)
    {
        return paces.Select(i => i == 0 ? 1 : Math.Abs(i)).Sum();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position.GetHashCode(), StepsX.Count == 0 ? 0 : StepsX[StepsX.Count - 1].GetHashCode(), StepsY.Count == 0 ? 0 : StepsY[StepsY.Count - 1].GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        return obj is State other && Position.Equals(other.Position) && StepsX.LastOrDefault().Equals(other.StepsX.LastOrDefault()) && StepsY.LastOrDefault().Equals(other.StepsY.LastOrDefault());
    }
}
