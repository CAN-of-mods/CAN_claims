using claims.src.network.packets;
using claims.src.part.structure;
using claims.src.part.structure.plots;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;
using static claims.src.gui.playerGui.CANClaimsGui;

namespace claims.src.gui.playerGui.GuiPages
{
    public static class PlotPage
    {
        public static void BuildPlotPage(CANClaimsGui gui, ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var compo = gui.SingleComposer;
            currentBounds = currentBounds.BelowCopy(0, 40);
            var plotTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            currentBounds.fixedWidth /= 2;
            currentBounds.Alignment = EnumDialogArea.LeftTop;
            compo.AddStaticText("[" + clientInfo.CurrentPlotInfo.PlotPosition.X + "/" + clientInfo.CurrentPlotInfo.PlotPosition.Y + "]",
                plotTabFont,
                currentBounds, "plotPos");
            //compo.AddInset(currentBounds);
            ElementBounds refreshPlotButtonBounds = currentBounds.RightCopy();
            refreshPlotButtonBounds.WithFixedSize(35, 35);
            compo.AddIconButton("redo", (bool t) =>
            {
                if (t)
                {
                    claims.clientChannel.SendPacket(new SavedPlotsPacket()
                    {
                        type = PacketsContentEnum.CURRENT_PLOT_CLIENT_REQUEST,
                        data = ""
                    });
                }
            }, refreshPlotButtonBounds);


            /*==============================================================================================*/
            /*=====================================PLOT NAME================================================*/
            /*==============================================================================================*/
            currentBounds = currentBounds.BelowCopy(0, 5);
            compo.AddStaticText(Lang.Get("claims:gui-plot-name", clientInfo.CurrentPlotInfo.PlotName),
                plotTabFont,
                currentBounds, "plotName");

            ElementBounds setPlotNameButtonBounds = currentBounds.RightCopy();
            setPlotNameButtonBounds.WithFixedSize(25, 25);
            compo.AddIconButton("hat", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.PLOT_SET_NAME;

                    gui.BuildUpperWindow();
                }
                else
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                    gui.BuildUpperWindow();
                }
            }, setPlotNameButtonBounds, "setPlotName");
            compo.GetToggleButton("setPlotName").Toggleable = true;
            compo.AddHoverText("Set plot name", plotTabFont, 180, setPlotNameButtonBounds);


            currentBounds = currentBounds.BelowCopy(0, 5);
            compo.AddStaticText(Lang.Get("claims:gui-owner-name", clientInfo.CurrentPlotInfo.OwnerName),
                plotTabFont,
                currentBounds, "ownerName");

            if (clientInfo.CurrentPlotInfo.Price > -1)
            {
                ElementBounds buyPlotButtonBounds = currentBounds.RightCopy();
                buyPlotButtonBounds.WithFixedSize(25, 25);
                compo.AddIconButton("medal", (bool t) =>
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.PLOT_CLAIM;
                    gui.BuildUpperWindow();
                }, buyPlotButtonBounds);
                compo.AddHoverText("Buy plot", plotTabFont, 180, buyPlotButtonBounds);

            }
            if (clientInfo.CurrentPlotInfo.OwnerName?.Length > 0)
            {
                ElementBounds sellPlotButtonBounds = currentBounds.RightCopy();
                sellPlotButtonBounds.WithFixedSize(25, 25);
                compo.AddIconButton("medal", (bool t) =>
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.PLOT_UNCLAIM;
                    gui.BuildUpperWindow();
                }, sellPlotButtonBounds);
                compo.AddHoverText("Unclaim plot", plotTabFont, 180, sellPlotButtonBounds);
            }

            /*==============================================================================================*/
            /*=====================================PLOT TYPE================================================*/
            /*==============================================================================================*/
            currentBounds = currentBounds.BelowCopy(0, 5);

            compo.AddStaticText(Lang.Get("claims:gui-plot-type",
                (PlotInfo.dictPlotTypes.TryGetValue(clientInfo.CurrentPlotInfo.PlotType, out PlotInfo plotInfo) ? plotInfo.getFullName() : "-")),
                plotTabFont,
                currentBounds, "plotType");

            ElementBounds setPlotTypeButtonBounds = currentBounds.RightCopy();
            setPlotTypeButtonBounds.WithFixedSize(25, 25);
            compo.AddIconButton("hat", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.PLOT_SET_TYPE;
                    gui.BuildUpperWindow();
                }
                else
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                    gui.BuildUpperWindow();
                }
            }, setPlotTypeButtonBounds, "setPlotType");
            compo.GetToggleButton("setPlotType").Toggleable = true;
            compo.AddHoverText("Set plot type", plotTabFont, 180, setPlotTypeButtonBounds);

            ElementBounds addCellButtonBounds = setPlotTypeButtonBounds.RightCopy();
            addCellButtonBounds.WithFixedSize(25, 25);

            if (clientInfo.CurrentPlotInfo.PlotType == PlotType.PRISON)
            {
                compo.AddIconButton("wpCircle", (bool t) =>
                {
                    if (t)
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/c prison addcell", EnumChatType.Macro, "");
                        gui.BuildUpperWindow();
                    }
                }, addCellButtonBounds, "addPrisonCell");
                compo.AddHoverText("Add prison cell", plotTabFont, 180, addCellButtonBounds);
            }
            else if (clientInfo.CurrentPlotInfo.PlotType == PlotType.SUMMON)
            {
                compo.AddIconButton("wpCircle", (bool t) =>
                {
                    if (t)
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/c summon set point", EnumChatType.Macro, "");
                        gui.BuildUpperWindow();
                    }
                }, addCellButtonBounds, "setSummonPoint");
                compo.AddHoverText("Set summon point.", plotTabFont, 180, addCellButtonBounds);
            }
            /*==============================================================================================*/
            /*=====================================PLOT TAX=================================================*/
            /*==============================================================================================*/
            currentBounds = currentBounds.BelowCopy(0, 5);
            compo.AddStaticText(Lang.Get("claims:gui-plot-custom-tax", clientInfo.CurrentPlotInfo.CustomTax),
                plotTabFont,
                currentBounds, "customTax");

            ElementBounds setPlotTaxButtonBounds = currentBounds.RightCopy();
            setPlotTaxButtonBounds.WithFixedSize(25, 25);
            compo.AddIconButton("medal", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.PLOT_SET_TAX;
                    gui.BuildUpperWindow();
                }
                else
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                    gui.BuildUpperWindow();
                }
            }, setPlotTaxButtonBounds, "setPlotTax");
            compo.GetToggleButton("setPlotTax").Toggleable = true;

            /*==============================================================================================*/
            /*=====================================PLOT PRICES==============================================*/
            /*==============================================================================================*/
            currentBounds = currentBounds.BelowCopy(0, 5);
            compo.AddStaticText(Lang.Get("claims:gui-plot-price", clientInfo.CurrentPlotInfo.Price > -1
                                                 ? clientInfo.CurrentPlotInfo.Price
                                                 : Lang.Get("claims:gui-not-for-sale")),
                plotTabFont,
                currentBounds, "plotPrice");

            ElementBounds setPlotPriceButtonBounds = currentBounds.RightCopy();
            setPlotPriceButtonBounds.WithFixedSize(25, 25);
            compo.AddIconButton("medal", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.PLOT_SET_PRICE_NEED_NUMBER;
                    gui.BuildUpperWindow();
                }
                else
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                    gui.BuildUpperWindow();
                }
            }, setPlotPriceButtonBounds, "setPlotPrice");
            compo.AddHoverText("Set plot for sale", plotTabFont, 180, setPlotPriceButtonBounds);
            compo.GetToggleButton("setPlotPrice").Toggleable = true;

            ElementBounds setPlotNotForSaleButtonBounds = setPlotPriceButtonBounds.RightCopy();
            setPlotNotForSaleButtonBounds.WithFixedSize(25, 25);
            compo.AddIconButton("line", (bool t) =>
            {
                if (t)
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot nfs", EnumChatType.Macro, "");
                }
            }, setPlotNotForSaleButtonBounds);
            compo.AddHoverText("Set plot NOT for sale", plotTabFont, 180, setPlotNotForSaleButtonBounds);


            /*==============================================================================================*/
            /*=====================================PLOT PERMISSIONS=========================================*/
            /*==============================================================================================*/
            currentBounds = currentBounds.BelowCopy(0, 5);
            compo.AddStaticText(Lang.Get("claims:gui-plot-permissions"),
                plotTabFont,
                currentBounds, "permissionHandler");


            ElementBounds showPermissionsButtonBounds = currentBounds.RightCopy();
            showPermissionsButtonBounds.WithFixedSize(25, 25);
            compo.AddIconButton("medal", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.PLOT_PERMISSIONS;
                    gui.BuildUpperWindow();
                }
                else
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                    gui.BuildUpperWindow();
                }
            }, showPermissionsButtonBounds, "setPermissions");
            compo.GetToggleButton("setPermissions").Toggleable = true;

            /*==============================================================================================*/
            /*=====================================PLOT SHOW BORDERS========================================*/
            /*==============================================================================================*/
            currentBounds = currentBounds.BelowCopy(0, 5);
            compo.AddStaticText(Lang.Get("claims:gui-plot-borders"),
                plotTabFont,
                currentBounds, "plotborders");


            ElementBounds showPlotBordersButtonBounds = currentBounds.RightCopy();
            showPlotBordersButtonBounds.WithFixedSize(25, 25);
            compo.AddIconButton("select", (bool t) =>
            {
                //if (t)
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot borders " + "on", EnumChatType.Macro, "");
                }
            }, showPlotBordersButtonBounds);

            ElementBounds hidePlotBordersButtonBounds = showPlotBordersButtonBounds.RightCopy();
            compo.AddIconButton("eraser", (bool t) =>
            {
                //if (t)
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot borders " + "off", EnumChatType.Macro, "");
                }
            }, hidePlotBordersButtonBounds);
        }
    }
}
