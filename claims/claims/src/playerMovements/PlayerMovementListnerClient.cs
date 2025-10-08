using claims.src.auxialiry;
using claims.src.messages;
using claims.src.part.structure;
using claims.src.part;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using claims.src.clientMapHandling;
using Vintagestory.GameContent;
using System.IO;
using Newtonsoft.Json;
using claims.src.claimsext.map;
using claims.src.network.packets;
using claims.src.commands.register;

namespace claims.src.playerMovements
{
    public class PlayerMovementListnerClient
    {
        public Vec3i playerLastPos;
        IClientPlayer player;
        ClientMapDB mapdb;
        public PlayerMovementListnerClient()
        {
            playerLastPos = new Vec3i(-1, -1, -1);
            mapdb = new ClientMapDB(claims.capi.World.Logger);
            string errorMessage = null;
            string mapdbfilepath = this.getMapDbFilePath();
            this.mapdb.OpenOrCreate(mapdbfilepath, ref errorMessage, true, true, false);
            if (errorMessage != null)
            {
                throw new Exception(string.Format("Cannot open {0}, possibly corrupted. Please fix manually or delete this file to continue playing", mapdbfilepath));
            }
        }
        public string getMapDbFilePath()
        {
            string text = Path.Combine(GamePaths.DataPath, "clientSavedZones");
            GamePaths.EnsurePathExists(text);
            return Path.Combine(text, claims.capi.World.SavegameIdentifier + ".db");
        }
        public string getMsgForChunkChange(SavedPlotInfo fromPlot, SavedPlotInfo toPlot, int state)
        {
            StringBuilder stringBuilder = new StringBuilder();
            //state - 
            //0 both empty
            //1 from has smth
            //2 to has smth
            //3 both has smth

            //JUST FROM ONE EMPTY TO ANOTHER
            if (state == 0)
            {
                return "";
            }

            //FROM VILLAGE OR CITY TO EMPTY
            if (state == 1)
            {
                stringBuilder.Append("To wild lands.");
                return stringBuilder.ToString();
            }

            //FROM EMPTY TO VILLAGE OR CITY
            if (state == 2)
            {
                if (toPlot.cityName.Length > 0)
                {
                    stringBuilder.Append(StringFunctions.setStringColor(toPlot.cityName, ColorsClaims.DARK_GRAY));
                }


                if (toPlot.groupName.Length > 0)
                {
                    stringBuilder.Append(" ").Append(toPlot.groupName);
                }

                if (toPlot.plotName.Length > 0)
                {
                    stringBuilder.Append(" ").Append("~").Append(toPlot.plotName).Append("~");
                }
                if (toPlot.price != -1)
                {
                    stringBuilder.Append(" ").Append("To sell: " + toPlot.price.ToString());
                }
                if (toPlot.PvPIsOn)
                {
                    stringBuilder.Append(" ").Append(StringFunctions.setBold(StringFunctions.setStringColor("PVP", ColorsClaims.DARK_RED)));
                }
                else
                {
                    stringBuilder.Append(" ").Append(StringFunctions.setBold(StringFunctions.setStringColor("NO-PVP", ColorsClaims.WHITE)));
                }
            }

            //BOTH HAS VILLAGE OR CITY 
            if (state == 3)
            {
                if (toPlot.cityName.Equals(fromPlot.cityName))
                {
                    stringBuilder.Append(StringFunctions.setStringColor(toPlot.cityName, ColorsClaims.DARK_GRAY) + " ");
                }

                if (toPlot.groupName.Length > 0)
                {
                    stringBuilder.Append(" ").Append(toPlot.groupName);
                }

                if (toPlot.plotName.Length > 0)
                {
                    stringBuilder.Append(" ").Append("~").Append(toPlot.plotName).Append("~");
                }
                if (toPlot.price != -1)
                {
                    stringBuilder.Append(" ").Append("To sell: " + toPlot.price.ToString());
                }
                if (toPlot.PvPIsOn)
                {
                    stringBuilder.Append(" ").Append(StringFunctions.setBold(StringFunctions.setStringColor("PVP", ColorsClaims.DARK_RED)));
                }
                else
                {
                    stringBuilder.Append(" ").Append(StringFunctions.setBold(StringFunctions.setStringColor("NO-PVP", ColorsClaims.WHITE)));
                }
            }
            return stringBuilder.ToString();
        }
        public void onPlayerChangePlotEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {
            TreeAttribute tree = data as TreeAttribute;
            Vec2i from = new Vec2i(tree.GetInt("xChO"), tree.GetInt("zChO"));
            Vec2i to = new Vec2i(tree.GetInt("xCh"), tree.GetInt("zCh"));

            IPlayer pl = claims.capi.World.Player;
            if (pl == null)
            {
                return;
            }

            if ((int)(from.X / claims.config.ZONE_PLOTS_LENGTH) != (int)(to.X / claims.config.ZONE_PLOTS_LENGTH) ||
                (int)(from.Y / claims.config.ZONE_PLOTS_LENGTH) != (int)(to.Y / claims.config.ZONE_PLOTS_LENGTH))
            {
                handleZoneChange(to, pl);
            }

            claims.clientDataStorage.getSavedPlot(to, out SavedPlotInfo toPlot);
            claims.clientDataStorage.getSavedPlot(from, out SavedPlotInfo fromPlot);

            //TODO
            /*if (playerInfo.showBorders)
            {
                plotPosition.makeChunkHighlight(claims.sapi.World, claims.sapi.World.PlayerByUid(playerInfo.getGuid()), toPlot);
            }*/
            //STILL IN THE WILD LANDS

            string st;
            if (toPlot == null && fromPlot == null)
            {
                /*st = getMsgForChunkChange(fromPlot, toPlot, 0, playerInfo);
                if (st.Length != 0)
                {
                    if (playerInfo.showBorderMsgs)
                        MessageHandler.sendMsgToPlayerInfo(playerInfo, st);
                }*/
                return;
            }

            if (claims.clientDataStorage.clientPlayerInfo != null)
            { 
                if (toPlot != null && fromPlot != null)
                {
                    st = getMsgForChunkChange(fromPlot, toPlot, 3);
                    if (st.Length != 0)
                    {
                        if (claims.clientDataStorage.clientPlayerInfo.ShowPlotMovement == EnumShowPlotMovement.SHOW_MESSAGE 
                            || claims.clientDataStorage.clientPlayerInfo.ShowPlotMovement == EnumShowPlotMovement.SHOW_MESSAGE_HUD)
                        {
                            claims.capi.ShowChatMessage(st);
                        }

                    }
                    //SAME CITY
                    if (toPlot.cityName.Equals(fromPlot.cityName))
                    {
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                if (toPlot != null && fromPlot == null)
                {
                    st = getMsgForChunkChange(fromPlot, toPlot, 2);
                    if (st.Length != 0)
                    {
                        if (claims.clientDataStorage.clientPlayerInfo.ShowPlotMovement == EnumShowPlotMovement.SHOW_MESSAGE
                            || claims.clientDataStorage.clientPlayerInfo.ShowPlotMovement == EnumShowPlotMovement.SHOW_MESSAGE_HUD)
                        {
                            claims.capi.ShowChatMessage(st);
                        }
                    }
                    //claims.sapi.World.Api.Event.PushEvent("claimsPlayerEnterCity", tree);
                    return;
                }

                st = getMsgForChunkChange(fromPlot, toPlot, 1);
                if (st.Length != 0)
                {
                    if (claims.clientDataStorage.clientPlayerInfo.ShowPlotMovement == EnumShowPlotMovement.SHOW_MESSAGE 
                        || claims.clientDataStorage.clientPlayerInfo.ShowPlotMovement == EnumShowPlotMovement.SHOW_MESSAGE_HUD)
                    {
                        claims.capi.ShowChatMessage(st);
                    }
                }
             }
        }
        public void checkPlayerMove(float dt)
        {
            player = claims.capi.World.Player;
            if ((playerLastPos.X / PlotPosition.plotSize != (int)(player.Entity.Pos.X / PlotPosition.plotSize)) || playerLastPos.Z / PlotPosition.plotSize != (int)(player.Entity.Pos.Z / PlotPosition.plotSize))
            {
                TreeAttribute tree = new TreeAttribute();
                tree.SetString("playerUID", player.PlayerUID);
                //new plot
                tree.SetInt("xCh", (int)player.Entity.Pos.X / PlotPosition.plotSize);
                tree.SetInt("zCh", (int)player.Entity.Pos.Z / PlotPosition.plotSize);
                //old plot
                tree.SetInt("xChO", (int)playerLastPos.X / PlotPosition.plotSize);
                tree.SetInt("zChO", (int)playerLastPos.Z / PlotPosition.plotSize);
                claims.capi.World.Api.Event.PushEvent("claimsPlayerChangePlot", tree);
                playerLastPos.X = (int)player.Entity.Pos.X;
                playerLastPos.Z = (int)player.Entity.Pos.Z;
            }
        }
        //check if current zone and zones around are already loaded
        //if not we prepare its' vectors and send them to server
        //if we have them, we send it anyway but also we add timestamp of this zone
        //so if zone's data is old - server will resend more relevant data to client
        public void handleZoneChange(Vec2i toPlot, IPlayer player)
        {
            //mapdb
            Vec2i tmpZoneCoords = new Vec2i();
            Vec2i centerZoneCoords = new Vec2i(toPlot.X / claims.config.ZONE_PLOTS_LENGTH,
                                               toPlot.Y / claims.config.ZONE_PLOTS_LENGTH);
            List<Tuple<Vec2i, long>> zonesTimestamps = new List<Tuple<Vec2i, long>>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    tmpZoneCoords.X = centerZoneCoords.X + i;
                    tmpZoneCoords.Y = centerZoneCoords.Y + j;

                    zonesTimestamps.Add(new Tuple<Vec2i, long>(tmpZoneCoords.Copy(), 0));
                    continue;
                    //ignore db for now


                    if (claims.clientDataStorage.getClientSavedZone(tmpZoneCoords, out ClientSavedZone oldClientSavedZone))
                    {
                        zonesTimestamps.Add(new Tuple<Vec2i, long>(tmpZoneCoords.Copy(), oldClientSavedZone.timestamp));
                        continue;
                    }
                    //get saved zone data if saved in DB
                    //and add to data storage
                    ClientSavedZone clientSavedZone = mapdb.GetMapPiece(tmpZoneCoords);
                    if (clientSavedZone != null)
                    {
                        claims.clientDataStorage.addClientSavedZone(tmpZoneCoords.Copy(), clientSavedZone);
                        zonesTimestamps.Add(new Tuple<Vec2i, long>(tmpZoneCoords.Copy(), clientSavedZone.timestamp));
                        claims.clientModInstance.plotsMapLayer.generateFromZoneSavedPlotsOnMap(tmpZoneCoords.Copy());
                    }
                    else
                    {
                        zonesTimestamps.Add(new Tuple<Vec2i, long>(tmpZoneCoords.Copy(), 0));
                    }
                }
            }
            string serializedZones = JsonConvert.SerializeObject(zonesTimestamps);

            claims.clientChannel.SendPacket(new SavedPlotsPacket()
            {
                type = PacketsContentEnum.CLIENT_INFORM_ZONES_TIMESTAMPS,
                data = serializedZones
            });

        }
        public void saveActiveZonesToDb()
        {
            mapdb.SetMapPieces(claims.clientDataStorage.getClientSavedPlots());         
        }

        //on join event player should check for already saved zones and load them from db
        //and send zones timestamps to server
        public void onPlayerJoin(/*IClientPlayer byPlayer*/)
        {
            IClientPlayer byPlayer = claims.capi.World.Player;
            if (byPlayer != null && claims.capi.World.Player == byPlayer)
            {
                claims.clientModInstance.plotsMapLayer = claims.capi.ModLoader.GetModSystem<WorldMapManager>().MapLayers.OfType<PlotsMapLayer>().FirstOrDefault();
                handleZoneChange(new Vec2i((int)byPlayer.Entity.Pos.X / claims.config.PLOT_SIZE,
                    (int)byPlayer.Entity.Pos.Z / claims.config.PLOT_SIZE), byPlayer);
                ClientCommands.RegisterCommands(claims.capi);
            }
        }
        public void OnShutDown()
        {
            if (mapdb == null)
			{
				return;
			}
            mapdb.Dispose();
        }

    }
}
