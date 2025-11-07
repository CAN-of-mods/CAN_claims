using System.Linq;
using claims.src.auxialiry;
using claims.src.gui.playerGui.GuiElements;
using claims.src.gui.playerGui.structures.cellElements;
using claims.src.rights;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;
using static claims.src.gui.playerGui.CANClaimsGui;

namespace claims.src.gui.playerGui.GuiPages
{
    public static class RanksPage
    {
        public static void BuildRanksPage(CANClaimsGui gui, ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var compo = gui.SingleComposer;
            currentBounds = currentBounds.BelowCopy(0, 20);
            ElementBounds createCityBounds = currentBounds.FlatCopy();
            int numClaimsToSkip = gui.selectedClaimsPage * gui.claimsPerPage;
            ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

            ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, gui.mainBounds.fixedHeight - 230).FixedUnder(topTextBounds, 5);
            ElementBounds invitationTextBounds = createCityBounds.BelowCopy();
            invitationTextBounds.fixedHeight -= 50;
            //invitationTextBounds.fixedWidth -= 100;
            invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
            ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

            ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

            ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

            compo.AddStaticText("Ranks",
                CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                invitationTextBounds);

            //currentBounds = currentBounds.RightCopy(0, 0);

            //currentBounds.fixedWidth -= 20;
            ElementBounds addPlayerButtonEB = invitationTextBounds.RightCopy().WithFixedSize(24, 24);
            addPlayerButtonEB.fixedX = invitationTextBounds.absOffsetX + invitationTextBounds.fixedWidth / 2 - 60;
            /*removeRankEB.fixedX = 0;
            removeRankEB.fixedY += 20;*/
            compo.AddIconButton("plus", (bool t) =>
            {
                gui.CreateNewCityState = EnumUpperWindowSelectedState.CITY_RANK_CREATION_NEED_NAME;
                gui.BuildUpperWindow();
            },
                             addPlayerButtonEB);


            gui.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

            compo.BeginChildElements(invitationTextBounds.BelowCopy())
            .BeginClip(clippingBounds)
            .AddInset(insetBounds, 3)
            .AddCellList(gui.listRanksBounds = gui.clippingRansksBounds.ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
            (CityRankCellElement cell, ElementBounds bounds) =>
            {
                return new GuiElementCityRanks(compo.Api, cell, bounds)
                {
                    On = true,
                };
            }, claims.clientDataStorage.clientPlayerInfo.CityInfo.CityRanks, "citizensranks")
            .EndClip()
            .AddVerticalScrollbar((float value) =>
            {
                ElementBounds bounds = compo.GetCellList<CityRankCellElement>("citizensranks").Bounds;
                bounds.fixedY = (double)(0f - value);
                bounds.CalcWorldBounds();
            }, scrollbarBounds, "scrollbar")
            .EndChildElements();
            compo.GetCellList<CityRankCellElement>("citizensranks").BeforeCalcBounds();

            compo.Compose();

            compo.GetScrollbar("scrollbar").SetHeights((float)gui.clippingRansksBounds.fixedHeight, (float)gui.listRanksBounds.fixedHeight);
        }
        public static void BuildRanksInfoPage(CANClaimsGui gui, ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var compo = gui.SingleComposer;
            if (claims.clientDataStorage.clientPlayerInfo.CityInfo == null)
            {
                compo.Compose();
                return;
            }
            CityRankCellElement cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.CityRanks.FirstOrDefault(rc => rc.Name.Equals(gui.selectedString), null);
            if (cell == null)
            {
                return;
            }

            currentBounds = currentBounds.BelowCopy(0, 40).WithAlignment(EnumDialogArea.LeftTop);
            var plotTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            var clientInfo = claims.clientDataStorage.clientPlayerInfo;

            compo.AddStaticText(string.Format("{0}", cell.Name),
                plotTabFont, EnumTextOrientation.Center,
                currentBounds, "rankName");
            currentBounds.fixedWidth -= 20;
            ElementBounds removeRankEB = currentBounds.RightCopy().WithFixedSize(24, 24);

            compo.AddIconButton("eraser", (bool t) =>
            {
                gui.CreateNewCityState = EnumUpperWindowSelectedState.CITY_RANK_DELETE_CONFIRM;
                gui.BuildUpperWindow();
            }, removeRankEB);

            currentBounds = currentBounds.BelowCopy();
            currentBounds.fixedY += 25;
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            compo.AddStaticText(Lang.Get("claims:gui-rank-members", cell.Citizens.Count), plotTabFont, currentBounds, "members");

            compo.AddHoverText(StringFunctions.concatStringsWithDelim(cell.Citizens, ','),
                                        plotTabFont.WithOrientation(EnumTextOrientation.Center),
                                        (int)currentBounds.fixedWidth, currentBounds);

            ElementBounds multiSelectBounds = currentBounds.BelowCopy(0, 10);

            EnumPlayerPermissions[] availableToAdd = claims.config.AVAILABLE_CITY_PERMISSIONS.Where(v => !cell.Permissions.Contains(v)).ToArray();

            var availableToAddStrings = availableToAdd.Select(s => s.ToString()).ToArray();

            compo.AddMultiSelectDropDown(availableToAddStrings, availableToAddStrings, -1, null, multiSelectBounds, "addPermissionsMultiDrop");
            currentBounds = multiSelectBounds;
            ElementBounds colorSelectedButton = currentBounds.BelowCopy().WithFixedSize(48, 24);
            colorSelectedButton.fixedX = 0;
            colorSelectedButton.fixedY += 20;
            compo.AddButton("Add", new ActionConsumable(() =>
            {
                ClientEventManager clientEventManager = (compo.Api.World as ClientMain).eventManager;
                var dropDown = compo.GetDropDown("addPermissionsMultiDrop");
                if (dropDown.SelectedValues.Length > 0)
                {
                    string fullList = string.Join(' ', dropDown.SelectedValues);
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                    string.Format("/c rank addperm {0} {1}",
                    gui.selectedString, fullList), EnumChatType.Macro, "");
                }
                dropDown.SetSelectedValue("");
                return true;
            }),
                             colorSelectedButton);

