using claims.src.auxialiry;
using claims.src.gui.playerGui.structures;
using claims.src.messages;
using claims.src.network.packets;
using claims.src.part.structure;
using claims.src.part.structure.plots;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace claims.src.part
{
    public class PartInits
    {
        public static void initNewCity(PlayerInfo creator, PlotPosition pos, string cityName)
        {
            string guid;

            while(true)
            {
                guid = Guid.NewGuid().ToString();
                if (claims.dataStorage.checkGuidForCityVillage(guid))
                    break;
            }
            City city = new City(cityName, guid);
            claims.dataStorage.addCity(city);
           // DataStorage.nameToCityDict.TryAdd(cityName, city);

            if (creator != null)
            {
                creator.setCity(city);
                city.setMayor(creator);
                city.getCityCitizens().Add(creator);
                city.setIsTechnicalCity(false);
            }
            else
            {
                city.setIsTechnicalCity(true);
            }
            city.TimeStampCreated = TimeFunctions.getEpochSeconds();

            Plot newPlot = new Plot(pos);
            newPlot.Price = - 1;
            newPlot.setCity(city);
            newPlot.Type = PlotType.MAIN_CITY_PLOT;
            claims.dataStorage.addClaimedPlot(newPlot.plotPosition, newPlot);
            city.getCityPlots().Add(newPlot);
            if (creator != null)
            {
                creator.saveToDatabase();
            }
            
            city.saveToDatabase();
            newPlot.saveToDatabase();
            claims.dataStorage.clearCacheForPlayersInPlot(newPlot);
            claims.serverPlayerMovementListener.markPlotToWasReUpdated(newPlot.getPos());
            claims.dataStorage.setNowEpochZoneTimestampFromPlotPosition(newPlot.getPos());

            MessageHandler.sendGlobalMsg(Lang.Get("claims:new_city_created", StringFunctions.replaceUnderscore(cityName), creator != null ? creator.GetPartName() : ""));
            TreeAttribute tree = new TreeAttribute();
            tree.SetInt("chX", newPlot.getPos().X);
            tree.SetInt("chZ", newPlot.getPos().Y);
            tree.SetString("name", newPlot.getCity().GetPartName());
            claims.sapi.World.Api.Event.PushEvent("plotclaimed", tree);
            if (creator != null)
            {
                RightsHandler.reapplyRights(creator);
            }

            var player = creator != null ? claims.sapi.World.PlayerByUid(creator.Guid) : null;
            if (player != null) {

                Dictionary<EnumPlayerRelatedInfo, string> collector = new Dictionary<EnumPlayerRelatedInfo, string>
                {
                    { EnumPlayerRelatedInfo.CITY_NAME, city.GetPartName() }
                };
                if (city.getMayor() != null)
                {
                    collector.Add(EnumPlayerRelatedInfo.MAYOR_NAME, city.getMayor().GetPartName());
                }
                collector.Add(EnumPlayerRelatedInfo.CITY_CREATED_TIMESTAMP, city.TimeStampCreated.ToString());
                collector.Add(EnumPlayerRelatedInfo.CITY_MEMBERS, JsonConvert.SerializeObject(StringFunctions.getNamesOfCitizens(city)));
                collector.Add(EnumPlayerRelatedInfo.MAX_COUNT_PLOTS, Settings.getMaxNumberOfPlotForCity(city).ToString());
                collector.Add(EnumPlayerRelatedInfo.CLAIMED_PLOTS, city.getCityPlots().Count.ToString());
                collector.Add(EnumPlayerRelatedInfo.PLAYER_PREFIX, creator.Prefix);
                collector.Add(EnumPlayerRelatedInfo.PLAYER_AFTER_NAME, creator.AfterName);
                collector.Add(EnumPlayerRelatedInfo.PLAYER_CITY_TITLES, JsonConvert.SerializeObject(creator.getCityTitles()));
                collector.Add(EnumPlayerRelatedInfo.SHOW_PLOT_MOVEMENT, ((int)creator.showPlotMovement).ToString());

                claims.serverChannel.SendPacket(new SavedPlotsPacket()
                {
                    type = PacketsContentEnum.OWN_NEW_CITY_CREATED,
                    data = JsonConvert.SerializeObject(collector)

                }, player as IServerPlayer);
            }
           
            claims.economyHandler.newAccount(city.MoneyAccountName, new Dictionary<string, object> { { "lastknownname", city.GetPartName() } });
            return;
        }      
        public static void initPrison(Plot plot, City city, IServerPlayer creator)
        {
            Guid guid;
            while (true)
            {
                guid = Guid.NewGuid();
                if(claims.dataStorage.prisonExistsByGUID(guid.ToString()))
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            plot.Type = PlotType.PRISON;
            plot.Prison = new Prison("", guid.ToString());
            claims.dataStorage.addPrison(plot.Prison);
            plot.Prison.addPrisonCell(new PrisonCellInfo(new Vec3i((int)creator.Entity.ServerPos.X, (int)creator.Entity.ServerPos.Y, (int)creator.Entity.ServerPos.Z)));
            plot.getCity().getPrisons().Add(plot.Prison);
            plot.Prison.setPlot(plot);
            plot.Prison.setCity(plot.getCity());
            PlotDescPrison pdp = new PlotDescPrison(plot.Prison.Guid);
            plot.PlotDesc = pdp;

            plot.getCity().saveToDatabase();
            plot.saveToDatabase();
            plot.Prison.saveToDatabase();
        }
    }
}
