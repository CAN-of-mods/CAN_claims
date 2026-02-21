using System.Numerics;
using ImGuiNET;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace claims.src.gui.prettyGui.GuiTabs
{
    public class CANRanksTab : CANGuiTab
    {
        public CANRanksTab(ICoreClientAPI capi, IconHandler iconHandler)
        {
            this.capi = capi;
            this.iconHandler = iconHandler;
        }
        public override void DrawTab()
        {
            string text = Lang.Get("claims:gui-ranks-title");
            float windowWidth = ImGui.GetWindowSize().X;
            float textWidth = ImGui.CalcTextSize(text).X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.Text(text);
            ImGui.SameLine();
            if (ImGui.ImageButton("createrank", this.iconHandler.GetOrLoadIcon("circle"), new Vector2(16)))
            {
                capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.CITY_RANK_CREATION_NEED_NAME;
            }

            int i = 100;

            ImGui.BeginChild("Ranksscroll", new Vector2(0, 300), true);
            foreach (var rankCell in claims.clientDataStorage.clientPlayerInfo.CityInfo.CityRanks)
            {
                ImGui.PushID(i);

                Vector2 start = ImGui.GetCursorScreenPos();
                float width = ImGui.GetContentRegionAvail().X;

                ImGui.BeginGroup();

                ImGui.Text(Lang.Get("claims:gui-rank-name", rankCell.Name));

                if (ImGui.ImageButton("promotewithrank" + i.ToString(), this.iconHandler.GetOrLoadIcon("private"), new Vector2(16)))
                {
                    capi.ModLoader.GetModSystem<claimsGui>().textInput = rankCell.Name;
                    capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.CITY_RANK_ADD;
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Lang.Get("claims:gui-add-rank-tooltip"));
                }
                ImGui.SameLine();
                if (ImGui.ImageButton("openrankinfo" + i.ToString(), this.iconHandler.GetOrLoadIcon("info"), new Vector2(16)))
                {
                    capi.ModLoader.GetModSystem<claimsGui>().selectedTab = EnumSelectedTab.RANKINFOPAGE;
                    capi.ModLoader.GetModSystem<claimsGui>().textInput = rankCell.Name;
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Lang.Get("claims:gui-info-rank-tooltip"));
                }
                foreach (var plName in rankCell.Citizens)
                {
                    if(ImGui.Button(plName))
                    {
                        capi.ModLoader.GetModSystem<claimsGui>().textInput = rankCell.Name;
                        capi.ModLoader.GetModSystem<claimsGui>().textInput2 = plName;
                        capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.CITY_RANK_REMOVE_CONFIRM;
                    }
                }

                ImGui.EndGroup();

                Vector2 end = ImGui.GetItemRectMax();
                var draw = ImGui.GetWindowDrawList();

                ImGui.PopID();

                ImGui.Dummy(new Vector2(0, 8));
                ImGui.Separator();
                i++;
            }
            ImGui.EndChild();
        }
    }
}
