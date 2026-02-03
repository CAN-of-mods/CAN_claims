using System.Linq;
using System.Numerics;
using claims.src.auxialiry;
using claims.src.gui.playerGui.GuiElements;
using claims.src.gui.playerGui.structures.cellElements;
using ImGuiNET;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;
using static System.Net.Mime.MediaTypeNames;

namespace claims.src.gui.prettyGui.GuiTabs
{
    public class CANPlotsGroupInvitesTab : CANGuiTab
    {
        public CANPlotsGroupInvitesTab(ICoreClientAPI capi, IconHandler iconHandler)
        {
            this.capi = capi;
            this.iconHandler = iconHandler;
        }
        public override void DrawTab()
        {
            if (claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations.Count > 0)
            {
                ImGui.Text(Lang.Get("claims:gui-invites-list-title"));

                ImGui.BeginChild("InvitesScroll", new Vector2(0, 300), true);
                int i = 0;
                foreach (var invite in claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations)
                {
                    ImGui.PushID(i);

                    Vector2 start = ImGui.GetCursorScreenPos();
                    float width = ImGui.GetContentRegionAvail().X;

                    ImGui.BeginGroup();

                    var text = invite.CityName + " - " + invite.PlotsGroupName;
                    ImGui.Text(text);

                    if (ImGui.ImageButton("acceptplotsgroup", this.iconHandler.GetOrLoadIcon("expander"), new Vector2(16)))
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plotsgroupaccept "
                            + invite.CityName + " " + invite.PlotsGroupName, EnumChatType.Macro, "");
                        var cell = claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations.FirstOrDefault(c => c.CityName == invite.CityName && c.PlotsGroupName == invite.PlotsGroupName);
                        if (cell != null)
                        {
                            claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations.Remove(cell);
                        }
                    }
                   /* ImGui.SameLine();
                    if (ImGui.ImageButton("declineplotsgroup", this.iconHandler.GetOrLoadIcon("contract"), new Vector2(16)))
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plotsgroupaccept "
                            + invite.CityName + " " + invite.PlotsGroupName, EnumChatType.Macro, "");
                        var cell = claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations.FirstOrDefault(c => c.CityName == invite.CityName && c.PlotsGroupName == invite.PlotsGroupName);
                        if (cell != null)
                        {
                            claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations.Remove(cell);
                            claims.CANCityGui.BuildMainWindow();
                        }
                    }*/
                    ImGui.EndGroup();

                    Vector2 end = ImGui.GetItemRectMax();
                    var draw = ImGui.GetWindowDrawList();

                    ImGui.PopID();

                    ImGui.Dummy(new Vector2(0, 8));
                    ImGui.Separator();
                }
                ImGui.EndChild();
            }
            else
            {
                ImGui.Text(Lang.Get("claims:gui-no-plotsgroup-invites"));
            }

        }
    }
}
