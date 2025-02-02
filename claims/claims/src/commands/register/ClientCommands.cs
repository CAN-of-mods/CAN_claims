using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace claims.src.commands.register
{
    public static class ClientCommands
    {
        public static void RegisterMapCommands(CommandArgumentParsers parsers, ICoreClientAPI capi)
        {
            capi.ChatCommands.Create("cmap")
                .WithDescription("todo")
                .HandleWith(claims.clientModInstance.plotsMapLayer.onMapCmd)
               .RequiresPlayer().RequiresPrivilege(Privilege.chat)
               .WithArgs(parsers.Word("todo"))
               ;
        }
        public static void RegisterCommands(ICoreClientAPI capi)
        {
            var parsers = capi.ChatCommands.Parsers;
            RegisterMapCommands(parsers, capi);
        }
    }
}
