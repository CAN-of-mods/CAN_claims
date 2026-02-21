using System.Linq;
using System.Numerics;
using ImGuiNET;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;
using static claims.src.gui.playerGui.CANClaimsGui;

namespace claims.src.gui.prettyGui.GuiSecondaryTabs
{
    public class CANYesNoRemovePrisonCellTab : CANGuiSecondaryTab
    {
        private string TitleString;
        private string CommandToCallOnYes;
        private string YesButtonString;
        private string NoButtonString;
        public CANYesNoRemovePrisonCellTab(ICoreClientAPI capi, IconHandler iconHandler, string titleString, string commandToCallOnYes, string yesButtonString = "claims:gui-yes-string", string noButtonString = "claims:gui-no-string")
        {
            this.capi = capi;
            this.iconHandler = iconHandler;
            TitleString = titleString;
            CommandToCallOnYes = commandToCallOnYes;
            YesButtonString = yesButtonString;
            NoButtonString = noButtonString;
        }
        public override void DrawTab()
        {
            ImGui.SetNextWindowPos(
                new Vector2(capi.ModLoader.GetModSystem<claimsGui>().mainWindowPos.X + capi.ModLoader.GetModSystem<claimsGui>().mainWindowSize.X, capi.ModLoader.GetModSystem<claimsGui>().mainWindowPos.Y)
            );

            ImGuiWindowFlags flags1 =
                 ImGuiWindowFlags.NoScrollWithMouse;
            ImGui.Begin("ClaimsDetails", p_open: ref capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowOpen, flags1);
            
            ImGui.Text(Lang.Get(TitleString, capi.ModLoader.GetModSystem<claimsGui>().textInput, capi.ModLoader.GetModSystem<claimsGui>().textInput2));
            //ImGui.InputText("", ref capi.ModLoader.GetModSystem<claimsGui>().textInput, 256);

            if(ImGui.Button(Lang.Get(YesButtonString)))
            {
                ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, this.CommandToCallOnYes + " " +
                    capi.ModLoader.GetModSystem<claimsGui>().selectedPos.X + " " + capi.ModLoader.GetModSystem<claimsGui>().selectedPos.Y + " " + capi.ModLoader.GetModSystem<claimsGui>().selectedPos.Z, EnumChatType.Macro, "");
                capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.NONE;
                var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PrisonCells.FirstOrDefault(c => c.SpawnPosition == capi.ModLoader.GetModSystem<claimsGui>().selectedPos);
                if (cell != null)
                {
                    claims.clientDataStorage.clientPlayerInfo.CityInfo.PrisonCells.Remove(cell);
                }
            }
            ImGui.SameLine();
            if (ImGui.Button(Lang.Get(NoButtonString)))
            {
                capi.ModLoader.GetModSystem<claimsGui>().secondaryWindowTab = EnumSecondaryWindowTab.NONE;
            }
            ImGui.End();
        }
    }
}
