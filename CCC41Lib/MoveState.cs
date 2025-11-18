using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CCC41Lib;

internal class MoveState
{
    public int Time { get; set; }
    public Vector2 Position { get; set; }
    public int SpeedX { get; set; }
    public int SpeedY { get; set; }
    public int PaceX { get; set; }
    public int PaceY { get; set; }
    public int TimeLeftX { get; set; }
    public int TimeLeftY { get; set; }
    public string AddMoveX { get; set; } = string.Empty;
    public string AddMoveY { get; set; } = string.Empty;
    public MoveState? Previous { get; set; }

}
