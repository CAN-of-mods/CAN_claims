using Vintagestory.API.MathTools;

namespace claims.src.auxialiry
{
    public class TrueLocation
    {
        string worldSeed;
        Vec3d position;
        public TrueLocation(string worldSeed, double x, double y, double z)
        {
            this.worldSeed = worldSeed;
            position = new Vec3d(x, y, z);
        }
        public TrueLocation(string worldSeed, Vec3d pos)
        {
            this.worldSeed = worldSeed;
            position = pos;
        }
        public string getWorldSeed()
        {
            return worldSeed;
        }
    }
}
