using claims.src.beb;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

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
