using System;
using Vintagestory.API.MathTools;

namespace claims.src.auxialiry
{
    public class MathClaims
    {
        public static int distanceBetween(Vec2i cityPlotPos, Vec2i checkPos)
        {
            return (int)Math.Sqrt(Math.Pow(checkPos.X - cityPlotPos.X, 2) + Math.Pow(checkPos.Y - cityPlotPos.Y, 2));
        }

    }
}
