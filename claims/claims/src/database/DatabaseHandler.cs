using claims.src.part;
using claims.src.part.structure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.database
{
    public abstract class DatabaseHandler
    {
        public DatabaseHandler()
        {

        }

        //Maintenance
        abstract public bool makeBackup(string fileName);
        

        //PlayerInfo
        abstract public bool savePlayerInfo(PlayerInfo player, bool update = true);
        abstract public bool deleteFromDatabasePlayerInfo(PlayerInfo player);
        abstract public bool loadPlayerInfo(DataRow it);
        abstract public bool loadAllPlayersInfo();
        abstract public bool loadDummyPlayers();
        //City
        abstract public bool saveCity(City city, bool update = true);
        abstract public bool deleteFromDatabaseCity(City city);
        abstract public bool loadCity(DataRow it);
        abstract public bool loadAllCitis();
        abstract public bool loadDummyCitis();
        //Plot
        abstract public bool savePlot(Plot plot, bool update = true);
        abstract public bool deleteFromDatabasePlot(Plot plot);
        abstract public bool loadPlot(DataRow it);
        abstract public bool loadAllPlots();
        abstract public bool loadDummyPlots();
        //WorldInfo
        abstract public bool saveWorldInfo(WorldInfo worldInfo, bool update = true);
        abstract public bool deleteFromDatabaseWorldInfo(WorldInfo worldInfo);
        abstract public bool loadWorldInfo(DataRow it);
        abstract public bool loadDummyWolrdInfo();

        //PRISON
        abstract public bool savePrison(Prison prison, bool update = true);
        abstract public bool deleteFromDatabasePrison(Prison prison);
        abstract public bool loadPrison(DataRow it);
        abstract public bool loadAllPrisons();
        abstract public bool loadDummyPrisons();

        //CITYPLOTGROUP
        abstract public bool saveCityPlotGroup(CityPlotsGroup plotgroup, bool update = true);
        abstract public bool deleteFromDatabaseCityPlotGroup(CityPlotsGroup plotgroup);
        abstract public bool loadCityPlotGroup(DataRow it);
        abstract public bool loadAllCityPlotGroups();
        abstract public bool loadDummyCityPlotGroups();

        //ALLIANCE
        abstract public bool saveAlliance(Alliance alliance, bool update = true);
        abstract public bool deleteFromDatabaseAlliance(Alliance alliance);
        abstract public bool loadAlliance(DataRow it);
        abstract public bool loadAllAlliancies();
        abstract public bool loadDummyAlliancies();

        //General
        public bool loadEveryThing()
        {
            return loadDummyWolrdInfo()
               && loadDummyCitis()
               && loadDummyPlayers()
               && loadDummyPlots()
               && loadDummyPrisons()
               && loadDummyCityPlotGroups()
               && loadAllPlayersInfo()
               && loadAllPlots()
               && loadAllCityPlotGroups()
               && loadAllCitis()        
               && loadAllPrisons();              
        }
        abstract public bool saveEveryThing();

    }
}
