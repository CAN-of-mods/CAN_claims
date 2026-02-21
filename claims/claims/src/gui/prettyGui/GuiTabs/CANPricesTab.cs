using ImGuiNET;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace claims.src.gui.prettyGui.GuiTabs
{
    public class CANPricesTab: CANGuiTab
    {
        public CANPricesTab(ICoreClientAPI capi, IconHandler iconHandler)
        {
            this.capi = capi;
            this.iconHandler = iconHandler;
        }
        public override void DrawTab()
        {
            ImGui.Text(Lang.Get("claims:gui-new-city-cost", claims.config.NEW_CITY_COST.ToString()));

            ImGui.Text(Lang.Get("claims:gui-city-plot-cost", claims.config.PLOT_CLAIM_PRICE.ToString()));

            ImGui.Text(Lang.Get("claims:gui-city-name-change-cost", claims.config.CITY_NAME_CHANGE_COST.ToString()));

            ImGui.Text(Lang.Get("claims:gui-city-base-cost", claims.config.CITY_BASE_CARE.ToString()));

            ImGui.Text(Lang.Get("claims:gui-teleportation-cost", claims.config.SUMMON_PAYMENT.ToString()));

            ImGui.Text(Lang.Get("claims:gui-new-alliance-cost", claims.config.NEW_ALLIANCE_COST.ToString()));
        }
    }
}
