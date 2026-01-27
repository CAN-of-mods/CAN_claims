using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace claims.src.gui.prettyGui.GuiTabs
{
    public abstract class CANGuiTab
    {
        public ICoreClientAPI capi;
        public IconHandler iconHandler;
        public abstract void DrawTab();
    }
}
