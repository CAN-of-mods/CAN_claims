using claims.src.auxialiry;
using claims.src.part.structure;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace claims.src.events
{
    public class OnTryEntitySpawn
    {
        public static bool Event_OnTrySpawnEntity(IBlockAccessor blockAccessor, ref EntityProperties properties, Vec3d spawnPosition, long herdId)
        {
            var hostile = properties.Server?.SpawnConditions?.Runtime?.Group.Equals("hostile");
            if(hostile.HasValue && hostile.Value)
            {
                if(claims.dataStorage.getPlot(PlotPosition.fromXZ((int)spawnPosition.X, (int)spawnPosition.Z), out Plot plot))
                {
                    //TODO 
                    //check for plot/city flags
                    return false;
                }
                //CHECK FOR CITY HERE
                //LATER ON CHECK FOR CHUNK/CITY FLAGS
            }
            return true;
        }
    }
}