            ElementBounds multiSelectBoundsRemove = multiSelectBounds.RightCopy(0, 0);

            EnumPlayerPermissions[] availableToRemove = claims.config.AVAILABLE_CITY_PERMISSIONS == null
                                            ? new EnumPlayerPermissions[] { }
                                            : claims.config.AVAILABLE_CITY_PERMISSIONS.Where(v => cell.Permissions.Contains(v)).ToArray();

            var availableToRemoveStrings = availableToRemove.Select(s => s.ToString()).ToArray();

            compo.AddMultiSelectDropDown(availableToRemoveStrings, availableToRemoveStrings, -1, null, multiSelectBoundsRemove, "removePermissionsMultiDrop");

            ElementBounds removeSelectedButtonEB = multiSelectBoundsRemove.BelowCopy().WithFixedSize(58, 24);
            /* removeSelectedButtonEB.fixedX = 0;*/
            removeSelectedButtonEB.fixedY += 20;
            compo.AddButton("Remove", new ActionConsumable(() =>
            {
                ClientEventManager clientEventManager = (compo.Api.World as ClientMain).eventManager;
                var dropDown = compo.GetDropDown("removePermissionsMultiDrop");
                if (dropDown.SelectedValues.Length > 0)
                {
                    string fullList = string.Join(' ', dropDown.SelectedValues);
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                    string.Format("/c rank removeperm {0} {1}",
                    gui.selectedString, fullList), EnumChatType.Macro, "");
                }
                dropDown.SetSelectedValue("");
                return true;
            }),
                             removeSelectedButtonEB);



            var mainBounds = ElementBounds.Fixed(35.0, 35.0, lineBounds.fixedWidth - 40, 220);
            mainBounds.fixedOffsetY = currentBounds.fixedOffsetY + 110;
            var textBounds = mainBounds.FlatCopy();
            mainBounds.Alignment = EnumDialogArea.LeftMiddle;
            int insetDepth = 3;
            int insetWidth = (int)mainBounds.fixedWidth;
            int insetHeight = (int)mainBounds.fixedHeight;
            int rowHeight = 25;

            ElementBounds insetBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, insetWidth, insetHeight);
            ElementBounds scrollbarBounds = insetBounds.RightCopy().WithFixedWidth(20);
            ElementBounds clipBounds = insetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding);
            ElementBounds containerBounds = insetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding);
            ElementBounds containerRowBounds = ElementBounds.Fixed(0, 0, insetWidth, rowHeight);

            compo.BeginChildElements(mainBounds)
               .AddInset(insetBounds, insetDepth)
                    .BeginClip(clipBounds)
                        .AddContainer(containerBounds, "scroll-content1")
                    .EndClip()
                    .AddVerticalScrollbar((value) =>
                    {
                        ElementBounds bounds = compo.GetContainer("scroll-content1").Bounds;
                        bounds.fixedY = 5 - value;
                        bounds.CalcWorldBounds();
                    }, scrollbarBounds, "scrollbar1")
            .EndChildElements();
            GuiElementContainer scrollArea = compo.GetContainer("scroll-content1");
            var li = cell.Permissions.Select(v => v.ToString()).ToList();
            foreach (var it in li)
            {
                scrollArea.Add(new GuiElementRichtext(compo.Api, VtmlUtil.Richtextify(compo.Api, it, CairoFont.WhiteMediumText().WithFontSize(20)), containerRowBounds));
                containerRowBounds = containerRowBounds.BelowCopy();
            }

            compo.Compose();

            // After composing dialog, need to set the scrolling area heights to enable scroll behavior
            float scrollVisibleHeight = (float)clipBounds.fixedHeight;
            float scrollTotalHeight = rowHeight * li.Count;
            compo.GetScrollbar("scrollbar1").SetHeights(scrollVisibleHeight, scrollTotalHeight);
            //var sc = SingleComposer.GetScrollbar("scrollbar1");
            //sc.Enabled = false;
            currentBounds = currentBounds.BelowCopy();
            currentBounds.fixedY += 25;
            currentBounds.fixedWidth = 300;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            compo.AddStaticText("Perms: " + string.Join('\n', cell.Permissions),
                       plotTabFont,
                       currentBounds, "permissions");
        }
    }
}
