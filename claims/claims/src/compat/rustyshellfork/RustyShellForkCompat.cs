using HarmonyLib;
using RustyShell.Utilities.Blasts;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace claims.src.rustyshellfork
{
    public class RustyShellForkCompat: ModSystem
    {
        public static Harmony harmonyInstance;
        public const string harmonyID = "claims.RustyShellFork.Patches";
        public override double ExecuteOrder()
        {
            return 3;
        }
        public override bool ShouldLoad(ICoreAPI api)
        {
            if (base.ShouldLoad(api))
            {
                if (api.Side == EnumAppSide.Client || !api.ModLoader.IsModEnabled("rustyshellfork"))
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        public override void StartServerSide(ICoreServerAPI api)
        {
            harmonyInstance = new Harmony(harmonyID);
            harmonyInstance.Patch(typeof(BlastExtensions).GetMethod("CommonBlast"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_IServerWorldAccessor_CommonBlast")));
            harmonyInstance.Patch(typeof(BlastExtensions).GetMethod("GasBlast"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_IServerWorldAccessor_GasBlast")));
            harmonyInstance.Patch(typeof(BlastExtensions).GetMethod("IncendiaryBlast"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_IServerWorldAccessor_IncendiaryBlast")));           
        }
    }
}
