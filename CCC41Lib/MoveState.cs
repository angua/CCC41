using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CCC41Lib;

public class MoveState
{
    public int Time { get; set; }
    public Vector2 Position { get; set; }
    public int SpeedX { get; set; }
    public int SpeedY { get; set; }
    public int PaceX { get; set; }
    public int PaceY { get; set; }
    public int TimeLeftX { get; set; }
    public int TimeLeftY { get; set; }
    public int? AddMoveX { get; set; } = null;
    public int? AddMoveY { get; set; } = null;
    public MoveState? Previous { get; set; }
    public int TimeToReachX { get; set; }
    public int TimeToReachY { get; set; }
    public int TimeToReach { get; set; }
    public int Cost { get; set; }

}
