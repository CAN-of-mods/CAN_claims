using Vintagestory.API.Client;

namespace claims.src.gui.prettyGui.GuiSecondaryTabs
{
    public abstract class CANGuiSecondaryTab
    {
        public ICoreClientAPI capi;
        public IconHandler iconHandler;
        public abstract void DrawTab();
    }
}
