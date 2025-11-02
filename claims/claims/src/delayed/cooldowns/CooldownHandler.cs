using System.Collections.Generic;
using claims.src.auxialiry;
using claims.src.part.interfaces;

namespace claims.src.delayed.cooldowns
{
    public class CooldownHandler
    {
        public static Dictionary<ICooldown, HashSet<CooldownInfo>> cooldowns = new Dictionary<ICooldown, HashSet<CooldownInfo>>();

        public static void processCooldowns()
        {
            if (cooldowns == null || cooldowns.Count == 0)
                return;

            long timeNow = TimeFunctions.getEpochSeconds();
            foreach (HashSet<CooldownInfo> cooldownInfo in cooldowns.Values)
            {
                foreach (CooldownInfo cooldown in cooldownInfo)
                {
                    if (cooldown.getStamp() < timeNow)
                    {
                        cooldownInfo.Remove(cooldown);
                        break;
                    }
                }
            }
        }
        public static long hasCooldown(ICooldown canHasCooldown, CooldownType cooldownType)
        {
            if (!cooldowns.TryGetValue(canHasCooldown, out HashSet<CooldownInfo> infosSet))
            {
                return 0;
            }
            else
            {
                foreach (CooldownInfo cooldown in infosSet)
                {
                    if (cooldown.getType().Equals(cooldownType))
                    {
                        return cooldown.getStamp();
                    }
                }
                return 0;
            }
        }
        public static void addCooldown(ICooldown target, CooldownInfo cooldownInfo)
        {
            if (cooldowns.ContainsKey(target))
            {
                cooldowns[target].Add(cooldownInfo);
            }
            else
            {
                cooldowns.Add(target, new HashSet<CooldownInfo> { cooldownInfo });
            }
        }
    }
}
