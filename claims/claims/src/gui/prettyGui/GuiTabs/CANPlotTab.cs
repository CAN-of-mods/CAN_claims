using System.Numerics;
using claims.src.network.packets;
using claims.src.part.structure;
using claims.src.part.structure.plots;
using ImGuiNET;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;

namespace claims.src.gui.prettyGui.GuiTabs
{
    public class CANPlotTab: CANGuiTab
    {
        public CANPlotTab(ICoreClientAPI capi, IconHandler iconHandler)
        {
            this.capi = capi;
            this.iconHandler = iconHandler;
        }
        public override void DrawTab()
        {
            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            ImGui.Text("[" + clientInfo.CurrentPlotInfo.PlotPosition.X + "/" + clientInfo.CurrentPlotInfo.PlotPosition.Y + "]");
            ImGui.SameLine();
            if (ImGui.ImageButton("plotinfoget", this.iconHandler.GetOrLoadIcon("info"), new Vector2(15)))
            {
                claims.clientChannel.SendPacket(new SavedPlotsPacket()
                {
                    type = PacketsContentEnum.CURRENT_PLOT_CLIENT_REQUEST,
                    data = ""
                });
            }

            /*==============================================================================================*/
            /*=====================================PLOT NAME================================================*/
            /*==============================================================================================*/

            ImGui.Text(Lang.Get("claims:gui-plot-name", clientInfo.CurrentPlotInfo.PlotName));
            ImGui.SameLine();
            if (ImGui.ImageButton("setplotname", this.iconHandler.GetOrLoadIcon("info"), new Vector2(15)))
            {
                capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.PLOT_SET_NAME;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Set plot name");
            }

            ImGui.Text(Lang.Get(Lang.Get("claims:gui-owner-name", clientInfo.CurrentPlotInfo.OwnerName)));

            

            if (clientInfo.CurrentPlotInfo.Price > -1)
            {
                ImGui.SameLine();
                if (ImGui.ImageButton("plotclaim", this.iconHandler.GetOrLoadIcon("id-card"), new Vector2(15)))
                {
                    capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.PLOT_CLAIM;
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Buy plot");
                }
            }
            
            if (clientInfo.CurrentPlotInfo.OwnerName?.Length > 0)
            {
                ImGui.SameLine();
                if (ImGui.ImageButton("plotunclaim", this.iconHandler.GetOrLoadIcon("id-card"), new Vector2(15)))
                {
                    capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.PLOT_UNCLAIM;
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Unclaim plot");
                }
            }

            /*==============================================================================================*/
            /*=====================================PLOT TYPE================================================*/
            /*==============================================================================================*/
            ImGui.Text(Lang.Get("claims:gui-plot-type",
                (PlotInfo.dictPlotTypes.TryGetValue(clientInfo.CurrentPlotInfo.PlotType, out PlotInfo plotInfo) ? plotInfo.getFullName() : "-")));

            ImGui.SameLine();
            if (ImGui.ImageButton("plotsettype", this.iconHandler.GetOrLoadIcon("files"), new Vector2(15)))
            {
                capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.PLOT_SET_TYPE;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Set plot type");
            }

            if (clientInfo.CurrentPlotInfo.PlotType == PlotType.PRISON)
            {
                ImGui.SameLine();
                if (ImGui.ImageButton("addprisoncell", this.iconHandler.GetOrLoadIcon("pencil"), new Vector2(15)))
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/c prison addcell", EnumChatType.Macro, "");
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Add prison cell");
                }
            }
            else if (clientInfo.CurrentPlotInfo.PlotType == PlotType.SUMMON)
            {
                ImGui.SameLine();
                if (ImGui.ImageButton("setsummonpoint", this.iconHandler.GetOrLoadIcon("pencil"), new Vector2(15)))
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/c summon set point", EnumChatType.Macro, "");
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Set summon point.");
                }
            }
            /*==============================================================================================*/
            /*=====================================PLOT TAX=================================================*/
            /*==============================================================================================*/

            ImGui.Text(Lang.Get("claims:gui-plot-custom-tax", clientInfo.CurrentPlotInfo.CustomTax));
            ImGui.SameLine();
            if (ImGui.ImageButton("settax", this.iconHandler.GetOrLoadIcon("medal"), new Vector2(15)))
            {
                capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.PLOT_SET_TAX;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Set custom tax for plot");
            }
            /*==============================================================================================*/
            /*=====================================PLOT PRICES==============================================*/
            /*==============================================================================================*/
            ImGui.Text(Lang.Get("claims:gui-plot-price", clientInfo.CurrentPlotInfo.Price > -1
                                                 ? clientInfo.CurrentPlotInfo.Price
                                                 : Lang.Get("claims:gui-not-for-sale")));
            ImGui.SameLine();
            if (ImGui.ImageButton("setplotprice", this.iconHandler.GetOrLoadIcon("medal"), new Vector2(15)))
            {
                capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.PLOT_SET_PRICE_NEED_NUMBER;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Set plot for sale");
            }
            ImGui.SameLine();
            if (ImGui.ImageButton("plotnfs", this.iconHandler.GetOrLoadIcon("contract"), new Vector2(15)))
            {
                ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot nfs", EnumChatType.Macro, "");
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Set plot NOT for sale");
            }

            /*==============================================================================================*/
            /*=====================================PLOT PERMISSIONS=========================================*/
            /*==============================================================================================*/

            ImGui.Text(Lang.Get("claims:gui-plot-permissions"));
            ImGui.SameLine();
            if (ImGui.ImageButton("plotpermissions", this.iconHandler.GetOrLoadIcon("medal"), new Vector2(15)))
            {
                capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.PLOT_PERMISSIONS;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Set plot permissions");
            }
            /*==============================================================================================*/
            /*=====================================PLOT SHOW BORDERS========================================*/
            /*==============================================================================================*/
            ImGui.Text(Lang.Get("claims:gui-plot-borders"));
            ImGui.SameLine();
            if (ImGui.ImageButton("showplotborders", this.iconHandler.GetOrLoadIcon("medal"), new Vector2(15)))
            {
                ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot borders " + "on", EnumChatType.Macro, "");
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Show plot borders");
            }
            ImGui.SameLine();
            if (ImGui.ImageButton("hideplotborders", this.iconHandler.GetOrLoadIcon("medal"), new Vector2(15)))
            {
                ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot borders " + "off", EnumChatType.Macro, "");
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Hide plot borders");
            }
        }
    }
}
