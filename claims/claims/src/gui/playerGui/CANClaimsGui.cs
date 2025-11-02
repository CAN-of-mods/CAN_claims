using System;
using System.Collections.Generic;
using System.Linq;
using Cairo;
using claims.src.auxialiry;
using claims.src.gui.playerGui.GuiElements;
using claims.src.gui.playerGui.structures;
using claims.src.gui.playerGui.structures.cellElements;
using claims.src.network.packets;
using claims.src.part.structure;
using claims.src.part.structure.conflict;
using claims.src.part.structure.plots;
using claims.src.rights;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace claims.src.gui.playerGui
{
    public class CANClaimsGui : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "CANClaimsGui";
        public float Width { get; private set; }
        public float Height { get; private set; }
        public EnumSelectedTab SelectedTab { get; set; }
        public enum EnumUpperWindowSelectedState
        {
            NONE, NEED_NAME, NEED_AGREE, INVITE_TO_CITY_NEED_NAME, KICK_FROM_CITY_NEED_NAME, UNINVITE_TO_CITY,
            CLAIM_CITY_PLOT_CONFIRM, UNCLAIM_CITY_PLOT_CONFIRM, PLOT_PERMISSIONS, ADD_FRIEND_NEED_NAME, REMOVE_FRIEND,
            PLOT_SET_PRICE_NEED_NUMBER, PLOT_SET_TAX, PLOT_SET_TYPE, PLOT_SET_NAME, PLOT_CLAIM, PLOT_UNCLAIM,
            CITY_TITLE_SELECT_CITIZEN, CITY_TITLE_CITIZEN_SELECTED, LEAVE_CITY_CONFIRM,
            CITY_RANK_REMOVE_CONFIRM, CITY_RANK_ADD, SELECT_NEW_CITY_NAME, ADD_CRIMINAL_NEED_NAME, REMOVE_CRIMINAL,
            CITY_PRISON_REMOVE_CELL_CONFIRM, CITY_SUMMON_NEED_NAME, ADD_PLOTSGROUP_MEMBER_NEED_NAME, REMOVE_PLOTSGROUP_MEMBER_SELECT,
            REMOVE_PLOTSGROUP_MEMBER_CONFIRM,
            CITY_PLOTSGROUP_PERMISSIONS, CITY_PLOTSGROUP_PLOT_CLAIM_CONFIRM, CITY_PLOTSGROUP_PLOT_UNCLAIM_CONFIRM,
            CITY_PLOTSGROUP_ADD_NEW_NEED_NAME, CITY_PLOTSGROUP_REMOVE_SELECT, CITY_PLOTSGROUP_REMOVE_CONFIRM,

            SELECT_NEW_ALLIANCE_NAME, INVITE_TO_ALLIANCE_NEED_NAME, KICK_FROM_ALLIANCE_NEED_NAME, UNINVITE_TO_ALLIANCE,
            LEAVE_ALLIANCE_CONFIRM, NEW_ALLIANCE_NEED_NAME, ALLIANCE_PREFIX_NEED_NAME, ALLIANCE_SEND_NEW_CONFLICT_LETTER_NEED_NAME,
            ALLIANCE_SEND_PEACE_OFFER_CONFIRM,
            CITY_PLOTS_PERMISSIONS, CITY_RANK_DELETE_CONFIRM, CITY_RANK_CREATION_NEED_NAME
        }
        public EnumUpperWindowSelectedState CreateNewCityState { get; set; } = EnumUpperWindowSelectedState.NONE;
        public string collectedNewCityName { get; set; } = "";
        public string firstValueCollected { get; set; } = "";
        public string secondValueCollected { get; set; } = "";
        public Vec3i selectedPos { get; set; } = null;
        public string selectedString { get; set; } = "";
        public string secondSelectedString { get; set; } = "";
        public enum EnumSelectedTab
        {
            City, Player, Prices, Plot, Prison, Summon, PlotsGroup, PlotsGroupReceivedInvites, Ranks, RankInfoPage, CityPlotsColorSelector, PlotsGroupInfoPage, 
            AllianceInfoPage, CitiesListPage, ConflictLettersPage, ConflictsPage, ConflictInfoPage
        }
        private Dictionary<EnumSelectedTab, Action<ElementBounds, ElementBounds>> TabDictionary = new Dictionary<EnumSelectedTab, Action<ElementBounds, ElementBounds>>();
        public int selectedClaimsPage = 0;
        public int claimsPerPage = 3;
        private int selectedColor = -1;
        private int SelectedTabGroup = 0;
        public enum EnumSelectedWarRangesTab
        {
            APPROVED, SUGGESTIONS
        }

        private ElementBounds clippingInvitationsBounds;
        private ElementBounds listInvitationsBounds;

        private ElementBounds clippingRansksBounds;
        private ElementBounds listRanksBounds;
        ElementBounds mainBounds;


        public CANClaimsGui(ICoreClientAPI capi) : base(capi)
        {
            Width = 500;
            Height = 600;
            SelectedTab = 0;
            TabDictionary.Add(EnumSelectedTab.PlotsGroupInfoPage, BuildPlotsGroupInfoPage);
            TabDictionary.Add(EnumSelectedTab.PlotsGroup, BuildPlotsGroupPage);
            TabDictionary.Add(EnumSelectedTab.Summon, BuildSummonPage);
            TabDictionary.Add(EnumSelectedTab.Prison, BuildPrisonPage);
            TabDictionary.Add(EnumSelectedTab.CityPlotsColorSelector, BuildPlotColorSelectorPage);
            TabDictionary.Add(EnumSelectedTab.Ranks, BuildRanksPage);
            TabDictionary.Add(EnumSelectedTab.RankInfoPage, BuildRanksInfoPage);
            TabDictionary.Add(EnumSelectedTab.Plot, BuildPlotPage);
            TabDictionary.Add(EnumSelectedTab.Prices, BuildPricesPage);
            TabDictionary.Add(EnumSelectedTab.Player, BuildPlayerPage);
            TabDictionary.Add(EnumSelectedTab.City, BuildCityPage);
            TabDictionary.Add(EnumSelectedTab.PlotsGroupReceivedInvites, BuildPlotsGroupReceivedInvitesPage);
            TabDictionary.Add(EnumSelectedTab.AllianceInfoPage, BuildAlliancePage);
            TabDictionary.Add(EnumSelectedTab.CitiesListPage, BuildCitiesListPage);
            TabDictionary.Add(EnumSelectedTab.ConflictLettersPage, BuildConflictLettersPage);
            TabDictionary.Add(EnumSelectedTab.ConflictsPage, BuildConflictsPage);
            TabDictionary.Add(EnumSelectedTab.ConflictInfoPage, BuildConflictInfoPage);
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

            mainBounds = ElementBounds.FixedPos(EnumDialogArea.CenterTop, 0, 15).WithFixedSize(Width, Height);

            ElementBounds leftArrowBounds = ElementBounds.FixedPos(EnumDialogArea.LeftMiddle, 0, 0).WithFixedHeight(50).WithFixedWidth(50);

            ElementBounds rightArrowBounds = ElementBounds.FixedPos(EnumDialogArea.RightMiddle, 0, 0).WithFixedHeight(50).WithFixedWidth(50);

            ElementBounds middleBounds = ElementBounds.FixedPos(EnumDialogArea.CenterMiddle, 0, 0).WithFixedHeight(Height).WithFixedWidth(Width - 100);

            ElementBounds tabNameBounds = ElementBounds.FixedPos(EnumDialogArea.CenterFixed, 0, 0).WithFixedHeight(40).WithFixedWidth(100);

            int fixedY2 = fixedY1 + 28;

            globalBounds.WithChildren(backgroundBounds);
            backgroundBounds.BothSizing = ElementSizing.Fixed;

            backgroundBounds.WithChildren(mainBounds);
            mainBounds.WithChildren(middleBounds, leftArrowBounds, rightArrowBounds);
            middleBounds.WithChildren(leftArrowBounds);
            
            SingleComposer = Composers["canclaimsgui"] = capi.Gui.CreateCompo("canclaimsgui", globalBounds)
                                                                    .AddShadedDialogBG(backgroundBounds)
                                                                    .AddDialogTitleBar(Lang.Get("claims:gui-tab-name"), () => this.TryClose());
            ElementBounds currentBounds = mainBounds.FlatCopy().WithAlignment(EnumDialogArea.CenterTop);
            currentBounds.WithFixedSize(mainBounds.fixedWidth, 40);
            
            mainBounds.WithChildren(currentBounds);
            currentBounds.fixedY += 15;
            ElementBounds cityButtonBounds = currentBounds.FlatCopy().WithAlignment(EnumDialogArea.LeftTop).WithFixedSize(48, 48);
            cityButtonBounds.fixedX += 5;
            ElementBounds playerButtonBounds = cityButtonBounds.RightCopy(25);
            ElementBounds pricesButtonBounds = playerButtonBounds.RightCopy(25);
            ElementBounds plotButtonBounds = pricesButtonBounds.RightCopy(25);
            ElementBounds prisonButtonBounds = plotButtonBounds.RightCopy(25);
            ElementBounds summonButtonBounds = prisonButtonBounds.RightCopy(25);
            ElementBounds plotsGroupButtonBounds = summonButtonBounds.RightCopy(25);
            SingleComposer.AddIconToggleButtons(new string[] { "claims:qaitbay-citadel", "claims:magnifying-glass",
                                                               "claims:price-tag", "claims:flat-platform",
                                                               "claims:prisoner", "claims:magic-portal",
                                                               "claims:huts-village"        },
                                                CairoFont.ButtonText(),
                                                OnTabToggled,
                                                new ElementBounds[] { cityButtonBounds, playerButtonBounds,
                                                                      pricesButtonBounds, plotButtonBounds,
                                                                      prisonButtonBounds, summonButtonBounds,
                                                                      plotsGroupButtonBounds},
                                                "selectedTab");

            if (SingleComposer.GetToggleButton("selectedTab-" + (int)SelectedTab) != null)
            {
                SingleComposer.GetToggleButton("selectedTab-" + (int)SelectedTab).SetValue(true);
            }
            
            var lineBounds = currentBounds.BelowCopy(0, 20).WithFixedHeight(5);
            SingleComposer.AddInset(lineBounds);
         
            if(this.TabDictionary.TryGetValue(SelectedTab, out var tabBuilder))
            {
                tabBuilder(currentBounds, lineBounds);
            }

            Composers["canclaimsgui"].Compose();
            BuildUpperWindow();
        }
        public void OnColorPicked(int index)
        {
            selectedColor = claims.config.PLOT_COLORS[index];
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
            SingleComposer.AddInset(el);
            el.fixedY += 20;
            if (CreateNewCityState == EnumUpperWindowSelectedState.NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-new-city-name"),
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
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-player-name"),
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

                ///ATTACK ANIMALS SWITCHES
                ///
                ElementBounds attackAnimalsTextBounds = useTextBounds.BelowCopy(0, 15);
                Composers["canclaimsgui-upper"].AddStaticText("Attack animals", CairoFont.WhiteDetailText(), attackAnimalsTextBounds);
                bgBounds.WithChildren(attackAnimalsTextBounds);

                ElementBounds friendAttackBounds = attackAnimalsTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set p friend attack " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, friendAttackBounds, "friend-attack");

                Composers["canclaimsgui-upper"].GetSwitch("friend-attack").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.getPerm(perms.PermGroup.COMRADE, perms.type.PermType.ATTACK_ANIMALS_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("friend", CairoFont.WhiteDetailText(), 60, friendAttackBounds);



                ElementBounds citizenAttackBounds = friendAttackBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set p citizen attack " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, citizenAttackBounds, "citizen-attack");
                Composers["canclaimsgui-upper"].GetSwitch("citizen-attack").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.getPerm(perms.PermGroup.CITIZEN, perms.type.PermType.ATTACK_ANIMALS_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("citizen", CairoFont.WhiteDetailText(), 60, citizenAttackBounds);
                ElementBounds strangerAttackBounds = citizenAttackBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot set p stranger attack " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, strangerAttackBounds, "stranger-attack");
                Composers["canclaimsgui-upper"].GetSwitch("stranger-attack").SetValue(claims.clientDataStorage.clientPlayerInfo.CurrentPlotInfo.PermsHandler.getPerm(perms.PermGroup.STRANGER, perms.type.PermType.ATTACK_ANIMALS_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("stranger", CairoFont.WhiteDetailText(), 60, strangerAttackBounds);
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

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot claim", EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-decline-button"), new ActionConsumable(() =>
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

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/plot unclaim", EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-decline-button"), new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.LEAVE_CITY_CONFIRM)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-leave-city-confirm-button"),
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city leave", EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-decline-button"), new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_RANK_REMOVE_CONFIRM)
            {
                el.fixedWidth += 40;
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-strip-rank-from-player", secondValueCollected, firstValueCollected),
                                                                            CairoFont.WhiteDetailText(),
                                                                            el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city rank remove " + firstValueCollected + " " +  secondValueCollected, EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.CityRanks.FirstOrDefault(c => c.Name ==  firstValueCollected);
                    if(cell != null)
                    {
                        cell.Citizens.Remove(secondValueCollected);
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

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-add-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city rank add " + firstValueCollected + " " + collectedNewCityName, EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-close-button"), new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.SELECT_NEW_CITY_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-new-city-name"),
                  CairoFont.WhiteDetailText(),
                  el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedFriendName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-set-name-button"), new ActionConsumable(() =>
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
            else if (CreateNewCityState == EnumUpperWindowSelectedState.ADD_CRIMINAL_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-player-name"),
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedCriminalName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-add-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city criminal add " + collectedNewCityName, EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedCriminalName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.REMOVE_CRIMINAL)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-player-name"),
                                                               CairoFont.WhiteDetailText(),
                                                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);

                Composers["canclaimsgui-upper"].AddDropDown(claims.clientDataStorage.clientPlayerInfo.CityInfo.Criminals.ToArray(),
                                                            claims.clientDataStorage.clientPlayerInfo.CityInfo.Criminals.ToArray(),
                                                            -1,
                                                            OnSelectedNameFromDropDown,
                                                            inputNameBounds);

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-remove-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city criminal remove " + collectedNewCityName, EnumChatType.Macro, "");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_PRISON_REMOVE_CELL_CONFIRM)
            {
                el.fixedWidth += 40;
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-prison-cell-remove-confirm", firstValueCollected),
                                                                    CairoFont.WhiteDetailText(),
                                                                    el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/c prison cremovecell {0} {1} {2}", this.selectedPos.X, this.selectedPos.Y, this.selectedPos.Z),
                        EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PrisonCells.FirstOrDefault(c => c.SpawnPosition == this.selectedPos);
                    if (cell != null)
                    {
                        claims.clientDataStorage.clientPlayerInfo.CityInfo.PrisonCells.Remove(cell);
                        BuildMainWindow();
                    }
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-decline-button"), new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if(CreateNewCityState == EnumUpperWindowSelectedState.ADD_PLOTSGROUP_MEMBER_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-player-name"),
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedPlayerName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);
                PlotsGroupCellElement cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells.FirstOrDefault(gr => gr.Guid.Equals(this.selectedString), null);
                if(cell != null)
                {
                    Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-invite-button"), new ActionConsumable(() =>
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                            string.Format("/c plotsgroup add {0} {1}", cell.Name, collectedNewCityName), EnumChatType.Macro, "");
                        Composers["canclaimsgui-upper"].GetTextInput("collectedPlayerName").SetValue("");
                        collectedNewCityName = "";
                        CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                        BuildUpperWindow();
                        return true;
                    }), enterNameBounds, EnumButtonStyle.Normal);
                }
            }
            else if(CreateNewCityState == EnumUpperWindowSelectedState.REMOVE_PLOTSGROUP_MEMBER_SELECT)
            {
                el.fixedWidth += 40;
                PlotsGroupCellElement cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells.FirstOrDefault(gr => gr.Guid.Equals(this.selectedString), null);
                if (cell != null)
                {
                    Composers["canclaimsgui-upper"].AddStaticText(
                        string.Format("Kick from plots group {0}", cell.Name),
                    CairoFont.WhiteDetailText(),
                    el);

                    ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                    bgBounds.WithChildren(inputNameBounds);

                    Composers["canclaimsgui-upper"].AddDropDown(cell.PlayersNames.ToArray(),
                                                                cell.PlayersNames.ToArray(),
                                                               -1,
                                                               OnSelectedNameFromDropDown,
                                                               inputNameBounds);

                    ElementBounds yesButtonBounds = inputNameBounds.BelowCopy(0, 15);
                    yesButtonBounds.fixedWidth /= 2;
                    bgBounds.WithChildren(yesButtonBounds);

                    Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-kick-button"), new ActionConsumable(() =>
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.REMOVE_PLOTSGROUP_MEMBER_CONFIRM;
                        BuildUpperWindow();
                        return true;
                    }), yesButtonBounds, EnumButtonStyle.Normal);

                    ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                    Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-close-button"), new ActionConsumable(() =>
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                        BuildUpperWindow();
                        return true;
                    }), noButtonBounds, EnumButtonStyle.Normal);
                }
            }
            else if(CreateNewCityState == EnumUpperWindowSelectedState.REMOVE_PLOTSGROUP_MEMBER_CONFIRM)
            {
                el.fixedWidth += 40;
                var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells.FirstOrDefault(gr => gr.Guid.Equals(this.selectedString), null);
                if (cell != null)
                {
                    Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-kick-player-from-plotsgroup", cell.Name, this.collectedNewCityName),
                                                                                CairoFont.WhiteDetailText(),
                                                                                el);
                    ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                    yesButtonBounds.fixedWidth /= 2;
                    bgBounds.WithChildren(yesButtonBounds);

                    Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, 
                           string.Format("/c plotsgroup kick {0} {1}", cell.Name, collectedNewCityName), EnumChatType.Macro, "");
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
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PERMISSIONS)
            {
                Composers["canclaimsgui-upper"].AddStaticText("Plot group permissions",
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells.FirstOrDefault(gr => gr.Guid.Equals(this.selectedString), null);

                ElementBounds pvpToggleTextBounds = el.BelowCopy(0, 15);
                pvpToggleTextBounds.fixedWidth = 80;
                Composers["canclaimsgui-upper"].AddStaticText("PVP", CairoFont.WhiteDetailText(), pvpToggleTextBounds);

                ElementBounds pvpToggleButtonBounds = pvpToggleTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/c plotsgroup set pvp {0} {1}", cell?.Name, (t ? "on" : "off")), EnumChatType.Macro, "");
                    cell.PermsHandler.setPvp(t);
                },
                                                pvpToggleButtonBounds,
                                                "pvp-switch");
                Composers["canclaimsgui-upper"].GetSwitch("pvp-switch").SetValue(cell.PermsHandler.pvpFlag);
                bgBounds.WithChildren(pvpToggleTextBounds);


                ElementBounds fireToggleTextBounds = pvpToggleTextBounds.BelowCopy(0, 15);
                fireToggleTextBounds.fixedWidth = 80;
                Composers["canclaimsgui-upper"].AddStaticText("Fire", CairoFont.WhiteDetailText(), fireToggleTextBounds);

                ElementBounds fireToggleButtonBounds = fireToggleTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                       string.Format("/c plotsgroup set fire {0} {1}", cell?.Name, (t ? "on" : "off")), EnumChatType.Macro, "");
                    cell.PermsHandler.setFire(t);
                },
                                                fireToggleButtonBounds,
                                                "fire-switch");
                Composers["canclaimsgui-upper"].GetSwitch("fire-switch").SetValue(cell.PermsHandler.fireFlag);
                bgBounds.WithChildren(fireToggleTextBounds);

                ElementBounds blastToggleTextBounds = fireToggleTextBounds.BelowCopy(0, 15);
                blastToggleTextBounds.fixedWidth = 80;
                Composers["canclaimsgui-upper"].AddStaticText("Blast", CairoFont.WhiteDetailText(), blastToggleTextBounds);

                ElementBounds blastToggleButtonBounds = blastToggleTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/c plotsgroup set blast {0} {1}", cell?.Name, (!t ? "on" : "off")), EnumChatType.Macro, "");
                    cell.PermsHandler.setBlast(!t);
                },
                                                blastToggleButtonBounds,
                                                "blast-switch");
                Composers["canclaimsgui-upper"].GetSwitch("blast-switch").SetValue(!cell.PermsHandler.blastFlag);
                bgBounds.WithChildren(blastToggleButtonBounds);


                /////BUILD SWITCHES
                ElementBounds buildTextBounds = blastToggleTextBounds.BelowCopy(0, 15);
                Composers["canclaimsgui-upper"].AddStaticText("Build", CairoFont.WhiteDetailText(), buildTextBounds);
                bgBounds.WithChildren(buildTextBounds);


                ElementBounds citizenBuildBounds = buildTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/c plotsgroup set p {0} citizen build {1}", cell?.Name ?? "_", (t ? "on" : "off")), EnumChatType.Macro, "");
                    cell.PermsHandler.setPerm(perms.PermGroup.CITIZEN, perms.type.PermType.BUILD_AND_DESTROY_PERM, t);
                }, citizenBuildBounds, "citizen-build");
                Composers["canclaimsgui-upper"].GetSwitch("citizen-build")
                    .SetValue(cell.PermsHandler.getPerm(perms.PermGroup.CITIZEN, perms.type.PermType.BUILD_AND_DESTROY_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("citizen", CairoFont.WhiteDetailText(), 60, citizenBuildBounds);

                bgBounds.WithChildren(blastToggleButtonBounds);

                ///USE SWITCHES
                ///

                ElementBounds useTextBounds = buildTextBounds.BelowCopy(0, 15);
                Composers["canclaimsgui-upper"].AddStaticText("Use", CairoFont.WhiteDetailText(), useTextBounds);
                bgBounds.WithChildren(useTextBounds);


                ElementBounds citizenUseBounds = useTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/c plotsgroup set p {0} citizen use {1}", cell?.Name ?? "_", (t ? "on" : "off")), EnumChatType.Macro, "");
                    cell.PermsHandler.setPerm(perms.PermGroup.CITIZEN, perms.type.PermType.USE_PERM, t);
                }, citizenUseBounds, "citizen-use");
                Composers["canclaimsgui-upper"].GetSwitch("citizen-use")
                    .SetValue(cell.PermsHandler.getPerm(perms.PermGroup.CITIZEN, perms.type.PermType.USE_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("citizen", CairoFont.WhiteDetailText(), 60, citizenUseBounds);

            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_SUMMON_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-summon-point-name"),
                               CairoFont.WhiteDetailText(),
                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedSummonPointName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-set-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, string.Format("/city summon set cname {0} {1} {2} {3}",
                        this.selectedPos.X, this.selectedPos.Y, this.selectedPos.Z, collectedNewCityName), EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedSummonPointName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if(CreateNewCityState == EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PLOT_CLAIM_CONFIRM)
            {
                el.fixedWidth += 40;
                var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells.FirstOrDefault(gr => gr.Guid.Equals(this.selectedString), null);
                if (cell != null)
                {
                    Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-claim-plot-plotsgroup", cell.Name),
                                                                                CairoFont.WhiteDetailText(),
                                                                                el);
                    ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                    yesButtonBounds.fixedWidth /= 2;
                    bgBounds.WithChildren(yesButtonBounds);

                    Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                           string.Format("/c plotsgroup plotadd {0}", cell.Name), EnumChatType.Macro, "");
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
            }
            else if(CreateNewCityState == EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PLOT_UNCLAIM_CONFIRM)
            {
                el.fixedWidth += 40;
                var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells.FirstOrDefault(gr => gr.Guid.Equals(this.selectedString), null);
                if (cell != null)
                {
                    Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-unclaim-plot-plotsgroup", cell.Name),
                                                                                CairoFont.WhiteDetailText(),
                                                                                el);
                    ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                    yesButtonBounds.fixedWidth /= 2;
                    bgBounds.WithChildren(yesButtonBounds);

                    Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                           string.Format("/c plotsgroup plotremove {0}", cell.Name), EnumChatType.Macro, "");
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
            }
            else if(CreateNewCityState == EnumUpperWindowSelectedState.CITY_PLOTSGROUP_ADD_NEW_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-plotsgroup-name"),
                               CairoFont.WhiteDetailText(),
                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewPlotsGroupName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-add-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/c plotsgroup create {0}",
                        this.collectedNewCityName), EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewPlotsGroupName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_PLOTSGROUP_REMOVE_SELECT)
            {
                el.fixedWidth += 40;

                Composers["canclaimsgui-upper"].AddStaticText(
                    string.Format("Select plots group to remove"),
                CairoFont.WhiteDetailText(),
                el);

                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);

                var plotsGroupsArray = claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells.Select(gr => gr.Name).ToArray();
                Composers["canclaimsgui-upper"].AddDropDown(plotsGroupsArray,
                                                            plotsGroupsArray,
                                                            -1,
                                                            OnSelectedNameFromDropDown,
                                                            inputNameBounds);

                ElementBounds yesButtonBounds = inputNameBounds.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-remove-button"), new ActionConsumable(() =>
                {                   
                    CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_REMOVE_CONFIRM;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-close-button"), new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);                
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_PLOTSGROUP_REMOVE_CONFIRM)
            {
                el.fixedWidth += 40;
                var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells.FirstOrDefault(gr => gr.Name.Equals(this.collectedNewCityName), null);
                if (cell != null)
                {
                    Composers["canclaimsgui-upper"].AddStaticText(
                        string.Format("Remove group {0}", this.secondValueCollected),
                    CairoFont.WhiteDetailText(),
                    el);

                    ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                    yesButtonBounds.fixedWidth /= 2;
                    bgBounds.WithChildren(yesButtonBounds);

                    Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                           string.Format("/c plotsgroup delete {0}", cell.Name), EnumChatType.Macro, "");
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
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.NEW_ALLIANCE_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-alliance-name"),
                               CairoFont.WhiteDetailText(),
                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewPlotsGroupName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-create-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/a create {0}",
                        this.collectedNewCityName), EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewPlotsGroupName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);

            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.INVITE_TO_ALLIANCE_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-alliance-name"),
                               CairoFont.WhiteDetailText(),
                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewPlotsGroupName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-add-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/a invite {0}",
                        this.collectedNewCityName), EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewPlotsGroupName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);

            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.KICK_FROM_ALLIANCE_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-alliance-name"),
                               CairoFont.WhiteDetailText(),
                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewPlotsGroupName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-remove-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/a kick {0}",
                        this.collectedNewCityName), EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewPlotsGroupName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);

            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.SELECT_NEW_ALLIANCE_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-alliance-name"),
                               CairoFont.WhiteDetailText(),
                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewPlotsGroupName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-set-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/a set name {0}",
                        this.collectedNewCityName), EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewPlotsGroupName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);

            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.ALLIANCE_PREFIX_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-alliance-prefix"),
                               CairoFont.WhiteDetailText(),
                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewPlotsGroupName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-set-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/a set prefix {0}",
                        this.collectedNewCityName), EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewPlotsGroupName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);

            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.LEAVE_ALLIANCE_CONFIRM)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-leave-alliance-confirm-button"),
                CairoFont.WhiteDetailText(),
                el);
                ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                yesButtonBounds.fixedWidth /= 2;
                bgBounds.WithChildren(yesButtonBounds);


                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/alliance leave", EnumChatType.Macro, "");
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), yesButtonBounds, EnumButtonStyle.Normal);

                ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-decline-button"), new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), noButtonBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.ALLIANCE_SEND_NEW_CONFLICT_LETTER_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:name_of_alliance_to_send_conflict_letter"),
                CairoFont.WhiteDetailText(),
                el);

                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewPlotsGroupName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/a conflict declare {0}",
                        this.collectedNewCityName), EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewPlotsGroupName").SetValue("");
                    collectedNewCityName = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.ALLIANCE_SEND_PEACE_OFFER_CONFIRM)
            {
                var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientConflictCellElements.FirstOrDefault(c => c.Guid == this.selectedString);
                if (cell != null)
                {
                    Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui_send_peace_offer", this.secondSelectedString),
                    CairoFont.WhiteDetailText(),
                    el);
                    ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                    yesButtonBounds.fixedWidth /= 2;
                    bgBounds.WithChildren(yesButtonBounds);


                    Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/alliance conflict offerstop " + this.secondSelectedString, EnumChatType.Macro, "");
                        CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                        BuildUpperWindow();
                        return true;
                    }), yesButtonBounds, EnumButtonStyle.Normal);

                    ElementBounds noButtonBounds = yesButtonBounds.RightCopy(0, 0);
                    Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-decline-button"), new ActionConsumable(() =>
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                        BuildUpperWindow();
                        return true;
                    }), noButtonBounds, EnumButtonStyle.Normal);
                }
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_PLOTS_PERMISSIONS)
            {
                Composers["canclaimsgui-upper"].AddStaticText("City permissions",
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
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set pvp " + (t ? "on" : "off"), EnumChatType.Macro, "");
                },
                                                pvpToggleButtonBounds,
                                                "pvp-switch");
                Composers["canclaimsgui-upper"].GetSwitch("pvp-switch").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.pvpFlag);
                bgBounds.WithChildren(pvpToggleTextBounds);


                ElementBounds fireToggleTextBounds = pvpToggleTextBounds.BelowCopy(0, 15);
                fireToggleTextBounds.fixedWidth = 80;
                Composers["canclaimsgui-upper"].AddStaticText("Fire", CairoFont.WhiteDetailText(), fireToggleTextBounds);

                ElementBounds fireToggleButtonBounds = fireToggleTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set fire " + (t ? "on" : "off"), EnumChatType.Macro, "");
                },
                                                fireToggleButtonBounds,
                                                "fire-switch");
                Composers["canclaimsgui-upper"].GetSwitch("fire-switch").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.fireFlag);
                bgBounds.WithChildren(fireToggleTextBounds);

                ElementBounds blastToggleTextBounds = fireToggleTextBounds.BelowCopy(0, 15);
                blastToggleTextBounds.fixedWidth = 80;
                Composers["canclaimsgui-upper"].AddStaticText("Blast", CairoFont.WhiteDetailText(), blastToggleTextBounds);

                ElementBounds blastToggleButtonBounds = blastToggleTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set blast " + (!t ? "on" : "off"), EnumChatType.Macro, "");
                },
                                                blastToggleButtonBounds,
                                                "blast-switch");
                Composers["canclaimsgui-upper"].GetSwitch("blast-switch").SetValue(!claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.blastFlag);
                bgBounds.WithChildren(blastToggleButtonBounds);


                /////BUILD SWITCHES
                ElementBounds buildTextBounds = blastToggleTextBounds.BelowCopy(0, 15);
                Composers["canclaimsgui-upper"].AddStaticText("Build", CairoFont.WhiteDetailText(), buildTextBounds);
                bgBounds.WithChildren(buildTextBounds);

                ElementBounds friendBuildBounds = buildTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set p friend build " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, friendBuildBounds, "friend-build");

                Composers["canclaimsgui-upper"].GetSwitch("friend-build").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.getPerm(perms.PermGroup.COMRADE, perms.type.PermType.BUILD_AND_DESTROY_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("friend", CairoFont.WhiteDetailText(), 60, friendBuildBounds);



                ElementBounds citizenBuildBounds = friendBuildBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set p citizen build " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, citizenBuildBounds, "citizen-build");
                Composers["canclaimsgui-upper"].GetSwitch("citizen-build").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.getPerm(perms.PermGroup.CITIZEN, perms.type.PermType.BUILD_AND_DESTROY_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("citizen", CairoFont.WhiteDetailText(), 60, citizenBuildBounds);
                ElementBounds strangerBuildBounds = citizenBuildBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set p stranger build " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, strangerBuildBounds, "stranger-build");
                Composers["canclaimsgui-upper"].GetSwitch("stranger-build").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.getPerm(perms.PermGroup.STRANGER, perms.type.PermType.BUILD_AND_DESTROY_PERM));
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
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set p friend use " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, friendUseBounds, "friend-use");

                Composers["canclaimsgui-upper"].GetSwitch("friend-use").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.getPerm(perms.PermGroup.COMRADE, perms.type.PermType.USE_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("friend", CairoFont.WhiteDetailText(), 60, friendUseBounds);



                ElementBounds citizenUseBounds = friendUseBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set p citizen use " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, citizenUseBounds, "citizen-use");
                Composers["canclaimsgui-upper"].GetSwitch("citizen-use").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.getPerm(perms.PermGroup.CITIZEN, perms.type.PermType.USE_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("citizen", CairoFont.WhiteDetailText(), 60, citizenUseBounds);
                ElementBounds strangerUseBounds = citizenUseBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set p stranger use " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, strangerUseBounds, "stranger-use");
                Composers["canclaimsgui-upper"].GetSwitch("stranger-use").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.getPerm(perms.PermGroup.STRANGER, perms.type.PermType.USE_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("stranger", CairoFont.WhiteDetailText(), 60, strangerUseBounds);

                ///ATTACK ANIMALS SWITCHES
                ///
                ElementBounds attackAnimalsTextBounds = useTextBounds.BelowCopy(0, 15);
                Composers["canclaimsgui-upper"].AddStaticText("Attack animals", CairoFont.WhiteDetailText(), attackAnimalsTextBounds);
                bgBounds.WithChildren(attackAnimalsTextBounds);

                ElementBounds friendAttackBounds = attackAnimalsTextBounds.RightCopy(0, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set p friend attack " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, friendAttackBounds, "friend-attack");

                Composers["canclaimsgui-upper"].GetSwitch("friend-attack").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.getPerm(perms.PermGroup.COMRADE, perms.type.PermType.ATTACK_ANIMALS_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("friend", CairoFont.WhiteDetailText(), 60, friendAttackBounds);



                ElementBounds citizenAttackBounds = friendAttackBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set p citizen attack " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, citizenAttackBounds, "citizen-attack");
                Composers["canclaimsgui-upper"].GetSwitch("citizen-attack").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.getPerm(perms.PermGroup.CITIZEN, perms.type.PermType.ATTACK_ANIMALS_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("citizen", CairoFont.WhiteDetailText(), 60, citizenAttackBounds);
                ElementBounds strangerAttackBounds = citizenAttackBounds.RightCopy(5, 0);
                Composers["canclaimsgui-upper"].AddSwitch((t) =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/city set p stranger attack " + (t ? "on" : "off"), EnumChatType.Macro, "");
                }, strangerAttackBounds, "stranger-attack");
                Composers["canclaimsgui-upper"].GetSwitch("stranger-attack").SetValue(claims.clientDataStorage.clientPlayerInfo.CityInfo.PermsHandler.getPerm(perms.PermGroup.STRANGER, perms.type.PermType.ATTACK_ANIMALS_PERM));
                Composers["canclaimsgui-upper"].AddHoverText("stranger", CairoFont.WhiteDetailText(), 60, strangerAttackBounds);
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_RANK_DELETE_CONFIRM)
            {
                el.fixedWidth += 40;
                var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.CityRanks.FirstOrDefault(gr => gr.Name.Equals(this.selectedString), null);
                if (cell != null)
                {
                    Composers["canclaimsgui-upper"].AddStaticText(
                        string.Format("Remove city rank {0}?", this.selectedString),
                    CairoFont.WhiteDetailText(),
                    el);

                    ElementBounds yesButtonBounds = el.BelowCopy(0, 15);
                    yesButtonBounds.fixedWidth /= 2;
                    bgBounds.WithChildren(yesButtonBounds);

                    Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-confirm-button"), new ActionConsumable(() =>
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/c rank delete {0}",
                        this.selectedString), EnumChatType.Macro, "");
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
            }
            else if (CreateNewCityState == EnumUpperWindowSelectedState.CITY_RANK_CREATION_NEED_NAME)
            {
                Composers["canclaimsgui-upper"].AddStaticText(Lang.Get("claims:gui-enter-rank-name"),
                               CairoFont.WhiteDetailText(),
                               el);
                ElementBounds inputNameBounds = el.BelowCopy(0, 15);
                bgBounds.WithChildren(inputNameBounds);
                Composers["canclaimsgui-upper"].AddTextInput(inputNameBounds,
                    (name) => collectedNewCityName = name, null, "collectedNewPlotsGroupName");

                ElementBounds enterNameBounds = inputNameBounds.BelowCopy(0, 15);
                bgBounds.WithChildren(enterNameBounds);

                Composers["canclaimsgui-upper"].AddButton(Lang.Get("claims:gui-add-button"), new ActionConsumable(() =>
                {
                    ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                        string.Format("/c rank create {0}",
                        this.collectedNewCityName), EnumChatType.Macro, "");
                    Composers["canclaimsgui-upper"].GetTextInput("collectedNewPlotsGroupName").SetValue("");
                    collectedNewCityName = "";
                    selectedString = "";
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    BuildUpperWindow();
                    return true;
                }), enterNameBounds, EnumButtonStyle.Normal);

            }
            Composers["canclaimsgui-upper"].AddDialogTitleBar(Lang.Get(""), 
                () => 
                { 
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;
                    
                    BuildUpperWindow();
                }
            ).Compose();
        }
        public void OnTabToggled(int tabIndex)
        {
            /*if(tabIndex == 6 || tabIndex == 7 || tabIndex == 8)
            {
                tabIndex -= 2;
            }*/
            SelectedTab = (EnumSelectedTab)tabIndex;
            BuildMainWindow();
        }
        public void OnSelectedNameFromDropDown(string code, bool selected)
        {
            collectedNewCityName = code;
        }
        public void BuildCityPage(ElementBounds currentBounds, ElementBounds lineBounds)
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

                /*==============================================================================================*/
                /*=====================================CITY PERMISSIONS=========================================*/
                /*==============================================================================================*/
                ElementBounds cityPermBounds = unclaimCityPlotButtonBounds.RightCopy();
                ElementBounds showPermissionsButtonBounds = currentBounds.RightCopy();
                cityPermBounds.WithFixedSize(25, 25);
                SingleComposer.AddIconButton("medal", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTS_PERMISSIONS;
                        BuildUpperWindow();
                    }
                    else
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                        BuildUpperWindow();
                    }
                }, cityPermBounds, "setPermissions");
                SingleComposer.GetToggleButton("setPermissions").Toggleable = true;



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
                    SingleComposer.AddStaticText(Lang.Get("claims:gui-city-balance", clientInfo.CityInfo.CityBalance),
                        cityTabFont,
                        currentBounds, "cityBalance");
                }
                
                if (clientInfo.CityInfo.CityDebt > 0)
                {
                    currentBounds = currentBounds.BelowCopy(0, 5);
                    SingleComposer.AddStaticText(Lang.Get("claims:gui-city-debt", clientInfo.CityInfo.CityDebt),
                        cityTabFont,
                        currentBounds, "cityDebt");
                }
                
                if (clientInfo.CityInfo.CityDayPayment > 0)
                {
                    currentBounds = currentBounds.BelowCopy(0, 5);
                    SingleComposer.AddStaticText(Lang.Get("claims:gui-city-payment", clientInfo.CityInfo.CityDayPayment),
                        cityTabFont,
                        currentBounds, "cityPayment");
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

                if (claims.clientDataStorage.clientPlayerInfo.PlayerPermissions.HasPermission(rights.EnumPlayerPermissions.CITY_SET_PLOTS_COLOR))
                {
                    SingleComposer.AddIconButton("claims:large-paint-brush", new Action<bool>((b) =>
                    {
                        this.SelectedTab = EnumSelectedTab.CityPlotsColorSelector;
                        BuildMainWindow();
                        return;
                    }), nextIconBounds);
                    nextIconBounds = nextIconBounds.RightCopy(20);
                }
                SingleComposer.AddIconButton("claims:vertical-banner", new Action<bool>((b) =>
                {
                    this.SelectedTab = EnumSelectedTab.AllianceInfoPage;
                    BuildMainWindow();
                    return;
                }), nextIconBounds);
                nextIconBounds = nextIconBounds.RightCopy(20);
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
                    .AddCellList(this.listInvitationsBounds = this.clippingInvitationsBounds.ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
                    (ClientToCityInvitation cell, ElementBounds bounds) =>
                    {
                        return new GuiElementCityInvitation(capi, cell, bounds)
                        {
                            //"claims:textures/icons/warning.svg")
                            On = true,
                            OnMouseDownOnCellLeft = new Action<int>(this.OnClickCellLeft),
                            OnMouseDownOnCellMiddle = new Action<int>(this.OnClickCellMiddle),
                            OnMouseDownOnCellRight = new Action<int>(this.OnClickCellRight)
                        };
                    }, claims.clientDataStorage.clientPlayerInfo.ReceivedInvitations, "modstable")
                    .EndClip()
                    .AddVerticalScrollbar((float value) =>
                    {
                        ElementBounds bounds = SingleComposer.GetCellList<ClientToCityInvitation>("modstable").Bounds;
                        bounds.fixedY = (double)(0f - value);
                        bounds.CalcWorldBounds();
                    }, scrollbarBounds, "scrollbar")
                //.AddSmallButton("Close", OnButtonClose, closeButtonBounds)
                .EndChildElements()

                .Compose();

                    SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingInvitationsBounds.fixedHeight, (float)this.listInvitationsBounds.fixedHeight);
                }
            }
        }
        public void BuildPlayerPage(ElementBounds currentBounds, ElementBounds lineBounds)
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



            SingleComposer.AddIconButton("claims:village", new Action<bool>((b) =>
            {
                SelectedTab = EnumSelectedTab.CitiesListPage;
                BuildMainWindow();
                return;
            }), nextIconBounds);
        }
        public void BuildPricesPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            currentBounds = currentBounds.BelowCopy(0, 40);
            var pricesTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            currentBounds.Alignment = EnumDialogArea.LeftTop;
            currentBounds.fixedY += 15;
            string currencyStr = Lang.Get("claims:gui-currency-item");
            TextExtents textExtents = pricesTabFont.GetTextExtents(currencyStr);
            currentBounds.fixedWidth = textExtents.Width + 10;
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
                SingleComposer.AddRichtext(new RichTextComponentBase[] { currencyStack }, cityPriceBounds, "coin-item");
            }
            currentBounds = currentBounds.BelowCopy(0, 0);
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            string cityCostString = Lang.Get("claims:gui-new-city-cost", claims.config.NEW_CITY_COST.ToString());
            textExtents = pricesTabFont.GetTextExtents(cityCostString);
            currentBounds.fixedWidth = textExtents.Width + 10;
            SingleComposer.AddStaticText(Lang.Get("claims:gui-new-city-cost", claims.config.NEW_CITY_COST.ToString()),
                    pricesTabFont,
                    currentBounds, "new-city-price");

            currentBounds = currentBounds.BelowCopy(0, 0);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            

            string cityPlotCost = Lang.Get("claims:gui-city-plot-cost", claims.config.PLOT_CLAIM_PRICE.ToString());
            currentBounds.fixedWidth = pricesTabFont.GetTextExtents(cityPlotCost).Width + 10;
            SingleComposer.AddStaticText(cityPlotCost,
                    pricesTabFont,
                    currentBounds, "plot-claim-price");

            currentBounds = currentBounds.BelowCopy(0, 0);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            string cityNameChangeCost = Lang.Get("claims:gui-city-name-change-cost", claims.config.CITY_NAME_CHANGE_COST.ToString());
            currentBounds.fixedWidth = pricesTabFont.GetTextExtents(cityNameChangeCost).Width + 10;
            SingleComposer.AddStaticText(cityNameChangeCost,
                    pricesTabFont,
                    currentBounds, "city-name-price");

            currentBounds = currentBounds.BelowCopy(0, 0);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            

            SingleComposer.AddStaticText(Lang.Get("claims:gui-city-base-cost", claims.config.CITY_BASE_CARE.ToString()),
                    pricesTabFont,
                    currentBounds, "city-base-care");

            currentBounds = currentBounds.BelowCopy(0, 0);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            

            SingleComposer.AddStaticText(Lang.Get("claims:gui-teleportation-cost", claims.config.SUMMON_PAYMENT.ToString()),
                    pricesTabFont,
                    currentBounds, "city-summon-price");

            currentBounds = currentBounds.BelowCopy(0, 0);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            


            SingleComposer.AddStaticText(Lang.Get("claims:gui-new-alliance-cost", claims.config.NEW_ALLIANCE_COST.ToString()),
                    pricesTabFont,
                    currentBounds, "city-alliance-price");
        }
        public void BuildPlotPage(ElementBounds currentBounds, ElementBounds lineBounds)
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

            if (clientInfo.CurrentPlotInfo.Price > -1)
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
            if (clientInfo.CurrentPlotInfo.OwnerName?.Length > 0)
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

            ElementBounds addCellButtonBounds = setPlotTypeButtonBounds.RightCopy();
            addCellButtonBounds.WithFixedSize(25, 25);

            if (clientInfo.CurrentPlotInfo.PlotType == PlotType.PRISON)
            {
                SingleComposer.AddIconButton("wpCircle", (bool t) =>
                {
                    if (t)
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/c prison addcell", EnumChatType.Macro, "");
                        BuildUpperWindow();
                    }
                }, addCellButtonBounds, "addPrisonCell");
                SingleComposer.AddHoverText("Add prison cell", plotTabFont, 180, addCellButtonBounds);
            }
            else if (clientInfo.CurrentPlotInfo.PlotType == PlotType.SUMMON)
            {
                SingleComposer.AddIconButton("wpCircle", (bool t) =>
                {
                    if (t)
                    {
                        ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                        clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup, "/c summon set point", EnumChatType.Macro, "");
                        BuildUpperWindow();
                    }
                }, addCellButtonBounds, "setSummonPoint");
                SingleComposer.AddHoverText("Set summon point.", plotTabFont, 180, addCellButtonBounds);
            }
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
        public void BuildRanksPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            currentBounds = currentBounds.BelowCopy(0, 20);
            ElementBounds createCityBounds = currentBounds.FlatCopy();
            int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
            ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

            ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 230).FixedUnder(topTextBounds, 5);
            ElementBounds invitationTextBounds = createCityBounds.BelowCopy();
            invitationTextBounds.fixedHeight -= 50;
            //invitationTextBounds.fixedWidth -= 100;
            invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
            ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

            ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

            ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

            SingleComposer.AddStaticText("Ranks",
                CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                invitationTextBounds);

            //currentBounds = currentBounds.RightCopy(0, 0);

            //currentBounds.fixedWidth -= 20;
            ElementBounds addPlayerButtonEB = invitationTextBounds.RightCopy().WithFixedSize(24, 24);
            addPlayerButtonEB.fixedX = invitationTextBounds.absOffsetX + invitationTextBounds.fixedWidth / 2 - 60;
            /*removeRankEB.fixedX = 0;
            removeRankEB.fixedY += 20;*/
            SingleComposer.AddIconButton("plus", (bool t) =>
            {
                CreateNewCityState = EnumUpperWindowSelectedState.CITY_RANK_CREATION_NEED_NAME;
                BuildUpperWindow();
            },
                             addPlayerButtonEB);


            this.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

            /*foreach (var it in claims.clientDataStorage.clientPlayerInfo.CityInfo.PossibleCityRanks)
            {
                if (claims.clientDataStorage.clientPlayerInfo.CityInfo.CitizensRanks.All(cell => cell.RankName != it.ToLower()))
                {
                    claims.clientDataStorage.clientPlayerInfo.CityInfo.CitizensRanks.Add(new CityRankCellElement(it.ToLower(), new List<string> { }));
                }
            }*/

            SingleComposer.BeginChildElements(invitationTextBounds.BelowCopy())
            .BeginClip(clippingBounds)
            .AddInset(insetBounds, 3)
            .AddCellList(this.listRanksBounds = this.clippingRansksBounds.ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
            (CityRankCellElement cell, ElementBounds bounds) =>
            {
                return new GuiElementCityRanks(capi, cell, bounds)
                {
                    //"claims:textures/icons/warning.svg")
                    On = true,
                    /*OnMouseDownOnCellLeft = new Action<int>(this.OnClickCellLeft),
                    OnMouseDownOnCellMiddle = new Action<int>(this.OnClickCellMiddle),
                    OnMouseDownOnCellRight = new Action<int>(this.OnClickCellRight)*/
                };
            }, claims.clientDataStorage.clientPlayerInfo.CityInfo.CityRanks, "citizensranks")
            .EndClip()
            .AddVerticalScrollbar((float value) =>
            {
                ElementBounds bounds = SingleComposer.GetCellList<CityRankCellElement>("citizensranks").Bounds;
                bounds.fixedY = (double)(0f - value);
                bounds.CalcWorldBounds();
            }, scrollbarBounds, "scrollbar")
            .EndChildElements();
            var c = SingleComposer.GetCellList<CityRankCellElement>("citizensranks");
            c.BeforeCalcBounds();

            SingleComposer.Compose();

            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingRansksBounds.fixedHeight, (float)this.listRanksBounds.fixedHeight);
        }
        public void BuildRanksInfoPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            if (claims.clientDataStorage.clientPlayerInfo.CityInfo == null)
            {
                SingleComposer.Compose();
                return;
            }
            CityRankCellElement cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.CityRanks.FirstOrDefault(rc => rc.Name.Equals(this.selectedString), null);
            if (cell == null)
            {
                return;
            }

            currentBounds = currentBounds.BelowCopy(0, 40);
            var plotTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            var clientInfo = claims.clientDataStorage.clientPlayerInfo;

            currentBounds.Alignment = EnumDialogArea.LeftTop;
            SingleComposer.AddStaticText(string.Format("{0}", cell.Name),
                plotTabFont, EnumTextOrientation.Center,
                currentBounds, "rankName");
            currentBounds.fixedWidth -= 20;
            ElementBounds removeRankEB = currentBounds.RightCopy().WithFixedSize(24, 24);
            /*removeRankEB.fixedX = 0;
            removeRankEB.fixedY += 20;*/
            SingleComposer.AddIconButton("eraser", (bool t) =>
            {
                CreateNewCityState = EnumUpperWindowSelectedState.CITY_RANK_DELETE_CONFIRM;
                BuildUpperWindow();
            },
                             removeRankEB);


            currentBounds = currentBounds.BelowCopy();
            currentBounds.fixedY += 25;
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            SingleComposer.AddStaticText(Lang.Get("claims:gui-rank-members", cell.Citizens.Count),
                        plotTabFont,
                        currentBounds, "members");

            SingleComposer.AddHoverText(StringFunctions.concatStringsWithDelim(cell.Citizens, ','),
                                        plotTabFont.WithOrientation(EnumTextOrientation.Center),
                                        (int)currentBounds.fixedWidth, currentBounds);

            ElementBounds multiSelectBounds = currentBounds.BelowCopy(0, 10);

            EnumPlayerPermissions[] availableToAdd = claims.config.AVAILABLE_CITY_PERMISSIONS.Where(v => !cell.Permissions.Contains(v)).ToArray();

            var availableToAddStrings = availableToAdd.Select(s => s.ToString()).ToArray();
            
            SingleComposer.AddMultiSelectDropDown(availableToAddStrings, availableToAddStrings, -1, MultiSelectRank, multiSelectBounds, "addPermissionsMultiDrop");
            currentBounds = multiSelectBounds;
            ElementBounds colorSelectedButton = currentBounds.BelowCopy().WithFixedSize(48, 24);
            colorSelectedButton.fixedX = 0;
            colorSelectedButton.fixedY += 20;
            SingleComposer.AddButton("Add", new ActionConsumable(() =>
            {
                ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                var dropDown = SingleComposer.GetDropDown("addPermissionsMultiDrop");
                if (dropDown.SelectedValues.Length > 0)
                {
                    string fullList = string.Join(' ', dropDown.SelectedValues);
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                    string.Format("/c rank addperm {0} {1}",
                    this.selectedString, fullList), EnumChatType.Macro, "");
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

            SingleComposer.AddMultiSelectDropDown(availableToRemoveStrings, availableToRemoveStrings, -1, MultiSelectRank, multiSelectBoundsRemove, "removePermissionsMultiDrop");

            ElementBounds removeSelectedButtonEB = multiSelectBoundsRemove.BelowCopy().WithFixedSize(58, 24);
           /* removeSelectedButtonEB.fixedX = 0;*/
            removeSelectedButtonEB.fixedY += 20;
            SingleComposer.AddButton("Remove", new ActionConsumable(() =>
            {
                ClientEventManager clientEventManager = (claims.capi.World as ClientMain).eventManager;
                var dropDown = SingleComposer.GetDropDown("removePermissionsMultiDrop");
                if (dropDown.SelectedValues.Length > 0)
                {
                    string fullList = string.Join(' ', dropDown.SelectedValues);
                    clientEventManager.TriggerNewClientChatLine(GlobalConstants.CurrentChatGroup,
                    string.Format("/c rank removeperm {0} {1}",
                    this.selectedString, fullList), EnumChatType.Macro, "");
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

            SingleComposer.BeginChildElements(mainBounds)
               .AddInset(insetBounds, insetDepth)
                    .BeginClip(clipBounds)
                        .AddContainer(containerBounds, "scroll-content1")
                    .EndClip()
                    .AddVerticalScrollbar((value) =>
                    {
                        ElementBounds bounds = SingleComposer.GetContainer("scroll-content1").Bounds;
                        bounds.fixedY = 5 - value;
                        bounds.CalcWorldBounds();
                    }, scrollbarBounds, "scrollbar1")
            .EndChildElements();
            GuiElementContainer scrollArea = SingleComposer.GetContainer("scroll-content1");
            var li = cell.Permissions.Select(v => v.ToString()).ToList();
            foreach (var it in li)
            {
                scrollArea.Add(new GuiElementRichtext(SingleComposer.Api, VtmlUtil.Richtextify(SingleComposer.Api, it, CairoFont.WhiteMediumText().WithFontSize(20)), containerRowBounds));
                containerRowBounds = containerRowBounds.BelowCopy();
            }
           
            SingleComposer.Compose();

            // After composing dialog, need to set the scrolling area heights to enable scroll behavior
            float scrollVisibleHeight = (float)clipBounds.fixedHeight;
            float scrollTotalHeight = rowHeight * li.Count;
            SingleComposer.GetScrollbar("scrollbar1").SetHeights(scrollVisibleHeight, scrollTotalHeight);
            //var sc = SingleComposer.GetScrollbar("scrollbar1");
            //sc.Enabled = false;
            currentBounds = currentBounds.BelowCopy();
            currentBounds.fixedY += 25;
            currentBounds.fixedWidth = 300;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            SingleComposer.AddStaticText("Perms: " + string.Join('\n', cell.Permissions),
                       plotTabFont,
                       currentBounds, "permissions");

            

            //f.SetSelectedIndex(0);
            //f.SetSelectedIndex(1);
            //.SetSelectedValue("name1");
            /* SingleComposer.AddStaticText(Lang.Get("claims:gui-group-members", cell.PlayersNames.Count),
                         plotTabFont,
                         currentBounds, "criminals");*/

            /* SingleComposer.AddHoverText(StringFunctions.concatStringsWithDelim(cell.PlayersNames, ','),
                                         plotTabFont.WithOrientation(EnumTextOrientation.Center),
                                         (int)currentBounds.fixedWidth, currentBounds);

             ElementBounds addCriminalBounds = currentBounds.RightCopy();
             addCriminalBounds.WithFixedWidth(25).WithFixedHeight(25);
             ElementBounds removeFriendBounds = addCriminalBounds.RightCopy();
             SingleComposer.AddIconButton("plus", (bool t) =>
             {
                 if (t)
                 {
                     CreateNewCityState = EnumUpperWindowSelectedState.ADD_PLOTSGROUP_MEMBER_NEED_NAME;
                     BuildUpperWindow();
                 }
             }, addCriminalBounds);


             SingleComposer.AddIconButton("line", (bool t) =>
             {
                 if (t)
                 {
                     CreateNewCityState = EnumUpperWindowSelectedState.REMOVE_PLOTSGROUP_MEMBER_SELECT;
                     BuildUpperWindow();
                 }
             }, removeFriendBounds);

             currentBounds = currentBounds.BelowCopy(0, 5);
             SingleComposer.AddStaticText(Lang.Get("claims:gui-plot-permissions"),
                 plotTabFont, EnumTextOrientation.Left,
                 currentBounds, "permissionHandler");


             ElementBounds showPermissionsButtonBounds = currentBounds.RightCopy();
             showPermissionsButtonBounds.WithFixedSize(25, 25);
             SingleComposer.AddIconButton("medal", (bool t) =>
             {
                 if (t)
                 {
                     CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PERMISSIONS;
                     BuildUpperWindow();
                 }
                 else
                 {
                     CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                     BuildUpperWindow();
                 }
             }, showPermissionsButtonBounds, "setPermissions");
             SingleComposer.GetToggleButton("setPermissions").Toggleable = true;

             ElementBounds claimCityPlotButtonBounds = currentBounds.BelowCopy();
             claimCityPlotButtonBounds.WithFixedWidth(25).WithFixedHeight(25);
             ElementBounds unclaimCityPlotButtonBounds = claimCityPlotButtonBounds.RightCopy();
             SingleComposer.AddIconButton("plus", (bool t) =>
             {
                 if (t)
                 {
                     CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PLOT_CLAIM_CONFIRM;
                     BuildUpperWindow();
                 }
             }, claimCityPlotButtonBounds);
             SingleComposer.AddHoverText("Add current plot to plots group",
                                 CairoFont.SmallButtonText(),
                                 (int)currentBounds.fixedWidth / 2, claimCityPlotButtonBounds);

             SingleComposer.AddIconButton("line", (bool t) =>
             {
                 if (t)
                 {
                     CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PLOT_UNCLAIM_CONFIRM;
                     BuildUpperWindow();
                 }
             }, unclaimCityPlotButtonBounds);
             SingleComposer.AddHoverText("Remove current plot from plots group",
                                             CairoFont.SmallButtonText(),
                                             (int)currentBounds.fixedWidth / 2, unclaimCityPlotButtonBounds);*/
        }
        public void MultiSelectRank(string code, bool selected)
        {
            var c = 3;
        }
        public void BuildPlotColorSelectorPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            if (claims.clientDataStorage.clientPlayerInfo.CityInfo == null)
            {
                SingleComposer.Compose();
                return;
            }
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

            SingleComposer.AddColorListPicker(claims.config.PLOT_COLORS == null ? new int[] { 0, 8888888 } : claims.config.PLOT_COLORS
                                                           , OnColorPicked,
                                                          bounds = selectColorBounds.BelowCopy(0.0, 0.0, 0.0, 0.0).WithFixedSize((double)25, (double)25), 200, "picker-2");

            ElementBounds colorSelectedButton = bounds.BelowCopy().WithFixedSize(48, 48);
            colorSelectedButton.fixedX = 0;
            colorSelectedButton.fixedY += 20;
            SingleComposer.AddButton("Select", new ActionConsumable(() =>
            {
                if (selectedColor == -1)
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
        public void BuildPrisonPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var criminalsTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            currentBounds = currentBounds.BelowCopy(0, 30);
            currentBounds.fixedY += 25;
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            if (clientInfo.CityInfo == null)
            {
                SingleComposer.Compose();
                return;
            }
            SingleComposer.AddStaticText(Lang.Get("claims:gui-criminals", clientInfo.CityInfo.Criminals.Count),
                        criminalsTabFont,
                        currentBounds, "criminals");

            SingleComposer.AddHoverText(StringFunctions.concatStringsWithDelim(clientInfo.CityInfo.Criminals, ','),
                                        criminalsTabFont.WithOrientation(EnumTextOrientation.Center),
                                        (int)currentBounds.fixedWidth, currentBounds);

            ElementBounds addCriminalBounds = currentBounds.RightCopy();
            addCriminalBounds.WithFixedWidth(25).WithFixedHeight(25);
            ElementBounds removeFriendBounds = addCriminalBounds.RightCopy();
            SingleComposer.AddIconButton("plus", (bool t) =>
            {
                if (t)
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.ADD_CRIMINAL_NEED_NAME;
                    BuildUpperWindow();
                }
            }, addCriminalBounds);

            SingleComposer.AddIconButton("line", (bool t) =>
            {
                if (t)
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.REMOVE_CRIMINAL;
                    BuildUpperWindow();
                }
            }, removeFriendBounds);

            currentBounds.fixedWidth = lineBounds.fixedWidth;

            ElementBounds createCityBounds = currentBounds.FlatCopy();
            int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
            ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

            ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 300).FixedUnder(topTextBounds, 5);
            //logtextBounds.fixedHeight = 150;
            ElementBounds invitationTextBounds = createCityBounds.BelowCopy();

            invitationTextBounds.fixedHeight -= 50;
            invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
            ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

            ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

            ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

            SingleComposer.AddStaticText("Prison cells",
                CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                invitationTextBounds);

            this.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

            SingleComposer.BeginChildElements(invitationTextBounds)
            .BeginClip(clippingBounds)
            .AddInset(insetBounds, 3)
            .AddCellList(this.listRanksBounds = this.clippingRansksBounds.ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
            (PrisonCellElement cell, ElementBounds bounds) =>
            {
                return new GuiElementCityPrisonCell(capi, cell, bounds)
                {

                    On = true
                };
            }, claims.clientDataStorage.clientPlayerInfo.CityInfo.PrisonCells, "prisoncellscells")
            .EndClip()
            .AddVerticalScrollbar((float value) =>
            {
                ElementBounds bounds = SingleComposer.GetCellList<PrisonCellElement>("prisoncellscells").Bounds;
                bounds.fixedY = (double)(0f - value);
                bounds.CalcWorldBounds();
            }, scrollbarBounds, "scrollbar")
            .EndChildElements();
            var c = SingleComposer.GetCellList<PrisonCellElement>("prisoncellscells");
            c.BeforeCalcBounds();

            SingleComposer.Compose();

            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingRansksBounds.fixedHeight, (float)this.listRanksBounds.fixedHeight);
        }
        public void BuildSummonPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            if (claims.clientDataStorage.clientPlayerInfo.CityInfo == null)
            {
                SingleComposer.Compose();
                return;
            }
            var criminalsTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            currentBounds.fixedWidth = lineBounds.fixedWidth;

            currentBounds = currentBounds.BelowCopy(0, 0);
            ElementBounds createCityBounds = currentBounds.FlatCopy();
            int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
            ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

            ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 250).FixedUnder(topTextBounds, 5);
            ElementBounds invitationTextBounds = createCityBounds.BelowCopy();

            invitationTextBounds.fixedHeight -= 50;
            invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
            ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

            ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

            ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

            SingleComposer.AddStaticText("Summon points",
                CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                invitationTextBounds);
            if (clientInfo.CityInfo == null)
            {
                SingleComposer.Compose();
                return;
            }
            this.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

            SingleComposer.BeginChildElements(invitationTextBounds)
                .BeginClip(clippingBounds)
                .AddInset(insetBounds, 3)
                .AddCellList(this.listRanksBounds = this.clippingRansksBounds.
                ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
                (SummonCellElement cell, ElementBounds bounds) =>
                {
                    return new GuiElementCitySummonCell(capi, cell, bounds)
                    {
                        On = true
                    };
                },
                claims.clientDataStorage.clientPlayerInfo.CityInfo.SummonCells, "summoncellscells")
                .EndClip()
                .AddVerticalScrollbar((float value) =>
                {
                    ElementBounds bounds = SingleComposer.GetCellList<SummonCellElement>("summoncellscells").Bounds;
                    bounds.fixedY = (double)(0f - value);
                    bounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar")
                .EndChildElements();
            var c = SingleComposer.GetCellList<SummonCellElement>("summoncellscells");
            c.BeforeCalcBounds();

            SingleComposer.Compose();

            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingRansksBounds.fixedHeight, (float)this.listRanksBounds.fixedHeight);
        }
        public void BuildPlotsGroupPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            if (claims.clientDataStorage.clientPlayerInfo.CityInfo == null)
            {
                SingleComposer.Compose();
                return;
            }
            var criminalsTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            currentBounds.fixedWidth = lineBounds.fixedWidth;

            currentBounds = currentBounds.BelowCopy(0, 0);

            ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, currentBounds.fixedWidth - 30, 30);

            ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, currentBounds.fixedWidth - 30, mainBounds.fixedHeight - 250).FixedUnder(topTextBounds, 5);
            ElementBounds invitationTextBounds = currentBounds.BelowCopy();
            
            invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
            ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

            ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

            ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

            SingleComposer.AddStaticText("Plots groups",
                CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                invitationTextBounds);

            ElementBounds addPlotsGroupBounds = insetBounds.BelowCopy(15, 15);
            addPlotsGroupBounds.WithFixedWidth(25).WithFixedHeight(25);
            SingleComposer.AddInset(addPlotsGroupBounds);
            SingleComposer.AddIconButton("plus", (bool t) =>
            {
                if (t)
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_ADD_NEW_NEED_NAME;
                    BuildUpperWindow();
                }
            }, addPlotsGroupBounds);
            SingleComposer.AddHoverText("Add new plots group",
                                            CairoFont.SmallButtonText(),
                                            (int)currentBounds.fixedWidth / 2, addPlotsGroupBounds);

            ElementBounds removePlotsGroupBounds = addPlotsGroupBounds.RightCopy(15);
            SingleComposer.AddInset(removePlotsGroupBounds);
            SingleComposer.AddIconButton("line", (bool t) =>
            {
                if (t)
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_REMOVE_SELECT;
                    BuildUpperWindow();
                }
            }, removePlotsGroupBounds);
            SingleComposer.AddHoverText("Remove plots group",
                                            CairoFont.SmallButtonText(),
                                            (int)currentBounds.fixedWidth / 2, removePlotsGroupBounds);

            ElementBounds receivedInvitesPageButtonBounds = removePlotsGroupBounds.RightCopy(15);
            SingleComposer.AddInset(receivedInvitesPageButtonBounds);
            SingleComposer.AddIconButton("ring", (bool t) =>
            {
                if (t)
                {
                    SelectedTab = EnumSelectedTab.PlotsGroupReceivedInvites;
                    BuildMainWindow();
                }
            }, receivedInvitesPageButtonBounds);
            SingleComposer.AddHoverText("Show received invites",
                                CairoFont.SmallButtonText(),
                                (int)currentBounds.fixedWidth / 2, receivedInvitesPageButtonBounds);

            this.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

            SingleComposer.BeginChildElements(invitationTextBounds)
                .BeginClip(clippingBounds)
                .AddInset(insetBounds, 3)
                .AddCellList(this.listRanksBounds = this.clippingRansksBounds.
                                    ForkContainingChild(0.0, 0.0, 0.0, -3.0).
                                    WithFixedPadding(5.0), (PlotsGroupCellElement cell, ElementBounds bounds) =>
                                    {
                                        return new GuiElementCityPlotsGroupCell(capi, cell, bounds)
                                        {
                                            On = true
                                        };
                                    },
                                    claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells, "plotsgroupcellscells")
                .EndClip()
                .AddVerticalScrollbar((float value) =>
                {
                    ElementBounds bounds = SingleComposer.GetCellList<PlotsGroupCellElement>("plotsgroupcellscells").Bounds;
                    bounds.fixedY = (double)(0f - value);
                    bounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar")
                .EndChildElements();
            var c = SingleComposer.GetCellList<PlotsGroupCellElement>("plotsgroupcellscells");
            c.BeforeCalcBounds();

            SingleComposer.Compose();

            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingRansksBounds.fixedHeight, (float)this.listRanksBounds.fixedHeight);


        }
        public void BuildPlotsGroupInfoPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            if (claims.clientDataStorage.clientPlayerInfo.CityInfo == null)
            {
                SingleComposer.Compose();
                return;
            }
            PlotsGroupCellElement cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.PlotsGroupCells.FirstOrDefault(gr => gr.Guid.Equals(this.selectedString), null);
            if (cell == null)
            {
                return;
            }

            currentBounds = currentBounds.BelowCopy(0, 40);
            var plotTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            var clientInfo = claims.clientDataStorage.clientPlayerInfo;

            currentBounds.Alignment = EnumDialogArea.LeftTop;
            SingleComposer.AddStaticText(string.Format("{0}: {1}", cell.CityName, cell.Name),
                plotTabFont, EnumTextOrientation.Center,
                currentBounds, "plotPos");

            currentBounds = currentBounds.BelowCopy();
            currentBounds.fixedY += 25;
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            SingleComposer.AddStaticText(Lang.Get("claims:gui-group-members", cell.PlayersNames.Count),
                        plotTabFont,
                        currentBounds, "criminals");

            SingleComposer.AddHoverText(StringFunctions.concatStringsWithDelim(cell.PlayersNames, ','),
                                        plotTabFont.WithOrientation(EnumTextOrientation.Center),
                                        (int)currentBounds.fixedWidth, currentBounds);

            ElementBounds addCriminalBounds = currentBounds.RightCopy();
            addCriminalBounds.WithFixedWidth(25).WithFixedHeight(25);
            ElementBounds removeFriendBounds = addCriminalBounds.RightCopy();
            SingleComposer.AddIconButton("plus", (bool t) =>
            {
                if (t)
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.ADD_PLOTSGROUP_MEMBER_NEED_NAME;
                    BuildUpperWindow();
                }
            }, addCriminalBounds);


            SingleComposer.AddIconButton("line", (bool t) =>
            {
                if (t)
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.REMOVE_PLOTSGROUP_MEMBER_SELECT;
                    BuildUpperWindow();
                }
            }, removeFriendBounds);
            
            currentBounds = currentBounds.BelowCopy(0, 5);
            SingleComposer.AddStaticText(Lang.Get("claims:gui-plot-permissions"),
                plotTabFont, EnumTextOrientation.Left,
                currentBounds, "permissionHandler");


            ElementBounds showPermissionsButtonBounds = currentBounds.RightCopy();
            showPermissionsButtonBounds.WithFixedSize(25, 25);
            SingleComposer.AddIconButton("medal", (bool t) =>
            {
                if (t)
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PERMISSIONS;
                    BuildUpperWindow();
                }
                else
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.NONE;

                    BuildUpperWindow();
                }
            }, showPermissionsButtonBounds, "setPermissions");
            SingleComposer.GetToggleButton("setPermissions").Toggleable = true;

            ElementBounds claimCityPlotButtonBounds = currentBounds.BelowCopy();
            claimCityPlotButtonBounds.WithFixedWidth(25).WithFixedHeight(25);
            ElementBounds unclaimCityPlotButtonBounds = claimCityPlotButtonBounds.RightCopy();
            SingleComposer.AddIconButton("plus", (bool t) =>
            {
                if (t)
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PLOT_CLAIM_CONFIRM;
                    BuildUpperWindow();
                }
            }, claimCityPlotButtonBounds);
            SingleComposer.AddHoverText("Add current plot to plots group",
                                CairoFont.SmallButtonText(),
                                (int)currentBounds.fixedWidth / 2, claimCityPlotButtonBounds);

            SingleComposer.AddIconButton("line", (bool t) =>
            {
                if (t)
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.CITY_PLOTSGROUP_PLOT_UNCLAIM_CONFIRM;
                    BuildUpperWindow();
                }
            }, unclaimCityPlotButtonBounds);
            SingleComposer.AddHoverText("Remove current plot from plots group",
                                            CairoFont.SmallButtonText(),
                                            (int)currentBounds.fixedWidth / 2, unclaimCityPlotButtonBounds);
            
        }
        public void BuildPlotsGroupReceivedInvitesPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            ElementBounds createCityBounds = currentBounds.FlatCopy();

            if (claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations.Count > 0)
            {
                int renderedClaimsInfoCounter = 0;
                int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
                ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

                ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 230).FixedUnder(topTextBounds, 5);
                ElementBounds invitationTextBounds = createCityBounds.BelowCopy(0, 35);
                invitationTextBounds.fixedHeight -= 50;
                invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
                ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

                ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

                ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

                SingleComposer.AddStaticText("Invitations" + " [" + claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations.Count + "]",
                    CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                    invitationTextBounds);


                this.clippingInvitationsBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

                SingleComposer.BeginChildElements(invitationTextBounds.BelowCopy())
                .BeginClip(clippingBounds)
                .AddInset(insetBounds, 3)
                .AddCellList(this.listInvitationsBounds = this.clippingInvitationsBounds.ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
                (ClientToPlotsGroupInvitation cell, ElementBounds bounds) =>
                {
                    return new GuiElementPlotsGroupInvitation(capi, cell, bounds)
                    {
                        //"claims:textures/icons/warning.svg")
                        On = true,
                        OnMouseDownOnCellLeft = new Action<int>((int cellIndex) => {
                           
                        }),
                        OnMouseDownOnCellMiddle = new Action<int>((int cellIndex) =>
                        {
                            
                        }),
                        OnMouseDownOnCellRight = new Action<int>(this.OnClickCellRight)
                    };
                }, claims.clientDataStorage.clientPlayerInfo.ReceivedPlotsGroupInvitations, "modstable")
                .EndClip()
                .AddVerticalScrollbar((float value) =>
                {
                    ElementBounds bounds = SingleComposer.GetCellList<ClientToPlotsGroupInvitation>("modstable").Bounds;
                    bounds.fixedY = (double)(0f - value);
                    bounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar")
            //.AddSmallButton("Close", OnButtonClose, closeButtonBounds)
            .EndChildElements()

            .Compose();

                SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingInvitationsBounds.fixedHeight, (float)this.listInvitationsBounds.fixedHeight);
            }
            else
            {
                int renderedClaimsInfoCounter = 0;
                int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
                ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

                ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 230).FixedUnder(topTextBounds, 5);
                ElementBounds invitationTextBounds = createCityBounds.BelowCopy(0, 35);
                invitationTextBounds.fixedHeight -= 50;
                invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
                ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

                ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

                ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

                SingleComposer.AddStaticText(Lang.Get("claims:gui-no-plotsgroup-invites"),
                    CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                    invitationTextBounds);
            }
        }
        public void BuildAlliancePage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            currentBounds = currentBounds.BelowCopy(0, 40);
            if (claims.clientDataStorage.clientPlayerInfo?.AllianceInfo != null)
            {
                var clientInfo = claims.clientDataStorage.clientPlayerInfo;
                var allianceTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
                TextExtents textExtents = CairoFont.ButtonText().GetTextExtents(clientInfo.AllianceInfo.Name);
                var allianceNameBounds = currentBounds.FlatCopy().WithFixedWidth(textExtents.Width + 10);

                SingleComposer.AddButton(clientInfo.AllianceInfo.Name, new ActionConsumable(() =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.SELECT_NEW_ALLIANCE_NAME;
                    BuildUpperWindow();
                    return true;
                }), allianceNameBounds, EnumButtonStyle.Normal);


                currentBounds = currentBounds.BelowCopy(0, 10);
                SingleComposer.AddStaticText(Lang.Get("claims:gui-leader-name", clientInfo.AllianceInfo.LeaderName),
                    allianceTabFont,
                    currentBounds, "leaderName");

                currentBounds = currentBounds.BelowCopy();
                SingleComposer.AddStaticText(Lang.Get("claims:gui-date-created", TimeFunctions.getDateFromEpochSeconds(clientInfo.AllianceInfo.TimeStampCreated)),
                    allianceTabFont,
                    currentBounds, "createdAt");

                currentBounds = currentBounds.BelowCopy(0, 5);
                currentBounds.fixedWidth /= 2;
                currentBounds.Alignment = EnumDialogArea.LeftFixed;
                SingleComposer.AddStaticText(Lang.Get("claims:gui-alliance-cities-list", clientInfo.AllianceInfo.Cities.Count),
                    allianceTabFont,
                    currentBounds, "cities-count");

                SingleComposer.AddHoverText(StringFunctions.concatStringsWithDelim(clientInfo.AllianceInfo.Cities, ','),
                                            CairoFont.ButtonText(),
                                            (int)currentBounds.fixedWidth, currentBounds);
                //SingleComposer.AddInset(currentBounds);
                ElementBounds inviteCityButtonBounds = currentBounds.RightCopy();
                inviteCityButtonBounds.WithFixedWidth(25).WithFixedHeight(25);
                ElementBounds kickCityButtonBounds = inviteCityButtonBounds.RightCopy();
                ElementBounds uninviteCityButtonBounds = kickCityButtonBounds.RightCopy();
                SingleComposer.AddIconButton("plus", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.INVITE_TO_ALLIANCE_NEED_NAME;
                        BuildUpperWindow();
                    }
                }, inviteCityButtonBounds);

                SingleComposer.AddIconButton("line", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.KICK_FROM_ALLIANCE_NEED_NAME;
                        BuildUpperWindow();
                    }
                }, kickCityButtonBounds);

                SingleComposer.AddIconButton("eraser", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.UNINVITE_TO_ALLIANCE;
                        BuildUpperWindow();
                    }
                }, uninviteCityButtonBounds);

                currentBounds = currentBounds.BelowCopy(0, 5);

                SingleComposer.AddStaticText(Lang.Get("claims:gui-alliance-balance", clientInfo.AllianceInfo.Balance),
                       CairoFont.ButtonText(),
                       EnumTextOrientation.Left,
                       currentBounds, "allianceBalance");

                currentBounds = currentBounds.BelowCopy(0, 5);

                SingleComposer.AddStaticText(Lang.Get("claims:gui-alliance-prefix", clientInfo.AllianceInfo.Prefix),
                       CairoFont.ButtonText(),
                       EnumTextOrientation.Left,
                       currentBounds, "alliancePreifx");

                ElementBounds changeAlliancePrefixButtonBounds = currentBounds.RightCopy();
                changeAlliancePrefixButtonBounds.WithFixedWidth(25).WithFixedHeight(25);
                SingleComposer.AddIconButton("claims:soldering-iron", (bool t) =>
                {
                    if (t)
                    {
                        CreateNewCityState = EnumUpperWindowSelectedState.ALLIANCE_PREFIX_NEED_NAME;
                        BuildUpperWindow();
                    }
                }, changeAlliancePrefixButtonBounds);
                              
                /*==============================================================================================*/
                /*=====================================UNDER 2 LINE=============================================*/
                /*==============================================================================================*/
                var line2Bounds = currentBounds.BelowCopy(0, 20).WithFixedHeight(5).WithFixedWidth(lineBounds.fixedWidth);
                line2Bounds.fixedX = 0;
                line2Bounds.fixedY = mainBounds.fixedHeight * 0.85;
                SingleComposer.AddInset(line2Bounds);

                ElementBounds nextIconBounds = line2Bounds.BelowCopy().WithFixedSize(48, 48).WithAlignment(EnumDialogArea.LeftTop);
                nextIconBounds.fixedX = 15;
                nextIconBounds.fixedY = mainBounds.fixedHeight * 0.90;



                SingleComposer.AddIconButton("claims:exit-door", new Action<bool>((b) =>
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.LEAVE_ALLIANCE_CONFIRM;
                    BuildUpperWindow();
                    return;
                }), nextIconBounds);

                ElementBounds conflictLettersButtonBounds = nextIconBounds.RightCopy(15).WithFixedSize(48, 48);
                SingleComposer.AddIconButton("claims:envelope", (bool t) =>
                {
                    if (t)
                    {
                        SelectedTab = EnumSelectedTab.ConflictLettersPage;
                        BuildMainWindow();
                    }
                }, conflictLettersButtonBounds);

                ElementBounds conflictsButtonBounds = conflictLettersButtonBounds.RightCopy(15).WithFixedSize(48, 48);
                SingleComposer.AddIconButton("claims:frog-mouth-helm", (bool t) =>
                {
                    if (t)
                    {
                        SelectedTab = EnumSelectedTab.ConflictsPage;
                        BuildMainWindow();
                    }
                }, conflictsButtonBounds);
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
                    CreateNewCityState = EnumUpperWindowSelectedState.NEW_ALLIANCE_NEED_NAME;
                    BuildUpperWindow();                   
                    return;
                }), crownButtonBounds);
                TextExtents textExtents = CairoFont.WhiteSmallText().GetTextExtents(Lang.Get("claims:gui-new-alliance-button"));
                SingleComposer.AddHoverText(Lang.Get("claims:gui-new-alliance-button"),
                                        CairoFont.WhiteSmallText().WithOrientation(EnumTextOrientation.Center),
                                        (int)textExtents.Width, crownButtonBounds);
                var clientInfo = claims.clientDataStorage.clientPlayerInfo;
                currentBounds.fixedWidth = lineBounds.fixedWidth;

                int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
                ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

                ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 250).FixedUnder(topTextBounds, 5);
                ElementBounds invitationTextBounds = createCityBounds.BelowCopy();

                invitationTextBounds.fixedHeight -= 50;
                invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
                ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

                ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

                ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

                SingleComposer.AddStaticText("To alliancies invites",
                    CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                    invitationTextBounds);

                this.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

                SingleComposer.BeginChildElements(invitationTextBounds)
                    .BeginClip(clippingBounds)
                    .AddInset(insetBounds, 3)
                    .AddCellList(this.listRanksBounds = this.clippingRansksBounds.
                    ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
                    (ClientToAllianceInvitationCellElement cell, ElementBounds bounds) =>
                    {
                        return new GuiElementToAllianceInvitation(capi, cell, bounds)
                        {
                            On = true
                        };
                    },
                    claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientToAllianceInvitations, "toallianciesinvitescells")
                    .EndClip()
                    .AddVerticalScrollbar((float value) =>
                    {
                        ElementBounds bounds = SingleComposer.GetCellList<ClientToAllianceInvitationCellElement>("toallianciesinvitescells").Bounds;
                        bounds.fixedY = (double)(0f - value);
                        bounds.CalcWorldBounds();
                    }, scrollbarBounds, "scrollbar")
                    .EndChildElements();
                var c = SingleComposer.GetCellList<ClientToAllianceInvitationCellElement>("toallianciesinvitescells");
                c.BeforeCalcBounds();

                SingleComposer.Compose();

                SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingRansksBounds.fixedHeight, (float)this.listRanksBounds.fixedHeight);
            }
        }
        public void BuildCitiesListPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var criminalsTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            currentBounds.fixedWidth = lineBounds.fixedWidth;

            currentBounds = currentBounds.BelowCopy(0, 0);
            ElementBounds createCityBounds = currentBounds.FlatCopy();
            int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
            ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

            ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 250).FixedUnder(topTextBounds, 5);
            ElementBounds invitationTextBounds = createCityBounds.BelowCopy();

            invitationTextBounds.fixedHeight -= 50;
            invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
            ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

            ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

            ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

            SingleComposer.AddStaticText("Cities",
                CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                invitationTextBounds);

            this.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

            SingleComposer.BeginChildElements(invitationTextBounds)
                .BeginClip(clippingBounds)
                .AddInset(insetBounds, 3)
                .AddCellList(this.listRanksBounds = this.clippingRansksBounds.
                ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
                (ClientCityInfoCellElement cell, ElementBounds bounds) =>
                {
                    return new GuiElementCityStatCell(capi, cell, bounds)
                    {
                        On = true
                    };
                },
                claims.clientDataStorage.clientPlayerInfo.AllCitiesList, "citystatcells")
                .EndClip()
                .AddVerticalScrollbar((float value) =>
                {
                    ElementBounds bounds = SingleComposer.GetCellList<ClientCityInfoCellElement>("citystatcells").Bounds;
                    bounds.fixedY = (double)(0f - value);
                    bounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar")
                .EndChildElements();
            var c = SingleComposer.GetCellList<ClientCityInfoCellElement>("citystatcells");
            c.BeforeCalcBounds();

            SingleComposer.Compose();

            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingRansksBounds.fixedHeight, (float)this.listRanksBounds.fixedHeight);
        }
        public void BuildConflictLettersPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var criminalsTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            currentBounds.fixedWidth = lineBounds.fixedWidth;

            currentBounds = currentBounds.BelowCopy(0, 0);
            ElementBounds createCityBounds = currentBounds.FlatCopy();
            int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
            ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

            ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 350).FixedUnder(topTextBounds, 5);
            ElementBounds invitationTextBounds = createCityBounds.BelowCopy();

            invitationTextBounds.fixedHeight -= 50;
            invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
            ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

            ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

            ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

            SingleComposer.AddStaticText(Lang.Get("claims:conflict_letters_list"),
                CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                invitationTextBounds);

            this.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

            SingleComposer.BeginChildElements(invitationTextBounds)
                .BeginClip(clippingBounds)
                .AddInset(insetBounds, 3)
                .AddCellList(this.listRanksBounds = this.clippingRansksBounds.
                ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
                (ClientConflictLetterCellElement cell, ElementBounds bounds) =>
                {
                    return new GuiElementConflictLetterCell(capi, cell, bounds)
                    {
                        On = true
                    };
                },
                claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientConflictLetterCellElements, "conflictletterscells")
                .EndClip()
                .AddVerticalScrollbar((float value) =>
                {
                    ElementBounds bounds = SingleComposer.GetCellList<ClientConflictLetterCellElement>("conflictletterscells").Bounds;
                    bounds.fixedY = (double)(0f - value);
                    bounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar")
                .EndChildElements();
            var c = SingleComposer.GetCellList<ClientConflictLetterCellElement>("conflictletterscells");
            c.BeforeCalcBounds();

            ElementBounds addPlotsGroupBounds = insetBounds.BelowCopy(15, 15);
            addPlotsGroupBounds.WithFixedWidth(25).WithFixedHeight(25);
            SingleComposer.AddInset(addPlotsGroupBounds);
            SingleComposer.AddIconButton("claims:sword-brandish", (bool t) =>
            {
                if (t)
                {
                    CreateNewCityState = EnumUpperWindowSelectedState.ALLIANCE_SEND_NEW_CONFLICT_LETTER_NEED_NAME;
                    BuildUpperWindow();
                }
            }, addPlotsGroupBounds);
            SingleComposer.AddHoverText("Send a new conflict letter",
                                            CairoFont.SmallButtonText(),
                                            (int)currentBounds.fixedWidth / 2, addPlotsGroupBounds);

            /*==============================================================================================*/
            /*=====================================UNDER 2 LINE=============================================*/
            /*==============================================================================================*/
            var line2Bounds = currentBounds.BelowCopy(0, 20).WithFixedHeight(5).WithFixedWidth(lineBounds.fixedWidth);
            line2Bounds.fixedX = 0;
            line2Bounds.fixedY = mainBounds.fixedHeight * 0.85;
            SingleComposer.AddInset(line2Bounds);

            ElementBounds nextIconBounds = line2Bounds.BelowCopy().WithFixedSize(48, 48).WithAlignment(EnumDialogArea.LeftTop);
            nextIconBounds.fixedX = 15;
            nextIconBounds.fixedY = mainBounds.fixedHeight * 0.90;

            SingleComposer.AddIconButton("claims:vertical-banner", new Action<bool>((b) =>
            {
                this.SelectedTab = EnumSelectedTab.AllianceInfoPage;
                BuildMainWindow();
                return;
            }), nextIconBounds);
            nextIconBounds = nextIconBounds.RightCopy(20);

            /*SingleComposer.AddIconButton("claims:exit-door", new Action<bool>((b) =>
            {
                CreateNewCityState = EnumUpperWindowSelectedState.LEAVE_ALLIANCE_CONFIRM;
                BuildUpperWindow();
                return;
            }), nextIconBounds);

            ElementBounds conflictLettersButtonBounds = nextIconBounds.RightCopy(15).WithFixedSize(48, 48);
            SingleComposer.AddIconButton("claims:envelope", (bool t) =>
            {
                if (t)
                {
                    SelectedTab = EnumSelectedTab.ConflictLettersPage;
                    BuildMainWindow();
                }
            }, conflictLettersButtonBounds);*/

            //ElementBounds conflictsButtonBounds = nextIconBounds.RightCopy(15).WithFixedSize(48, 48);
            SingleComposer.AddIconButton("claims:frog-mouth-helm", (bool t) =>
            {
                if (t)
                {
                    SelectedTab = EnumSelectedTab.ConflictsPage;
                    BuildMainWindow();
                }
            }, nextIconBounds);

            SingleComposer.Compose();

            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingRansksBounds.fixedHeight, (float)this.listRanksBounds.fixedHeight);
        }
        public void BuildConflictsPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var criminalsTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            currentBounds.fixedWidth = lineBounds.fixedWidth;

            currentBounds = currentBounds.BelowCopy(0, 0);
            ElementBounds createCityBounds = currentBounds.FlatCopy();
            int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
            ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

            ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 350).FixedUnder(topTextBounds, 5);
            ElementBounds invitationTextBounds = createCityBounds.BelowCopy();

            invitationTextBounds.fixedHeight -= 50;
            invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
            ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();

            ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);

            ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);

            SingleComposer.AddStaticText(Lang.Get("claims:conflict_list"),
                CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                invitationTextBounds);

            this.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);

            SingleComposer.BeginChildElements(invitationTextBounds)
                .BeginClip(clippingBounds)
                .AddInset(insetBounds, 3)
                .AddCellList(this.listRanksBounds = this.clippingRansksBounds.
                ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
                (ClientConflictCellElement cell, ElementBounds bounds) =>
                {
                    return new GuiElementConflictCell(capi, cell, bounds)
                    {
                        On = true
                    };
                },
                claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientConflictCellElements, "conflicscells")
                .EndClip()
                .AddVerticalScrollbar((float value) =>
                {
                    ElementBounds bounds = SingleComposer.GetCellList<ClientConflictCellElement>("conflicscells").Bounds;
                    bounds.fixedY = (double)(0f - value);
                    bounds.CalcWorldBounds();
                }, scrollbarBounds, "scrollbar")
                .EndChildElements();
            var c = SingleComposer.GetCellList<ClientConflictCellElement>("conflicscells");
            c.BeforeCalcBounds();

            /*==============================================================================================*/
            /*=====================================UNDER 2 LINE=============================================*/
            /*==============================================================================================*/
            var line2Bounds = currentBounds.BelowCopy(0, 20).WithFixedHeight(5).WithFixedWidth(lineBounds.fixedWidth);
            line2Bounds.fixedX = 0;
            line2Bounds.fixedY = mainBounds.fixedHeight * 0.85;
            SingleComposer.AddInset(line2Bounds);

            ElementBounds nextIconBounds = line2Bounds.BelowCopy().WithFixedSize(48, 48).WithAlignment(EnumDialogArea.LeftTop);
            nextIconBounds.fixedX = 15;
            nextIconBounds.fixedY = mainBounds.fixedHeight * 0.90;

            SingleComposer.AddIconButton("claims:vertical-banner", new Action<bool>((b) =>
            {
                this.SelectedTab = EnumSelectedTab.AllianceInfoPage;
                BuildMainWindow();
                return;
            }), nextIconBounds);
            nextIconBounds = nextIconBounds.RightCopy(20);

            /*SingleComposer.AddIconButton("claims:exit-door", new Action<bool>((b) =>
            {
                CreateNewCityState = EnumUpperWindowSelectedState.LEAVE_ALLIANCE_CONFIRM;
                BuildUpperWindow();
                return;
            }), nextIconBounds);

            ElementBounds conflictLettersButtonBounds = nextIconBounds.RightCopy(15).WithFixedSize(48, 48);
            SingleComposer.AddIconButton("claims:envelope", (bool t) =>
            {
                if (t)
                {
                    SelectedTab = EnumSelectedTab.ConflictLettersPage;
                    BuildMainWindow();
                }
            }, conflictLettersButtonBounds);*/

            //ElementBounds conflictsButtonBounds = nextIconBounds.RightCopy(15).WithFixedSize(48, 48);
            SingleComposer.AddIconButton("claims:envelope", (bool t) =>
            {
                if (t)
                {
                    SelectedTab = EnumSelectedTab.ConflictLettersPage;
                    BuildMainWindow();
                }
            }, nextIconBounds);

            SingleComposer.Compose();

            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingRansksBounds.fixedHeight, (float)this.listRanksBounds.fixedHeight);
        }
        public void BuildConflictInfoPage(ElementBounds currentBounds, ElementBounds lineBounds)
        {
            var criminalsTabFont = CairoFont.ButtonText().WithFontSize(20).WithOrientation(EnumTextOrientation.Left);
            currentBounds.fixedWidth /= 2;
            currentBounds.WithAlignment(EnumDialogArea.LeftTop);

            var clientInfo = claims.clientDataStorage.clientPlayerInfo;
            currentBounds.fixedWidth = lineBounds.fixedWidth;

            currentBounds = currentBounds.BelowCopy(0, 0);
            ElementBounds createCityBounds = currentBounds.FlatCopy();
            int numClaimsToSkip = selectedClaimsPage * claimsPerPage;
            ElementBounds topTextBounds = ElementBounds.Fixed(GuiStyle.ElementToDialogPadding, 40, createCityBounds.fixedWidth - 30, 30);

            ElementBounds logtextBounds = ElementBounds.Fixed(0, 0, createCityBounds.fixedWidth - 30, mainBounds.fixedHeight - 300).FixedUnder(topTextBounds, 5);
            ElementBounds invitationTextBounds = createCityBounds.BelowCopy();

            invitationTextBounds.WithAlignment(EnumDialogArea.CenterTop);
            ElementBounds clippingBounds = logtextBounds.ForkBoundingParent();
            //clippingBounds.fixedY += 20;
            ElementBounds insetBounds = logtextBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);
            //insetBounds.fixedY += 40;
            ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(logtextBounds.fixedWidth + 7).WithFixedWidth(20);
            //SingleComposer.AddInset(createCityBounds);
            //SingleComposer.AddInset(logtextBounds);
            var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientConflictCellElements.FirstOrDefault(c => c.Guid == selectedString);
            if(cell == null)
            {
                SingleComposer.Compose();
                return;
            }

            SingleComposer.AddStaticText(string.Format("{0} x {1}", cell.FirstAllianceName, cell.SecondAllianceName),
                                            CairoFont.WhiteMediumText().WithOrientation(EnumTextOrientation.Center),
                                            invitationTextBounds);
            currentBounds = invitationTextBounds.BelowCopy();
            SingleComposer.AddStaticText(Lang.Get("claims:gui_last_start_end_battle",
                TimeFunctions.getDateFromEpochSecondsWithHoursMinutes(((DateTimeOffset)cell.LastBattleDateStart).ToUnixTimeSeconds()),
                TimeFunctions.getDateFromEpochSecondsWithHoursMinutes(((DateTimeOffset)cell.LastBattleDateEnd).ToUnixTimeSeconds())),
                CairoFont.WhiteSmallText().WithOrientation(EnumTextOrientation.Left),
                currentBounds, "MinimumDaysBetweenBattles");

            currentBounds = currentBounds.BelowCopy();
            SingleComposer.AddStaticText(Lang.Get("claims:gui_next_start_end_battle",
                TimeFunctions.getDateFromEpochSecondsWithHoursMinutes(((DateTimeOffset)cell.NextBattleDateStart).ToUnixTimeSeconds()),
                TimeFunctions.getDateFromEpochSecondsWithHoursMinutes(((DateTimeOffset)cell.NextBattleDateEnd).ToUnixTimeSeconds())),
                CairoFont.WhiteSmallText().WithOrientation(EnumTextOrientation.Left),
                currentBounds, "TimeStampCreated");
            var p = currentBounds.BelowCopy().WithFixedSize(500, 30);

            GuiTab[] horizontalTabs = new GuiTab[2];

            horizontalTabs[0] = new GuiTab();
            horizontalTabs[0].Name = "Selected";
            horizontalTabs[0].DataInt = 0;

            horizontalTabs[1] = new GuiTab();
            horizontalTabs[1].Name = "Suggested";
            horizontalTabs[1].DataInt = 1;
            SingleComposer.AddHorizontalTabs(horizontalTabs, p, (int value) =>
                                            {
                                                var tabs = SingleComposer.GetHorizontalTabs("groupTabs");
                                                if (tabs != null)
                                                {
                                                    if (tabs.activeElement != value)
                                                    {
                                                        tabs.activeElement = value;
                                                        SelectedTabGroup = value;
                                                        var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientConflictCellElements.FirstOrDefault(c => c.Guid == selectedString);
                                                        if (cell == null)
                                                        {
                                                            return;
                                                        }
                                                        if (value == (int)EnumSelectedWarRangesTab.APPROVED)
                                                        {                                                            
                                                            FillWarRangeArrays(cell.WarRanges);                                                           
                                                        }
                                                        else
                                                        {
                                                            if (claims.clientDataStorage.clientPlayerInfo.AllianceInfo.Name.Equals(cell.FirstAllianceName))
                                                            {
                                                                FillTwoWarRangesArrays(cell.FirstWarRanges, cell.SecondWarRanges);
                                                            }
                                                            else
                                                            {
                                                                FillTwoWarRangesArrays(cell.SecondWarRanges, cell.FirstWarRanges);
                                                            }
                                                        }
                                                        BuildMainWindow();
                                                    }
                                                }
                                            }, CairoFont.WhiteSmallText(), CairoFont.WhiteSmallText(), "groupTabs");
            SingleComposer.GetHorizontalTabs("groupTabs").activeElement = SelectedTabGroup;
            this.clippingRansksBounds = insetBounds.ForkContainingChild(3.0, 3.0, 3.0, 3.0);
            if (SelectedTabGroup == (int)EnumSelectedWarRangesTab.APPROVED)
            {
                SingleComposer.BeginChildElements(currentBounds)
                    .BeginClip(clippingBounds)
                    .AddInset(insetBounds, 3)
                    .AddCellList(this.listRanksBounds = this.clippingRansksBounds.
                    ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
                    (ClientWarRangeCellElement cell, ElementBounds bounds) =>
                    {
                        return new GuiElementWarRangeCell(capi, cell, bounds, SelectedTabGroup != 0)
                        {
                            On = true
                        };
                    },
                    claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientWarRangeCellElements, "conflicscells")
                    .EndClip()
                    .AddVerticalScrollbar((float value) =>
                    {
                        ElementBounds bounds = SingleComposer.GetCellList<ClientWarRangeCellElement>("conflicscells").Bounds;
                        bounds.fixedY = (double)(0f - value);
                        bounds.CalcWorldBounds();
                    }, scrollbarBounds, "scrollbar")
                    .EndChildElements();
                SingleComposer.GetCellList<ClientWarRangeCellElement>("conflicscells").BeforeCalcBounds();
            }
            else
            {
                SingleComposer.BeginChildElements(currentBounds)
                   .BeginClip(clippingBounds)
                   .AddInset(insetBounds, 3)
                   .AddCellList(this.listRanksBounds = this.clippingRansksBounds.
                   ForkContainingChild(0.0, 0.0, 0.0, -3.0).WithFixedPadding(5.0),
                   (ClientTwoWarRangesCellElement cell, ElementBounds bounds) =>
                   {
                       return new GuiElementTwoWarRangesCell(capi, cell, bounds)
                       {
                           On = true
                       };
                   },
                   claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientTwoWarRangesCellElement, "conflicscells")
                   .EndClip()
                   .AddVerticalScrollbar((float value) =>
                   {
                       ElementBounds bounds = SingleComposer.GetCellList<ClientTwoWarRangesCellElement>("conflicscells").Bounds;
                       bounds.fixedY = (double)(0f - value);
                       bounds.CalcWorldBounds();
                   }, scrollbarBounds, "scrollbar")
                   .EndChildElements();
                SingleComposer.GetCellList<ClientTwoWarRangesCellElement>("conflicscells").BeforeCalcBounds();
            }

            currentBounds = insetBounds.BelowCopy(0, 10).WithFixedSize(25, 25);
            SingleComposer.AddIconButton("line", (bool t) =>
            {
                if (t)
                {
                    var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientConflictCellElements.FirstOrDefault(c => c.Guid == selectedString);
                    if (cell == null)
                    {
                        SingleComposer.Compose();
                        return;
                    }

                    List<SelectedWarRange> selectedWarRanges = new List<SelectedWarRange>();
                    int? startIndex = null;
                    int? savedStartIndex = null;
                    DayOfWeek? startDay = null;
                    DayOfWeek? savedStartDay = null;
                    bool? lastCellState = null;
                    bool firstGo = true;
                    //try find start of range

                    for (int day = 0; day < 8; day++)
                    {
                        var currDay = claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientTwoWarRangesCellElement[day % 7];
                        var warRange = currDay.OurWarRangeArray;
                        for (int i = 0; i < 48; i++)
                        {
                            //find start of the range
                            if (warRange[i] && lastCellState.HasValue && !lastCellState.Value)
                            {
                                startIndex = i;
                                savedStartIndex = i;
                                startDay = (DayOfWeek)(day % 7);
                                savedStartDay = (DayOfWeek)(day % 7);
                                goto foundStart;
                            }
                            lastCellState = warRange[i];
                        }
                    }

                    foundStart:
                    if(startIndex == null)
                    {
                        startIndex = 0;
                        startDay = DayOfWeek.Sunday;
                    }
                    bool firstStart = true;
                    for (int day = 0; day < 8; day++) 
                    {
                        ClientTwoWarRangesCellElement it = claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientTwoWarRangesCellElement[((int)startDay + day) % 7];

                        for (int i = (startIndex.HasValue && firstStart) ? startIndex.Value : 0; i < 48; i++)
                        {
                            if(it.DayOfWeek == savedStartDay)
                            {
                                if (savedStartIndex != null && i == savedStartIndex - 1)
                                {
                                    if (startIndex != null)
                                    {
                                        int startDayNum = (int)startDay;
                                        int startMinutes = startDayNum * 24 * 60 + (startIndex ?? 0) * 30;
                                        int endMinutes = ((int)it.DayOfWeek) * 24 * 60 + i * 30;
                                        int diff = endMinutes - startMinutes;
                                        if (diff < 0)
                                        {
                                            diff += 7 * 24 * 60;
                                        }
                                        selectedWarRanges.Add(new SelectedWarRange((startDay ?? DayOfWeek.Sunday), it.DayOfWeek,
                                            new TimeSpan(hours: (i * 30) / 60, minutes: (i * 30) % 60, seconds: 0),
                                            TimeSpan.FromMinutes(diff), claims.clientDataStorage.clientPlayerInfo.AllianceInfo.Guid));
                                    }
                                    goto searchedAll;
                                }
                            }
                            if (it.OurWarRangeArray[i])
                            {
                                if (startIndex == null)
                                {
                                    startDay = it.DayOfWeek;
                                    startIndex = i;
                                }
                            }
                            else
                            {
                                if (startIndex != null)
                                {
                                    int startDayNum = (int)startDay;
                                    int startMinutes = startDayNum * 24 * 60 + (startIndex ?? 0) * 30;
                                    int endMinutes = ((int)it.DayOfWeek) * 24 * 60 + i * 30;
                                    int diff = endMinutes - startMinutes;
                                    if (diff < 0)
                                    {
                                        diff += 7 * 24 * 60;
                                    }
                                    selectedWarRanges.Add(new SelectedWarRange((startDay ?? DayOfWeek.Sunday), it.DayOfWeek,
                                        new TimeSpan(hours: ((startIndex ?? 0) * 30) / 60, minutes: ((startIndex ?? 0) * 30) % 60, seconds: 0),
                                        TimeSpan.FromMinutes(diff), claims.clientDataStorage.clientPlayerInfo.AllianceInfo.Guid));
                                    startIndex = null;
                                    //startDay = null;
                                }
                            }
                            firstStart = false;
                        } }

                    searchedAll:
                    if (cell.FirstAllianceName.Equals(claims.clientDataStorage.clientPlayerInfo.AllianceInfo.Name))
                    {
                        cell.FirstWarRanges = selectedWarRanges;
                    }
                    else
                    {
                        cell.SecondWarRanges = selectedWarRanges;
                    }
                    Dictionary<EnumPlayerRelatedInfo, string> collector = new Dictionary<EnumPlayerRelatedInfo, string>
                    {
                        { EnumPlayerRelatedInfo.CLIENT_CONFLICT_SUGGESTED_WARRANGE, JsonConvert.SerializeObject(cell) }
                    };
                    claims.clientChannel.SendPacket(new PlayerGuiRelatedInfoPacket()
                    {
                        playerGuiRelatedInfoDictionary = collector
                    });
                }
            }, currentBounds);
            /*SingleComposer.AddIconButton("line", (bool t) =>
            {
                for (int day = 0; day < 7; day++)
                {
                    ClientWarRangeCellElement it = claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientWarRangeCellElements[((int)day) % 7];
                    for(int i = 0; i < 48; i++)
                    {
                        it.WarRangeArray[i] = false;
                    }
                    this.BuildMainWindow();
                }
                        
            }, currentBounds.RightCopy());*/
            SingleComposer.AddHoverText("Send updated times.", CairoFont.WhiteDetailText(), 60, currentBounds);

            SingleComposer.Compose();

            SingleComposer.GetScrollbar("scrollbar").SetHeights((float)this.clippingRansksBounds.fixedHeight, (float)this.listRanksBounds.fixedHeight);

            SingleComposer.Compose();

        }
        public void FillListValues(List<SelectedWarRange> ranges, bool forEnemy = false)
        {
            foreach (var range in ranges)
            {
                int slotCount = (int)(range.Duration.TotalMinutes / claims.config.MIN_RANGE_CELL_DURATION_MINUTES);
                int startSlot = (int)(range.StartTime.TotalMinutes / claims.config.MIN_RANGE_CELL_DURATION_MINUTES);

                for (int i = (int)range.StartDay, k = 0; ; i++, k++)
                {
                    if (k > 6)
                    {
                        break;
                    }
                    int dayIndex = i % 7;
                    ClientTwoWarRangesCellElement cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientTwoWarRangesCellElement.FirstOrDefault(c => c.DayOfWeek == (DayOfWeek)dayIndex);
                    if (cell == null)
                    {
                        continue;
                    }
                    int startPoint = dayIndex == (int)range.StartDay ? startSlot : 0;
                    for (int j = startPoint; j < 48; j++)
                    {
                        if (forEnemy)
                        {
                            cell.EnemyWarRangeArray[j] = true;
                        }
                        else
                        {
                            cell.OurWarRangeArray[j] = true;
                        }
                        slotCount--;
                        if (slotCount <= 0)
                        {
                            goto finshedRange;
                        }
                    }
                }
            finshedRange:
                ;
            }
        }
        public void FillTwoWarRangesArrays(List<SelectedWarRange> ourRanges, List<SelectedWarRange> enemyRanges)
        {
            foreach (var it in claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientTwoWarRangesCellElement)
            {
                for (int i = 0; i < it.OurWarRangeArray.Length; i++)
                {
                    it.OurWarRangeArray[i] = false;
                    it.EnemyWarRangeArray[i] = false;
                }
            }
            FillListValues(ourRanges);
            FillListValues(enemyRanges, true);
        }
        public void FillWarRangeArrays(List<SelectedWarRange> ranges)
        {
            foreach(var it in claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientWarRangeCellElements) { 
                for(int i = 0; i < it.WarRangeArray.Length; i++)
                {
                    it.WarRangeArray[i] = false;
                }
            }        
            foreach(var range in ranges)
            {
                int slotCount = (int)(range.Duration.TotalMinutes / claims.config.MIN_RANGE_CELL_DURATION_MINUTES);
                int startSlot = (int)(range.StartTime.TotalMinutes / claims.config.MIN_RANGE_CELL_DURATION_MINUTES);

                for(int i = (int)range.StartDay, k = 0; ;i++, k++)
                {
                    if(k > 6)
                    {
                        break;
                    }
                    int dayIndex = i % 7;
                    ClientWarRangeCellElement cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientWarRangeCellElements.FirstOrDefault(c => c.DayOfWeek == (DayOfWeek)dayIndex);
                    if (cell == null)
                    {
                        continue;
                    }
                    int startPoint = dayIndex == (int)range.StartDay ? startSlot : 0;
                    for (int j = startPoint; j < 48; j++)
                    {
                        cell.WarRangeArray[j] = true;
                        slotCount--;
                        if (slotCount <= 0)
                        {
                            goto finshedRange;
                        }
                    }
                }
            finshedRange:
                ;
            }
        }
        public void SelectRangeAndFill()
        {
            var tabs = SingleComposer.GetHorizontalTabs("groupTabs");
            if (tabs != null)
            {

                var cell = claims.clientDataStorage.clientPlayerInfo.CityInfo.ClientConflictCellElements.FirstOrDefault(c => c.Guid == selectedString);
                if (cell == null)
                {
                    return;
                }
                if (tabs.activeElement == 0)
                {
                    FillWarRangeArrays(cell.WarRanges);
                }
                else
                {
                    if (claims.clientDataStorage.clientPlayerInfo.AllianceInfo.Name.Equals(cell.FirstAllianceName))
                    {
                        FillTwoWarRangesArrays(cell.FirstWarRanges, cell.SecondWarRanges);
                    }
                    else
                    {
                        FillTwoWarRangesArrays(cell.SecondWarRanges, cell.FirstWarRanges);
                    }
                }
                BuildMainWindow();
            }
            
        }
    }
}
