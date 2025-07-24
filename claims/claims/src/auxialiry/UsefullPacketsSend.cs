using claims.src.part;
using claims.src.gui.playerGui.structures;
using Newtonsoft.Json;
using System.Collections.Generic;
using Vintagestory.API.Server;
using System.Linq;
using claims.src.network.packets;
using claims.src.delayed.invitations;
using System.Collections.Concurrent;
using Vintagestory.API.Util;
using Vintagestory.API.Client;
using claims.src.part.structure;
using ProtoBuf;
using claims.src.part.structure.plots;
using Vintagestory.API.Datastructures;
using caneconomy.src.interfaces;
using claims.src.gui.playerGui.structures.cellElements;
using System.Globalization;

namespace claims.src.auxialiry
{
    public static class UsefullPacketsSend
    {
        public static ConcurrentDictionary<string, Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>>> cityDelayedInfoCollector =
            new ConcurrentDictionary<string, Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>>>();

        public static ConcurrentDictionary<string, Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>>> playerDelayedInfoCollector =
            new ConcurrentDictionary<string, Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>>>();

        public static void sendAllCitiesColorsToPlayer(IServerPlayer player)
        {
            Dictionary<string, int> cityColors = new Dictionary<string, int>();
            foreach (City cityItem in claims.dataStorage.getCitiesList())
            {
                cityColors.Add(cityItem.GetPartName(), cityItem.cityColor);
            }
            string serializedPlots = JsonConvert.SerializeObject(cityColors);

            claims.serverChannel.SendPacket(new SavedPlotsPacket()
            {
                type = PacketsContentEnum.ALL_CITY_COLORS,
                data = serializedPlots
            }, player);
        }       
        public static void SendPlayerCityRelatedInfo(IServerPlayer player)
        {
            Dictionary<EnumPlayerRelatedInfo, string> collector = new Dictionary<EnumPlayerRelatedInfo, string>();

            if(!claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo))
            {
                return;
            }
            if (playerInfo.hasCity())
            {
                City city = playerInfo.City;
                collector.Add(EnumPlayerRelatedInfo.CITY_NAME, city.GetPartName());
                if (city.getMayor() != null)
                {
                    collector.Add(EnumPlayerRelatedInfo.MAYOR_NAME, city.getMayor().GetPartName());
                }
                collector.Add(EnumPlayerRelatedInfo.CITY_CREATED_TIMESTAMP, city.TimeStampCreated.ToString());
                collector.Add(EnumPlayerRelatedInfo.CITY_MEMBERS, JsonConvert.SerializeObject(StringFunctions.getNamesOfCitizens(city)));
                collector.Add(EnumPlayerRelatedInfo.MAX_COUNT_PLOTS, Settings.getMaxNumberOfPlotForCity(city).ToString());
                collector.Add(EnumPlayerRelatedInfo.CLAIMED_PLOTS, city.getCityPlots().Count.ToString());
                collector.Add(EnumPlayerRelatedInfo.PLAYER_PREFIX, playerInfo.Prefix);
                collector.Add(EnumPlayerRelatedInfo.PLAYER_AFTER_NAME, playerInfo.AfterName);
                collector.Add(EnumPlayerRelatedInfo.PLAYER_CITY_TITLES, JsonConvert.SerializeObject(playerInfo.getCityTitles()));
                collector.Add(EnumPlayerRelatedInfo.CITY_POSSIBLE_RANKS, JsonConvert.SerializeObject(RightsHandler.GetCityRanks()));
                collector.Add(EnumPlayerRelatedInfo.CITY_PLOTS_COLOR, city.cityColor.ToString());
                collector.Add(EnumPlayerRelatedInfo.CITY_DEBT, city.DebtBalance.ToString(CultureInfo.InvariantCulture));
                if (city.isMayor(playerInfo)) { 
                    Dictionary<string, List<string>> rankToCitizens = new Dictionary<string, List<string>>();
                    foreach(var citizen in city.getCityCitizens())
                    {
                        foreach(var title in citizen.getCityTitles())
                        {
                            if(rankToCitizens.TryGetValue(title, out var li))
                            {
                                li.Add(citizen.GetPartName());
                            }
                            else
                            {
                                rankToCitizens.Add(title, new List<string> { citizen.GetPartName() });
                            }
                        }
                    
                    }

                
                
                    collector.Add(EnumPlayerRelatedInfo.CITY_CITIZENS_RANKS, JsonConvert.SerializeObject(rankToCitizens));
                }
                if(playerInfo.PlayerPermissionsHandler.HasPermission(rights.EnumPlayerPermissions.CITY_SEE_BALANCE))
                {
                    collector.Add(EnumPlayerRelatedInfo.CITY_BALANCE, claims.economyHandler.getBalance(city.MoneyAccountName).ToString(CultureInfo.InvariantCulture));
                }
                if(city.criminals.Count > 0)
                {
                    collector.Add(EnumPlayerRelatedInfo.CITY_CRIMINALS_LIST, JsonConvert.SerializeObject(StringFunctions.getNamesOfCriminals(city)));
                }
                if(city.hasPrison())
                {
                    HashSet<PrisonCellElement> prisonCellElements = new HashSet<PrisonCellElement>();
                    foreach (var it in city.getPrisons())
                    {
                        foreach(PrisonCellInfo cell in it.getPrisonCells())
                        {
                            prisonCellElements.Add(new PrisonCellElement(cell.getSpawnPosition(), cell.GetPlayersNames()));
                        }
                    }
                    collector.Add(EnumPlayerRelatedInfo.CITY_PRISON_CELL_ALL, JsonConvert.SerializeObject(prisonCellElements));
                }

                if(city.summonPlots.Count > 0)
                {
                    HashSet<SummonCellElement> summonCellElements = new HashSet<SummonCellElement>();
                    foreach (var it in city.summonPlots)
                    {
                        summonCellElements.Add(new SummonCellElement((it.PlotDesc as PlotDescSummon).SummonPoint.AsVec3i.Clone(), (it.PlotDesc as PlotDescSummon).Name));
                    }
                    collector.Add(EnumPlayerRelatedInfo.CITY_SUMMON_POINT_ALL, JsonConvert.SerializeObject(summonCellElements));
                }

                if (city.getCityPlotsGroups().Count > 0)
                {
                    HashSet<PlotsGroupCellElement> plotsgroupCellElements = new HashSet<PlotsGroupCellElement>();
                    foreach (var it in city.getCityPlotsGroups())
                    {
                        plotsgroupCellElements.Add(
                            new PlotsGroupCellElement(it.Guid, it.GetPartName(), it.City.GetPartName(),
                                                      it.PlayersList.Select(pl => pl.GetPartName()).ToList(),
                                                      it.PermsHandler, it.PlotsGroupFee));
                    }
                    collector.Add(EnumPlayerRelatedInfo.CITY_PLOTS_GROUPS_ALL, JsonConvert.SerializeObject(plotsgroupCellElements));
                }
            }

