using claims.src.part;
using claims.src.rights;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;

namespace claims.src.gui.playerGui.structures
{
    public class ClientPlayerInfo 
    {
        public CityInfo CityInfo { get; set; }
        public HashSet<string> Friends { get; set; } = new HashSet<string>();
        public List<ClientToCityInvitation> ReceivedInvitations { get; set; } = new List<ClientToCityInvitation>();
        public List<ClientToPlotsGroupInvitation> ReceivedPlotsGroupInvitations { get; set; } = new List<ClientToPlotsGroupInvitation>();
        public EnumShowPlotMovement ShowPlotMovement { get; set; } = EnumShowPlotMovement.SHOW_NONE;
        public PlayerPermissions PlayerPermissions { get; set; }
        public CurrentPlotInfo CurrentPlotInfo { get; set; }
        public AllianceInfo AllianceInfo { get; set; }
        public List<ClientCityInfoCellElement> AllCitiesList { get; set; } = new List<ClientCityInfoCellElement>();
        public ClientPlayerInfo()
        {
            CityInfo = null;
            ShowPlotMovement = EnumShowPlotMovement.SHOW_HUD;
            PlayerPermissions = new PlayerPermissions();
            CurrentPlotInfo = new CurrentPlotInfo();
        }
        public ClientPlayerInfo(string cityName, string mayorName, long timeStampCreated, HashSet<string> citizens, int maxCountPlots, int countPlots, string prefix,
            string afterName, HashSet<string> cityTitles, EnumShowPlotMovement showPlotMovement, int PlotColor, double cityBalance, HashSet<string> criminals)
        {
            CityInfo = new CityInfo(cityName, mayorName, timeStampCreated, citizens, maxCountPlots, countPlots, prefix, afterName, cityTitles, PlotColor, cityBalance, criminals);
            ShowPlotMovement = showPlotMovement;
        }

        public ClientPlayerInfo(CityInfo cityInfo, HashSet<string> friends, List<ClientToCityInvitation> receivedInvitations, EnumShowPlotMovement showPlotMovement)
        {
            CityInfo = cityInfo;
            Friends = friends;
            ReceivedInvitations = receivedInvitations;
            ShowPlotMovement = showPlotMovement;
        }
        public ClientPlayerInfo(string cityName, string mayorName, string timeStampCreated, string citizens, string maxCountPlots, string countPlots, string prefix,
           string afterName, string cityTitles, string showPlotMovement, int PlotColor, double cityBalance, string cityCriminals)
        {
            long.TryParse(timeStampCreated, out long longStamp);

            HashSet<string> citizenList = JsonConvert.DeserializeObject<HashSet<string>>(citizens);

            int.TryParse(maxCountPlots, out int maxCountInt);
            int.TryParse(countPlots, out int curCountPlotInt);

            HashSet<string> titles = JsonConvert.DeserializeObject<HashSet<string>>(cityTitles);

            int.TryParse(showPlotMovement, out int showInt);

            HashSet<string> criminals = JsonConvert.DeserializeObject<HashSet<string>>(cityCriminals);

            CityInfo = new CityInfo(cityName, mayorName, longStamp, citizenList, maxCountInt, curCountPlotInt, prefix, afterName, titles, PlotColor, cityBalance, criminals);
            ShowPlotMovement = (EnumShowPlotMovement)showInt;
        }

        public static ClientPlayerInfo OnJoinAllInfo(string cityName, string mayorName, string timeStampCreated, string citizens, string maxCountPlots, string countPlots, string prefix,
                                                     string afterName, string cityTitles, string showPlotMovement, string friendsList, int PlotColor, double cityBalance, string cityCriminals)
        {
            long.TryParse(timeStampCreated, out long longStamp);

            HashSet<string> citizenList = JsonConvert.DeserializeObject<HashSet<string>>(citizens);

            int.TryParse(maxCountPlots, out int maxCountInt);
            int.TryParse(countPlots, out int curCountPlotInt);

            HashSet<string> titles = JsonConvert.DeserializeObject<HashSet<string>>(cityTitles);
            int.TryParse(showPlotMovement, out int showInt);

            HashSet<string> friends = JsonConvert.DeserializeObject<HashSet<string>>(friendsList);

            List<ClientToCityInvitation> receivedInvitations = new List<ClientToCityInvitation>(); //todo

            HashSet<string> criminals = JsonConvert.DeserializeObject<HashSet<string>>(cityCriminals);

            CityInfo cityInfo = new CityInfo(cityName, mayorName, longStamp, citizenList, maxCountInt, curCountPlotInt, prefix, afterName, titles, PlotColor, cityBalance, criminals);

            return new ClientPlayerInfo(cityInfo, friends, receivedInvitations, (EnumShowPlotMovement)showInt);
        }

