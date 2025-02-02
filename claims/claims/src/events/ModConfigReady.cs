using caneconomy.src.accounts;
using caneconomy.src.implementations.RealMoney;
using caneconomy.src.implementations.VirtualMoney;
using claims.src.auxialiry;
using claims.src.part.structure;
using System;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Server;

namespace claims.src.events
{
    public class ModConfigReady
    {
        public static void onModsAndConfigReady()
        {
            claims.loadDatabase();
            claims.getModInstance().getDatabaseHandler().loadEveryThing();
            Settings.loadAll();
            foreach (var plot in claims.dataStorage.getClaimedPlots().Values)
            {
                claims.dataStorage.addPlotToZoneSet(plot);
            }
            claims.dataStorage.ResetAllZoneTimestamps();
            var world = claims.dataStorage.getWorldInfo();
            if (world == null)
            {
                world = new WorldInfo(claims.sapi.World.Seed.ToString(), Guid.NewGuid().ToString());
                world.saveToDatabase();
            }
            if(claims.config.SELECTED_ECONOMY_HANDLER == "REAL_MONEY")
            {
                claims.economyHandler = caneconomy.caneconomy.getHandler();
                //new RealMoneyEconomyHandler();
                caneconomy.caneconomy.OnBlockRemovedBlockEntityOpenableContainer += OnEconomyActions.OnBlockRemoved;
                caneconomy.caneconomy.OnReceivedClientPacketBlockEntitySign += OnEconomyActions.OnButtonSave;
            }
            if (claims.config.SELECTED_ECONOMY_HANDLER == "VIRTUAL_MONEY")
            {
                var parsers = claims.sapi.ChatCommands.Parsers;
                claims.economyHandler = caneconomy.caneconomy.getHandler();
                   // new VirtualMoneyEconomyHandler();

                claims.sapi.ChatCommands.Get("city")
                                            .BeginSub("balance")
                                                .HandleWith(commands.MoneyCommands.OnCityBalance)
                                                .WithDesc("Show city balance.")
                                            .EndSub()
                                            .BeginSub("deposit")
                                                .HandleWith(commands.MoneyCommands.OnCityDeposit)
                                                .WithDesc("Deposit money to city account.")
                                            .EndSub()
                                            .BeginSub("withdraw")
                                                .HandleWith(commands.MoneyCommands.OnCityWithdraw)
                                                .WithDesc("Withdraw money from city account.")
                                            .EndSub()

                                            ;
            }
            claims.dataStorage.FillHashSets(claims.sapi);
        }

    }
}
