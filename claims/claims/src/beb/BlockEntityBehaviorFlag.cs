using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using claims.src.auxialiry;
using claims.src.bb;
using claims.src.part.structure.conflict;
using claims.src.part.structure;
using claims.src.renderer;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using claims.src.part;
using claims.src.part.structure.war;
using claims.src.gui.playerGui.structures;
using static OpenTK.Graphics.OpenGL.GL;

namespace claims.src.beb
{
    public class BlockEntityBehaviorFlag : BlockEntityBehavior
    {
        public ItemStack Banner { get; protected set; }
        public float CapturedPercent { get; protected set; }
        protected float captureDuration;
        private FlagRenderer renderer;
        public BlockBehaviorFlag BlockBehavior { get; protected set; }

        private long? updateRef;
        private long? captureRef;
        private long? computeRef;
        public string AllianceGuid { get; set; }
        public string CityGuid { get; set; }
        public string ConflictGuid { get; set; }
        public string PlayerGuid { get; set; }
        public int TimesToBreak { get; set; } = 0;
        public BlockEntityBehaviorFlag(BlockEntity blockEntity) : base(blockEntity) { }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

            this.BlockBehavior = this.Block.GetBehavior<BlockBehaviorFlag>();

            this.captureDuration = claims.config.FLAG_CAPTURE_DURATION_SECONDS;

            this.Banner?.ResolveBlockOrItem(this.Api.World);
            if (this.Banner != null && this.Api is ICoreClientAPI client)
            {
                client.Tesselator.TesselateShape(this.Banner.Item, Shape.TryGet(client, "claims:shapes/flag/banner.json"), out MeshData meshData);
                this.renderer = new FlagRenderer(client, meshData, this.Pos, this, this.BlockBehavior.PoleTop, this.BlockBehavior.PoleBottom);
                client.Event.RegisterRenderer(this.renderer, EnumRenderStage.Opaque, "flag");

            }

