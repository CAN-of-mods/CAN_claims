using claims.src.auxialiry;
using claims.src.auxialiry.claimAreas;
using claims.src.network.packets;
using claims.src.part;
using claims.src.part.structure;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using static claims.src.auxialiry.claimAreas.ClaimAreaHandler;

namespace claims.src.commands
{
    public class ClaimAreaCommands : BaseCommand
    {
        public static TextCommandResult NewArea(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (claims.dataStorage.serverClaimAreaHandler.TryGetCurrentArea(player.PlayerUID, out var area))
            {
                return TextCommandResult.Error("claims:area_already_prepared");
            }
            claims.dataStorage.serverClaimAreaHandler.CreateNewItemCurrentArea(player.PlayerUID);
            return TextCommandResult.Success("claims:new_area_started");
        }
        private static void HighlightArea(TemporaryArea area, IServerPlayer player)
        {
            List<BlockPos> bList = new List<BlockPos>
            {
                area.start.ToBlockPos().Copy(),
                area.end.ToBlockPos().Copy()
            };
            List<int> colors = new List<int>
            {
                ColorUtil.ToRgba(claims.config.PLOT_BORDERS_COLOR_OTHER_PLOT[0],
                                        claims.config.PLOT_BORDERS_COLOR_OTHER_PLOT[1],
                                        claims.config.PLOT_BORDERS_COLOR_OTHER_PLOT[2],
                                        claims.config.PLOT_BORDERS_COLOR_OTHER_PLOT[3])
            };
            claims.sapi.World.HighlightBlocks(player, 59, bList, colors, shape: EnumHighlightShape.Cubes);
            return;
        }
        private static void HighlightArea(ClaimArea area, IServerPlayer player)
        {
            List<BlockPos> bList = new List<BlockPos>
            {
                area.pos1.ToBlockPos().Copy(),
                area.pos2.ToBlockPos().Copy()
            };
            List<int> colors = new List<int>
            {
                ColorUtil.ToRgba(claims.config.PLOT_BORDERS_COLOR_OTHER_PLOT[0],
                                        claims.config.PLOT_BORDERS_COLOR_OTHER_PLOT[1],
                                        claims.config.PLOT_BORDERS_COLOR_OTHER_PLOT[2],
                                        claims.config.PLOT_BORDERS_COLOR_OTHER_PLOT[3])
            };
            claims.sapi.World.HighlightBlocks(player, 59, bList, colors, shape: EnumHighlightShape.Cubes);
            return;
        }
        public static TextCommandResult SaveArea(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            string newName = args.Parsers[0].GetValue().ToString();
            if (claims.dataStorage.serverClaimAreaHandler.GetAreaByName(newName, out _))
            {
                return TextCommandResult.Error("claims:name_is_used");
            }
            if (!claims.dataStorage.serverClaimAreaHandler.TryGetCurrentArea(player.PlayerUID, out var temporaryArea))
            {
                return TextCommandResult.Error("claims:no_claim_area_created");
            }
            var tmpArea = new ClaimArea();
            tmpArea.pos1 = temporaryArea.start.Clone();
            tmpArea.pos2 = temporaryArea.end.Clone();
            tmpArea.areaName = newName;
            if (claims.dataStorage.serverClaimAreaHandler.AddAreaClaim(tmpArea))
            {
                return TextCommandResult.Success("claims:new_area_saved");
            }
            return TextCommandResult.Error("claims:failed_to_create");
        }
        public static TextCommandResult StartPoint(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.serverClaimAreaHandler.TryGetCurrentArea(player.PlayerUID, out var temporaryArea))
            {
                return TextCommandResult.Error("claims:no_claim_area_created");
            }
            if (player.Entity.BlockSelection == null)
            {
                return TextCommandResult.Error("claims:no_block_selected");
            }
            temporaryArea.start = player.Entity.BlockSelection.Position.AsVec3i;
            if (temporaryArea.start != null && temporaryArea.end != null)
            {
                HighlightArea(temporaryArea, player);
            }
            return TextCommandResult.Success("claims:start_point_set");
        }
        public static TextCommandResult EndPoint(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.serverClaimAreaHandler.TryGetCurrentArea(player.PlayerUID, out var temporaryArea))
            {
                return TextCommandResult.Error("claims:no_claim_area_created");
            }
            if (player.Entity.BlockSelection == null)
            {
                return TextCommandResult.Error("claims:no_block_selected");
            }
            temporaryArea.end = player.Entity.BlockSelection.Position.AsVec3i;
            if (temporaryArea.start != null && temporaryArea.end != null)
            {
                HighlightArea(temporaryArea, player);
            }
            return TextCommandResult.Success("claims:end_point_set");
        }
        public static TextCommandResult ListAreas(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            int page = int.Parse(args.Parsers[0].GetValue().ToString()) - 1;
            if (page < 0)
            {
                return TextCommandResult.Success("claims:need_bigger_number");
            }
            int perPage = 10;
            var pageContent = claims.dataStorage.serverClaimAreaHandler.GetPageAreas(page * perPage, (page + 1) * perPage);

            StringBuilder sb = new StringBuilder();

            int counter = page * perPage;
            foreach (var it in pageContent)
            {
                sb.Append(counter).Append(".").Append(it.areaName).Append(" ").Append(it.pos1.ToString()).Append("/").Append(it.pos2.ToString()).AppendLine();
                counter++;
            }
            player.SendMessage(0, sb.ToString(), EnumChatType.Notification);
            return TextCommandResult.Success();
        }
        public static TextCommandResult ShowAreaHere(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.serverClaimAreaHandler.TryGetArea(player.Entity.ServerPos.AsBlockPos.ToVec3d(), out var area))
            {
                return TextCommandResult.Error("claims:no_area_here");
            }
            HighlightArea(area, player);
            return TextCommandResult.Success();
        }
        public static TextCommandResult ShowAreaInfoHere(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.serverClaimAreaHandler.TryGetArea(player.Entity.ServerPos.AsBlockPos.ToVec3d(), out var area))
            {
                return TextCommandResult.Error("claims:no_area_here");
            }
            player.SendMessage(0, area.GetAreaInfo(), EnumChatType.Notification);
            return TextCommandResult.Success();
        }
        public static TextCommandResult ShowAreaById(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            return TextCommandResult.Success();
        }
        public static TextCommandResult DeleteAreaNumber(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            int number = (int)args.LastArg;
            if (claims.dataStorage.serverClaimAreaHandler.GetAreaByNumber(number, out var area))
            {
                claims.dataStorage.serverClaimAreaHandler.RemoveAreaClaim(area);
                return TextCommandResult.Success();
            }
            return TextCommandResult.Error("claims:no_area_found");
        }
        public static TextCommandResult DeleteAreaName(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            string name = (string)args.LastArg;
            if (claims.dataStorage.serverClaimAreaHandler.GetAreaByName(name, out var area))
            {
                claims.dataStorage.serverClaimAreaHandler.RemoveAreaClaim(area);
                return TextCommandResult.Success();
            }
            return TextCommandResult.Error("claims:no_area_found");
        }
        static Dictionary<string, EnumCANPlotAccessFlags> blockAccessMapping = new Dictionary<string, EnumCANPlotAccessFlags> {
            { "build", EnumCANPlotAccessFlags.Build },
            { "use", EnumCANPlotAccessFlags.Use },
            { "attack", EnumCANPlotAccessFlags.Attack },
            { "break", EnumCANPlotAccessFlags.Break }};

        static Dictionary<string, EnumCANClaimSettingFlag> claimFlagsMapping = new Dictionary<string, EnumCANClaimSettingFlag> {
            { "pvp", EnumCANClaimSettingFlag.PVP },
            { "fire", EnumCANClaimSettingFlag.FIRE },
            { "blast", EnumCANClaimSettingFlag.BLAST }};
        public static TextCommandResult SetAreaBlockAccess(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!blockAccessMapping.TryGetValue((string)args.Parsers[0].GetValue(), out EnumCANPlotAccessFlags flag))
            {
                return TextCommandResult.Error("claims:wrong_flag");
            }
            if (!claims.dataStorage.serverClaimAreaHandler.TryGetArea(player.Entity.ServerPos.AsBlockPos.ToVec3d(), out var claim))
            {
                return TextCommandResult.Error("claims:no_area_claim");
            }

            claim.permissionsFlags[(int)flag] = args.Parsers[1].GetValue().Equals("on");
          
            foreach (var pl in claims.sapi.World.AllOnlinePlayers)
            {
                claims.serverChannel.SendPacket(new ClaimAreasPacket()
                {
                    type = ClaimAreasPacketEnum.Update,
                    claims = new List<ClaimArea> { claim }

                }, (IServerPlayer)pl);
            }
            claims.dataStorage.serverClaimAreaHandler.SaveIntoMap();
            return TextCommandResult.Success();
        }
        public static TextCommandResult SetAreaClaimFlag(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claimFlagsMapping.TryGetValue((string)args.Parsers[0].GetValue(), out EnumCANClaimSettingFlag flag))
            {
                return TextCommandResult.Error("claims:wrong_flag");
            }
            if (!claims.dataStorage.serverClaimAreaHandler.TryGetArea(player.Entity.ServerPos.AsBlockPos.ToVec3d(), out var claim))
            {
                return TextCommandResult.Error("claims:no_area_claim");
            }

            claim.settingFlags[(int)flag] = args.Parsers[1].GetValue().Equals("on");
            foreach (var pl in claims.sapi.World.AllOnlinePlayers)
            {
                claims.serverChannel.SendPacket(new ClaimAreasPacket()
                {
                    type = ClaimAreasPacketEnum.Update,
                    claims = new List<ClaimArea> { claim }

                }, (IServerPlayer)pl);
            }
            claims.dataStorage.serverClaimAreaHandler.SaveIntoMap();
            return TextCommandResult.Success();
        }
    }
}
