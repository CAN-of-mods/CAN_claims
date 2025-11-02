using claims.src.part;
using Vintagestory.API.MathTools;

namespace claims.src.delayed.teleportation
{
    public class TeleportationInfo
    {
        PlayerInfo targetPlayer;
        Vec3d targetPoint;
        bool canBeCanceled;
        long timeStampWhenFinished;
        public TeleportationInfo(PlayerInfo playerInfo, Vec3d targetPoint, bool canBeCanceled, long timeStamp)
        {
            targetPlayer = playerInfo;
            this.targetPoint = targetPoint;
            this.canBeCanceled = canBeCanceled;
            timeStampWhenFinished = timeStamp;
        }
        public PlayerInfo getTargetPlayer()
        {
            return targetPlayer;
        }
        public Vec3d getTargetPoint()
        {
            return targetPoint;
        }
        public bool getCanBeCanceled()
        {
            return canBeCanceled;
        }
        public long getTimeStamp()
        {
            return timeStampWhenFinished;
        }
    }
}
