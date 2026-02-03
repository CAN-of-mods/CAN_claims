using System.Numerics;
using ImGuiNET;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;

namespace claims.src.gui.prettyGui.GuiSecondaryTabs
{
    public class CANYesNoSummonNameTab : CANGuiSecondaryTab
    {
        private string TitleString;
        private string CommandToCallOnYes;
        private string YesButtonString;
        private string NoButtonString;
        public CANYesNoSummonNameTab(ICoreClientAPI capi, IconHandler iconHandler, string titleString, string yesButtonString = "claims:gui-yes-string", string noButtonString = "claims:gui-no-string")
        {
            this.capi = capi;
            this.iconHandler = iconHandler;
            TitleString = titleString;
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
            
            ImGui.Text(Lang.Get(TitleString, capi.ModLoader.GetModSystem<claimsGui>().textInput2, capi.ModLoader.GetModSystem<claimsGui>().textInput));
            ImGui.InputText("", ref capi.ModLoader.GetModSystem<claimsGui>().textInput, 256);
            if (ImGui.Button(Lang.Get(this.YesButtonString)))
            {
                ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, string.Format("/city summon set cname {0} {1} {2} {3}",
                   capi.ModLoader.GetModSystem<claimsGui>().selectedPos.X, capi.ModLoader.GetModSystem<claimsGui>().selectedPos.Y, capi.ModLoader.GetModSystem<claimsGui>().selectedPos.Z,
                   capi.ModLoader.GetModSystem<claimsGui>().textInput), EnumChatType.Macro, "");
                capi.ModLoader.GetModSystem<claimsGui>().textInput = "";
            }
            ImGui.End();
        }
    }
}
