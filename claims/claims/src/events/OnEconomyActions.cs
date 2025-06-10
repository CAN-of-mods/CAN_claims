using caneconomy.src.implementations.RealMoney;
using claims.src.auxialiry;
using claims.src.messages;
using claims.src.part;
using claims.src.part.structure;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace claims.src.events
{
    public class OnEconomyActions
    {
        public static void OnBlockRemoved(BlockEntityOpenableContainer be)
        {
            if ((be is BlockEntityGenericTypedContainer))
            {
                Vec3i tmp = new Vec3i(be.Pos);
                string accName = "";

                if((claims.economyHandler as RealMoneyEconomyHandler).TryGetRealBankInfo(tmp, out RealBankInfo rbi))
                {
                    claims.economyHandler.deleteAccount(rbi.AccountName);
                    if (accName.StartsWith(claims.config.CITY_ACCOUNT_STRING_PREFIX))
                    {
                        claims.dataStorage.getCityByName(accName.Substring(claims.config.CITY_ACCOUNT_STRING_PREFIX.Length), out City city);
                        if (city != null)
                        {
                            MessageHandler.sendMsgToPlayerInfo(city.getMayor(), Lang.Get("claims:city_bank_was_destroyed"));
                            return;
                        }
                        return;
                    }
                    else
                    {
                        claims.dataStorage.getPlayerByUid(accName, out PlayerInfo playerInfo);
                        if (playerInfo != null)
                        {
                            MessageHandler.sendMsgToPlayerInfo(playerInfo, Lang.Get("claims:your_bank_was_destroyed"));
                            return;
                        }
                        return;
                    }

                }
            }
        }

        public static void OnButtonSave(BlockEntitySign __instance, IPlayer player, int packetid)
        {
            if (packetid == 1002)
            {
                //Find where chest should be
                string signCodePath = __instance.Block.Code.Path;
                BlockPos chestPos = new(__instance.Pos.X, __instance.Pos.Y, __instance.Pos.Z);
                if (signCodePath.EndsWith("east"))
                {
                    chestPos.X += 1;
                }
                else if (signCodePath.EndsWith("west"))
                {
                    chestPos.X -= 1;
                }
                else if (signCodePath.EndsWith("north"))
                {
                    chestPos.Z -= 1;
                }
                else if (signCodePath.EndsWith("south"))
                {
                    chestPos.Z += 1;
                }

                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);

                //Remove all new lines, so we have all content as one long string
                string signText = __instance.text.Replace("\n", "");

                if (signText.StartsWith(Lang.Get("claims:economy_chest_bank_city_sign_prefix")))
                {
                    //City bank can be created only in city claimed plot
                    claims.dataStorage.getPlot(PlotPosition.fromXZ(chestPos.X, chestPos.Z), out Plot plot);
                    if (plot == null)
                    {
                        return;
                    }
                    //Village probably
                    City city = plot.getCity();
                    if (city == null)
                    {
                        MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_there_is_no_city_in_this_plot"));
                        return;
                    }
                    //Different city
                    if (signText.Substring(Lang.Get("claims:economy_chest_bank_city_sign_prefix").Length) != city.GetPartName().Replace("_", " "))
                    {
                        MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_different_name_city_plot_sign"));
                        return;
                    }
                    //Only by city mayor
                    if (playerInfo == null || !city.isMayor(playerInfo))
                    {
                        MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_you_need_to_be_mayor"));
                        return;
                    }
                    else
                    {
                        if ((claims.economyHandler as RealMoneyEconomyHandler).TryGetRealBankInfo(city.MoneyAccountName, out RealBankInfo tmpVecCity))
                        {
                            Vec3i RBIchestCoords = tmpVecCity.getChestCoors();
                            if ((RBIchestCoords.X == chestPos.X &&
                                                       RBIchestCoords.Z == chestPos.Z &&
                                                       RBIchestCoords.Y == chestPos.Y))
                            {
                                MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_chest_bank_already_set_here"));
                                return;
                            }
                            else
                            {
                                if (claims.economyHandler.accountExist(city.MoneyAccountName))
                                {
                                    claims.economyHandler.deleteAccount(city.MoneyAccountName);
                                    MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_city_chest_bank_removed"));
                                }
                                claims.economyHandler.newAccount(city.MoneyAccountName, new Dictionary<string, object> { { "chestPos", new Vec3i(chestPos.X, chestPos.Y, chestPos.Z) } });
                                MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_city_chest_bank_created"));
                                return;
                            }
                        }
                        else
                        {
                            if (claims.economyHandler.accountExist(city.MoneyAccountName))
                            {
                                claims.economyHandler.deleteAccount(city.MoneyAccountName);
                                MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_city_chest_bank_removed"));
                            }
                            claims.economyHandler.newAccount(city.MoneyAccountName, new Dictionary<string, object> { { "chestPos", new Vec3i(chestPos.X, chestPos.Y, chestPos.Z) } });
                            MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_city_chest_bank_created"));
                            return;
                        }
                    }
                }
                //player had perms to change text of sign if plot, so let him has here bank chest
                else if (signText.StartsWith(Lang.Get("claims:economy_chest_bank_player_sign_prefix")))
                {
                    //Different player's name
                    if (!player.PlayerName.Equals(signText.Substring(Lang.Get("claims:economy_chest_bank_player_sign_prefix").Length)))
                    {
                        MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_make_only_own_chest"));
                        return;
                    }
                    if ((claims.economyHandler as RealMoneyEconomyHandler).TryGetRealBankInfo(player.PlayerUID, out RealBankInfo tmpVecPlayer))
                    {
                        Vec3i RBIchestCoords = tmpVecPlayer.getChestCoors();
                        if (tmpVecPlayer != null && (RBIchestCoords.X == chestPos.X &&
                                                     RBIchestCoords.Z == chestPos.Z &&
                                                     RBIchestCoords.Y == chestPos.Y))
                        {
                            MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_chest_bank_already_set_here"));
                            return;
                        }
                    }
                    else
                    {
                        if (claims.economyHandler.accountExist(player.PlayerUID))
                        {
                            claims.economyHandler.deleteAccount(player.PlayerUID);
                            MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_player_chest_bank_removed"));
                        }
                        claims.economyHandler.newAccount(player.PlayerUID, new Dictionary<string, object> { { "chestPos", new Vec3i(chestPos.X, chestPos.Y, chestPos.Z) } });
                        MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_player_chest_bank_created"));
                        return;
                    }
                }
                else
                {
                    MessageHandler.sendMsgToPlayer(player as IServerPlayer, Lang.Get("claims:economy_help_creation"));
                    return;
                }


            }
            return;
        }

    }
}

