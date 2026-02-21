using claims.src.auxialiry;
using ImGuiNET;
using System;
using System.Numerics;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;
using static System.Net.Mime.MediaTypeNames;

namespace claims.src.gui.prettyGui.GuiTabs
{
    public class CANCityListTab : CANGuiTab
    {
        public CANCityListTab(ICoreClientAPI capi, IconHandler iconHandler)
        {
            this.capi = capi;
            this.iconHandler = iconHandler;
        }
        public override void DrawTab()
        {
            string text = Lang.Get("claims:gui_city_list_title");
            float windowWidth = ImGui.GetWindowSize().X;
            float textWidth = ImGui.CalcTextSize(text).X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.Text(text);

            ImGui.BeginChild("InvitesScroll", new Vector2(0, 300), true);
            int i = 0;
            foreach (var cityStatCell in claims.clientDataStorage.clientPlayerInfo.AllCitiesList)
            {
                ImGui.PushID(i);

                Vector2 start = ImGui.GetCursorScreenPos();
                float width = ImGui.GetContentRegionAvail().X;

                ImGui.BeginGroup();

                ImGui.Text(Lang.Get("claims:gui_citylist_city_name", cityStatCell.Name));

                if(cityStatCell.AllianceName.Length > 0)
                {
                    ImGui.Text(Lang.Get("claims:gui_citylist_alliance_name", cityStatCell.AllianceName));
                }

                ImGui.Text(Lang.Get("claims:gui-mayor-name", cityStatCell.MayorName));

                ImGui.Text(Lang.Get("claims:gui-city-population", cityStatCell.CitizensAmount));

                ImGui.Text(Lang.Get("claims:gui-claimed-plots", cityStatCell.ClaimedPlotsAmount));

                if (cityStatCell.InvMsg.Length > 0)
                {
                    if (ImGui.ImageButton("cityinfo", this.iconHandler.GetOrLoadIcon("info"), new Vector2(16)))
                    {

                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(cityStatCell.InvMsg);
                        ImGui.EndTooltip();
                    }
                }
                ImGui.Text(Lang.Get("claims:gui-date-created", TimeFunctions.getDateFromEpochSeconds(cityStatCell.TimeStampCreated)));

                ImGui.SameLine();

                if (cityStatCell.Open)
                {
                    if (ImGui.ImageButton("joincity", this.iconHandler.GetOrLoadIcon("stairs-goal"), new Vector2(16)))
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/c join " + cityStatCell.Name, EnumChatType.Macro, "");
                    }
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(Lang.Get("claims:gui_citylist_join_city_hover"));
                    ImGui.EndTooltip();
                }

                //ImGui.Text(AllPlayers);

                ImGui.EndGroup();

                Vector2 end = ImGui.GetItemRectMax();
                var draw = ImGui.GetWindowDrawList();

                ImGui.PopID();

                ImGui.Dummy(new Vector2(0, 8));
                ImGui.Separator();
            }
            ImGui.EndChild();

        }
    }
}
