using System.Collections.Generic;
using System.Security.AccessControl;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Server;

namespace claims.src.events
{
    public class ServerEvents
    {
        public static void AddEvents(ICoreServerAPI sapi)
        {
            sapi.Event.RegisterGameTickListener(PlayerMovementsListnerServer.checkPlayerMove, claims.config.DELTA_TIME_PLAYER_POSITION_CHECK);
            sapi.Event.RegisterEventBusListener(claims.serverPlayerMovementListener.onPlayerChangePlotEvent, 0.5, "claimsPlayerChangePlot");

            sapi.Event.PlayerJoin += events.OnPlayerJoin.Event_OnPlayerJoin;
            sapi.Event.PlayerDisconnect += events.OnPlayerDisconnect.Event_OnPlayerDisconnect;
         
            sapi.Event.CanUseBlock += events.OnBlockAction.Event_OnBlockUse;
            sapi.Event.CanPlaceOrBreakBlock += events.OnBlockAction.Event_OnBlockDestroy;

            sapi.Event.PlayerChat += events.OnPlayerChat.onPlayerChat;
            sapi.Event.OnTrySpawnEntity += events.OnTryEntitySpawn.Event_OnTrySpawnEntity;
            sapi.Event.PlayerDeath += events.OnPlayerDeath.Event_OnPlayerDeath;

            sapi.Event.ServerRunPhase(EnumServerRunPhase.ModsAndConfigReady, events.ModConfigReady.onModsAndConfigReady);
            sapi.Event.ServerRunPhase(EnumServerRunPhase.RunGame, () => 
            {
                claims.config.COINS_VALUES_TO_CODE.Clear();
                caneconomy.caneconomy.config.COINS_VALUES_TO_CODE.Foreach(el => claims.config.COINS_VALUES_TO_CODE.Add((double)el.Key, el.Value));
                claims.config.ID_TO_COINS_VALUES.Clear();
                caneconomy.caneconomy.config.ID_TO_COINS_VALUES.Foreach(el => claims.config.ID_TO_COINS_VALUES.Add(el.Key, (double)el.Value));
            });

            
            //sapi.Event.ServerRunPhase(EnumServerRunPhase.Shutdown, onShutdown);
            //sapi.Event.ServerRunPhase(EnumServerRunPhase.Exit, onServerExit);
        }

        public static void onShutdown()
        {
            Dictionary<string, int> tmpDict = new Dictionary<string, int>();
            foreach (var pl in claims.dataStorage.getPlayersDict())
            {
                tmpDict[pl.Key] = (int)pl.Value.showPlotMovement;
            }
            if (tmpDict.Count != 0)
            {
                claims.sapi.WorldManager.SaveGame.StoreData<Dictionary<string, int>>("claimsshowchunkmsgs", tmpDict);
            }

            if (claims.dataStorage.serverClaimAreaHandler != null)
            {
                claims.dataStorage.serverClaimAreaHandler.SaveIntoMap();
            }

        }

        public static void onServerExit()
        {
            //claims.NullOnServerExit();     
        }
    }
}
