using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using claims.src.auxialiry;
using claims.src.beb;
using claims.src.part.structure;
using claims.src.part.structure.conflict;
using claims.src.part.structure.war;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace claims.src.blocks
{
    public class CaptureFlagBlock: Block
    {
        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            if(world.BlockAccessor.GetBlockEntity(pos)?.GetBehavior<BlockEntityBehaviorFlag>() is BlockEntityBehaviorFlag beh)
            {
                return beh.TimesToBreak.ToString();
            }
            return "";
        }
    }
}
