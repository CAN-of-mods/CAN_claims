using claims.src.auxialiry;
using claims.src.delayed.invitations;
using claims.src.network.packets;
using claims.src.part.structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace claims.src.part
{
    public class PartDemolition
    {
        public static void demolishCity (City city)
        {
            foreach(var plot in city.getCityPlots())
            {
                claims.dataStorage.setNowEpochZoneTimestampFromPlotPosition(plot.getPos());
                claims.serverPlayerMovementListener.markPlotToWasRemoved(plot.getPos());
            }
            demolishCityPlots(city);
            InvitationHandler.deleteAllInvitationsForReceiver(city);
            InvitationHandler.deleteAllInvitationsForSender(city);
            foreach (var it in city.getCityCitizens().ToArray())
            {
                RightsHandler.reapplyRights(it);
                it.clearCity();
                IPlayer player = claims.sapi.World.PlayerByUid(it.Guid);
                if (player != null)
                {
                    claims.serverChannel.SendPacket(new SavedPlotsPacket()
                    {
                        type = PacketsContentEnum.OWN_CITY_DELETED,
                        data = ""

                    }, player as IServerPlayer);
                }
            }
            claims.economyHandler.deleteAccount(city.MoneyAccountName);
            claims.dataStorage.removeCityByGUID(city.Guid);
            //DataStorage.nameToCityDict.TryRemove(city.getPartName(), out _);
            claims.getModInstance().getDatabaseHandler().deleteFromDatabaseCity(city);
        }
      
        public static void demolishCityPlots(City city)
        {
            foreach(Plot plot in city.getCityPlots().ToArray())
            {
                demolishCityPlot(plot);
            }
        }
        public static void demolishCityPlot(Plot plot)
        {
            PlayerInfo player = plot.getPlayerInfo();
            if (player != null)
            {
                player.PlayerPlots.Remove(plot);
            }
            City city = plot.getCity();
            if(city != null)
            {
                city.getCityPlots().Remove(plot);
            }
            if(plot.Type == PlotType.PRISON)
            {
                plot.CleanUpCurrentPlotTypeData();
            }
            //DataStorage.claimedPlots.TryRemove(plot.chunkLocation, out _);
            claims.getModInstance().getDatabaseHandler().deleteFromDatabasePlot(plot);
            claims.dataStorage.removeClaimedPlot(plot.plotPosition);
            TreeAttribute tree = new TreeAttribute();
            tree.SetInt("chX", plot.getPos().X);
            tree.SetInt("chZ", plot.getPos().Y);
            tree.SetString("name", plot.getCity().GetPartName());
            claims.sapi.World.Api.Event.PushEvent("plotunclaimed", tree);
        }
    }
}
