namespace CCC41Lib
{
    internal class StateComparer : IComparer<State>
    {
        public int Compare(State? x, State? y)
        {
            return x!.DistanceToStation.CompareTo(y!.DistanceToStation);
        }
    }
}