using claims.src.network.packets;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace claims.src.network.handlers
{
    public static class Common
    {
        public static void RegisterMessageTypes(INetworkChannel channel, ICoreAPI api)
        {
            if (claims.config.VERBOSE_LOGGING)
            {
                api.Logger.VerboseDebug("[claims] RegisterMessageType(SavedPlotsPacket)");
                channel.RegisterMessageType(typeof(SavedPlotsPacket));

                api.Logger.VerboseDebug("[claims] RegisterMessageType(PlayerGuiRelatedInfoPacket)");
                channel.RegisterMessageType(typeof(PlayerGuiRelatedInfoPacket));

                api.Logger.VerboseDebug("[claims] RegisterMessageType(ConfigUpdateValuesPacket)");
                channel.RegisterMessageType(typeof(ConfigUpdateValuesPacket));
            }
        }
    }
}
