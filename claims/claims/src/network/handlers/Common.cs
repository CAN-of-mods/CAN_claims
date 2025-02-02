using claims.src.network.packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace claims.src.network.handlers
{
    public static class Common
    {
        public static void RegisterMessageTypes(INetworkChannel channel, ICoreAPI api)
        {
            api.Logger.VerboseDebug("[claims] RegisterMessageType(SavedPlotsPacket)");
            channel.RegisterMessageType(typeof(SavedPlotsPacket));

            api.Logger.VerboseDebug("[claims] RegisterMessageType(PlayerGuiRelatedInfoPacket)");
            channel.RegisterMessageType(typeof(PlayerGuiRelatedInfoPacket));

            api.Logger.VerboseDebug("[claims] RegisterMessageType(ConfigUpdateValuesPacket)");
            channel.RegisterMessageType(typeof(ConfigUpdateValuesPacket));

            api.Logger.VerboseDebug("[claims] RegisterMessageType(ClaimAreasPacket)");
            channel.RegisterMessageType(typeof(ClaimAreasPacket));
        }
    }
}
