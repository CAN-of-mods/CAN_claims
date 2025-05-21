using claims.src.auxialiry;
using claims.src.auxialiry.innerclaims;
using claims.src.part.interfaces;
using claims.src.part.structure.plots;
using claims.src.perms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace claims.src.part.structure
{
    public class Plot : Part, IGetStatus
    {
        City city;
        PlayerInfo ownerOfPlot;
        PlotType type;
        public PlotPosition plotPosition;
        double customTax = 0;
        Prison prison;
        int price = -1;
        CityPlotsGroup plotGroup;
        PermsHandler permsHandler = new PermsHandler();
        bool markedNoPvp = false;
        PlotDesc plotDesc;
        public bool extraBought { get; set; }
        public Plot(Vec2i chunkPos) : base("", "")
        {
            this.plotPosition = new PlotPosition(chunkPos);
        }
        public Plot(PlotPosition plp) : base("", "")
        {
            this.plotPosition = plp;
        }
        /*****************************************************************/
        public void setPrison(Prison prison)
        {
            this.prison = prison;
        }
        public PlotDesc getPlotDesc()
        {
            return plotDesc;
        }
        public void setPlotDesc(PlotDesc plotDesc)
        {
            this.plotDesc = plotDesc;
        }
        public bool isMarkedNoPVP()
        {
            return markedNoPvp;
        }
        public void setMarkedNoPVP(bool val)
        {
            markedNoPvp = val;
        }
        public bool hasCutomTax()
        {
            return customTax > 0;
        }
        public void setPlotGroup(CityPlotsGroup cityPlotsGroup)
        {
            this.plotGroup = cityPlotsGroup;
        }
        public void setPlotOwner(PlayerInfo playerInfo)
        {
            ownerOfPlot = playerInfo;
        }

        public PermsHandler getPermsHandler()
        {
            return permsHandler;
        }
         
        public bool hasPlotOwner()
        {
            return ownerOfPlot != null;
        }
        public bool hasCity()
        {
            return city != null;
        }

        
        public bool hasPlotGroup()
        {
            return plotGroup != null;
        }
        public CityPlotsGroup getPlotGroup()
        {
            return plotGroup;
        }

        public PlayerInfo getPlotOwner()
        {
            return ownerOfPlot;
        }

        public double getPrice()
        {
            return price;
        }
        public void setPrice(int val)
        {
            price = val;
        }
        public void resetOwner()
        {
            ownerOfPlot = null;
        }
        public Prison getPrison()
        {
            return prison;
        }
        public City getCity()
        {
            return city;
        }
        public PlayerInfo getPlayerInfo()
        {
            return this.ownerOfPlot;
        }
        public void setCity(City city)
        {
            this.city = city;
        }
        public Vec2i getPos()
        {
            return plotPosition.getPos();
        }
        public PlotType getType()
        {
            return type;
        }
        public void setType(PlotType val)
        {
            this.type = val;
        }
        /// <summary>
        /// Set new type of the plot, there is some co-routines for some types of plots which should be run after we change some of them.
        /// Jail should be removed for example, or summon points removed, etc.
        /// </summary>
        /// <param name="tcr"></param>
        /// <param name="newPlotType"></param>
        /// <param name="player"></param>
        /// <returns>If type was changed successfully.</returns>
        public bool setNewType(TextCommandResult tcr, string newPlotType, IServerPlayer player)
        {
            PlotType plotType = PlotInfo.nameToPlotType[newPlotType];

            if(plotType == this.type)
            {
                tcr.StatusMessage = "claims:plot_the_same_type_set";
                return false; 
            }

            if (plotType == PlotType.CAMP || plotType == PlotType.TOURNAMENT)
            {
                tcr.StatusMessage = "claims:use_other_command_for_that";
                return false;
            }

            //Remove all saved data for plot type
            CleanUpCurrentPlotTypeData();


            //Set new type and init info needed
            if (plotType == PlotType.SUMMON)
            {
                CityLevelInfo cli = Settings.getCityLevelInfo(getCity().getCityCitizens().Count);
                if (getCity().summonPlots.Count >= cli.SummonPlots)
                {
                    tcr.StatusMessage = "claims:limit_summon_plots";
                    return false;
                }
                setType(PlotType.SUMMON);
                getCity().summonPlots.Add(this);
                PlotDescSummon pds = new PlotDescSummon(player.Entity.ServerPos.XYZ);
                setPlotDesc(pds);
                saveToDatabase();
                getCity().saveToDatabase();
                tcr.StatusMessage = "claims:plot_set_type";
                tcr.MessageParams = new object[] { newPlotType };
                return true;
            }
            else if (plotType == PlotType.PRISON)
            {
                PartInits.initPrison(this, getCity(), player);
                setType(plotType);
                tcr.StatusMessage = "claims:plot_set_type";
                tcr.MessageParams = new object[] { newPlotType };
                return true;
            }
            else if (plotType == PlotType.TAVERN)
            {
                int tavernCount = 0;
                foreach (var it in getCity().getCityPlots())
                {
                    if (it.getType() == PlotType.TAVERN)
                        tavernCount++;
                }
                if (tavernCount >= claims.config.MAX_NUMBER_TAVERN_PER_CITY)
                {
                    tcr.StatusMessage = "claims:too_much_taverns";
                    tcr.MessageParams = new object[] { claims.config.MAX_NUMBER_TAVERN_PER_CITY };
                    return false;
                }
                setType(plotType);
                PlotDescTavern pdt = new PlotDescTavern();
                setPlotDesc(pdt);
                saveToDatabase();
                tcr.StatusMessage = "claims:plot_set_type";
                tcr.MessageParams = new object[] { newPlotType };
                return true;
            }
            else if( plotType == PlotType.TEMPLE)
            {
                setType(plotType);

            }


            setType(plotType);
            saveToDatabase();
            tcr.StatusMessage = "claims:plot_set_type";
            tcr.MessageParams = new object[] { newPlotType };
            return true;
        }

        public void CleanUpCurrentPlotTypeData()
        {
            PlotType currentPlotType = getType();
            if (currentPlotType == PlotType.SUMMON)
            {
                setPlotDesc(null);
                getCity().summonPlots.Remove(this);
            }
            else if (currentPlotType == PlotType.PRISON)
            {
                foreach (PrisonCellInfo cell in prison.getPrisonCells())
                {
                    foreach (PlayerInfo player in cell.getPlayerInfos())
                    {
                        EntityPos ep = claims.sapi.World.DefaultSpawnPosition;
                        (claims.sapi.World.PlayerByUid(player.Guid) as IServerPlayer).SetSpawnPosition(new PlayerSpawnPos((int)ep.X, (int)ep.Y, (int)ep.Z));
                        (claims.sapi.World.PlayerByUid(player.Guid) as IServerPlayer).
                            Entity.TeleportToDouble(ep.X, ep.Y, ep.Z);
                    }
                }

                claims.dataStorage.removePrison(prison.Guid);
                if (prison.getCity() != null)
                {
                    prison.getCity().getPrisons().Remove(prison);
                    prison.getCity().saveToDatabase();
                }
                prison.getPlot().setType(PlotType.DEFAULT);
                claims.getModInstance().getDatabaseHandler().deleteFromDatabasePrison(prison);
                prison.getPlot().setPrison(null);
                this.saveToDatabase();                
            }
            else if (currentPlotType == PlotType.EMBASSY)
            {
                if (!this.hasPlotOwner())
                {
                    return;
                }

                PlayerInfo playerInfo = this.getPlotOwner();
                if (playerInfo.hasCity() && this.getCity().Equals(playerInfo.City))
                {
                    return;
                }
                playerInfo.PlayerPlots.Remove(this);
                playerInfo.saveToDatabase();
            }
            else if(currentPlotType == PlotType.TEMPLE)
            {
                city.RemoveTempleRespawnPoint(this);
            }
        }
        public double getCustomTax()
        {
            return customTax;
        }
        public bool setCustomTax(double val)
        {
            if(customTax == val)
            {
                return false;
            }
            customTax = val;
            return true;
        }
        /*****************************************************************/
        public override bool saveToDatabase(bool update = true)
        {
            return claims.getModInstance().getDatabaseHandler().savePlot(this, update);
        }
        public bool hasCityPlotsGroup()
        {
            return plotGroup != null;
        }
        public List<ClientInnerClaim> GetClientInnerClaimFromDefault(PlayerInfo playerInfo)
        {
            List<ClientInnerClaim> tmpCIC = new List<ClientInnerClaim>();
            foreach (var it in (plotDesc as PlotDescTavern).innerClaims)
            {
                if(it.membersUids.Contains(playerInfo.Guid))
                {
                    tmpCIC.Add(new ClientInnerClaim(it.pos1, it.pos2, it.permissionsFlags));
                }
            }
            if(tmpCIC.Count > 0) 
            {
                return tmpCIC;
            }
            return null;
        }
        public List<string> getStatus(PlayerInfo forPlayer = null)
        {           
            List<string> outStrings = new List<string>();
            if(GetPartName() != "")
            {
                outStrings.Add(GetPartName() + "\n");
            }
           
            if(hasCity())
            {
                outStrings.Add(Lang.Get("claims:city") + getCity().getPartNameReplaceUnder() + "\n");
            }

            if(hasPlotOwner())
            {
                outStrings.Add(Lang.Get("claims:plot_owner", getPlotOwner().GetPartName()) + "\n");
            }
            PlotInfo.dictPlotTypes.TryGetValue(getType(), out PlotInfo plotInfo);
            outStrings.Add(Lang.Get("claims:" + plotInfo.getFullName()) + "\n");
            if(customTax > 0)
            {
                outStrings.Add(Lang.Get("claims:custom_plottax", customTax));
                outStrings.Add("\n");
            }
            if(hasPlotGroup())
            {
                outStrings.Add(Lang.Get("claims:plot_group", getPlotGroup().getPartNameReplaceUnder()));
            }
            outStrings.Add(permsHandler.getStringForChat() + "\n");
            return outStrings;
        }
    }
}
