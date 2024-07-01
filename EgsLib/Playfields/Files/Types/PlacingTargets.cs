using System;
using System.Collections.Generic;
using System.Text;

namespace EgsLib.Playfields.Files.Types
{
    [Flags]
    public enum PlacingTargets
    {
        Default = 0,
        Terrain = 1,
        OnWater = 2,
        UnderWater = 4,
        ResourceAll = 5
    }
}
