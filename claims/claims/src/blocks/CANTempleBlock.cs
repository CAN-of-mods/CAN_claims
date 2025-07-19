using claims.src.auxialiry;
using claims.src.part.structure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace claims.src.blocks
{
    public class CANTempleBlock : BlockShapeFromAttributes
    {
        public override string ClassType
        {
            get
            {
                return "cantempleblock";
            }
        }

        public override IEnumerable<IShapeTypeProps> AllTypes
        {
            get
            {
                return this.clutterByCode.Values;
            }
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            //api.Event.RegisterEventBusListener(new EventBusListenerDelegate(this.onExpClang), 0.5, "expclang");
        }

        private void onExpClang(string eventName, ref EnumHandling handling, IAttribute data)
        {
            ITreeAttribute tree = data as ITreeAttribute;
            foreach (KeyValuePair<string, ClutterTypeProps> val in this.clutterByCode)
            {
                string str = (this.Code.Domain == "game") ? "" : (this.Code.Domain + ":");
                string classType = this.ClassType;
                string str2 = "-";
                string key = val.Key;
                string langKey = str + classType + str2 + ((key != null) ? key.Replace("/", "-") : null);
                if (!Lang.HasTranslation(langKey, true, true))
                {
                    tree[langKey] = new StringAttribute(string.Concat(new string[]
                    {
                        "\t\"",
                        langKey,
                        "\": \"",
                        Lang.GetNamePlaceHolder(new AssetLocation(val.Key)),
                        "\","
                    }));
                }
            }
        }

        public override void LoadTypes()
        {
            ClutterTypeProps[] array = this.Attributes["types"].AsObject<ClutterTypeProps[]>(null);
            this.basePath = "game:shapes/" + this.Attributes["shapeBasePath"].AsString(null) + "/";
            List<JsonItemStack> stacks = new List<JsonItemStack>();
            ModelTransform defaultGui = ModelTransform.BlockDefaultGui();
            ModelTransform defaultFp = ModelTransform.BlockDefaultFp();
            ModelTransform defaultTp = ModelTransform.BlockDefaultTp();
            ModelTransform defaultGround = ModelTransform.BlockDefaultGround();
            foreach (ClutterTypeProps ct in array)
            {
                this.clutterByCode[ct.Code] = ct;
                if (ct.GuiTf != null)
                {
                    ct.GuiTransform = new ModelTransform(ct.GuiTf, defaultGui);
                }
                if (ct.FpTf != null)
                {
                    ct.FpTtransform = new ModelTransform(ct.FpTf, defaultFp);
                }
                if (ct.TpTf != null)
                {
                    ct.TpTransform = new ModelTransform(ct.TpTf, defaultTp);
                }
                if (ct.GroundTf != null)
                {
                    ct.GroundTransform = new ModelTransform(ct.GroundTf, defaultGround);
                }
                if (ct.ShapePath == null)
                {
                    ct.ShapePath = AssetLocation.Create(this.basePath + ct.Code + ".json", this.Code.Domain);
                }
                else if (ct.ShapePath.Path.StartsWith('/'))
                {
                    ct.ShapePath.WithPathPrefixOnce("shapes").WithPathAppendixOnce(".json");
                }
                else
                {
                    ct.ShapePath.WithPathPrefixOnce(this.basePath).WithPathAppendixOnce(".json");
                }
                JsonItemStack jstack = new JsonItemStack
                {
                    Code = this.Code,
                    Type = EnumItemClass.Block,
                    Attributes = new JsonObject(JToken.Parse("{ \"type\": \"" + ct.Code + "\" }"))
                };
                jstack.Resolve(this.api.World, this.ClassType + " type", true);
                stacks.Add(jstack);
            }
            this.CreativeInventoryStacks = new CreativeTabAndStackList[]
            {
                new CreativeTabAndStackList
                {
                    Stacks = stacks.ToArray(),
                    Tabs = new string[]
                    {
                        "general",
                        "Claims"
                    }
                }
            };
        }

        public static string Remap(IWorldAccessor worldAccessForResolve, string type)
        {
            if (type.StartsWithFast("pipes/"))
            {
                return "pipe-veryrusted-" + type.Substring(6);
            }
            return type;
        }

        public override bool IsClimbable(BlockPos pos)
        {
            BEBehaviorShapeFromAttributes bec = this.GetBEBehavior<BEBehaviorShapeFromAttributes>(pos);
            if (bec != null && bec.Type != null && this.clutterByCode.TryGetValue(bec.Type, out ClutterTypeProps props))
            {
                return props.Climbable;
            }
            return this.Climbable;
        }

        public override IShapeTypeProps GetTypeProps(string code, ItemStack stack, BEBehaviorShapeFromAttributes be)
        {
            if (code == null)
            {
                return null;
            }
            this.clutterByCode.TryGetValue(code, out ClutterTypeProps cprops);
            return cprops;
        }

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return new BlockDropItemStack[]
            {
                new BlockDropItemStack(handbookStack, 1f)
            };
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            dsc.AppendLine(Lang.Get("claims:cantempleblock-desc", Array.Empty<object>()));
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);
            if (world.Side == EnumAppSide.Server)
            {
                claims.dataStorage.getPlot(PlotPosition.fromBlockPos(blockPos), out Plot plot);
                if (plot == null)
                {
                    return;
                }
                if (plot.Type != PlotType.TEMPLE)
                {
                    return;
                }

                plot.getCity().AddTempleRespawnPoint(plot, blockPos);
                plot.getCity().saveToDatabase();
                foreach (var pl in world.GetPlayersAround(blockPos.ToVec3d(), 10, 10))
                {
                    world.PlaySoundAt(new AssetLocation("game:sounds/block/heavymetal-hit"), pl, null, true, 32f, 1f);
                }
                
            }
            
        }
        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            ItemStack it = new ItemStack(world.GetBlock(new AssetLocation("claims:cantempleblock")), 1);
            it.Attributes.SetString("type", GetBEBehavior<BEBehaviorShapeFromAttributes>(pos)?.Type);
            return new ItemStack[] { it };
        }
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
            if (world.Side == EnumAppSide.Server)
            {
                claims.dataStorage.getPlot(PlotPosition.fromBlockPos(pos), out Plot plot);
                if (plot == null)
                {
                    return;
                }
                if (plot.Type != PlotType.TEMPLE)
                {
                    return;
                }
                plot.getCity().RemoveTempleRespawnPoint(plot);
            }
        }

        public Dictionary<string, ClutterTypeProps> clutterByCode = new Dictionary<string, ClutterTypeProps>();

        private string basePath;
    }
}
