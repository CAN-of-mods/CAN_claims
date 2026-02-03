using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using claims.src.auxialiry;
using claims.src.gui.playerGui.structures.cellElements;
using claims.src.network.handlers;
using claims.src.rights;
using ImGuiNET;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;
using static claims.src.gui.playerGui.CANClaimsGui;

namespace claims.src.gui.prettyGui.GuiTabs
{
    public class CANRanksInfoTab : CANGuiTab
    {
        EnumPlayerPermissions[] availableToAdd;
        bool[] availableToAddSelected;
        public CANRanksInfoTab(ICoreClientAPI capi, IconHandler iconHandler)
        {
            this.capi = capi;
            this.iconHandler = iconHandler;
        }
        public override void DrawTab()
        {
            if (claims.clientDataStorage.clientPlayerInfo.CityInfo == null)
            {
                return;
            }
            CityRankCellElement cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.CityRanks.FirstOrDefault(rc => rc.Name.Equals(capi.ModLoader.GetModSystem<claimsGui>().textInput), null);
            if (cell == null)
            {
                return;
            }

            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            string text = string.Format("{0}", cell.Name);
            float windowWidth = ImGui.GetWindowSize().X;
            float textWidth = ImGui.CalcTextSize(text).X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.Text(text);

            /*if (ImGui.ImageButton("promotewithrank", this.iconHandler.GetOrLoadIcon("dodging"), new Vector2(16)))
            {
                capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.CITY_RANK_DELETE_CONFIRM;
            }*/

            ImGui.Text(Lang.Get("claims:gui-rank-members", cell.Citizens.Count));

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(StringFunctions.concatStringsWithDelim(cell.Citizens, ','));
            }

            EnumPlayerPermissions[] availableToAdd = claims.config.AVAILABLE_CITY_PERMISSIONS.Where(v => !cell.Permissions.Contains(v)).ToArray();

            var availableToAddStrings = availableToAdd.Select(s => s.ToString()).ToArray();
            capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems = availableToAddStrings;
            if (capi.ModLoader.GetModSystem<claimsGui>().selectedItems.Count() != availableToAddStrings.Count()) {
                capi.ModLoader.GetModSystem<claimsGui>().selectedItems = new bool[availableToAddStrings.Count()];
            }
            string PreviewText(string[] items, bool[]selected)
            {
                var list = new List<string>();
                for (int i = 0; i < items.Length; i++)
                    if (selected[i]) list.Add(items[i]);

                return list.Count > 0 ? string.Join(", ", list) : "None";
            }

            if (ImGui.BeginCombo("Select to add", PreviewText(capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems, capi.ModLoader.GetModSystem<claimsGui>().selectedItems)))
            {
                for (int i = 0; i < capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems.Length; i++)
                {
                    ImGui.Checkbox(capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems[i], ref capi.ModLoader.GetModSystem<claimsGui>().selectedItems[i]);
                }
                ImGui.EndCombo();
            }

            if(ImGui.Button("Add"))
            {
                ClientEventManager clientEventManager = (capi.World as ClientMain).eventManager;
                List<string> fullList = new List<string>();
                for (int i = 0; i < capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems.Length; i++)
                {
                    if (capi.ModLoader.GetModSystem<claimsGui>().selectedItems[i])
                    {
                        fullList.Add(capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems[i]);
                    }
                }
                string allPerms = string.Join(' ', fullList);
                clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                    string.Format("/c rank addperm {0} {1}",
                    capi.ModLoader.GetModSystem<claimsGui>().textInput, allPerms), EnumChatType.Macro, "");
                for(var i = 0; i < capi.ModLoader.GetModSystem<claimsGui>().selectedItems.Count(); i++)
                {
                    capi.ModLoader.GetModSystem<claimsGui>().selectedItems[i] = false;
                }
            }
          
            EnumPlayerPermissions[] availableToRemove = claims.config.AVAILABLE_CITY_PERMISSIONS == null
                                            ? new EnumPlayerPermissions[] { }
                                            : claims.config.AVAILABLE_CITY_PERMISSIONS.Where(v => cell.Permissions.Contains(v)).ToArray();

            var availableToRemoveStrings = availableToRemove.Select(s => s.ToString()).ToArray();
            capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems2 = availableToRemoveStrings;
            if (capi.ModLoader.GetModSystem<claimsGui>().selectedItems2.Count() != availableToRemoveStrings.Count())
            {
                capi.ModLoader.GetModSystem<claimsGui>().selectedItems2 = new bool[availableToRemoveStrings.Count()];
            }
            var c = capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems2;
            var c2 = capi.ModLoader.GetModSystem<claimsGui>().selectedItems2;

            if (ImGui.BeginCombo("Select to remove", PreviewText(capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems2, capi.ModLoader.GetModSystem<claimsGui>().selectedItems2)))
            {
                for (int i = 0; i < capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems2.Length; i++)
                {
                    ImGui.Checkbox(capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems2[i], ref capi.ModLoader.GetModSystem<claimsGui>().selectedItems2[i]);
                }
                ImGui.EndCombo();
            }

            //compo.AddMultiSelectDropDown(availableToRemoveStrings, availableToRemoveStrings, -1, null, multiSelectBoundsRemove, "removePermissionsMultiDrop");

            if (ImGui.Button("Remove"))
            {
                ClientEventManager clientEventManager = (capi.World as ClientMain).eventManager;
                List<string> fullList = new List<string>();
                for (int i = 0; i < capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems2.Length; i++)
                {
                    if (capi.ModLoader.GetModSystem<claimsGui>().selectedItems2[i])
                    {
                        fullList.Add(capi.ModLoader.GetModSystem<claimsGui>().multiSelectItems2[i]);
                    }
                }
                string allPerms = string.Join(' ', fullList);
                clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                    string.Format("/c rank removeperm {0} {1}",
                    capi.ModLoader.GetModSystem<claimsGui>().textInput, allPerms), EnumChatType.Macro, "");
                for (var i = 0; i < capi.ModLoader.GetModSystem<claimsGui>().selectedItems.Count(); i++)
                {
                    capi.ModLoader.GetModSystem<claimsGui>().selectedItems[i] = false;
                }
            }



            ImGui.BeginChild("InvitesScroll", new Vector2(0, 300), true);
            int j = 0;
            foreach (var permissions in cell.Permissions.Select(v => v.ToString()).ToList())
            {
                ImGui.PushID(j);

                Vector2 start = ImGui.GetCursorScreenPos();
                float width = ImGui.GetContentRegionAvail().X;

                ImGui.BeginGroup();

                ImGui.Text(permissions);

                ImGui.EndGroup();

                Vector2 end = ImGui.GetItemRectMax();
                var draw = ImGui.GetWindowDrawList();

                ImGui.PopID();

                ImGui.Dummy(new Vector2(0, 8));
                ImGui.Separator();
            }
            ImGui.EndChild();

            //ImGui.Text("Perms: " + string.Join('\n', cell.Permissions));
        }
    }
}
