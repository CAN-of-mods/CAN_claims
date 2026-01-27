using System.Collections.Generic;
using Cairo;
using claims.src.gui.prettyGui.GuiTabs;
using Vintagestory.API.Client;

namespace claims.src.gui.prettyGui
{
    public class TabDrawHandler
    {
        private Dictionary<EnumSelectedTab, CANGuiTab> TabDictionary;
        public ICoreClientAPI capi;
        public IconHandler iconHandler;
        public TabDrawHandler(ICoreClientAPI capi, IconHandler iconHandler)
        {
            TabDictionary = new();
            this.capi = capi;
            this.TabDictionary.Add(EnumSelectedTab.CITY, new CANCityTab(capi, iconHandler));
            this.iconHandler = iconHandler;
        }
        public void DrawTab(EnumSelectedTab selectedTab)
        {
            if (this.TabDictionary.TryGetValue(selectedTab, out var guiTab))
            {
                guiTab.DrawTab();
            }
        }
    }
}
