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
        public static Mod ModPC;
        public override double ExecuteOrder()
        {
            return 3;
        }
        /*public override bool ShouldLoad(EnumAppSide forSide)
        {
            if(forSide == EnumAppSide.Client)
            {
                if(claims.src.claims.capi)
            }
            if(claims.api == null)
            {
                return false;
            }
            return dummyplayer.api.ModLoader.IsModEnabled("playercorpse");
        }*/
        public override void StartServerSide(ICoreServerAPI api)
        {

            if(!api.ModLoader.IsModEnabled("rustyshellfork"))
            {
                return;
            }
            harmonyInstance = new Harmony(harmonyID);
            var c = typeof(BlastExtensions).GetMethodNames();
            //api.World.CommonBlast
            harmonyInstance.Patch(typeof(BlastExtensions).GetMethod("CommonBlast"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_IServerWorldAccessor_CommonBlast")));
            harmonyInstance.Patch(typeof(BlastExtensions).GetMethod("GasBlast"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_IServerWorldAccessor_GasBlast")));
            harmonyInstance.Patch(typeof(BlastExtensions).GetMethod("IncendiaryBlast"), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_IServerWorldAccessor_IncendiaryBlast")));
            //ModPC = api.ModLoader.GetModSystem<PlayerCorpse.Systems.DeathContentManager>().Mod;
            //harmonyInstance.Patch(typeof(DeathContentManager).GetMethod("OnEntityDeath", BindingFlags.NonPublic | BindingFlags.Instance), prefix: new HarmonyMethod(typeof(harmPatch).GetMethod("Prefix_DeathContentManager_OnEntityDeath")));
        }
    }
}
