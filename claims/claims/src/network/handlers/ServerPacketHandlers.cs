using claims.src.auxialiry;
using claims.src.clientMapHandling;
using claims.src.events;
using claims.src.gui.playerGui.structures;
using claims.src.network.packets;
using claims.src.part;
using claims.src.part.structure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace claims.src.network.handlers
{
    public static class ServerPacketHandlers
    {
        public static void RegisterHandlers()
        {
            claims.serverChannel.SetMessageHandler<SavedPlotsPacket>((player, packet) =>
            {
                if (packet.type == PacketsContentEnum.CLIENT_INFORM_ZONES_TIMESTAMPS)
                {
                    claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                    if (playerInfo == null)
                    {
                        return;
                    }
                    List<Tuple<Vec2i, long>> zonesTimestamps = JsonConvert.DeserializeObject<List<Tuple<Vec2i, long>>>(packet.data);
                    List<Vec2i> needUpdateZones = new List<Vec2i>();

                    Vec2i playerServerPos = new Vec2i((int)player.Entity.ServerPos.X / 512, (int)player.Entity.ServerPos.Z / 512);

                    //iterate through all pairs
                    //add only which need update - have 0 timestamp
                    //or server's timestamp is newer than client's
                    foreach (var zoneItem in zonesTimestamps)
                    {
                        //player can spoof zones coords and get data about far land from him
                        if (zoneItem.Item1.X > playerServerPos.X + 3 || zoneItem.Item1.X < playerServerPos.X - 3 || zoneItem.Item1.Y > playerServerPos.Y + 3 || zoneItem.Item1.Y < playerServerPos.Y - 3)
                        {
                            continue;
                        }
                        if (zoneItem.Item2 == 0)
                        {
                            needUpdateZones.Add(zoneItem.Item1);
                            continue;
                        }
                        else
                        {
                            if (claims.dataStorage.serverZonesTimestamps.TryGetValue(zoneItem.Item1, out long timestamp))
                            {
                                if (timestamp > zoneItem.Item2)
                                {
                                    continue;
                                }
                                else
                                {
                                    needUpdateZones.Add(zoneItem.Item1);
                                }
                            }
                        }
                    }
                    HashSet<Tuple<Vec2i, long, List<KeyValuePair<Vec2i, SavedPlotInfo>>>> preparedData = new HashSet<Tuple<Vec2i, long, List<KeyValuePair<Vec2i, SavedPlotInfo>>>>();
                    long savedTimestamp = TimeFunctions.getEpochSeconds();
                    foreach (Vec2i zoneVec in needUpdateZones)
                    {
                        if (claims.dataStorage.getZone(zoneVec, out ServerZoneInfo serverZoneInfo))
                        {
                            List<KeyValuePair<Vec2i, SavedPlotInfo>> preparedSavedPlots = new List<KeyValuePair<Vec2i, SavedPlotInfo>>();
                            foreach (Plot plot in serverZoneInfo.zonePlots)
                            {
                                preparedSavedPlots.Add(new KeyValuePair<Vec2i, SavedPlotInfo>(plot.getPos(),
                                    new SavedPlotInfo((int)plot.getPrice(), plot.getPermsHandler().pvpFlag,
                                        player.WorldData.CurrentGameMode == EnumGameMode.Creative || OnBlockAction.canBlockDestroyWithOutCacheUpdate(playerInfo, plot),
                                        player.WorldData.CurrentGameMode == EnumGameMode.Creative || OnBlockAction.canBlockUseWithOutCacheUpdate(playerInfo, plot),
                                        player.WorldData.CurrentGameMode == EnumGameMode.Creative || OnBlockAction.canAttackAnimalsWithOutCacheUpdate(playerInfo, plot),
                                        plot.getCity().GetPartName(), plot.GetPartName(),
                                        plot.hasCityPlotsGroup()
                                            ? plot.getPlotGroup().GetPartName()
                                            : "",
                                        plot.getType() == PlotType.TAVERN
                                            ? plot.GetClientInnerClaimFromDefault(playerInfo)
                                            : null)));
                            }
                            preparedData.Add(new Tuple<Vec2i, long, List<KeyValuePair<Vec2i, SavedPlotInfo>>>
                            (zoneVec, savedTimestamp, preparedSavedPlots));
                        }
                    }
                    if (preparedData.Count > 0)
                    {
                        string serializedZones = JsonConvert.SerializeObject(preparedData);

                        claims.serverChannel.SendPacket(new SavedPlotsPacket()
                        {
                            type = PacketsContentEnum.SERVER_UPDATED_ZONES_ANSWER,
                            data = serializedZones

                        }, player);
                    }
                }
                else if(packet.type == PacketsContentEnum.CURRENT_PLOT_CLIENT_REQUEST)
                {
                    var currentPos = player.Entity.ServerPos;
                    if(claims.dataStorage.getPlot(PlotPosition.fromEntityyPos(currentPos), out Plot plot))
                    {
                        CurrentPlotInfo cpi = new CurrentPlotInfo(plot.GetPartName(), plot.getPlotOwner()?.GetPartName() ?? "",
                            plot.getType(), plot.getCustomTax(), plot.getPrice(), plot.getPermsHandler(), plot.extraBought, plot.getPos());
                        string serializedZones = JsonConvert.SerializeObject(cpi);

                        claims.serverChannel.SendPacket(new SavedPlotsPacket()
                        {
                            type = PacketsContentEnum.CURRENT_PLOT_INFO,
                            data = serializedZones

                        }, player);
                    }                                      
                }
                else if (packet.type == PacketsContentEnum.CITY_CITIZENS_RANKS_REQUEST)
                {

                    //get player
                    //city
                    //skip if not mayor
                    //send dict with ranks
                    //add handler on client
                    var currentPos = player.Entity.ServerPos;
                    if (claims.dataStorage.getPlot(PlotPosition.fromEntityyPos(currentPos), out Plot plot))
                    {
                        CurrentPlotInfo cpi = new CurrentPlotInfo(plot.GetPartName(), plot.getPlotOwner()?.GetPartName() ?? "",
                            plot.getType(), plot.getCustomTax(), plot.getPrice(), plot.getPermsHandler(), plot.extraBought, plot.getPos());
                        string serializedZones = JsonConvert.SerializeObject(cpi);

                        claims.serverChannel.SendPacket(new SavedPlotsPacket()
                        {
                            type = PacketsContentEnum.CURRENT_PLOT_INFO,
                            data = serializedZones

                        }, player);
                    }
                }
            });

        }
    }
}
