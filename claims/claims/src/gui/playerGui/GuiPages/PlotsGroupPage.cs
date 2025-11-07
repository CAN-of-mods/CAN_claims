using System.Linq;
using claims.src.auxialiry;
using claims.src.gui.playerGui.GuiElements;
using claims.src.gui.playerGui.structures.cellElements;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using static claims.src.gui.playerGui.CANClaimsGui;

namespace claims.src.gui.playerGui.GuiPages
{
    public static class PlotsGroupPage
    {
        public static void BuildPlotsGroupPage(CANClaimsGui gui, ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var compo = gui.SingleComposer;
            if (claims.clientDataStorage.clientPlayerInfo.CityInfo == null)
            {
                compo.Compose();
                return;
            }
            var criminalsTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            currentBounds.fixedWidth = lineBounds.fixedWidth;

            currentBounds = currentBounds.BelowCopy(0, 0);

            ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, currentBounds.fixedWidth - 30, 30);

            ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, currentBounds.fixedWidth - 30, gui.mainBounds.fixedHeight - 250).FixedUnder(topTextBounds, 5);
            ElementBounds invitationTextBounds = currentBounds.BelowCopy();

            invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
            ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

            ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

            ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

            compo.AddStaticText("Plots groups",
                CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                invitationTextBounds);

            ElementBounds addPlotsGroupBounds = insetBounds.BelowCopy(15, 15);
            addPlotsGroupBounds.WithFixedWidth(25).WithFixedHeight(25);
            compo.AddInset(addPlotsGroupBounds);
            compo.AddIconButton("plus", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_ADD_NEW_NEED_NAME;
                    gui.BuildUpperWindow();
                }
            }, addPlotsGroupBounds);
            compo.AddHoverText("Add new plots group",
                                            CairoFont.SmallButtonText(),
                                            (int)currentBounds.fixedWidth / 2, addPlotsGroupBounds);

            ElementBounds removePlotsGroupBounds = addPlotsGroupBounds.RightCopy(15);
            compo.AddInset(removePlotsGroupBounds);
            compo.AddIconButton("line", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_REMOVE_SELECT;
                    gui.BuildUpperWindow();
                }
            }, removePlotsGroupBounds);
            compo.AddHoverText("Remove plots group",
                                            CairoFont.SmallButtonText(),
                                            (int)currentBounds.fixedWidth / 2, removePlotsGroupBounds);

            ElementBounds receivedInvitesPageButtonBounds = removePlotsGroupBounds.RightCopy(15);
            compo.AddInset(receivedInvitesPageButtonBounds);
            compo.AddIconButton("ring", (bool t) =>
            {
                if (t)
                {
                    gui.SelectedTab = EnumSelectedTab.PlotsGroupReceivedInvites;
                    gui.BuildMainWindow();
                }
            }, receivedInvitesPageButtonBounds);
            compo.AddHoverText("Show received invites",
                                CairoFont.SmallButtonText(),
                                (int)currentBounds.fixedWidth / 2, receivedInvitesPageButtonBounds);

            gui.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

            compo.BeginChildElements(invitationTextBounds)
                .BeginClip(clippingBounds)
                .AddInset(insetBounds, 3)
                .AddCellList(gui.listRanksBounds = gui.clippingRansksBounds.
                                    ForkContainingChild(0.0, 0.0, 0.0, -3.0).
                                    WithFixedPadding(5.0), (PlotsGroupCellElement cell, ElementBounds bounds) =>
                                    {
                                        return new GuiElementCityPlotsGroupCell(compo.Api, cell, bounds)
                                        {
                                            On = true
                                        };
                                    },
                                    claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells, "plotsgroupcellscells")
                .EndClip()
                .AddVerticalScrollbar((float value) =>
                {
                    ElementBounds bounds = compo.GetCellList<PlotsGroupCellElement>("plotsgroupcellscells").Bounds;
                    bounds.fixedY = (double)(0f - value);
                    bounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar")
                .EndChildElements();
            compo.GetCellList<PlotsGroupCellElement>("plotsgroupcellscells").BeforeCalcBounds();

            compo.Compose();

            compo.GetScrollbar("scrollbar").SetHeights((float)gui.clippingRansksBounds.fixedHeight, (float)gui.listRanksBounds.fixedHeight);


        }
        public static void BuildPlotsGroupInfoPage(CANClaimsGui gui, ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var compo = gui.SingleComposer;
            if (claims.clientDataStorage.clientPlayerInfo.CityInfo == null)
            {
                compo.Compose();
                return;
            }
            PlotsGroupCellElement cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells.FirstOrDefault(gr => gr.Guid.Equals(gui.selectedString), null);
            if (cell == null)
            {
                return;
            }

            currentBounds = currentBounds.BelowCopy(0, 40);
            var plotTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            var clientInfo = claims.clientDataStorage.clientPlayerInfo;

            currentBounds.Alignment = EnumDialogArea.LeftTop;
            compo.AddStaticText(string.Format("{0}: {1}", cell.CityName, cell.Name),
                plotTabFont, EnumTextOrientation.Center,
                currentBounds, "plotPos");

            currentBounds = currentBounds.BelowCopy();
            currentBounds.fixedY += 25;
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            compo.AddStaticText(Lang.Get("claims:gui-group-members", cell.PlayersNames.Count),
                        plotTabFont,
                        currentBounds, "criminals");

            compo.AddHoverText(StringFunctions.concatStringsWithDelim(cell.PlayersNames, ','),
                                        plotTabFont.WithOrientation(EnumTextOrientation.Center),
                                        (int)currentBounds.fixedWidth, currentBounds);

            ElementBounds addCriminalBounds = currentBounds.RightCopy();
            addCriminalBounds.WithFixedWidth(25).WithFixedHeight(25);
            ElementBounds removeFriendBounds = addCriminalBounds.RightCopy();
            compo.AddIconButton("plus", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.ADD_PLOTSGROUP_MEMBER_NEED_NAME;
                    gui.BuildUpperWindow();
                }
            }, addCriminalBounds);


            compo.AddIconButton("line", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.REMOVE_PLOTSGROUP_MEMBER_SELECT;
                    gui.BuildUpperWindow();
                }
            }, removeFriendBounds);

            currentBounds = currentBounds.BelowCopy(0, 5);
            compo.AddStaticText(Lang.Get("claims:gui-plot-permissions"),
                plotTabFont, EnumTextOrientation.Left,
                currentBounds, "permissionHandler");


            ElementBounds showPermissionsButtonBounds = currentBounds.RightCopy();
            showPermissionsButtonBounds.WithFixedSize(25, 25);
            compo.AddIconButton("medal", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PERMISSIONS;
                    gui.BuildUpperWindow();
                }
                else
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                    gui.BuildUpperWindow();
                }
            }, showPermissionsButtonBounds, "setPermissions");
            compo.GetToggleButton("setPermissions").Toggleable = true;

            ElementBounds claimCityPlotButtonBounds = currentBounds.BelowCopy();
            claimCityPlotButtonBounds.WithFixedWidth(25).WithFixedHeight(25);
            ElementBounds unclaimCityPlotButtonBounds = claimCityPlotButtonBounds.RightCopy();
            compo.AddIconButton("plus", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PLOT_CLAIM_CONFIRM;
                    gui.BuildUpperWindow();
                }
            }, claimCityPlotButtonBounds);
            compo.AddHoverText("Add current plot to plots group",
                                CairoFont.SmallButtonText(),
                                (int)currentBounds.fixedWidth / 2, claimCityPlotButtonBounds);

            compo.AddIconButton("line", (bool t) =>
            {
                if (t)
                {
                    gui.CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PLOT_UNCLAIM_CONFIRM;
                    gui.BuildUpperWindow();
                }
            }, unclaimCityPlotButtonBounds);
            compo.AddHoverText("Remove current plot from plots group",
                                            CairoFont.SmallButtonText(),
                                            (int)currentBounds.fixedWidth / 2, unclaimCityPlotButtonBounds);

        }
        public static void BuildPlotsGroupReceivedInvitesPage(CANClaimsGui gui, ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var compo = gui.SingleComposer;
            ElementBounds createCityBounds = currentBounds.FlatCopy();

            if (claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations.Count > 0)
            {
                int numClaimsToSkip = gui.selectedClaimsPage * gui.claimsPerPage;
                ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

                ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, gui.mainBounds.fixedHeight - 230).FixedUnder(topTextBounds, 5);
                ElementBounds invitationTextBounds = createCityBounds.BelowCopy(0, 35);
                invitationTextBounds.fixedHeight -= 50;
                invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
                ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

                ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

                ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

                compo.AddStaticText("Invitations" + " [" + claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations.Count + "]",
                    CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                    invitationTextBounds);


                gui.clippingInvitationsBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

                compo.BeginChildElements(invitationTextBounds.BelowCopy())
                .BeginClip(clippingBounds)
                .AddInset(insetBounds, 3)
                .AddCellList(gui.listInvitationsBounds = gui.clippingInvitationsBounds.ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
                (ClientToPlotsGroupInvitation cell, ElementBounds bounds) =>
                {
                    return new GuiElementPlotsGroupInvitation(compo.Api, cell, bounds)
                    {
                        On = true
                    };
                }, claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations, "modstable")
                .EndClip()
                .AddVerticalScrollbar((float value) =>
                {
                    ElementBounds bounds = compo.GetCellList<ClientToPlotsGroupInvitation>("modstable").Bounds;
                    bounds.fixedY = (double)(0f - value);
                    bounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar")
                .EndChildElements()
                .Compose();

                compo.GetScrollbar("scrollbar").SetHeights((float)gui.clippingInvitationsBounds.fixedHeight, (float)gui.listInvitationsBounds.fixedHeight);
            }
            else
            {
                int renderedClaimsInfoCounter = 0;
                int numClaimsToSkip = gui.selectedClaimsPage * gui.claimsPerPage;
                ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

                ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, gui.mainBounds.fixedHeight - 230).FixedUnder(topTextBounds, 5);
                ElementBounds invitationTextBounds = createCityBounds.BelowCopy(0, 35);
                invitationTextBounds.fixedHeight -= 50;
                invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
                ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

                ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

                ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

                compo.AddStaticText(Lang.Get("claims:gui-no-plotsgroup-invites"),
                    CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                    invitationTextBounds);
            }
        }
    }
}
