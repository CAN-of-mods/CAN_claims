using claims.src.auxialiry;
using claims.src.claimsext.map;
using claims.src.clextentions;
using claims.src.clientMapHandling;
using claims.src.database;
using claims.src.events;
using claims.src.harmony;
using claims.src.messages;
using claims.src.part;
using claims.src.part.structure.plots;
using claims.src.perms;
using claims.src.playerMovements;
using claims.src.timers;
using HarmonyLib;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using caneconomy.src.interfaces;
using claims.src.gui.plotMovementGui;
using claims.src.gui.playerGui.structures;
using claims.src.gui.playerGui;
using claims.src.network.handlers;
using Cairo;
using System.Collections;
using claims.src.blocks;
using claims.src.auxialiry.claimAreas;
using System;

namespace claims.src
{
    public class claims : ModSystem
    {
        /*==============================================================================================*/
        /*=====================================CLAIMS===================================================*/
        /*==============================================================================================*/
        public static Harmony harmonyInstance;
        public const string harmonyID = "claims.Patches";

        public static claims modInstance;
        public static claims clientModInstance;
        public static ICoreServerAPI sapi;
        public static ICoreClientAPI capi;
        internal static IServerNetworkChannel serverChannel;
        internal static IClientNetworkChannel clientChannel;
        static DatabaseHandler databaseHandler;

        //Because overwise on signleplayer it will collide
        public static DataStorage dataStorage {  get; private set; }
        public static DataStorage clientDataStorage {  get; private set; }
        public PlayerMovementListnerClient pmlc;
        public static PlayerMovementsListnerServer serverPlayerMovementListener;
        public static Config config;
        public static EconomyHandler economyHandler;

        /*==============================================================================================*/
        /*=====================================GUI/CLIENT===============================================*/
        /*==============================================================================================*/
        public PlotsMapLayer plotsMapLayer;
        WorldMapManager mapmgr;
        public static CANClaimsGui CANCityGui { get; set; }
        public static CityInfo playerCityInfo;

        public static ClaimsPlayerMovementGUI movementClaimGui { get; set; }             
        /*==============================================================================================*/
        /*=====================================FUNCTIONS================================================*/
        /*==============================================================================================*/

        public claims()
        {
            //modInstance = this;
            playerCityInfo = null;
        }
        public static claims getModInstance()
        {
            return modInstance;
        }
        public DatabaseHandler getDatabaseHandler()
        {
            return databaseHandler;
        }
        public void AddCustomIcons()
        {
            List<string> iconList = new List<string> { "queen-crown", "exit-door", "achievement",
                                                       "flat-platform", "magnifying-glass", "price-tag",
                                                       "qaitbay-citadel", "large-paint-brush", "prisoner",
                                                       "magic-portal", "dodging", "highlighter",
                                                       "huts-village", "vertical-banner", "village", "stairs-goal",
                                                        "info", "pencil", "soldering-iron", "envelope", "peace-dove",
                                                        "sword-brandish", "frog-mouth-helm"};
            foreach (var icon in iconList)
            {
                capi.Gui.Icons.CustomIcons["claims:" + icon] = delegate (Context ctx, int x, int y, float w, float h, double[] rgba)
                {
                    AssetLocation location = new AssetLocation("claims:textures/icons/" + icon + ".svg");
                    IAsset svgAsset = capi.Assets.TryGet(location, true);
                    int value = ColorUtil.ColorFromRgba(175, 200, 175, 125);
                    capi.Gui.DrawSvg(svgAsset, ctx.GetTarget() as ImageSurface, x, y, (int)w, (int)h, new int?(value));
                };
            }
        }
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("CANTempleBlock", typeof(CANTempleBlock));
        }
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            clientModInstance = this;
            capi = api;

            AddCustomIcons();

            harmonyInstance = new Harmony(harmonyID);
            ApplyPatches.ApplyClientPatches(harmonyInstance, harmonyID);

            PermsHandler.initDicts();
            if (config == null)
            {
                Config.LoadConfig(capi);
            }
            PlotInfo.initDicts();


            clientChannel = api.Network.RegisterChannel("claimsExt");
            Common.RegisterMessageTypes(clientChannel, capi);
            clientDataStorage = new DataStorage(false);

            mapmgr = api.ModLoader.GetModSystem<WorldMapManager>();
            mapmgr.RegisterMapLayer<PlotsMapLayer>("Plots", 2);
            plotsMapLayer = mapmgr.MapLayers.OfType<PlotsMapLayer>().FirstOrDefault();

            pmlc = new PlayerMovementListnerClient();
            ClientEvents.AddEvents(capi, pmlc);

            int chunkSize = api.World.BlockAccessor.ChunkSize;

            capi.Event.RegisterEventBusListener(onPlayerChangePlotEvent, 0.5, "claimsPlayerChangePlot");

            api.Input.RegisterHotKey("canclaimsgui", "CAN Claims GUI", GlKeys.P, HotkeyType.GUIOrOtherControls, ctrlPressed: true);
            api.Input.SetHotKeyHandler("canclaimsgui", new ActionConsumable<KeyCombination>(this.OnHotKeySkillDialog));