            collector.Add(EnumPlayerRelatedInfo.SHOW_PLOT_MOVEMENT, ((int)playerInfo.showPlotMovement).ToString());
            collector.Add(EnumPlayerRelatedInfo.FRIENDS, JsonConvert.SerializeObject(StringFunctions.getNamesOfFriends(playerInfo)));
            collector.Add(EnumPlayerRelatedInfo.TO_CITY_INVITES, JsonConvert.SerializeObject(InvitationHandler.getInvitesForReceiver(playerInfo)));
            Dictionary<string, ClientCityInfoCellElement> CityStatsCashe =
                ObjectCacheUtil.GetOrCreate<Dictionary<string, ClientCityInfoCellElement>>(claims.sapi,
                "claims:cityinfocache", () => new Dictionary<string, ClientCityInfoCellElement>());
            if (CityStatsCashe.Count > 0)
            {
                collector.Add(EnumPlayerRelatedInfo.CITY_LIST_ALL, JsonConvert.SerializeObject(CityStatsCashe.Values.ToList()));
            }
            claims.serverChannel.SendPacket(
                new SavedPlotsPacket()
                {
                    data = JsonConvert.SerializeObject(collector),
                    type = PacketsContentEnum.OWN_CITY_INFO_ON_JOIN
                }
                , player);

        }
        public static void SendCityRelatedInfoToAllOnlineCitizensOnPlayerJoinCity(City city, List<string> exceptListPlayersUIDs)
        {
            foreach(var onlinePlayer in city.getOnlineCitizens())
            {
                if(exceptListPlayersUIDs.Contains(onlinePlayer.PlayerUID))
                {
                    continue;
                }
                Dictionary<EnumPlayerRelatedInfo, string> collector = new Dictionary<EnumPlayerRelatedInfo, string>
                {
                    { EnumPlayerRelatedInfo.CITY_MEMBERS, JsonConvert.SerializeObject(StringFunctions.getNamesOfCitizens(city)) },
                    { EnumPlayerRelatedInfo.MAX_COUNT_PLOTS, Settings.getMaxNumberOfPlotForCity(city).ToString() }
                };
                claims.serverChannel.SendPacket(
                    new SavedPlotsPacket()
                    {
                        data = JsonConvert.SerializeObject(collector),
                        type = PacketsContentEnum.ON_SOME_CITY_PARAMS_UPDATED
                    }
                    , onlinePlayer as IServerPlayer);
            }
        }
        public static void SendPlayerRelatedInfoOnCityJoined(PlayerInfo playerInfo)
        {
            Dictionary<EnumPlayerRelatedInfo, string> collector = new Dictionary<EnumPlayerRelatedInfo, string>();
            var player = claims.sapi.World.PlayerByUid(playerInfo.Guid);
            if (player != null)
            {
                if (playerInfo.hasCity())
                {
                    City city = playerInfo.City;
                    collector.Add(EnumPlayerRelatedInfo.CITY_NAME, city.GetPartName());
                    if (city.getMayor() != null)
                    {
                        collector.Add(EnumPlayerRelatedInfo.MAYOR_NAME, city.getMayor().GetPartName());
                    }
                    collector.Add(EnumPlayerRelatedInfo.CITY_CREATED_TIMESTAMP, city.TimeStampCreated.ToString());
                    collector.Add(EnumPlayerRelatedInfo.CITY_MEMBERS, JsonConvert.SerializeObject(StringFunctions.getNamesOfCitizens(city)));
                    collector.Add(EnumPlayerRelatedInfo.MAX_COUNT_PLOTS, Settings.getMaxNumberOfPlotForCity(city).ToString());
                    collector.Add(EnumPlayerRelatedInfo.CLAIMED_PLOTS, city.getCityPlots().Count.ToString());
                    collector.Add(EnumPlayerRelatedInfo.PLAYER_PREFIX, playerInfo.Prefix);
                    collector.Add(EnumPlayerRelatedInfo.PLAYER_AFTER_NAME, playerInfo.AfterName);
                    collector.Add(EnumPlayerRelatedInfo.PLAYER_CITY_TITLES, JsonConvert.SerializeObject(playerInfo.getCityTitles()));
                }
                claims.serverChannel.SendPacket(
                    new SavedPlotsPacket()
                    {
                        data = JsonConvert.SerializeObject(collector),
                        type = PacketsContentEnum.ON_CITY_JOINED
                    }
                    , player as IServerPlayer);
            }
        }
        public static void SendPlayerRelatedInfoOnKickFromCity(PlayerInfo playerInfo)
        {
            var player = claims.sapi.World.PlayerByUid(playerInfo.Guid);
            if (player != null)
            {
                claims.serverChannel.SendPacket(
                    new SavedPlotsPacket()
                    {
                        data = "",
                        type = PacketsContentEnum.ON_KICKED_FROM_CITY
                    }
                    , player as IServerPlayer);
            }
        }
        public static void SendUpdatedConfigValues(IServerPlayer player)
        {
            claims.serverChannel.SendPacket(
                   new ConfigUpdateValuesPacket()
                   {
                       NewCityCost = claims.config.NEW_CITY_COST,
                       NewPlotClaimCost = claims.config.PLOT_CLAIM_PRICE,
                       COINS_VALUES_TO_CODE = claims.config.COINS_VALUES_TO_CODE,
                       ID_TO_COINS_VALUES = claims.config.ID_TO_COINS_VALUES,
                       CITY_NAME_CHANGE_COST = claims.config.CITY_NAME_CHANGE_COST,
                       CITY_BASE_CARE = claims.config.CITY_BASE_CARE,
                       PLOTS_COLORS = Settings.colors,
                       NO_ACCESS_WITH_FOR_NOT_CLAIMED_AREA = claims.config.NO_ACCESS_WITH_FOR_NOT_CLAIMED_AREA,
                       POSSIBLE_BROKEN_BLOCKS_IN_WILDERNESS = claims.config.POSSIBLE_BROKEN_BLOCKS_IN_WILDERNESS,
                       POSSIBLE_BUILD_BLOCKS_IN_WILDERNESS = claims.config.POSSIBLE_BUILD_BLOCKS_IN_WILDERNESS,
                       POSSIBLE_USED_BLOCKS_IN_WILDERNESS = claims.config.POSSIBLE_USED_BLOCKS_IN_WILDERNESS,
                       POSSIBLE_BUILD_ITEMS_IN_WILDERNESS = claims.config.POSSIBLE_BUILD_ITEMS_IN_WILDERNESS
                   }
                   , player);
        }
        public static void AddToQueueCityInfoUpdate(string cityName, Dictionary<string, object> additionalInfo, EnumPlayerRelatedInfo toUpdate)
        {
            if (cityDelayedInfoCollector.TryGetValue(cityName, out Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> cityHashSet))
            {
                //such enum was added before, just add new additional info to it
                if (cityHashSet.TryGetValue(toUpdate, out var already_stored_dict))
                {
                    foreach (var value_pair in additionalInfo)
                    {
                        if (already_stored_dict.TryGetValue(value_pair.Key, out var inner_value))
                        {
                            inner_value.Add(value_pair.Value);
                        }
                    }
                }
                else
                {
                    cityHashSet.Add(toUpdate, additionalInfo.ToDictionary(k => k.Key, k => new List<object> { k.Value }));
                }                
            }
            else
            {               
                    cityDelayedInfoCollector.TryAdd(cityName,
                        new Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> { { toUpdate, additionalInfo.ToDictionary(k => k.Key, k => new List<object> { k.Value }) } });
            }
        }
        public static void AddToQueueCityInfoUpdate(string cityName, params EnumPlayerRelatedInfo[] toUpdate)
        {
            if (cityDelayedInfoCollector.TryGetValue(cityName, out Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> cityHashSet))
            {               
                foreach (var it in toUpdate)
                {
                    cityHashSet.TryAdd(it, null);
                }
            }
            else
            {
                cityDelayedInfoCollector.TryAdd(cityName, toUpdate.ToDictionary(k => k, k => (Dictionary<string, List<object>>)null));
            }
        }
        public static void AddToQueuePlayerInfoUpdate(string playerName, Dictionary<string, object> additionalInfo, EnumPlayerRelatedInfo toUpdate)
        {
            if (playerDelayedInfoCollector.TryGetValue(playerName, out Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> playerHashSet))
            {
                //such enum was added before, just add new additional info to it
                if (playerHashSet.TryGetValue(toUpdate, out var already_stored_dict))
                {
                    foreach (var value_pair in additionalInfo)
                    {
                        if (already_stored_dict.TryGetValue(value_pair.Key, out var inner_value))
                        {
                            if(value_pair.Value is List<object> listValue)
                            {
                                inner_value.AddRange(listValue);
                            }
                            else
                            {
                                inner_value.Add(value_pair.Value);
                            }
                        }
                    }
                }
                else
                {
                    foreach(var it in additionalInfo)
                    {
                        if(it.Value is System.Collections.IList list)
                        {
                            var k = list as IEnumerable<object>;
                        }
                    }
                    playerHashSet.Add(toUpdate, additionalInfo.ToDictionary(k => k.Key,
                                                                            k => k.Value is System.Collections.IList list
                                                                                    ? list.Cast<object>().ToList()
                                                                                    : new List<object> { k.Value }));
                }
            }
            else
            {
                playerDelayedInfoCollector.TryAdd(playerName,
                    new Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> { { toUpdate, additionalInfo.ToDictionary(k => k.Key, k => new List<object> { k.Value }) } });
            }
        }
        public static void AddToQueuePlayerInfoUpdate(string playerName, params EnumPlayerRelatedInfo[] toUpdate)
        {
            if (playerDelayedInfoCollector.TryGetValue(playerName, out Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> playerHashSet))
            {
                foreach (var it in toUpdate)
                {
                    if (!playerHashSet.ContainsKey(it))
                    {
                        playerHashSet.Add(it, null);
                    }
                }
            }
            else
            {
                playerDelayedInfoCollector.TryAdd(playerName, toUpdate.ToDictionary(k => k, k => (Dictionary<string, List<object>>)null));
            }
        }
        public static void AddToQueueAllPlayersInfoUpdate(Dictionary<string, object> additionalInfo, EnumPlayerRelatedInfo toUpdate)
        {
            foreach(var pl in claims.sapi.World.AllOnlinePlayers)
            {
                AddToQueuePlayerInfoUpdate(pl.PlayerName, additionalInfo, toUpdate);
            }
        }
        public static void SendAllCollectedCityUpdatesToCitizens()
        {
            while(!cityDelayedInfoCollector.IsEmpty)
            {
                string currentCityGuid = cityDelayedInfoCollector.ElementAt(0).Key;

                if(!cityDelayedInfoCollector.Remove(currentCityGuid, out Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> listToUpdate)) continue;
                if (!claims.dataStorage.getCityByGUID(currentCityGuid, out City city)) continue;

                var onlinePlayersFromCity = city.getOnlineCitizens();
                //nobody need this info since nobody from the city is online
                if(onlinePlayersFromCity.Count == 0) continue;

                Dictionary<EnumPlayerRelatedInfo, string> collector = CollectCityInfo(city, listToUpdate);
                
                //collector now contains only general info for all citizens

                foreach (var citizen in onlinePlayersFromCity)
                {
                    var playerCollector = new Dictionary<EnumPlayerRelatedInfo, string>();
                    if (playerDelayedInfoCollector.Remove(citizen.PlayerName, out Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> dictInfo))
                    {
                        if (claims.dataStorage.getPlayerByUid(citizen.PlayerUID, out PlayerInfo playerInfo))
                        {
                            playerCollector = CollectPlayerInfo(playerInfo, dictInfo);
                        }
                    }

                    var mergedCollector = playerCollector.Count > 0 
                                                                ? collector.Union(playerCollector).ToDictionary(kv => kv.Key, kv => kv.Value) 
                                                                : collector;
                    claims.serverChannel.SendPacket(
                        new SavedPlotsPacket
                        {
                            data = JsonConvert.SerializeObject(mergedCollector),
                            type = PacketsContentEnum.ON_SOME_CITY_PARAMS_UPDATED
                        },
                        citizen as IServerPlayer
                    );                    
                }
            }

            

            while(!playerDelayedInfoCollector.IsEmpty)
            {
                string currentPlayerUid = playerDelayedInfoCollector.ElementAt(0).Key;
                if (!playerDelayedInfoCollector.Remove(currentPlayerUid,
                                                                    out Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> listToUpdate)) continue;

                if (!claims.dataStorage.getPlayerByUid(currentPlayerUid, out PlayerInfo playerInfo)) continue;

                Dictionary<EnumPlayerRelatedInfo, string> collector = CollectPlayerInfo(playerInfo, listToUpdate);
            
                if (collector.Count > 0)
                {
                    var player = claims.sapi.World.PlayerByUid(currentPlayerUid);
                    if (player != null)
                    {
                        claims.serverChannel.SendPacket(
                            new SavedPlotsPacket()
                            {
                                data = JsonConvert.SerializeObject(collector),
                                type = PacketsContentEnum.ON_SOME_CITY_PARAMS_UPDATED
                            }
                            , player as IServerPlayer);
                    }
                }                                  
            }
        }
        public static void SendCurrentPlotUpdate(IServerPlayer player, Plot plot)
        {
            CurrentPlotInfo cpi = new CurrentPlotInfo(plot.GetPartName(), plot.getPlotOwner()?.GetPartName() ?? "",
                plot.Type, plot.getCustomTax(), plot.Price, plot.getPermsHandler(), plot.extraBought, plot.getPos());
            string serializedZones = JsonConvert.SerializeObject(cpi);

            claims.serverChannel.SendPacket(new SavedPlotsPacket()
            {
                type = PacketsContentEnum.CURRENT_PLOT_INFO,
                data = serializedZones

            }, player);            
        }
        public static void AddToQueueAllianceInfoUpdate(string allianceGuid, Dictionary<string, object> additionalInfo, EnumPlayerRelatedInfo toUpdate)
        {
            if (claims.dataStorage.GetAllianceByGUID(allianceGuid, out var alliance))
            {
                foreach (var city in alliance.Cities)
                {
                    if (cityDelayedInfoCollector.TryGetValue(city.Guid, out Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> cityHashSet))
                    {
                        //such enum was added before, just add new additional info to it
                        if (cityHashSet.TryGetValue(toUpdate, out var already_stored_dict))
                        {
                            foreach (var value_pair in additionalInfo)
                            {
                                if (already_stored_dict.TryGetValue(value_pair.Key, out var inner_value))
                                {
                                    inner_value.Add(value_pair.Value);
                                }
                            }
                        }
                        else
                        {
                            cityHashSet.Add(toUpdate, additionalInfo.ToDictionary(k => k.Key, k => new List<object> { k.Value }));
                        }
                    }
                    else
                    {
                        cityDelayedInfoCollector.TryAdd(city.Guid,
                            new Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> { { toUpdate, additionalInfo.ToDictionary(k => k.Key, k => new List<object> { k.Value }) } });
                    }
                }
            }
        }
        public static void AddToQueueAllianceInfoUpdate(string allianceGuid, params EnumPlayerRelatedInfo[] toUpdate)
        {
            if (claims.dataStorage.GetAllianceByGUID(allianceGuid, out var alliance))
            {
                foreach (var city in alliance.Cities)
                {
                    if (cityDelayedInfoCollector.TryGetValue(city.Guid, out Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> cityHashSet))
                    {
                        foreach (var it in toUpdate)
                        {
                            cityHashSet.Add(it, null);
                        }
                    }
                    else
                    {
                        cityDelayedInfoCollector.TryAdd(city.Guid, toUpdate.ToDictionary(k => k, k => (Dictionary<string, List<object>>)null));
                    }
                }
            }
        }
        public static void CheckCitisUpdatedAndSend()
        {
            List<City> updatedCities = new List<City>();
            foreach (var it in claims.dataStorage.getCitiesList())
            {
                if (it.Dirty)
                {
                    updatedCities.Add(it);
                    it.Dirty = false;
                }
            }
            if (updatedCities.Count == 0)
            {
                return;
            }
            Dictionary<string, ClientCityInfoCellElement> CityStatsCashe =
                ObjectCacheUtil.GetOrCreate<Dictionary<string, ClientCityInfoCellElement>>(claims.sapi,
                "claims:cityinfocache", () => new Dictionary<string, ClientCityInfoCellElement>());
            foreach (var it in updatedCities)
            {
                if (CityStatsCashe.TryGetValue(it.Guid, out var stat))
                {
                    stat.AllianceName = it?.Alliance.GetPartName() ?? "";
                    stat.MayorName = it.getMayor()?.GetPartName() ?? "";
                    stat.Name = it.GetPartName();
                    stat.InvMsg = it.invMsg;
                    stat.TimeStampCreated = it.TimeStampCreated;
                    stat.CitizensAmount = it.getCityCitizens().Count;
                    stat.Open = it.openCity;
                    stat.ClaimedPlotsAmount = it.getCityPlots().Count;
                }
                else
                {
                    CityStatsCashe.Add(it.Guid, new ClientCityInfoCellElement(it.getCityCitizens().Count, it.getMayor()?.GetPartName() ?? "",
                        it.getCityPlots().Count, it?.Alliance.GetPartName() ?? "", it.TimeStampCreated, it.GetPartName(), it.openCity,
                        it.invMsg, it.Guid));
                }
            }
            List<ClientCityInfoCellElement> elToSend = new List<ClientCityInfoCellElement>();
            foreach (var it in updatedCities)
            {
                if (CityStatsCashe.TryGetValue(it.Guid, out var stat))
                {
                    elToSend.Add(stat);
                }
            }
            foreach (var player in claims.sapi.World.AllOnlinePlayers)
            {
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    continue;
                }
                UsefullPacketsSend.AddToQueuePlayerInfoUpdate(playerInfo.Guid, new Dictionary<string, object> { { "value", elToSend } }, EnumPlayerRelatedInfo.CITY_LIST_UPDATE);
            }
        }
        private static Dictionary<EnumPlayerRelatedInfo, string> CollectCityInfo(City city, Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> info)
        {
            var result = new Dictionary<EnumPlayerRelatedInfo, string>();

            foreach (var pair in info)
            {
                switch (pair.Key)
                {
                    case EnumPlayerRelatedInfo.CITY_CREATED_TIMESTAMP:
                        result[pair.Key] = city.TimeStampCreated.ToString();
                        break;
                    case EnumPlayerRelatedInfo.CITY_MEMBERS:
                        result[pair.Key] = JsonConvert.SerializeObject(StringFunctions.getNamesOfCitizens(city));
                        break;
                    case EnumPlayerRelatedInfo.MAYOR_NAME:
                        result[pair.Key] = city.getMayor()?.GetPartName() ?? "";
                        break;
                    case EnumPlayerRelatedInfo.CITY_NAME:
                        result[pair.Key] = city.GetPartName();
                        break;
                    case EnumPlayerRelatedInfo.MAX_COUNT_PLOTS:
                        result[pair.Key] = Settings.getMaxNumberOfPlotForCity(city).ToString();
                        break;
                    case EnumPlayerRelatedInfo.CLAIMED_PLOTS:
                        result[pair.Key] = city.getCityPlots().Count.ToString();
                        break;
                    case EnumPlayerRelatedInfo.CITY_CITIZENS_RANKS:
                        var rankMap = city.getCityCitizens()
                            .SelectMany(c => c.getCityTitles().Select(t => (title: t, name: c.GetPartName())))
                            .GroupBy(t => t.title)
                            .ToDictionary(g => g.Key, g => g.Select(x => x.name).ToList());

                        result[pair.Key] = JsonConvert.SerializeObject(rankMap);
                        break;
                    case EnumPlayerRelatedInfo.CITY_BALANCE:
                        result[pair.Key] = claims.economyHandler.getBalance(city.MoneyAccountName).ToString(CultureInfo.InvariantCulture);
                        break;
                    case EnumPlayerRelatedInfo.CITY_DEBT:
                        result[pair.Key] = city.DebtBalance.ToString(CultureInfo.InvariantCulture);
                        break;
                    case EnumPlayerRelatedInfo.CITY_PLOTS_COLOR:
                        result[pair.Key] = city.cityColor.ToString();
                        break;
                    case EnumPlayerRelatedInfo.CITY_CRIMINALS_LIST:
                        result[pair.Key] = JsonConvert.SerializeObject(StringFunctions.getNamesOfCriminals(city));
                        break;
                    case EnumPlayerRelatedInfo.OWN_ALLIANCE_REMOVE:
                        result[pair.Key] = null;
                        break;
                    case EnumPlayerRelatedInfo.NEW_ALLIANCE_ALL:
                    case EnumPlayerRelatedInfo.ALLIANCE_NAME:
                        if (pair.Value.TryGetValue("value", out var allianceList) && allianceList.Count > 0)
                        {
                            var allianceJson = SerializeAllianceInfo((string)allianceList[0]);
                            if (allianceJson != null)
                                result[pair.Key] = allianceJson;
                        }
                        break;
                    case EnumPlayerRelatedInfo.ALLIANCE_BALANCE:
                        result[pair.Key] = claims.economyHandler.getBalance(city.Alliance.MoneyAccountName).ToString();
                        break;
                    default:
                        if (pair.Value.TryGetValue("value", out var list))
                            result[pair.Key] = JsonConvert.SerializeObject(list);
                        break;
                }
            }

            return result;
        }
        private static string SerializeAllianceInfo(string guid)
        {
            if (claims.dataStorage.GetAllianceByGUID(guid, out var alliance))
            {
                return JsonConvert.SerializeObject(new AllianceInfo(
                    alliance.GetPartName(),
                    alliance.Leader?.GetPartName() ?? "",
                    alliance.TimeStampCreated,
                    alliance.Prefix,
                    StringFunctions.GetPartsNames(alliance.Cities),
                    (double)claims.economyHandler.getBalance(alliance.MoneyAccountName),
                    alliance.Guid
                ));
            }
            return null;
        }
        private static Dictionary<EnumPlayerRelatedInfo, string> CollectPlayerInfo(PlayerInfo playerInfo, Dictionary<EnumPlayerRelatedInfo, Dictionary<string, List<object>>> info)
        {
            var result = new Dictionary<EnumPlayerRelatedInfo, string>();
            if (!playerInfo.PlayerPermissionsHandler.HasPermission(rights.EnumPlayerPermissions.CITY_SEE_BALANCE))
            {
                info.Remove(EnumPlayerRelatedInfo.CITY_BALANCE);
            }
            foreach (var pair in info)
            {
                switch (pair.Key)
                {
                    case EnumPlayerRelatedInfo.PLAYER_PERMISSIONS:
                        result[pair.Key] = JsonConvert.SerializeObject(playerInfo.PlayerPermissionsHandler.GetPermissions());
                        break;
                    case EnumPlayerRelatedInfo.PLAYER_PREFIX:
                        result[pair.Key] = playerInfo.Prefix;
                        break;
                    case EnumPlayerRelatedInfo.PLAYER_AFTER_NAME:
                        result[pair.Key] = playerInfo.AfterName;
                        break;
                    case EnumPlayerRelatedInfo.PLAYER_CITY_TITLES:
                        result[pair.Key] = JsonConvert.SerializeObject(playerInfo.getCityTitles());
                        break;
                    case EnumPlayerRelatedInfo.FRIENDS:
                        result[pair.Key] = JsonConvert.SerializeObject(StringFunctions.getNamesOfFriends(playerInfo));
                        break;
                    case EnumPlayerRelatedInfo.CITY_CITIZENS_RANKS:
                        if (!playerInfo.hasCity()) break;
                        var rankMap = playerInfo.City.getCityCitizens()
                            .SelectMany(c => c.getCityTitles().Select(t => (title: t, name: c.GetPartName())))
                            .GroupBy(t => t.title)
                            .ToDictionary(g => g.Key, g => g.Select(x => x.name).ToList());

                        result[pair.Key] = JsonConvert.SerializeObject(rankMap);
                        break;
                    case EnumPlayerRelatedInfo.OWN_ALLIANCE_REMOVE:
                        result[pair.Key] = null;
                        break;
                    case EnumPlayerRelatedInfo.NEW_ALLIANCE_ALL:
                    case EnumPlayerRelatedInfo.ALLIANCE_NAME:
                        if (pair.Value.TryGetValue("value", out var allianceList) && allianceList.Count > 0)
                        {
                            var allianceJson = SerializeAllianceInfo((string)allianceList[0]);
                            if (allianceJson != null)
                                result[pair.Key] = allianceJson;
                        }
                        break;
                    case EnumPlayerRelatedInfo.CITY_PLOT_RECOLOR:
                        if (pair.Value.TryGetValue("value", out var plotToRecolor) && plotToRecolor.Count > 0)
                        {
                            var allianceJson = JsonConvert.SerializeObject(plotToRecolor);
                            if (allianceJson != null)
                                result[pair.Key] = allianceJson;
                        }
                        break;
                    default:
                        if (pair.Value.TryGetValue("value", out var list))
                            result[pair.Key] = JsonConvert.SerializeObject(list);
                        break;
                }
            }

            return result;
        }
    }
}
