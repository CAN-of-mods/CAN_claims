using Cairo;
using claims.src.auxialiry;
using claims.src.gui.playerGui.GuiElements;
using claims.src.gui.playerGui.structures;
using claims.src.network.packets;
using claims.src.part;
using claims.src.part.structure;
using claims.src.part.structure.plots;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;
using Vintagestory.Common;
using Vintagestory.GameContent;
using static System.Net.Mime.MediaTypeNames;

namespace claims.src.gui.playerGui
{
    public class CANClaimsGui : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "CANClaimsGui";
        public float Width { get; private set; }
        public float Height { get; private set; }
        public EnumSelectedTab SelectedTab { get; private set; }
        public enum EnumUpperWindowSelectedState
        {
            NONE, NEED_NAME, NEED_AGREE, INVITE_TO_CITY_NEED_NAME, KICK_FROM_CITY_NEED_NAME, UNINVITE_TO_CITY,
            CLAIM_CITY_PLOT_CONFIRM, UNCLAIM_CITY_PLOT_CONFIRM, PLOT_PERMISSIONS, ADD_FRIEND_NEED_NAME, REMOVE_FRIEND,
            PLOT_SET_PRICE_NEED_NUMBER, PLOT_SET_TAX, PLOT_SET_TYPE, PLOT_SET_NAME, PLOT_CLAIM, PLOT_UNCLAIM,
            CITY_TITLE_SELECT_CITIZEN, CITY_TITLE_CITIZEN_SELECTED, LEAVE_CITY_CONFIRM,
            CITY_RANK_REMOVE_CONFIRM, CITY_RANK_ADD, SELECT_NEW_CITY_NAME
        }

        public EnumUpperWindowSelectedState CreateNewCityState { get; set; } = EnumUpperWindowSelectedState.NONE;
        public string collectedNewCityName { get; set; } = "";
        public string firstValueCollected { get; set; } = "";
        public string secondValueCollected { get; set; } = "";
        public enum EnumSelectedTab
        {
            City, Player, Prices, Plot, Ranks, CityPlotsColorSelector
        }
        public int selectedClaimsPage = 0;
        public int claimsPerPage = 3;
        private int selectedColor = -1;

        private ElementBounds clippingInvitationsBounds;
        private ElementBounds listInvitationsBounds;

        private ElementBounds clippingRansksBounds;
        private ElementBounds listRanksBounds;


