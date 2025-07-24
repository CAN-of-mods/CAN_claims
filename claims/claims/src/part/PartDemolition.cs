using claims.src.auxialiry;
using claims.src.delayed.invitations;
using claims.src.gui.playerGui.structures;
using claims.src.messages;
using claims.src.network.packets;
using claims.src.part.structure;
using claims.src.part.structure.conflict;
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
        public static void demolishCity (City city, string reason)
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
            MessageHandler.sendDebugMsg(string.Format("City {0} was deleted, ", city.GetPartName()) + reason);
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
        public static void DemolishAlliance(Alliance alliance)
        {
            InvitationHandler.deleteAllInvitationsForSender(alliance);

            //TODO
            /*foreach (Conflict conflict in claims.dataStorage.getConflictsList().ToArray())
            {
                if (conflict.getFirstSide().Equals(alliance))
                {
                    if (conflict.getConflictState() == ConflictState.FIRST_WON)
                    {
                        conflict.getSecondSide().setConqueredBy(null);
                        conflict.getSecondSide().setDaysBeforeFreedom(0);
                        lock (claims.dataStorage.getConflictsList())
                        {
                            claims.dataStorage.tryRemoveConflict(conflict);
                            claims.getModInstance().getDatabaseHandler().deleteFromDatabaseConflict(conflict);
                        }
                    }
                }
                else if (conflict.getSecondSide().Equals(alliance))
                {
                    if (conflict.getConflictState() == ConflictState.SECOND_WON)
                    {
                        conflict.getFirstSide().setConqueredBy(null);
                        conflict.getFirstSide().setDaysBeforeFreedom(0);
                        lock (claims.dataStorage.getConflictsList())
                        {
                            claims.dataStorage.tryRemoveConflict(conflict);
                            claims.getModInstance().getDatabaseHandler().deleteFromDatabaseConflict(conflict);
                        }
                    }
                }
            }*/

            //FOR HOSTILE ALLIANCE WE DELETE OUR CITIES FROM HOSTILES FOR THIER CITIES
            foreach (Alliance otherAlliance in alliance.Hostiles)
            {
                foreach (City otherCity in otherAlliance.Cities)
                {
                    foreach (City city in alliance.Cities)
                    {
                        otherCity.HostileCities.Remove(city);
                        otherCity.saveToDatabase();
                    }

                }
            }

            foreach (Alliance comradeAlliance in alliance.ComradAlliancies)
            {
                comradeAlliance.ComradAlliancies.Remove(alliance);
                comradeAlliance.saveToDatabase();
            }

            foreach (City city in alliance.Cities)
            {

                city.Alliance = null;
                foreach (var it in city.getCityCitizens())
                {
                    it.ClearAllAllianceTitles();
                    RightsHandler.reapplyRights(it);
                }
                city.saveToDatabase();
            }
            foreach (var it in alliance.Cities)
            {
                UsefullPacketsSend.AddToQueueCityInfoUpdate(it.Guid, EnumPlayerRelatedInfo.OWN_ALLIANCE_REMOVE);
            }
            claims.getModInstance().getDatabaseHandler().deleteFromDatabaseAlliance(alliance);
        }
        public static void DemolishConflict(Conflict conflict)
        {
            claims.dataStorage.TryRemoveConflict(conflict);
            foreach (City ourCity in conflict.First.Cities)
            {
                foreach (City targetCity in conflict.Second.Cities)
                {
                    ourCity.HostileCities.Add(targetCity);
                    ourCity.saveToDatabase();
                }
            }
            foreach (City targetCity in conflict.First.Cities)
            {
                foreach (City ourCity in conflict.Second.Cities)
                {
                    targetCity.HostileCities.Add(ourCity);
                    targetCity.saveToDatabase();
                }
            }
            conflict.First.RunningConflicts.Remove(conflict);
            conflict.Second.RunningConflicts.Remove(conflict);
            conflict.First.Hostiles.Remove(conflict.Second);
            conflict.Second.Hostiles.Remove(conflict.First);
            conflict.First.saveToDatabase();
            conflict.Second.saveToDatabase();
            claims.getModInstance().getDatabaseHandler().deleteFromDatabaseConflict(conflict);
        }
    }
}
