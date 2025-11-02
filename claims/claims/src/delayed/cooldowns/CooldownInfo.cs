namespace claims.src.delayed.cooldowns
{
    public class CooldownInfo
    {
        long timestampFinish;
        CooldownType type;
        public CooldownInfo(long stamp, CooldownType type)
        {
            timestampFinish = stamp;
            this.type = type;
        }

        public CooldownType getType()
        {
            return type;
        }
        public long getStamp()
        {
            return timestampFinish;
        }
    }
}