        public CANClaimsGui(ICoreClientAPI capi) : base(capi)
        {
            Width = 500;
            Height = 600;
            SelectedTab = 0;
        }
        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            BuildMainWindow();
        }
        public override void OnGuiClosed()
        {
            base.OnGuiClosed();
            this.CreateNewCityState = EnumUpperWindowSelectedState.NONE;
        }       
        /*==============================================================================================*/
        /*=====================================RANKS====================================================*/
        /*==============================================================================================*/
        private IGuiElementCell createRankCellElem(RankCellElement cell, ElementBounds bounds)
        {
            return new GuiElementCityRanks(capi, cell, bounds)
            {
                //"claims:textures/icons/warning.svg")
                On = true,
                OnMouseDownOnCellLeft = new Action<int>(this.OnClickCellLeft),
                OnMouseDownOnCellMiddle = new Action<int>(this.OnClickCellMiddle),
                OnMouseDownOnCellRight = new Action<int>(this.OnClickCellRight)
            };
        }
        private void OnNewRanksScrollbarvalue(float value)
        {
            ElementBounds bounds = SingleComposer.GetCellList<RankCellElement>("citizensranks").Bounds;
            bounds.fixedY = (double)(0f - value);
            bounds.CalcWorldBounds();
        }

        /*==============================================================================================*/
        /*=====================================INVITATIONS==============================================*/
        /*==============================================================================================*/
        private void OnNewScrollbarvalue(float value)
        {
            ElementBounds bounds = SingleComposer.GetCellList<ClientToCityInvitation>("modstable").Bounds;
            bounds.fixedY = (double)(0f - value);
            bounds.CalcWorldBounds();
        }
        private IGuiElementCell createCellElem(ClientToCityInvitation cell, ElementBounds bounds)
        {
            return new GuiElementCityInvitation(capi, cell, bounds)
            {
                //"claims:textures/icons/warning.svg")
                On = true,
                OnMouseDownOnCellLeft = new Action<int>(this.OnClickCellLeft),
                OnMouseDownOnCellMiddle = new Action<int>(this.OnClickCellMiddle),
                OnMouseDownOnCellRight = new Action<int>(this.OnClickCellRight)
            };
        }
        private void OnClickCellLeft(int cellIndex)
        {
            
        }
        private void OnClickCellMiddle(int cellIndex)
        {
            ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
            clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/accept " + claims.clientDataStorage.clientPlayerInfo?.ReceivedInvitations[cellIndex].CityName, EnumChatType.Macro, "");
        }     
        private void OnClickCellRight(int cellIndex)
        {
            ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
            clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/deny " + claims.clientDataStorage.clientPlayerInfo?.ReceivedInvitations[cellIndex].CityName, EnumChatType.Macro, "");
        }
        public void BuildMainWindow()
        {
            int fixedY1 = 20;
            ElementBounds globalBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            ElementBounds backgroundBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding).WithFixedSize(Width, Height);

            ElementBounds mainBounds = ElementBounds.FixedPos(EnumDialogArea.CenterTop, 0, 15).WithFixedSize(Width, Height);

            ElementBounds leftArrowBounds = ElementBounds.FixedPos(EnumDialogArea.LeftMiddle, 0, 0).WithFixedHeight(50).WithFixedWidth(50);

            ElementBounds rightArrowBounds = ElementBounds.FixedPos(EnumDialogArea.RightMiddle, 0, 0).WithFixedHeight(50).WithFixedWidth(50);

            ElementBounds middleBounds = ElementBounds.FixedPos(EnumDialogArea.CenterMiddle, 0, 0).WithFixedHeight(Height).WithFixedWidth(Width - 100);

            ElementBounds tabNameBounds = ElementBounds.FixedPos(EnumDialogArea.CenterFixed, 0, 0).WithFixedHeight(40).WithFixedWidth(100);

            int fixedY2 = fixedY1 + 28;

            globalBounds.WithChildren(backgroundBounds);
            backgroundBounds.BothSizing = ElementSizing.Fixed;

            backgroundBounds.WithChildren(mainBounds);
            mainBounds.WithChildren(middleBounds, leftArrowBounds, rightArrowBounds);
            middleBounds.WithChildren(tabNameBounds);

            SingleComposer = Composers["canclaimsgui"] = capi.Gui.CreateCompo("canclaimsgui", globalBounds)
                                                                    .AddShadedDialogBG(backgroundBounds)
                                                                    .AddDialogTitleBar(Lang.Get("claims:gui-tab-name"), () => this.TryClose());
            ElementBounds currentBounds = mainBounds.FlatCopy().WithAlignment(EnumDialogArea.CenterTop);
            currentBounds.WithFixedSize(mainBounds.fixedWidth, 40);
            mainBounds.WithChildren(currentBounds);
            currentBounds.fixedY += 15;
            ElementBounds cityButtonBounds = currentBounds.FlatCopy().WithAlignment(EnumDialogArea.LeftTop).WithFixedSize(48, 48);
            cityButtonBounds.fixedX += 10;
            ElementBounds playerButtonBounds = cityButtonBounds.RightCopy(25);
            ElementBounds pricesButtonBounds = playerButtonBounds.RightCopy(25);
            ElementBounds plotButtonBounds = pricesButtonBounds.RightCopy(25);
            SingleComposer.AddIconToggleButtons(new string[] { "claims:qaitbay-citadel", "claims:magnifying-glass", "claims:price-tag", "claims:flat-platform" },
                                                CairoFont.ButtonText(),
                                                OnTabToggled,
                                                new ElementBounds[] { cityButtonBounds, playerButtonBounds, pricesButtonBounds, plotButtonBounds },
                                                "selectedTab");

            if (SingleComposer.GetToggleButton("selectedTab-" + (int)SelectedTab) != null)
            {
                SingleComposer.GetToggleButton("selectedTab-" + (int)SelectedTab).SetValue(true);
            }
            
            var lineBounds = currentBounds.BelowCopy(0, 20).WithFixedHeight(5);
            SingleComposer.AddInset(lineBounds);
         
            
            if (SelectedTab == EnumSelectedTab.City)
            {
                currentBounds = currentBounds.BelowCopy(0, 40);
                if (claims.clientDataStorage.clientPlayerInfo?.CityInfo != null && claims.clientDataStorage.clientPlayerInfo?.CityInfo.Name != "")
                {
                    var clientInfo = claims.clientDataStorage.clientPlayerInfo;
                    var cityTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
                    TextExtents textExtents = CairoFont.ButtonText().GetTextExtents(clientInfo.CityInfo.Name);
                    var cityNameBounds = currentBounds.FlatCopy().WithFixedWidth(textExtents.Width + 10);

                    SingleComposer.AddButton(clientInfo.CityInfo.Name, new ActionConsumable(() =>
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.SELECT_NEW_CITY_NAME;
                        BuildUpperWindow();
                        return true;
                    }), cityNameBounds, EnumButtonStyle.Normal);


                    currentBounds = currentBounds.BelowCopy(0, 10);
                    SingleComposer.AddStaticText(Lang.Get("claims:gui-mayor-name", clientInfo.CityInfo.MayorName),
                        cityTabFont,
                        currentBounds, "mayorName");

                    currentBounds = currentBounds.BelowCopy();
                    SingleComposer.AddStaticText(Lang.Get("claims:gui-date-created", TimeFunctions.getDateFromEpochSeconds(clientInfo.CityInfo.TimeStampCreated)),
                        cityTabFont,
                        currentBounds, "createdAt");

                    currentBounds = currentBounds.BelowCopy();
                    currentBounds.fixedWidth /= 2;
                    currentBounds.WithAlignment(EnumDialogArea.LeftTop);
                    SingleComposer.AddStaticText(Lang.Get("claims:gui-claimed-max-plots", clientInfo.CityInfo.CountPlots, clientInfo.CityInfo.MaxCountPlots),
                        cityTabFont,
                        currentBounds, "claimedPlotsToMax");

                    ElementBounds claimCityPlotButtonBounds = currentBounds.RightCopy();
                    claimCityPlotButtonBounds.WithFixedWidth(25).WithFixedHeight(25);
                    ElementBounds unclaimCityPlotButtonBounds = claimCityPlotButtonBounds.RightCopy();
                    SingleComposer.AddIconButton("plus", (bool t) =>
                    {
                        if (t)
                        {
                            CreateNewCityState = EnumUpperWindowSelectedState.CLAIM_CITY_PLOT_CONFIRM;
                            BuildUpperWindow();
                        }
                    }, claimCityPlotButtonBounds);

                    SingleComposer.AddIconButton("line", (bool t) =>
                    {
                        if (t)
                        {
                            CreateNewCityState = EnumUpperWindowSelectedState.UNCLAIM_CITY_PLOT_CONFIRM;
                            BuildUpperWindow();
                        }
                    }, unclaimCityPlotButtonBounds);


                    currentBounds = currentBounds.BelowCopy(0, 5);
                    
                    SingleComposer.AddStaticText(Lang.Get("claims:gui-city-population", clientInfo.CityInfo.PlayersNames.Count),
                        cityTabFont,
                        currentBounds, "population");
                    
                    SingleComposer.AddHoverText(StringFunctions.concatStringsWithDelim(clientInfo.CityInfo.PlayersNames, ','),
                                                CairoFont.ButtonText(),
                                                (int)currentBounds.fixedWidth, currentBounds);
                    //SingleComposer.AddInset(currentBounds);
                    ElementBounds addCitizenButtonBounds = currentBounds.RightCopy();
                    addCitizenButtonBounds.WithFixedWidth(25).WithFixedHeight(25);
                    ElementBounds removeCitizenButtonBounds = addCitizenButtonBounds.RightCopy();
                    ElementBounds uninviteCitizenButtonBounds = removeCitizenButtonBounds.RightCopy();
                    SingleComposer.AddIconButton("plus", (bool t) =>
                    {
                        if (t)
                        {
                            CreateNewCityState = EnumUpperWindowSelectedState.INVITE_TO_CITY_NEED_NAME;
                            BuildUpperWindow();
                        }
                    }, addCitizenButtonBounds);

                    SingleComposer.AddIconButton("line", (bool t) =>
                    {
                        if (t)
                        {
                            CreateNewCityState = EnumUpperWindowSelectedState.KICK_FROM_CITY_NEED_NAME;
                            BuildUpperWindow();
                        }
                    }, removeCitizenButtonBounds);

                    SingleComposer.AddIconButton("eraser", (bool t) =>
                    {                      
                        if (t)
                        {
                            CreateNewCityState = EnumUpperWindowSelectedState.UNINVITE_TO_CITY;
                            BuildUpperWindow();
                        }
                    }, uninviteCitizenButtonBounds);

                    currentBounds = currentBounds.BelowCopy(0, 5);

                    if (claims.clientDataStorage.clientPlayerInfo.PlayerPermissions.HasPermission(rights.EnumPlayerPermissions.CITY_SEE_BALANCE))
                    {
                        SingleComposer.AddStaticText(Lang.Get("claims:gui-city-balance", clientInfo.CityInfo.cityBalance),
                            cityTabFont,
                            currentBounds, "cityBalance");
                    }


                    /*==============================================================================================*/
                    /*=====================================UNDER 2 LINE=============================================*/
                    /*==============================================================================================*/
                    var line2Bounds = currentBounds.BelowCopy(0, 20).WithFixedHeight(5).WithFixedWidth(lineBounds.fixedWidth);
                    line2Bounds.fixedX = 0;
                    line2Bounds.fixedY = mainBounds.fixedHeight * 0.85;
                    SingleComposer.AddInset(line2Bounds);

                    ElementBounds nextIconBounds = line2Bounds.BelowCopy().WithFixedSize(48, 48).WithAlignment(EnumDialogArea.LeftTop);
                    nextIconBounds.fixedX = 0;
                    nextIconBounds.fixedY = mainBounds.fixedHeight * 0.90;


                   
                    SingleComposer.AddIconButton("claims:exit-door", new Action<bool>((b) =>
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.LEAVE_CITY_CONFIRM;
                        BuildUpperWindow();
                        return;
                    }), nextIconBounds);

                    nextIconBounds = nextIconBounds.RightCopy(20);

                    if (claims.clientDataStorage.clientPlayerInfo.PlayerPermissions.HasPermission(rights.EnumPlayerPermissions.CITY_REMOVE_RANK) ||
                        claims.clientDataStorage.clientPlayerInfo.PlayerPermissions.HasPermission(rights.EnumPlayerPermissions.CITY_SET_RANK))
                    {
                        SingleComposer.AddIconButton("claims:achievement", new Action<bool>((b) =>
                        {
                            this.SelectedTab = EnumSelectedTab.Ranks;
                            BuildMainWindow();
                            return;
                        }), nextIconBounds);
                        nextIconBounds = nextIconBounds.RightCopy(20);
                    }

                    if(claims.clientDataStorage.clientPlayerInfo.PlayerPermissions.HasPermission(rights.EnumPlayerPermissions.CITY_SET_PLOTS_COLOR))
                    {
                        SingleComposer.AddIconButton("claims:large-paint-brush", new Action<bool>((b) =>
                        {
                            this.SelectedTab = EnumSelectedTab.CityPlotsColorSelector;
                            BuildMainWindow();
                            return;
                        }), nextIconBounds);
                        nextIconBounds = nextIconBounds.RightCopy(20);
                    }
                }
                else
                {
                    //add "new city" button which leads to additional window with input field
                    //on ok send commands with name

                    ElementBounds createCityBounds = currentBounds.FlatCopy();
                    ElementBounds crownButtonBounds = currentBounds.FlatCopy();
                    //createCityBounds.
                    crownButtonBounds.fixedWidth = 48;
                    crownButtonBounds.fixedHeight = 48;
                    crownButtonBounds.Alignment = EnumDialogArea.LeftTop;
                    crownButtonBounds.fixedY += 10;
                    SingleComposer.AddIconButton("claims:queen-crown", new Action<bool>((b) =>
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.NEED_NAME;
                        if (CreateNewCityState != EnumUpperWindowSelectedState.NONE)
                        {
                            BuildUpperWindow();
                        }
                        return;
                    }), crownButtonBounds);
                    TextExtents textExtents = CairoFont.WhiteSmallText().GetTextExtents(Lang.Get("claims:gui-new-city-button"));
                    SingleComposer.AddHoverText(Lang.Get("claims:gui-new-city-button"),
                                            CairoFont.WhiteSmallText().WithOrientation(EnumTextOrientation.Center),
                                            (int)textExtents.Width, crownButtonBounds);

                    if (claims.clientDataStorage.clientPlayerInfo.ReceivedInvitations.Count > 0)
                    {
                        int renderedClaimsInfoCounter = 0;
                        int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
                        ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

                        ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 230).FixedUnder(topTextBounds, 5);
                        ElementBounds invitationTextBounds = createCityBounds.BelowCopy();
                        invitationTextBounds.fixedHeight -= 50;
                        invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
                        ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

                        ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

                        ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

                        SingleComposer.AddStaticText("Invitations" + " [" + claims.clientDataStorage.clientPlayerInfo.ReceivedInvitations.Count + "]",
                            CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                            invitationTextBounds);

                        
                        this.clippingInvitationsBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);
                        
                        SingleComposer.BeginChildElements(invitationTextBounds.BelowCopy())
                        .BeginClip(clippingBounds)
                        .AddInset(insetBounds, 3)
                        .AddCellList(this.listInvitationsBounds = this.clippingInvitationsBounds.ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0), new OnRequireCell<ClientToCityInvitation>(this.createCellElem), claims.clientDataStorage.clientPlayerInfo.ReceivedInvitations, "modstable")
                        .EndClip()
                        .AddVerticalScrollbar(OnNewScrollbarvalue, scrollbarBounds, "scrollbar")
                    //.AddSmallButton("Close", OnButtonClose, closeButtonBounds)
                    .EndChildElements()
                    
                    .Compose();
                        
                        SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingInvitationsBounds.fixedHeight, (float)this.listInvitationsBounds.fixedHeight);
                    }
                }

            }
            else if(SelectedTab == EnumSelectedTab.Player) 
            {
                var playerTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
                currentBounds = currentBounds.BelowCopy(0, 40);
                currentBounds.fixedY += 25;
                currentBounds.fixedWidth /= 2;
                currentBounds.WithAlignment(EnumDialogArea.LeftTop);

                var clientInfo = claims.clientDataStorage.clientPlayerInfo;
                SingleComposer.AddStaticText(Lang.Get("claims:gui-friends", clientInfo.Friends.Count),
                            playerTabFont,
                            currentBounds, "friends");

                SingleComposer.AddHoverText(StringFunctions.concatStringsWithDelim(clientInfo.Friends, ','),
                                            playerTabFont.WithOrientation(EnumTextOrientation.Center),
                                            (int)currentBounds.fixedWidth, currentBounds);

                ElementBounds addFriendBounds = currentBounds.RightCopy();
                addFriendBounds.WithFixedWidth(25).WithFixedHeight(25);
                ElementBounds removeFriendBounds = addFriendBounds.RightCopy();
                SingleComposer.AddIconButton("plus", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.ADD_FRIEND_NEED_NAME;
                        BuildUpperWindow();
                    }
                }, addFriendBounds);

                SingleComposer.AddIconButton("line", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.REMOVE_FRIEND;
                        BuildUpperWindow();
                    }
                }, removeFriendBounds);

            }         
            else if(SelectedTab == EnumSelectedTab.Prices)
            {
                currentBounds = currentBounds.BelowCopy(0, 40);
                var pricesTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
                currentBounds.Alignment = EnumDialogArea.LeftTop;
                currentBounds.fixedY += 15;
                string currencyStr = Lang.Get("claims:gui-currency-item");
                TextExtents textExtents = pricesTabFont.GetTextExtents(currencyStr);
                currentBounds.fixedWidth = textExtents.Width;
                SingleComposer.AddStaticText(currencyStr,
                       pricesTabFont,
                       currentBounds, "currency-itme");

                ElementBounds cityPriceBounds = currentBounds.RightCopy();
                cityPriceBounds.fixedY -= 10;
                claims.config.COINS_VALUES_TO_CODE.TryGetValue(1, out string coin_code);
                if (coin_code != null)
                {
                    ItemStack coin = new ItemStack(capi.World.GetItem(new AssetLocation(coin_code)), 1);
                    ItemstackTextComponent currencyStack = new ItemstackTextComponent(capi, coin, 48);
                    //SlideshowItemstackTextComponent sitc = new SlideshowItemstackTextComponent(capi, new ItemStack[] { coin }, 48, EnumFloat.Inline);
                    SingleComposer.AddRichtext(new RichTextComponentBase[] { currencyStack }, cityPriceBounds, "coin-item");
                }
                currentBounds = currentBounds.BelowCopy(0, 0);
                SingleComposer.AddStaticText(Lang.Get("claims:gui-new-city-cost", claims.config.NEW_CITY_COST.ToString()),
                        pricesTabFont,
                        currentBounds, "new-city-price");

                currentBounds = currentBounds.BelowCopy(0, 0);
                string cityPlotCost = Lang.Get("claims:gui-city-plot-cost", claims.config.PLOT_CLAIM_PRICE.ToString());
                currentBounds.fixedWidth = pricesTabFont.GetTextExtents(cityPlotCost).Width;
                SingleComposer.AddStaticText(cityPlotCost,
                        pricesTabFont,
                        currentBounds, "plot-claim-price");

                currentBounds = currentBounds.BelowCopy(0, 0);
                string cityNameChangeCost = Lang.Get("claims:gui-city-name-change-cost", claims.config.CITY_NAME_CHANGE_COST.ToString());
                currentBounds.fixedWidth = pricesTabFont.GetTextExtents(cityNameChangeCost).Width;
                SingleComposer.AddStaticText(cityNameChangeCost,
                        pricesTabFont,
                        currentBounds, "city-name-price");

                currentBounds = currentBounds.BelowCopy(0, 0);
                SingleComposer.AddStaticText(Lang.Get("claims:gui-city-base-cost", claims.config.CITY_BASE_CARE.ToString()),
                        pricesTabFont,
                        currentBounds, "city-base-care");



            }
            else if (SelectedTab == EnumSelectedTab.Plot) 
            {
                currentBounds = currentBounds.BelowCopy(0, 40);
                var plotTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
                var clientInfo = claims.clientDataStorage.clientPlayerInfo;
                currentBounds.fixedWidth /= 2;
                currentBounds.Alignment = EnumDialogArea.LeftTop;
                SingleComposer.AddStaticText("[" + clientInfo.CurrentPlotInfo.PlotPosition.X + "/" + clientInfo.CurrentPlotInfo.PlotPosition.Y + "]",
                    plotTabFont,
                    currentBounds, "plotPos");
                //SingleComposer.AddInset(currentBounds);
                ElementBounds refreshPlotButtonBounds = currentBounds.RightCopy();
                refreshPlotButtonBounds.WithFixedSize(35, 35);
                SingleComposer.AddIconButton("redo", (bool t) =>
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
                SingleComposer.AddStaticText(Lang.Get("claims:gui-plot-name", clientInfo.CurrentPlotInfo.PlotName),
                    plotTabFont,
                    currentBounds, "plotName");

                ElementBounds setPlotNameButtonBounds = currentBounds.RightCopy();
                setPlotNameButtonBounds.WithFixedSize(25, 25);
                SingleComposer.AddIconButton("hat", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.PLOT_SET_NAME;
                        
                        BuildUpperWindow();
                    }
                    else
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                        BuildUpperWindow();
                    }
                }, setPlotNameButtonBounds, "setPlotName");
                SingleComposer.GetToggleButton("setPlotName").Toggleable = true;
                SingleComposer.AddHoverText("Set plot name", plotTabFont, 180, setPlotNameButtonBounds);


                currentBounds = currentBounds.BelowCopy(0, 5);
                SingleComposer.AddStaticText(Lang.Get("claims:gui-owner-name", clientInfo.CurrentPlotInfo.OwnerName),
                    plotTabFont,
                    currentBounds, "ownerName");

                if(clientInfo.CurrentPlotInfo.Price > -1)
                {
                    ElementBounds buyPlotButtonBounds = currentBounds.RightCopy();
                    buyPlotButtonBounds.WithFixedSize(25, 25);
                    SingleComposer.AddIconButton("medal", (bool t) =>
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.PLOT_CLAIM;
                        BuildUpperWindow();
                    }, buyPlotButtonBounds);
                    SingleComposer.AddHoverText("Buy plot", plotTabFont, 180, buyPlotButtonBounds);
                    
                }
                if(clientInfo.CurrentPlotInfo.OwnerName?.Length > 0)
                {
                    ElementBounds sellPlotButtonBounds = currentBounds.RightCopy();
                    sellPlotButtonBounds.WithFixedSize(25, 25);
                    SingleComposer.AddIconButton("medal", (bool t) =>
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.PLOT_UNCLAIM;
                        BuildUpperWindow();
                    }, sellPlotButtonBounds);
                    SingleComposer.AddHoverText("Unclaim plot", plotTabFont, 180, sellPlotButtonBounds);
                }

                /*==============================================================================================*/
                /*=====================================PLOT TYPE================================================*/
                /*==============================================================================================*/
                currentBounds = currentBounds.BelowCopy(0, 5);

                SingleComposer.AddStaticText(Lang.Get("claims:gui-plot-type",
                    (PlotInfo.dictPlotTypes.TryGetValue(clientInfo.CurrentPlotInfo.PlotType, out PlotInfo plotInfo) ? plotInfo.getFullName() : "-")),
                    plotTabFont,
                    currentBounds, "plotType");

                ElementBounds setPlotTypeButtonBounds = currentBounds.RightCopy();
                setPlotTypeButtonBounds.WithFixedSize(25, 25);
                SingleComposer.AddIconButton("hat", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.PLOT_SET_TYPE;
                        BuildUpperWindow();
                    }
                    else
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                        BuildUpperWindow();
                    }
                }, setPlotTypeButtonBounds, "setPlotType");
                SingleComposer.GetToggleButton("setPlotType").Toggleable = true;
                SingleComposer.AddHoverText("Set plot type", plotTabFont, 180, setPlotTypeButtonBounds);


                /*==============================================================================================*/
                /*=====================================PLOT TAX=================================================*/
                /*==============================================================================================*/
                currentBounds = currentBounds.BelowCopy(0, 5);
                SingleComposer.AddStaticText(Lang.Get("claims:gui-plot-custom-tax", clientInfo.CurrentPlotInfo.CustomTax),
                    plotTabFont,
                    currentBounds, "customTax");

                ElementBounds setPlotTaxButtonBounds = currentBounds.RightCopy();
                setPlotTaxButtonBounds.WithFixedSize(25, 25);
                SingleComposer.AddIconButton("medal", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.PLOT_SET_TAX;
                        BuildUpperWindow();
                    }
                    else
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                        BuildUpperWindow();
                    }
                }, setPlotTaxButtonBounds, "setPlotTax");
                SingleComposer.GetToggleButton("setPlotTax").Toggleable = true;

                /*==============================================================================================*/
                /*=====================================PLOT PRICES==============================================*/
                /*==============================================================================================*/
                currentBounds = currentBounds.BelowCopy(0, 5);
                SingleComposer.AddStaticText(Lang.Get("claims:gui-plot-price", clientInfo.CurrentPlotInfo.Price > -1
                                                     ? clientInfo.CurrentPlotInfo.Price
                                                     : Lang.Get("claims:gui-not-for-sale")),
                    plotTabFont,
                    currentBounds, "plotPrice");

                ElementBounds setPlotPriceButtonBounds = currentBounds.RightCopy();
                setPlotPriceButtonBounds.WithFixedSize(25, 25);
                SingleComposer.AddIconButton("medal", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.PLOT_SET_PRICE_NEED_NUMBER;
                        BuildUpperWindow();
                    }
                    else
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                        BuildUpperWindow();
                    }
                }, setPlotPriceButtonBounds, "setPlotPrice");
                SingleComposer.AddHoverText("Set plot for sale", plotTabFont, 180, setPlotPriceButtonBounds);
                SingleComposer.GetToggleButton("setPlotPrice").Toggleable = true;

                ElementBounds setPlotNotForSaleButtonBounds = setPlotPriceButtonBounds.RightCopy();
                setPlotNotForSaleButtonBounds.WithFixedSize(25, 25);
                SingleComposer.AddIconButton("line", (bool t) =>
                {
                    if (t)
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot nfs", EnumChatType.Macro, "");
                    }
                }, setPlotNotForSaleButtonBounds);
                SingleComposer.AddHoverText("Set plot NOT for sale", plotTabFont, 180, setPlotNotForSaleButtonBounds);


                /*==============================================================================================*/
                /*=====================================PLOT PERMISSIONS=========================================*/
                /*==============================================================================================*/
                currentBounds = currentBounds.BelowCopy(0, 5);
                SingleComposer.AddStaticText(Lang.Get("claims:gui-plot-permissions"),
                    plotTabFont,
                    currentBounds, "permissionHandler");


                ElementBounds showPermissionsButtonBounds = currentBounds.RightCopy();
                showPermissionsButtonBounds.WithFixedSize(25, 25);
                SingleComposer.AddIconButton("medal", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.PLOT_PERMISSIONS;
                        BuildUpperWindow();
                    }
                    else
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                        BuildUpperWindow();
                    }
                }, showPermissionsButtonBounds, "setPermissions");
                SingleComposer.GetToggleButton("setPermissions").Toggleable = true;

                /*==============================================================================================*/
                /*=====================================PLOT SHOW BORDERS========================================*/
                /*==============================================================================================*/
                currentBounds = currentBounds.BelowCopy(0, 5);
                SingleComposer.AddStaticText(Lang.Get("claims:gui-plot-borders"),
                    plotTabFont,
                    currentBounds, "plotborders");


                ElementBounds showPlotBordersButtonBounds = currentBounds.RightCopy();
                showPlotBordersButtonBounds.WithFixedSize(25, 25);
                SingleComposer.AddIconButton("select", (bool t) =>
                {
                    //if (t)
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot borders " + "on", EnumChatType.Macro, "");
                    }
                }, showPlotBordersButtonBounds);

                ElementBounds hidePlotBordersButtonBounds = showPlotBordersButtonBounds.RightCopy();
                SingleComposer.AddIconButton("eraser", (bool t) =>
                {
                    //if (t)
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot borders " + "off", EnumChatType.Macro, "");
                    }
                }, hidePlotBordersButtonBounds);

            }
            else if(SelectedTab == EnumSelectedTab.Ranks)
            {
                currentBounds = currentBounds.BelowCopy(0, 20);
                ElementBounds createCityBounds = currentBounds.FlatCopy();
                int renderedClaimsInfoCounter = 0;
                int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
                ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

                ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 230).FixedUnder(topTextBounds, 5);
                ElementBounds invitationTextBounds = createCityBounds.BelowCopy();
                invitationTextBounds.fixedHeight -= 50;
                invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
                ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

                ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

                ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

                SingleComposer.AddStaticText("Ranks",
                    CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                    invitationTextBounds);

                this.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);
                //claims.clientDataStorage.clientPlayerInfo.CityInfo.CitizensRanks.Clear();
                foreach (var it in claims.clientDataStorage.clientPlayerInfo.CityInfo.PossibleCityRanks)
                {
                    if(claims.clientDataStorage.clientPlayerInfo.CityInfo.CitizensRanks.All(cell => cell.RankName != it.ToLower()))
                    {
                        claims.clientDataStorage.clientPlayerInfo.CityInfo.CitizensRanks.Add(new RankCellElement(it.ToLower(), new List<string> { }));
                    }
                }

                SingleComposer.BeginChildElements(invitationTextBounds.BelowCopy())
                .BeginClip(clippingBounds)
                .AddInset(insetBounds, 3)
                .AddCellList(this.listRanksBounds = this.clippingRansksBounds.ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0), new OnRequireCell<RankCellElement>(this.createRankCellElem), claims.clientDataStorage.clientPlayerInfo.CityInfo.CitizensRanks, "citizensranks")
                .EndClip()
                .AddVerticalScrollbar(OnNewRanksScrollbarvalue, scrollbarBounds, "scrollbar")
            //.AddSmallButton("Close", OnButtonClose, closeButtonBounds)
            .EndChildElements();
                var c = SingleComposer.GetCellList<RankCellElement>("citizensranks");
                var f = SingleComposer.GetElement("citizensranks");
                c.BeforeCalcBounds();
                if (c.elementCells.Count() > 0)
                {
                    //c.tm[0].UpdateCellHeight();
                }
                SingleComposer
            .Compose();

                SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingRansksBounds.fixedHeight, (float)this.listRanksBounds.fixedHeight);
            }
            else if (SelectedTab == EnumSelectedTab.CityPlotsColorSelector)
            {
                currentBounds = currentBounds.BelowCopy(0, 40);
                var colorSelectTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
                TextExtents textExtents = colorSelectTabFont.GetTextExtents("Plots color: ");
                currentBounds.fixedWidth = textExtents.Width;
                currentBounds.Alignment = EnumDialogArea.LeftTop;
                SingleComposer.AddStaticText("Plots color: ",
                                               colorSelectTabFont, currentBounds);

                ElementBounds bounds = currentBounds.RightCopy().WithFixedSize(24, 24);

                SingleComposer.AddColorListPicker(new int[] { claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsColor }
                                                                , OnColorPicked,
                                                               bounds, 100);

                currentBounds = currentBounds.BelowCopy();

                SingleComposer.AddStaticText("Select color: ",
                                              colorSelectTabFont, currentBounds);

                ElementBounds selectColorBounds = currentBounds.FlatCopy();

                SingleComposer.AddColorListPicker(claims.config.PLOT_COLORS == null ? new int[] { 0, 8888888} : claims.config.PLOT_COLORS 
                                                               , OnColorPicked,
                                                              bounds = selectColorBounds.BelowCopy(0.0, 0.0, 0.0, 0.0).WithFixedSize((double)25, (double)25), 200, "picker-2");

                ElementBounds colorSelectedButton = bounds.BelowCopy().WithFixedSize(48, 48);
                colorSelectedButton.fixedX = 0;
                colorSelectedButton.fixedY += 20;
                SingleComposer.AddButton("Select", new ActionConsumable(() =>
                {
                    if(selectedColor == -1)
                    {
                        return true;
                    }
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set colorint " + selectedColor, EnumChatType.Macro, "");
                    selectedColor = -1;
                    return true;
                }),
                                 colorSelectedButton);
            }
            Composers["canclaimsgui"].Compose();
            BuildUpperWindow();
            //SingleComposer.ReCompose();
        }

        public void OnColorPicked(int index)
        {
            selectedColor = Settings.colors[index];
        }
        public void BuildUpperWindow()
        {
            if (!this.IsOpened())
            {
                return;
            }
            if(CreateNewCityState == EnumUpperWindowSelectedState.NONE)
            {
                this.Composers.Remove("canclaimsgui-upper");
                return;
            }
            ElementBounds leftDlgBounds = Composers["canclaimsgui"].Bounds;
            //Composers["canclaimsgui"].Bounds.ParentBounds
            double b = leftDlgBounds.InnerHeight / RuntimeEnv.GUIScale + 10.0;

            ElementBounds bgBounds = ElementBounds.Fixed(0.0, 0.0,
                235, leftDlgBounds.InnerHeight / (double)RuntimeEnv.GUIScale - GuiStyle.ElementToDialogPadding - 20.0 + b).WithFixedPadding(GuiStyle.ElementToDialogPadding);
            ElementBounds dialogBounds = bgBounds.ForkBoundingParent(0.0, 0.0, 0.0, 0.0)
                .WithAlignment(EnumDialogArea.None)
                .WithFixedAlignmentOffset((leftDlgBounds.renderX + leftDlgBounds.OuterWidth + 10.0) / (double)RuntimeEnv.GUIScale,
                                          (leftDlgBounds.renderY) / (double)RuntimeEnv.GUIScale);
            
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            dialogBounds.fixedX += leftDlgBounds.fixedWidth + 20;
            dialogBounds.fixedY = leftDlgBounds.absFixedY;

            dialogBounds.BothSizing = ElementSizing.FitToChildren;
            dialogBounds.WithChild(bgBounds);
            ElementBounds textBounds = ElementBounds.FixedPos(EnumDialogArea.LeftTop,
                                                               0,
                                                                0);
            bgBounds.WithChildren(textBounds);

            Composers["canclaimsgui-upper"] = capi.Gui.CreateCompo("canclaimsgui-upper", dialogBounds)
                                                        .AddShadedDialogBG(bgBounds, false, 5.0, 0.75f);
            ElementBounds el = textBounds.CopyOffsetedSibling()
                                        .WithFixedHeight(30)
                                        .WithFixedWidth(180);
            bgBounds.WithChildren(el);


            if (CreateNewCityState == EnumUpperWindowSelectedState.NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Enter new city name:",
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewCityName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("-->", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city new " + collectedNewCityName, EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewCityName").SetValue("");
                    collectedNewCityName = "";
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal, "create-new-city-button-enter-name");
            }
            else if(CreateNewCityState == EnumUpperWindowSelectedState.NEED_AGREE)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-agree-city-creation", collectedNewCityName),
                CairoFont.WhiteDetailText(),
                el);

                ElementBounds enterNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("agree", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/agree", EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    //Composers["canclaimsgui-upper"].GetTextInput("collectedNewCityName").SetValue("");
                    collectedNewCityName = "";
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal, "create-new-city-button-enter-name");
            }
            else if(CreateNewCityState == EnumUpperWindowSelectedState.INVITE_TO_CITY_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Enter player's name:",
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewCityName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("Invite", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city invite " + collectedNewCityName, EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewCityName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if(CreateNewCityState == EnumUpperWindowSelectedState.KICK_FROM_CITY_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Enter player's name:",
               CairoFont.WhiteDetailText(),
               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                
                Composers["canclaimsgui-upper"].AddDropDown(claims.clientDataStorage.clientPlayerInfo.CityInfo.PlayersNames.ToArray(),
                                                            claims.clientDataStorage.clientPlayerInfo.CityInfo.PlayersNames.ToArray(),  
                                                            -1,
                                                            OnSelectedNameFromDropDown,
                                                            inputNameBounds);

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("Kick", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city kick " + collectedNewCityName, EnumChatType.Macro, "");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.UNINVITE_TO_CITY)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Enter player's name:",
                 CairoFont.WhiteDetailText(),
                 el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewCityName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("Uninvite", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city uninvite " + collectedNewCityName, EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewCityName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CLAIM_CITY_PLOT_CONFIRM)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Claim current plot?",
                 CairoFont.WhiteDetailText(),
                 el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton("Yes", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city claim", EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton("No", new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);

            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.UNCLAIM_CITY_PLOT_CONFIRM)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Unclaim current plot?",
                 CairoFont.WhiteDetailText(),
                 el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton("Yes", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city unclaim", EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton("No", new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.PLOT_PERMISSIONS)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Plot permissions",
                 CairoFont.WhiteDetailText(),
                 el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);


                ElementBounds pvpToggleTextBounds = el.BelowCopy(0, 15);
                pvpToggleTextBounds.fixedWidth = 80;
                Composers["canclaimsgui-upper"].AddStaticText("PVP", CairoFont.WhiteDetailText(), pvpToggleTextBounds);

                ElementBounds pvpToggleButtonBounds = pvpToggleTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) => 
                                    {
                                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set pvp " + (t ? "on" : "off"), EnumChatType.Macro, "");
                                    },
                                                pvpToggleButtonBounds,
                                                "pvp-switch");
                Composers["canclaimsgui-upper"].GetSwitch("pvp-switch").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.pvpFlag);
                bgBounds.WithChildren(pvpToggleTextBounds);


                ElementBounds fireToggleTextBounds = pvpToggleTextBounds.BelowCopy(0, 15);
                fireToggleTextBounds.fixedWidth = 80;
                Composers["canclaimsgui-upper"].AddStaticText("Fire", CairoFont.WhiteDetailText(), fireToggleTextBounds);

                ElementBounds fireToggleButtonBounds = fireToggleTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) => 
                                {
                                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set fire " + (t ? "on" : "off"), EnumChatType.Macro, "");
                                },
                                                fireToggleButtonBounds,
                                                "fire-switch");
                Composers["canclaimsgui-upper"].GetSwitch("fire-switch").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.fireFlag);
                bgBounds.WithChildren(fireToggleTextBounds);

                ElementBounds blastToggleTextBounds = fireToggleTextBounds.BelowCopy(0, 15);
                blastToggleTextBounds.fixedWidth = 80;
                Composers["canclaimsgui-upper"].AddStaticText("Blast", CairoFont.WhiteDetailText(), blastToggleTextBounds);

                ElementBounds blastToggleButtonBounds = blastToggleTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) => 
                                 {
                                     ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                                     clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set blast " + (!t ? "on" : "off"), EnumChatType.Macro, "");
                                 },
                                                blastToggleButtonBounds,
                                                "blast-switch");
                Composers["canclaimsgui-upper"].GetSwitch("blast-switch").SetValue(!claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.blastFlag);
                bgBounds.WithChildren(blastToggleButtonBounds);


                /////BUILD SWITCHES
                ElementBounds buildTextBounds = blastToggleTextBounds.BelowCopy(0, 15);
                Composers["canclaimsgui-upper"].AddStaticText("Build", CairoFont.WhiteDetailText(), buildTextBounds);
                bgBounds.WithChildren(buildTextBounds);

                ElementBounds friendBuildBounds = buildTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set p friend build " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, friendBuildBounds, "friend-build");
                
                Composers["canclaimsgui-upper"].GetSwitch("friend-build").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.getPerm(perms.PermGroup.COMRADE, perms.type.PermType.BUILD_AND_DESTROY_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("friend", CairoFont.WhiteDetailText(), 60, friendBuildBounds);



                ElementBounds citizenBuildBounds = friendBuildBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set p citizen build " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, citizenBuildBounds, "citizen-build");
                Composers["canclaimsgui-upper"].GetSwitch("citizen-build").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.getPerm(perms.PermGroup.CITIZEN, perms.type.PermType.BUILD_AND_DESTROY_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("citizen", CairoFont.WhiteDetailText(), 60, citizenBuildBounds);
                ElementBounds strangerBuildBounds = citizenBuildBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set p stranger build " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, strangerBuildBounds, "stranger-build");
                Composers["canclaimsgui-upper"].GetSwitch("stranger-build").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.getPerm(perms.PermGroup.STRANGER, perms.type.PermType.BUILD_AND_DESTROY_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("stranger", CairoFont.WhiteDetailText(), 60, strangerBuildBounds);

                bgBounds.WithChildren(blastToggleButtonBounds);

                ///USE SWITCHES
                ///

                ElementBounds useTextBounds = buildTextBounds.BelowCopy(0, 15);
                Composers["canclaimsgui-upper"].AddStaticText("Use", CairoFont.WhiteDetailText(), useTextBounds);
                bgBounds.WithChildren(useTextBounds);

                ElementBounds friendUseBounds = useTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set p friend use " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, friendUseBounds, "friend-use");
                
                Composers["canclaimsgui-upper"].GetSwitch("friend-use").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.getPerm(perms.PermGroup.COMRADE, perms.type.PermType.USE_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("friend", CairoFont.WhiteDetailText(), 60, friendUseBounds);



                ElementBounds citizenUseBounds = friendUseBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set p citizen use " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, citizenUseBounds, "citizen-use");
                Composers["canclaimsgui-upper"].GetSwitch("citizen-use").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.getPerm(perms.PermGroup.CITIZEN, perms.type.PermType.USE_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("citizen", CairoFont.WhiteDetailText(), 60, citizenUseBounds);
                ElementBounds strangerUseBounds = citizenUseBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set p stranger use " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, strangerUseBounds, "stranger-use");
                Composers["canclaimsgui-upper"].GetSwitch("stranger-use").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.getPerm(perms.PermGroup.STRANGER, perms.type.PermType.USE_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("stranger", CairoFont.WhiteDetailText(), 60, strangerUseBounds);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.ADD_FRIEND_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Enter player's name:",
               CairoFont.WhiteDetailText(),
               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedFriendName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("Add", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/citizen friend add " + collectedNewCityName, EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedFriendName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.REMOVE_FRIEND)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Enter player's name:",
                                                               CairoFont.WhiteDetailText(),
                                                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);

                Composers["canclaimsgui-upper"].AddDropDown(claims.clientDataStorage.clientPlayerInfo.Friends.ToArray(),
                                                            claims.clientDataStorage.clientPlayerInfo.Friends.ToArray(),
                                                            -1,
                                                            OnSelectedNameFromDropDown,
                                                            inputNameBounds);

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("Remove", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/citizen friend remove " + collectedNewCityName, EnumChatType.Macro, "");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.PLOT_SET_PRICE_NEED_NUMBER)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Enter plot's price:",
               CairoFont.WhiteDetailText(),
               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddNumberInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedFriendName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("Set price", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot fs " + collectedNewCityName, EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedFriendName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.PLOT_SET_TAX)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Enter plot's tax:",
              CairoFont.WhiteDetailText(),
              el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddNumberInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedFriendName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("Set tax", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set fee " + collectedNewCityName, EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedFriendName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.PLOT_SET_TYPE)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Select plot's type:",
                                                               CairoFont.WhiteDetailText(),
                                                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);

                string[] plotNames = PlotInfo.plotAccessableForPlayersWithCode.Values.Select(ele => Lang.Get(ele)).ToArray();
                Composers["canclaimsgui-upper"].AddDropDown(PlotInfo.plotAccessableForPlayersWithCode.Keys.ToArray(),
                                                            plotNames,
                                                            -1,
                                                            OnSelectedNameFromDropDown,
                                                            inputNameBounds);

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("Set type", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set type " + collectedNewCityName, EnumChatType.Macro, "");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.PLOT_SET_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Enter name for the plot:",
                  CairoFont.WhiteDetailText(),
                  el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedFriendName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("Set name", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set name " + collectedNewCityName, EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedFriendName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.PLOT_CLAIM)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Claim current plot?",
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton("Yes", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot claim", EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton("No", new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.PLOT_UNCLAIM)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Unclaim current plot?",
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton("Yes", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot unclaim", EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton("No", new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.LEAVE_CITY_CONFIRM)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Leave city?",
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton("Yes", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city leave", EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton("No", new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_RANK_REMOVE_CONFIRM)
            {
                el.fixedWidth += 40;
                Composers["canclaimsgui-upper"].AddStaticText("Strip player " + secondValueCollected + " of " + firstValueCollected  + " rank?",
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton("Yes", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city rank remove " + firstValueCollected + " " +  secondValueCollected, EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.CitizensRanks.FirstOrDefault(c => c.RankName ==  firstValueCollected);
                    if(cell != null)
                    {
                        cell.CitizensRanks.Remove(secondValueCollected);
                        BuildMainWindow();
                    }
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton("No", new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_RANK_ADD)
            {
                el.fixedWidth += 40;
                Composers["canclaimsgui-upper"].AddStaticText("Add rank " + firstValueCollected,
                CairoFont.WhiteDetailText(),
                el);
              

                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);

                Composers["canclaimsgui-upper"].AddDropDown(claims.clientDataStorage.clientPlayerInfo.CityInfo.PlayersNames.ToArray(),
                                                           claims.clientDataStorage.clientPlayerInfo.CityInfo.PlayersNames.ToArray(),
                                                           -1,
                                                           OnSelectedNameFromDropDown,
                                                           inputNameBounds);

                ElementBounds yesButtonBounds = inputNameBounds.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton("Add", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city rank add " + firstValueCollected + " " + collectedNewCityName, EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton("Close", new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.SELECT_NEW_CITY_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Enter new name for the city:",
                  CairoFont.WhiteDetailText(),
                  el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedFriendName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton("Set name", new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set name " + collectedNewCityName, EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedFriendName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            Composers["canclaimsgui-upper"].Compose();
        }

        public void OnTabToggled(int tabIndex)
        {
            SelectedTab = (EnumSelectedTab)tabIndex;
            BuildMainWindow();
        }

        public void OnSelectedNameFromDropDown(string code, bool selected)
        {
            collectedNewCityName = code;
        }
    }
}
