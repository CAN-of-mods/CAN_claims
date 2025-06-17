using caneconomy.src.accounts;
using caneconomy.src.implementations.RealMoney;
using caneconomy.src.implementations.VirtualMoney;
using claims.src.auxialiry;
using claims.src.gui.playerGui.structures;
using claims.src.part;
using claims.src.part.structure;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

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
            if(caneconomy.caneconomy.config.SELECTED_ECONOMY_HANDLER == "REAL_MONEY")
            {
                claims.economyHandler = caneconomy.caneconomy.getHandler();
                //new RealMoneyEconomyHandler();
                caneconomy.caneconomy.OnBlockRemovedBlockEntityOpenableContainer += OnEconomyActions.OnBlockRemoved;
                caneconomy.caneconomy.OnReceivedClientPacketBlockEntitySign += OnEconomyActions.OnButtonSave;
            }

            if (caneconomy.caneconomy.config.SELECTED_ECONOMY_HANDLER == "VIRTUAL_MONEY")
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
                                                .WithArgs(parsers.Int("amount"))
                                            .EndSub()

                                            ;
                claims.sapi.ChatCommands.Get("alliance")
                                            .BeginSub("balance")
                                                .HandleWith(commands.MoneyCommands.OnAllianceBalance)
                                                .WithDesc("Show alliance balance.")
                                            .EndSub()
                                            .BeginSub("deposit")
                                                .HandleWith(commands.MoneyCommands.OnAllianceDeposit)
                                                .WithDesc("Deposit money to alliance account.")
                                            .EndSub()
                                            .BeginSub("withdraw")
                                                .HandleWith(commands.MoneyCommands.OnAllianceWithdraw)
                                                .WithDesc("Withdraw money from alliance account.")
                                                .WithArgs(parsers.Int("amount"))
                                            .EndSub()

                                            ;
            }
            claims.dataStorage.FillHashSets(claims.sapi);
            Dictionary<string, ClientCityInfoCellElement> CityStatsCashe =
                ObjectCacheUtil.GetOrCreate<Dictionary<string, ClientCityInfoCellElement>>(claims.sapi,
                "claims:cityinfocache", () => new Dictionary<string, ClientCityInfoCellElement>());
            foreach (var it in claims.dataStorage.getCitiesList())
            {
                if (CityStatsCashe.TryGetValue(it.Guid, out var stat))
                {
                    stat.AllianceName = it?.Alliance.GetPartName() ?? "";
                    stat.MayorName = it.getMayor()?.GetPartName() ?? "";
                    stat.Name = it.GetPartName();
                    stat.InvMsg = it.invMsg;
                    stat.TimeStampCreated = it.TimeStampCreated;
                    stat.CitizensAmount = it.getCityCitizens().Count;
                    stat.Open = it.openCity;
                    stat.ClaimedPlotsAmount = it.getCityPlots().Count;
                }
                else
                {
                    var f = it?.getMayor()?.GetPartName() ?? "";
                    CityStatsCashe.Add(it.Guid, new ClientCityInfoCellElement(it.getCityCitizens().Count, it.getMayor()?.GetPartName() ?? "",
                        it.getCityPlots().Count, it.Alliance?.GetPartName() ?? "", it.TimeStampCreated, it.GetPartName(), it.openCity,
                        it.invMsg, it.Guid));
                }
            }
            claims.sapi.Event.Timer(UsefullPacketsSend.CheckCitisUpdatedAndSend, claims.config.SEND_CITY_UPDATES_EVERY_N_SECONDS);
        }        
    }
}