            //this.updateRef = api.Event.RegisterGameTickListener(this.Update, 1000);

        }
        public override void OnBlockBroken(IPlayer byPlayer = null)
        {          
            base.OnBlockBroken(byPlayer);
            this.renderer?.Dispose();
            if (this.updateRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.updateRef.Value);
            if (this.computeRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.computeRef.Value);
            if (this.captureRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.captureRef.Value);
            if (this.Banner != null) this.Api.World.SpawnItemEntity(this.Banner, this.Pos.ToVec3d());
        }
        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            this.renderer?.Dispose();
            if (this.updateRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.updateRef.Value);
            if (this.computeRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.computeRef.Value);
            if (this.captureRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.captureRef.Value);
        }
        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            this.renderer?.Dispose();
            if (this.updateRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.updateRef.Value);
            if (this.computeRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.computeRef.Value);
            if (this.captureRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.captureRef.Value);
        }
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
        }
        private void Update(float deltaTime)
        {
            if(Banner == null)
            {
                return;
            }
            this.CapturedPercent += GameMath.Clamp(1.0f - this.CapturedPercent, -deltaTime / this.captureDuration, deltaTime / this.captureDuration);
            if (this.Api.Side == EnumAppSide.Server)
            {               
                if (this.CapturedPercent == 1f)
                {
                    if (this.captureRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.captureRef.Value);
                    this.captureRef = null;
                    this.Banner = null;
                    this.renderer?.Dispose();
                    this.renderer = null;
                    this.CapturedPercent = 0;
                    this.captureDuration = 0;
                    if (!ConflictHandler.TryGetConflictByGuid(this.ConflictGuid, out var conflict))
                    {
                        return;
                    }
                    if (!conflict.ActiveWarTime)
                    {
                        return;
                    }
                    if (!claims.dataStorage.WarsTimes.TryGetValue(conflict.Guid, out var warTime))
                    {
                        return;
                    }
                    if (!warTime.PlotAttacks.TryGetValue(PlotPosition.fromBlockPos(this.Pos), out var plotAttack))
                    {
                        return;
                    }
                    if (!claims.dataStorage.getCityByGUID(this.CityGuid, out City attackerCity))
                    {
                        return;
                    }
                    if (!claims.dataStorage.getPlot(PlotPosition.fromBlockPos(this.Pos), out var defenderPlot))
                    {
                        return;
                    }
                    if (defenderPlot.getCity().getCityPlots().Count == 1)
                    {
                        //TODO
                        //remove city and update all
                        warTime.PlotAttacks.Remove(PlotPosition.fromBlockPos(this.Pos));
                        TimesToBreak = 0;
                        this.Api.Event.RegisterCallback((float ft) =>
                        {
                            this.Api.World.BlockAccessor.BreakBlock(this.Pos, null);
                        }, 1000);
                        return;
                        return;
                    }
                    else
                    {
                        City defenderCity = defenderPlot.getCity();
                        defenderPlot.setCity(attackerCity);
                        defenderCity.getCityPlots().Remove(defenderPlot);
                        attackerCity.getCityPlots().Add(defenderPlot);
                        defenderPlot.setPlotOwner(null);
                        defenderPlot.UpdateBorderPlotValue();
                        defenderPlot.setCustomTax(0);
                        //shouldn't crash with default but better to remake it somehow with init functions
                        defenderPlot.setNewType(new TextCommandResult(), "default", null);
                        defenderPlot.setPlotGroup(null);
                        defenderPlot.extraBought = false;
                        defenderCity.saveToDatabase();
                        defenderPlot.saveToDatabase();
                        defenderPlot.getCity().saveToDatabase();

                        defenderPlot.CheckBorderPlotValue();
                        claims.dataStorage.setNowEpochZoneTimestampFromPlotPosition(defenderPlot.getPos());
                        claims.serverPlayerMovementListener.markPlotToWasReUpdated(defenderPlot.getPos());

                        UsefullPacketsSend.AddToQueueCityInfoUpdate(defenderPlot.getCity().Guid, EnumPlayerRelatedInfo.CLAIMED_PLOTS);
                        UsefullPacketsSend.AddToQueueCityInfoUpdate(defenderCity.Guid, EnumPlayerRelatedInfo.CLAIMED_PLOTS);
                        UsefullPacketsSend.AddToQueueAllPlayersInfoUpdate(new Dictionary<string, object> { { "value", defenderPlot.getPos() } }, EnumPlayerRelatedInfo.CITY_PLOT_RECOLOR);
                        warTime.PlotAttacks.Remove(PlotPosition.fromBlockPos(this.Pos));
                        TimesToBreak = 0;
                        this.Api.Event.RegisterCallback((float ft) =>
                        {
                            this.Api.World.BlockAccessor.BreakBlock(this.Pos, null);
                        }, 1000);
                        return;
                    }
                    TimesToBreak = 0;
                    //ModSystemBlockReinforcement bre = this.Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>(true);
                    //bre.ClearReinforcement(this.Pos);
                    warTime.PlotAttacks.Remove(PlotPosition.fromBlockPos(this.Pos));
                }
            }
        }
        public void TryStartCapture(IPlayer byPlayer)
        {
            if (this.Api.Side == EnumAppSide.Server)
            {
                if (this.updateRef == null)
                {
                    if (!claims.dataStorage.getPlayerByUid(byPlayer.PlayerUID, out var playerInfo))
                    {
                        return;
                    }
                    Alliance alliance = playerInfo.Alliance;
                    PlotPosition currentPlotPosition = PlotPosition.fromBlockPos(this.Pos);
                    if (!claims.dataStorage.getPlot(currentPlotPosition, out Plot plotHere))
                    {
                        return;
                    }
                    if (!plotHere.getCity()?.HasAlliance() ?? false)
                    {
                        return;
                    }
                    if (plotHere.getCity().Alliance.Equals(playerInfo.Alliance))
                    {
                        return;
                    }
                    if (!ConflictHandler.TryGetConflictWithSides(alliance, plotHere.getCity().Alliance, out var conflict))
                    {
                        return;
                    }
                    if (!conflict.ActiveWarTime)
                    {
                        return;
                    }
                    if (!claims.dataStorage.WarsTimes.TryGetValue(conflict.Guid, out var warTime))
                    {
                        return;
                    }
                    if (warTime.PlotAttacks.Count() >= claims.config.MAX_AMOUNT_OF_CAPTURE_FLAGS_ACTIVE)
                    {
                        return;
                    }
                    if (warTime.PlotAttacks.TryGetValue(currentPlotPosition, out var _))
                    {
                        return;
                    }

                    this.AllianceGuid = alliance.Guid;
                    this.CityGuid = playerInfo.City.Guid;
                    this.ConflictGuid = conflict.Guid;
                    this.PlayerGuid = playerInfo.Guid;
                    this.TimesToBreak = claims.config.FLAG_REINFORCEMENT_AMOUNT;
                    warTime.PlotAttacks.TryAdd(currentPlotPosition, new PlotAttack(this.Pos, this.PlayerGuid, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                    this.updateRef = this.Api.Event.RegisterGameTickListener(this.Update, 1000);
                    if (this.Banner == null
                        && byPlayer.InventoryManager.ActiveHotbarSlot.CanTake()
                        && (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Collectible.Code.Path.Contains("cloth-") ?? false)
                    )
                    {

                        this.Banner = byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);
                        this.Blockentity.MarkDirty();

                        if (this.Api is ICoreClientAPI client)
                        {

                            this.renderer?.Dispose();
                            client.Tesselator.TesselateShape(this.Banner.Item, Shape.TryGet(client, "claims:shapes/flag/banner.json"), out MeshData meshData);
                            this.renderer = new FlagRenderer(client, meshData, this.Pos, this, this.BlockBehavior.PoleTop, this.BlockBehavior.PoleBottom);
                            client.Event.RegisterRenderer(this.renderer, EnumRenderStage.Opaque, "flag");

                        }
                    }
                }
            }
        }
        public void EndCapture()
        {
            if (this.captureRef.HasValue) this.Api.Event.UnregisterGameTickListener(this.captureRef.Value);
            this.captureRef = null;
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            this.CapturedPercent = tree.GetFloat("capturedPercent");
            if (this.Api is ICoreClientAPI client)
            {
                if (this.Banner == null)
                {
                    var newBanner = tree.GetItemstack("banner");
                    if (newBanner != null)
                    {
                        this.Banner = newBanner;
                        this.Banner?.ResolveBlockOrItem(this.Api.World);
                        this.renderer?.Dispose();
                        client.Tesselator.TesselateShape(this.Banner.Item, Shape.TryGet(client, "claims:shapes/flag/banner.json"), out MeshData meshData);
                        this.renderer = new FlagRenderer(client, meshData, this.Pos, this, this.BlockBehavior.PoleTop, this.BlockBehavior.PoleBottom);
                        client.Event.RegisterRenderer(this.renderer, EnumRenderStage.Opaque, "flag");
                        if (this.updateRef == null)
                        {
                            this.updateRef = this.Api.Event.RegisterGameTickListener(this.Update, 1000);
                        }
                    }
                    else
                    {
                        if (this.updateRef == null)
                        {
                            this.Api.Event.UnregisterGameTickListener(this.updateRef.Value);
                        }
                    }
                }
            }
            this.Banner = tree.GetItemstack("banner");
            this.TimesToBreak = tree.GetInt("TimesToBreak");
            base.FromTreeAttributes(tree, worldForResolving);

        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {

            if (this.Block != null) tree.SetString("forBlockCode", this.Block.Code.ToShortString());

            if (this.Banner != null) tree.SetItemstack("banner", this.Banner); else tree.RemoveAttribute("banner");

            tree.SetFloat("capturedPercent", this.CapturedPercent);

            tree.SetInt("TimesToBreak", this.TimesToBreak);

            base.ToTreeAttributes(tree);

        }
    }
}