        public void AcceptChangedValues(Dictionary<EnumPlayerRelatedInfo, string> valueDict)
        {
            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.MAYOR_NAME, out string mayor))
            {
                this.CityInfo.MayorName = mayor;
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_NAME, out string cityName))
            {
                this.CityInfo.Name = cityName;
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_CREATED_TIMESTAMP, out string created))
            {
                long.TryParse(created, out long longStamp);
                CityInfo.TimeStampCreated = longStamp;
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_MEMBERS, out string cityMembers))
            {
                CityInfo.PlayersNames = JsonConvert.DeserializeObject<HashSet<string>>(cityMembers);
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.MAX_COUNT_PLOTS, out string maxPlotCount))
            {
                int.TryParse(maxPlotCount, out int maxPlot);
                CityInfo.MaxCountPlots = maxPlot;
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CLAIMED_PLOTS, out string claimedPlots))
            {
                int.TryParse(claimedPlots, out int claimed);
                CityInfo.CountPlots = claimed;
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.PLAYER_PREFIX, out string prefix))
            {
                CityInfo.Prefix = prefix;
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.PLAYER_AFTER_NAME, out string afterName))
            {
                CityInfo.AfterName = afterName;
            }
            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.PLAYER_CITY_TITLES, out string titles))
            {
                CityInfo.CityTitles = JsonConvert.DeserializeObject<HashSet<string>>(titles);
            }
            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.SHOW_PLOT_MOVEMENT, out string showMovement))
            {
                int.TryParse(showMovement, out int showInt);
                ShowPlotMovement = (EnumShowPlotMovement)showInt;
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_INVITE_ADD, out string inviteAddNew))
            {
                this.ReceivedInvitations.Add(JsonConvert.DeserializeObject<ClientToCityInvitation>(inviteAddNew));
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_INVITE_REMOVE, out string inviteRemove))
            {
                var invitationToReemove = this.ReceivedInvitations.Where(invitation => invitation.CityName == inviteRemove).FirstOrDefault();
                if (invitationToReemove != null)
                {
                    this.ReceivedInvitations.Remove(invitationToReemove);
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.PLAYER_PERMISSIONS, out string permissionsSet))
            {
                PlayerPermissions.ClearPermissions();
                PlayerPermissions.AddPermissions(JsonConvert.DeserializeObject<HashSet<EnumPlayerPermissions>>(permissionsSet));
            }
            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.FRIENDS, out string friends))
            {
                Friends = JsonConvert.DeserializeObject<List<string>>(friends).ToHashSet();
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_POSSIBLE_RANKS, out string cityPossibleRanks))
            {
                CityInfo.PossibleCityRanks = JsonConvert.DeserializeObject<List<string>>(cityPossibleRanks).ToHashSet();
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_CITIZENS_RANKS, out string citizenRaks))
            {
                Dictionary<string, List<string>> di = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(citizenRaks);
                foreach(var it in di)
                {
                    var foundCell = CityInfo.CitizensRanks.FirstOrDefault(cell => cell.RankName == it.Key);
                    if(foundCell != null) 
                    {
                        foundCell.CitizensRanks = it.Value;
                    }
                    else
                    {
                        CityInfo.CitizensRanks.Add(new RankCellElement(it.Key, it.Value));
                    }
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_CITIZEN_RANK_ADDED, out string citizenRaksAdded))
            {
                Dictionary<string, List<string>> di = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(citizenRaksAdded);
                foreach (var it in di)
                {
                    var foundCell = CityInfo.CitizensRanks.FirstOrDefault(cell => cell.RankName == it.Key);
                    if (foundCell != null)
                    {
                        foundCell.CitizensRanks = it.Value;
                    }
                    else
                    {
                        CityInfo.CitizensRanks.Add(new RankCellElement(it.Key, it.Value));
                    }
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_CITIZEN_RANK_REMOVED, out string citizenRaksRemoved))
            {
                Dictionary<string, List<string>> di = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(citizenRaksRemoved);
                foreach (var it in di)
                {
                    var foundCell = CityInfo.CitizensRanks.FirstOrDefault(cell => cell.RankName == it.Key);
                    if (foundCell != null)
                    {
                        foundCell.CitizensRanks = it.Value;
                    }
                    else
                    {
                        CityInfo.CitizensRanks.Add(new RankCellElement(it.Key, it.Value));
                    }
                }
            }
            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_PLOTS_COLOR, out string cityPlotsColor))
            {
                CityInfo.PlotsColor = int.Parse(cityPlotsColor);
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_BALANCE, out string cityBalance))
            {
                CityInfo.CityBalance = (double)decimal.Parse(cityBalance);
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_CRIMINAL_ADDED, out string criminalAdded))
            {
                HashSet<string> hs = JsonConvert.DeserializeObject<HashSet<string>>(criminalAdded);
                foreach (var it in hs)
                {
                    CityInfo.Criminals.Add(it);
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_CRIMINAL_REMOVED, out string criminalRemoved))
            {
                HashSet<string> hs = JsonConvert.DeserializeObject<HashSet<string>>(criminalRemoved);
                foreach (var it in hs)
                {
                    CityInfo.Criminals.Remove(it);
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_CRIMINALS_LIST, out string criminalsList))
            {
                
                HashSet<string> hs = JsonConvert.DeserializeObject<HashSet<string>>(criminalsList);
                CityInfo.Criminals = hs;              
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_PRISON_CELL_ALL, out string prisonCellList))
            {
                HashSet<PrisonCellElement> pc = JsonConvert.DeserializeObject<HashSet<PrisonCellElement>>(prisonCellList);
                CityInfo.PrisonCells = pc.ToList();
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_ADD_PRISON_CELL, out string prisonCellAdd))
            {
                HashSet<PrisonCellElement> pc = JsonConvert.DeserializeObject<HashSet<PrisonCellElement>>(prisonCellAdd);
                foreach(var it in pc)
                {
                    CityInfo.PrisonCells.Add(it);
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_REMOVE_PRISON_CELL, out string prisonCellRemove))
            {
                HashSet<PrisonCellElement> pc = JsonConvert.DeserializeObject<HashSet<PrisonCellElement>>(prisonCellRemove);
                foreach(var it in pc)
                {
                    CityInfo.PrisonCells.Remove(it);
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_CELL_PRISON_UPDATE, out string prisonCelUpdate))
            {
                HashSet<PrisonCellElement> pc = JsonConvert.DeserializeObject<HashSet<PrisonCellElement>>(prisonCelUpdate);
                foreach (var it in pc)
                {
                    foreach(var it_current in CityInfo.PrisonCells.ToArray())
                    {
                        if(it_current.SpawnPosition.Equals(it.SpawnPosition))
                        {
                            CityInfo.PrisonCells.Remove(it_current);
                            CityInfo.PrisonCells.Add(it);
                            break;
                        }
                    }
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_SUMMON_POINT_ALL, out string summonCellsAll))
            {
                HashSet<SummonCellElement> pc = JsonConvert.DeserializeObject<HashSet<SummonCellElement>>(summonCellsAll);
                CityInfo.SummonCells = pc.ToList();
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_SUMMON_POINT_ADD, out string summonPlotAdd))
            {
                HashSet<SummonCellElement> pc = JsonConvert.DeserializeObject<HashSet<SummonCellElement>>(summonPlotAdd);
                foreach (var it in pc)
                {
                    CityInfo.SummonCells.Add(it);
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_SUMMON_POINT_REMOVE, out string summonPlotRemove))
            {
                HashSet<SummonCellElement> pc = JsonConvert.DeserializeObject<HashSet<SummonCellElement>>(summonPlotRemove);
                foreach (var it in pc)
                {
                    foreach (var it_current in CityInfo.SummonCells.ToArray())
                    {
                        if (it_current.SpawnPosition.Equals(it.SpawnPosition))
                        {
                            CityInfo.SummonCells.Remove(it_current);
                            break;
                        }
                    }
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_SUMMON_POINT_UPDATE, out string summonPlotUpdate))
            {
                HashSet<SummonCellElement> pc = JsonConvert.DeserializeObject<HashSet<SummonCellElement>>(summonPlotUpdate);
                foreach (var it in pc)
                {
                    foreach (var it_current in CityInfo.SummonCells.ToArray())
                    {
                        if (it_current.SpawnPosition.Equals(it.SpawnPosition))
                        {
                            CityInfo.SummonCells.Remove(it_current);
                            CityInfo.SummonCells.Add(it);
                            break;
                        }
                    }
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_PLOTS_GROUPS_ALL, out string plotsgroupCellsAll))
            {
                HashSet<PlotsGroupCellElement> pc = JsonConvert.DeserializeObject<HashSet<PlotsGroupCellElement>>(plotsgroupCellsAll);
                CityInfo.PlotsGroupCells = pc.ToList();
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_PLOTS_GROUPS_REMOVE, out string cityplotsgroupRemove))
            {
                HashSet<PlotsGroupCellElement> pc = JsonConvert.DeserializeObject<HashSet<PlotsGroupCellElement>>(cityplotsgroupRemove);
                foreach (var it in pc)
                {
                    foreach (var it_current in CityInfo.PlotsGroupCells.ToArray())
                    {
                        if (it_current.Guid.Equals(it.Guid))
                        {
                            CityInfo.PlotsGroupCells.Remove(it_current);
                            break;
                        }
                    }
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_PLOTS_GROUPS_ADD, out string cityplotsgroupAdd))
            {
                HashSet<PlotsGroupCellElement> pc = JsonConvert.DeserializeObject<HashSet<PlotsGroupCellElement>>(cityplotsgroupAdd);
                foreach (var it in pc)
                {
                    CityInfo.PlotsGroupCells.Add(it);
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_PLOTS_GROUPS_UPDATE, out string cityplotsgroupUpdate))
            {
                HashSet<PlotsGroupCellElement> pc = JsonConvert.DeserializeObject<HashSet<PlotsGroupCellElement>>(cityplotsgroupUpdate);
                foreach (var it in pc)
                {
                    foreach (var it_current in CityInfo.PlotsGroupCells.ToArray())
                    {
                        if (it_current.Guid.Equals(it.Guid))
                        {
                            CityInfo.PlotsGroupCells.Remove(it_current);
                            CityInfo.PlotsGroupCells.Add(it);
                        }
                    }
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.TO_PLOTS_GROUP_INVITE_ADD, out string toPlotsGroupInviteAdd))
            {
                HashSet<ClientToPlotsGroupInvitation> pc = JsonConvert.DeserializeObject<HashSet<ClientToPlotsGroupInvitation>>(toPlotsGroupInviteAdd);
                foreach (var it in pc)
                {
                    this.ReceivedPlotsGroupInvitations.Add(it);
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.TO_PLOTS_GROUP_INVITE_REMOVE, out string toPlotsGroupInviteRemove))
            {
                HashSet<ClientToPlotsGroupInvitation> pc = JsonConvert.DeserializeObject<HashSet<ClientToPlotsGroupInvitation>>(toPlotsGroupInviteRemove);
                foreach (var it in pc)
                {
                    foreach (var it_current in this.ReceivedPlotsGroupInvitations.ToArray())
                    {
                        if (it_current.CityName.Equals(it.CityName))
                        {
                            this.ReceivedPlotsGroupInvitations.Remove(it_current);
                            this.ReceivedPlotsGroupInvitations.Add(it);
                        }
                    }
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.NEW_ALLIANCE_ALL, out string newAllianceInfo))
            {
                AllianceInfo ai = JsonConvert.DeserializeObject<AllianceInfo>(newAllianceInfo);
                claims.clientDataStorage.clientPlayerInfo.AllianceInfo = ai;
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.OWN_ALLIANCE_REMOVE, out string _))
            {
                claims.clientDataStorage.clientPlayerInfo.AllianceInfo = null;
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_LIST_ALL, out string allCitiesList))
            {
                List<ClientCityInfoCellElement> ai = JsonConvert.DeserializeObject<List<ClientCityInfoCellElement>>(allCitiesList);
                claims.clientDataStorage.clientPlayerInfo.AllCitiesList = ai;
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.CITY_LIST_UPDATE, out string cityListUpdate))
            {
                List<ClientCityInfoCellElement> ai = JsonConvert.DeserializeObject<List<ClientCityInfoCellElement>>(allCitiesList);
                foreach (var it in ai)
                {
                    var existing = AllCitiesList.FirstOrDefault(c => c.Guid == it.Guid);
                    if (existing != null)
                    {
                        existing.CitizensAmount = it.CitizensAmount;
                        existing.MayorName = it.MayorName;
                        existing.ClaimedPlotsAmount = it.ClaimedPlotsAmount;
                        existing.AllianceName = it.AllianceName;
                        existing.TimeStampCreated = it.TimeStampCreated;
                        existing.Name = it.Name;
                        existing.Open = it.Open;
                        existing.InvMsg = it.InvMsg;
                    }
                    else
                    {
                        AllCitiesList.Add(it);
                    }
                }
            }

            if (valueDict.TryGetValue(EnumPlayerRelatedInfo.ALLIANCE_NAME, out string allianceName))
            {
                Tuple<string, string> tup = JsonConvert.DeserializeObject<Tuple<string, string>>(allianceName);
                claims.clientDataStorage.clientPlayerInfo.AllianceInfo.Name = tup.Item2;
            }
        }
    }
}