            api.Input.RegisterHotKey("claimsplayermovementgui", "Plot info GUI", GlKeys.K, HotkeyType.GUIOrOtherControls);
            api.Input.SetHotKeyHandler("claimsplayermovementgui", new ActionConsumable<KeyCombination>(this.OnHotKeyPlayerMovementGUI));

            api.Event.LeftWorld += ShutDownClient;

            ClientPacketHandlers.RegisterHandlers();
            CANCityGui = new CANClaimsGui(capi);
        }
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            modInstance = this;
            sapi = api;
            Config.LoadConfig(sapi);

            PermsHandler.initDicts();
            PlotInfo.initDicts();

            //STORAGE WITH CITIES/PLAYERS/OTHER
            dataStorage = new DataStorage();

            sapi.Logger.Event("[claims] RegisterChannel(claimsExt)");
            serverChannel = sapi.Network.RegisterChannel("claimsExt");
            Common.RegisterMessageTypes(serverChannel, sapi);

            PlotEvents.lastTimePlayerAskedForPlotsAround = new Dictionary<string, long>();

            //Events for exclaims
            api.Event.RegisterEventBusListener(PlotEvents.updatedPlotHandlerUnclaimed, 0.5, "plotunclaimed");
            api.Event.RegisterEventBusListener(PlotEvents.updatedPlotHandlerClaimed, 0.5, "plotclaimed");

            sapi.Event.ServerRunPhase(EnumServerRunPhase.RunGame, loadShowChunksMsgsValues);
            sapi.Event.ServerRunPhase(EnumServerRunPhase.Shutdown, ShutDownServer);

            ///////////////////////////////
            harmonyInstance = new Harmony(harmonyID);
            ApplyPatches.ApplyServerPatches(harmonyInstance, harmonyID);
            serverPlayerMovementListener = new PlayerMovementsListnerServer();           
            //uses playermovement method
            events.ServerEvents.AddEvents(sapi);
            commands.register.ServerCommands.RegisterCommands(sapi);
            RightsHandler.readOrCreateRightPerms();
            TimerGeneral.StartServerTimers(sapi);

            ServerPacketHandlers.RegisterHandlers();
        }   
        public static void NullOnServerExit()
        {
            dataStorage = null;
            databaseHandler = null;
        }       
        public static void loadShowChunksMsgsValues()
        {
            var showMsgsDict = sapi.WorldManager.SaveGame.GetData<Dictionary<string, int>>("claimsshowchunkmsgs");
            
            if (showMsgsDict != null)
            {
                foreach (var pl in claims.dataStorage.getPlayersDict())
                {
                    if (showMsgsDict.TryGetValue(pl.Key, out int value))
                    {
                        pl.Value.showPlotMovement = (EnumShowPlotMovement)value;
                    }
                }
            }

            string claimAreas = sapi.WorldManager.SaveGame.GetData<string>("claimareas");

            if (claimAreas != null)
            {
                claims.dataStorage.serverClaimAreaHandler.DeserializeData(claimAreas);
            }

        }           
        public static bool loadDatabase()
        {
            try
            {
                databaseHandler = new SQLiteDatabaseHanlder();
                claims.getModInstance().getDatabaseHandler().loadDummyWolrdInfo();
            }
            catch (SqliteException ex)
            {
                MessageHandler.sendErrorMsg("loadDatabase:" + ex.Message);
                return false;
            }
            return false;
        }
        public static bool saveDatabase()
        {
            try
            {
                databaseHandler.saveEveryThing();
            }
            catch (SqliteException ex)
            {
                MessageHandler.sendErrorMsg("saveDatabase:" + ex.Message);
                return false;
            }
            return false;
        }

        public static void ShutDownServer()
        {
            ServerEvents.onShutdown();
            harmonyInstance.UnpatchAll(harmonyID);
            harmonyInstance = null;
            modInstance = null;
            
            serverChannel = null;
            databaseHandler = null;
            dataStorage = null;
            serverPlayerMovementListener = null;
            config = null;
            economyHandler = null;

        }

        public static void ShutDownClient()
        {
            if (claims.clientModInstance.pmlc != null && clientDataStorage != null)
            {
                claims.clientModInstance.pmlc.saveActiveZonesToDb();
            }
            if (claims.clientModInstance.pmlc != null)
            {
                claims.clientModInstance.pmlc.OnShutDown();
            }
            CANCityGui = null;
            harmonyInstance.UnpatchAll(harmonyID);
            harmonyInstance = null;
            clientModInstance = null;
            clientChannel = null;
            clientDataStorage = null;
            playerCityInfo = null;
            movementClaimGui = null;
            config = null;    
        }

        /*==============================================================================================*/
        /*=====================================ClaimsExt================================================*/
        /*==============================================================================================*/
        private bool OnHotKeySkillDialog(KeyCombination comb)
        {
            CANCityGui ??= new CANClaimsGui(capi);
            if (CANCityGui.IsOpened())
            {
                CANCityGui.TryClose();
            }
            else
                CANCityGui.TryOpen();
            return true;
        }
        private bool OnHotKeyPlayerMovementGUI(KeyCombination comb)
        {
            movementClaimGui ??= new ClaimsPlayerMovementGUI(capi);
            if (movementClaimGui.IsOpened())
            {
                movementClaimGui.TryClose();
            }
            else
                movementClaimGui.TryOpen();
            return true;
        }
        public void onPlayerChangePlotEvent(string eventName, ref EnumHandling handling, IAttribute data)
        {

            TreeAttribute tree = data as TreeAttribute;
            Vec2i from = new Vec2i(tree.GetInt("xChO"), tree.GetInt("zChO"));
            Vec2i to = new Vec2i(tree.GetInt("xCh"), tree.GetInt("zCh"));

            clientDataStorage.getSavedPlot(from, out SavedPlotInfo savedPlotInfoFrom);
            clientDataStorage.getSavedPlot(to, out SavedPlotInfo savedPlotInfoTo);
            //var c = capi.Assets;
            //STILL IN THE WILD LANDS
            if (savedPlotInfoFrom == null && savedPlotInfoTo == null)
            {
                //from wild to wild
                return;
            }
            if (movementClaimGui == null)
            {
                movementClaimGui = new ClaimsPlayerMovementGUI(capi);
                movementClaimGui.TryOpen();
                capi.Event.RegisterCallback((dt =>
                {
                    checkMovementStatus();
                }), 1000 * 25);
                return;
            }


            if (!movementClaimGui.IsOpened())
            {
                movementClaimGui.TryOpen();
                capi.Event.RegisterCallback((dt =>
                {
                    checkMovementStatus();
                }), 1000 * 25);
            }

            if (savedPlotInfoTo == null)
            {
                updateMovementGUIInfo();
            }
            else
            {
                updateMovementGUIInfo(savedPlotInfoTo);
            }
            movementClaimGui.timeStampShouldBeClosed = TimeFunctions.getEpochSeconds() + 15;
        }
        public void checkMovementStatus()
        {
            if (movementClaimGui.timeStampShouldBeClosed < TimeFunctions.getEpochSeconds())
            {
                movementClaimGui.TryClose();
                //capi.World.Player.ShowChatNotification("close gui");
            }
            else
            {
                //capi.World.Player.ShowChatNotification("wait again");
                capi.Event.RegisterCallback((dt =>
                {
                    checkMovementStatus();
                }), 1000 * 15);
            }
        }
        public static void updateMovementGUIInfo(SavedPlotInfo plot = null)
        {
            if (plot == null)
            {
                var cai = CairoFont.WhiteDetailText().WithFontSize(18);
                movementClaimGui.SingleComposer.GetRichtext("line_1")
                    .SetNewText(Lang.Get("claims:movementgui-wild-lands"), cai);
                movementClaimGui.SingleComposer.GetRichtext("line_2")
              .SetNewText("", cai);
                movementClaimGui.SingleComposer.GetRichtext("line_3")
               .SetNewText("", cai);
                movementClaimGui.SingleComposer.GetRichtext("line_4")
               .SetNewText("", cai);
                movementClaimGui.SingleComposer.GetRichtext("line_5")
              .SetNewText("", cai);
            }
            else
            {
                var cai = CairoFont.WhiteDetailText().WithFontSize(18).WithOrientation(EnumTextOrientation.Center);
               /* var f = movementClaimGui.Single*Composer.GetRichtext("line_1");
                var p = f.Components[0];*/

                //(p as RichTextComponent).Font.Orientation = EnumTextOrientation.Center;
                movementClaimGui.SingleComposer.GetRichtext("line_1")
               .SetNewText(Lang.Get("claims:movementgui-city-name", plot.cityName), cai);
                
                int currentLine = 2;

                if (plot.plotName.Length > 0)
                {
                    movementClaimGui.SingleComposer.GetRichtext("line_" + currentLine)
                   .SetNewText(Lang.Get("claims:movementgui-plot-name", plot.plotName), cai);
                    currentLine++;
                }

                if (plot.groupName.Length > 0)
                {
                    movementClaimGui.SingleComposer.GetRichtext("line_" + currentLine)
                    .SetNewText(Lang.Get("claims:movementgui-group-name", plot.groupName), cai);
                    currentLine++;
                }

                if (plot.PvPIsOn)
                {
                    movementClaimGui.SingleComposer.GetRichtext("line_" + currentLine)
                    .SetNewText(Lang.Get("claims:movementgui-pvp-on"), cai);
                }
                else
                {
                    movementClaimGui.SingleComposer.GetRichtext("line_" + currentLine)
                    .SetNewText(Lang.Get("claims:movementgui-pvp-off"), cai);
                }
                currentLine++;
                if (plot.price > -1)
                {
                    movementClaimGui.SingleComposer.GetRichtext("line_" + currentLine)
                   .SetNewText(Lang.Get("claims:movementgui-price", plot.price), cai);
                    currentLine++;
                }
                
                for (; currentLine <= 5; currentLine++)
                {
                    movementClaimGui.SingleComposer.GetRichtext("line_" + currentLine)
                   .SetNewText("", cai);
                }

                //claimsext.movementClaimGui.SetupDialog();
            }

        }

    }
}