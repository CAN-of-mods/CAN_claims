
using System;
using claims.src.auxialiry;
using claims.src.part.structure;
using HarmonyLib;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace claims.src.rustyshellfork
{
    [HarmonyPatch]
    public class harmPatch
    {
        public static bool CommonLogic(IServerWorldAccessor __instance,
        Entity byEntity,
        Vec3f pos,
        int blastRadius,
        int injureRadius,
        int strength)
        {
            int usedRadius = Math.Max(blastRadius, injureRadius);
            int tmpX = (int)pos.X;
            int tmpZ = (int)pos.Z;
            
            bool blastEV = claims.dataStorage.getWorldInfo().blastEverywhere;
            if (blastEV)
            {
                return true;
            }
            if (claims.dataStorage.getWorldInfo().blastForbidden)
            {
                return false;
            }
            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {

                    claims.dataStorage.getPlot(PlotPosition.fromXZ((int)(tmpX + (i * blastRadius)),
                                                                          (int)(tmpZ + (j * blastRadius))), out Plot tb);
                    if (tb == null)
                    {
                        continue;
                    }

                    if ((!tb.getPermsHandler().blastFlag || !tb.getCity().getPermsHandler().blastFlag))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static bool Prefix_IServerWorldAccessor_CommonBlast(IServerWorldAccessor __instance,
        Entity byEntity,
        Vec3f pos,
        int blastRadius,
        int injureRadius,
        int strength)
        {
            return CommonLogic(__instance, byEntity, pos, blastRadius, injureRadius, strength);
        }
        public static bool Prefix_IServerWorldAccessor_GasBlast(IServerWorldAccessor __instance,
        Entity byEntity,
        Vec3f pos,
        int blastRadius,
        int millisecondDuration)
        {
            return CommonLogic(__instance, byEntity, pos, blastRadius, 0, 0);
        }
        public static bool Prefix_IServerWorldAccessor_IncendiaryBlast(IServerWorldAccessor __instance,
        Entity byEntity,
        Vec3f pos,
        int blastRadius,
        int injureRadius)
        {
            return CommonLogic(__instance, byEntity, pos, blastRadius, injureRadius, 0);
        }
    }
}
