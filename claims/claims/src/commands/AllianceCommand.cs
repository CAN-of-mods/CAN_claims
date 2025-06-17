using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using claims.src.agreement;
using claims.src.auxialiry;
using claims.src.delayed.cooldowns;
using claims.src.delayed.invitations;
using claims.src.messages;
using claims.src.part.structure;
using claims.src.part;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using System.Collections;
using claims.src.gui.playerGui.structures;

namespace claims.src.commands
{
    public class AllianceCommand : BaseCommand
    {
        public static TextCommandResult CreateAlliance(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
            if (playerInfo == null)
            {
                return TextCommandResult.Error("claims:no_such_player_info");
            }

            if (!playerInfo.hasCity())
            {
                return TextCommandResult.Error(Lang.Get("claims:no_city"));
            }
            City city = playerInfo.City;
            if (city.HasAlliance())
            {
                return TextCommandResult.Error(Lang.Get("claims:already_has_alliance"));
            }
            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Error(Lang.Get("claims:invalid_alliance_name"));
            }
            if (claims.dataStorage.GetAllianceByName(name, out Alliance targetAlliance))
            {
                return TextCommandResult.Error(Lang.Get("claims:already_exists_alliance"));
            }
            if (!city.isMayor(playerInfo))
            {
                return TextCommandResult.Error(Lang.Get("claims:should_be_mayor"));
            }
            if ((claims.economyHandler.getBalance(city.MoneyAccountName) < (decimal)claims.config.NEW_ALLIANCE_COST))
            {
                return TextCommandResult.Error(Lang.Get("claims:not_enough_money"));
            }
            AgreementHandler.addNewAgreementOrReplace(new Agreement(
                new Thread(new ThreadStart(() =>
                {
                if (playerInfo.hasCity() && !playerInfo.City.HasAlliance())
                {
                    if ((claims.economyHandler.getBalance(city.MoneyAccountName) < (decimal)claims.config.NEW_ALLIANCE_COST))
                    {
                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:not_enough_money"));
                        return;
                    }
                        claims.economyHandler.withdraw(city.MoneyAccountName, (decimal)claims.config.NEW_ALLIANCE_COST);
                        PartInits.InitNewAlliance(playerInfo, name);
                        MessageHandler.sendGlobalMsg(Lang.Get("claims:new_alliance_created", playerInfo.GetPartName(), name));
                    }
                    else
                    {
                        MessageHandler.sendGlobalMsg(Lang.Get("claims:alliance_wont_be_created"));
                        return;
                    }
                })), player.PlayerUID));
            return TextCommandResult.Success(Lang.Get("claims:help_agreement_new_alliance", claims.config.AGREEMENT_COMMAND));
        }
        public static TextCommandResult DeleteAlliance(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_player_info"));
            }
            if (!playerInfo.HasAlliance())
            {
                return TextCommandResult.Error(Lang.Get("claims:no_alliance"));
            }
            Alliance alliance = playerInfo.City.Alliance;
            if (!alliance.IsLeader(playerInfo))
            {
                return TextCommandResult.Error(Lang.Get("claims:alliance_not_a_leader"));
            }
            PartDemolition.DemolishAlliance(alliance);
            MessageHandler.sendGlobalMsg(Lang.Get("claims:alliance_was_deleted", alliance.GetPartName()));
            return TextCommandResult.Success();
        }
        public static TextCommandResult LeaveAlliance(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_player_info"));
            }
            if (!playerInfo.HasAlliance())
            {
                return TextCommandResult.Error(Lang.Get("claims:no_alliance"));
            }
            Alliance alliance = playerInfo.City.Alliance;
            if (!alliance.IsLeader(playerInfo))
            {
                return TextCommandResult.Error(Lang.Get("claims:must_not_be_leader"));
            }
            City city = playerInfo.City;
            if (!city.isMayor(playerInfo))
            {
                return TextCommandResult.Error(Lang.Get("claims:must_be_a_city_mayor"));
            }

            alliance.Cities.Remove(city);
            //delete alliance titles
            foreach (var it in city.getCityCitizens())
            {
                it.ClearAllAllianceTitles();
                RightsHandler.reapplyRights(it);
            }
            //delete hostile cities
            foreach (var it in alliance.Hostiles)
            {
                foreach (var hosCity in it.Cities)
                {
                    hosCity.HostileCities.Remove(city);
                    hosCity.saveToDatabase();
                }
            }
            //delete comrade cities
            foreach (var it in alliance.ComradAlliancies)
            {
                foreach (var comCity in it.Cities)
                {
                    comCity.ComradeCities.Remove(city);
                    comCity.saveToDatabase();
                }

            }
            MessageHandler.SendMsgInAlliance(alliance, Lang.Get("claims:city_left_alliance", city.getPartNameReplaceUnder()));
            MessageHandler.sendMsgInCity(city, Lang.Get("claims:our_city_left_alliance"));
            city.ComradeCities.Clear();
            city.HostileCities.Clear();
            city.Alliance = null;
            UsefullPacketsSend.AddToQueueCityInfoUpdate(city.GetPartName(), EnumPlayerRelatedInfo.OWN_ALLIANCE_REMOVE);
            city.saveToDatabase();
            alliance.saveToDatabase();
            return TextCommandResult.Success();
        }        
        public static TextCommandResult KickFromAlliance(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_player_info"));
            }
            if (!playerInfo.HasAlliance())
            {
                return TextCommandResult.Error(Lang.Get("claims:no_alliance"));
            }           
            if (!playerInfo.City.Alliance.IsLeader(playerInfo))
            {
                return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
            }

            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Error(Lang.Get("claims:invalid_city_name"));
            }
            if(!claims.dataStorage.getCityByName(name, out City city))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_city"));
            }

            Alliance alliance = playerInfo.City.Alliance;
            if (city.Equals(alliance.MainCity))
            {
                return TextCommandResult.Error(Lang.Get("claims:not_main_city_to_kick"));
            }
            if (!alliance.Cities.Contains(city))
            {
                return TextCommandResult.Error(Lang.Get("claims:city_not_in_alliance"));
            }

            alliance.Cities.Remove(city);
            foreach (PlayerInfo playerInCity in city.getPlayerInfos())
            {
                RightsHandler.reapplyRights(playerInCity);
            }
            /*if (alliance.runningConflicts.Count > 0)
            {
                foreach (Conflict conflict in alliance.runningConflicts)
                {
                    if (conflict.getFirstSide().Equals(alliance))
                    {
                        foreach (City hostileCity in conflict.getSecondSide().getCities())
                        {
                            hostileCity.getHostiles().Remove(city);
                            hostileCity.saveToDatabase();
                        }
                    }
                    else
                    {
                        foreach (City hostileCity in conflict.getFirstSide().getCities())
                        {
                            hostileCity.getHostiles().Remove(city);
                            hostileCity.saveToDatabase();
                        }
                    }

                }
            }*/
            alliance.saveToDatabase();
            return TextCommandResult.Success();
        }
        public static void processAllianceUninvite(IServerPlayer player, CmdArgs args, TextCommandResult res)
        {

        }
        public static TextCommandResult InviteToAlliance(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_player_info"));
            }
            if (!playerInfo.HasAlliance())
            {
                return TextCommandResult.Error(Lang.Get("claims:no_alliance"));
            }
            if (!playerInfo.City.Alliance.IsLeader(playerInfo))
            {
                return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
            }

            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Error(Lang.Get("claims:invalid_city_name"));
            }
            if(!claims.dataStorage.getCityByName(name, out City city))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_city"));
            }
            Alliance alliance = playerInfo.City.Alliance;
            if (alliance.Cities.Contains(city))
            {
                return TextCommandResult.Success(Lang.Get("claims:already_our_city"));
            }
            if (city.HasAlliance())
            {
                return TextCommandResult.Success(Lang.Get("claims:has_other_alliance"));
            }
            if (InvitationHandler.addNewInvite(new Invitation(alliance, city, TimeFunctions.getEpochSeconds() + claims.config.HOUR_TIMEOUT_INVITATION_TO_ALLIANCE * 60,
                new Thread(new ThreadStart(() =>
                {
                    alliance.Cities.Add(city);
                    /*if (alliance.runningConflicts.Count > 0)
                    {
                        foreach (Conflict conflict in alliance.runningConflicts)
                        {
                            if (conflict.getFirstSide().Equals(alliance))
                            {
                                foreach (City hostileCity in conflict.getSecondSide().getCities())
                                {
                                    hostileCity.hostileCities.Add(city);
                                    hostileCity.saveToDatabase();
                                    city.hostileCities.Add(hostileCity);
                                }
                            }
                            else
                            {
                                foreach (City hostileCity in conflict.getFirstSide().getCities())
                                {
                                    hostileCity.hostileCities.Add(city);
                                    hostileCity.saveToDatabase();
                                    city.hostileCities.Add(hostileCity);
                                }
                            }
                        }
                    }*/
                    foreach (PlayerInfo playerInCity in city.getPlayerInfos())
                    {
                        RightsHandler.reapplyRights(playerInCity);
                    }
                    MessageHandler.SendMsgInAlliance(alliance, Lang.Get("claims:city_joined_alliance", city.GetPartName()));
                    city.Alliance = alliance;
                    city.saveToDatabase();
                    alliance.saveToDatabase();

                })),
                new Thread(new ThreadStart(() =>
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:disagrre_with_invitation_to_alliance", playerInfo.GetPartName(), city.GetPartName()));
                }))
                )))
            {
                MessageHandler.sendMsgInCity(city, Lang.Get("claims:your_city_was_invited_to_alliance", alliance));
                return TextCommandResult.Success(Lang.Get("claims:invitation_to_alliance_was_sent", city.GetPartName()));
            }
            else
            {
                return TextCommandResult.Success(Lang.Get("claims:city_already_has_invitation"));
            }

        }
        public static TextCommandResult SetNameAlliance(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo))
            {
                return TextCommandResult.Success(Lang.Get("claims:no_such_player_info"));
            }
            if (!playerInfo.HasAlliance())
            {
                return TextCommandResult.Success(Lang.Get("claims:no_alliance"));
            }
            if (!playerInfo.City.Alliance.IsLeader(playerInfo))
            {
                return TextCommandResult.Success(Lang.Get("claims:you_dont_have_right_for_that_command"));
            }

            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Success(Lang.Get("claims:invalid_alliance_name"));
            }
            if (claims.dataStorage.GetAllianceByName(name, out Alliance alliance))
            {
                return TextCommandResult.Success(Lang.Get("claims:name_is_taken"));
            }
            alliance = playerInfo.Alliance;

            if (claims.economyHandler.getBalance(alliance.MoneyAccountName) < (decimal)claims.config.ALLIANCE_RENAME_COST)
            {
                return TextCommandResult.Success(Lang.Get("claims:not_enough_money"));
            }
            else
            {
                long stamp = CooldownHandler.hasCooldown(alliance, CooldownType.RENAMING);
                if (stamp != 0)
                {
                    return TextCommandResult.Success(Lang.Get("claims:wait_before") + TimeFunctions.getDateFromEpochSeconds(stamp));
                }
                else
                {
                    CooldownHandler.addCooldown(alliance, new CooldownInfo(TimeFunctions.getEpochSeconds() + claims.config.SECONDS_ALLIANCE_RENAME_COOLDOWN, CooldownType.RENAMING));
                    claims.economyHandler.withdraw(alliance.MoneyAccountName, (decimal)claims.config.ALLIANCE_RENAME_COST);
                    alliance.SetPartName(name);
                    alliance.saveToDatabase();
                    UsefullPacketsSend.AddToQueueAllianceInfoUpdate(alliance.Guid, new Dictionary<string, object> { { "value", alliance.Guid } }, EnumPlayerRelatedInfo.ALLIANCE_NAME);
                    return TextCommandResult.Success();
                }
            }
        }
        public static TextCommandResult SetFeeAlliance(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo))
            {
                return TextCommandResult.Success(Lang.Get("claims:no_such_player_info"));
            }
            if (!playerInfo.HasAlliance())
            {
                return TextCommandResult.Success(Lang.Get("claims:no_alliance"));
            }
            if (!playerInfo.City.Alliance.IsLeader(playerInfo))
            {
                return TextCommandResult.Success(Lang.Get("claims:you_dont_have_right_for_that_command"));
            }

            int fee = (int)args.Parsers[0].GetValue();

            if (fee < 0 || fee > claims.config.ALLIANCE_MAX_FEE)
            {
                return TextCommandResult.Success(Lang.Get("claims:alliance_fee_too_high", claims.config.ALLIANCE_MAX_FEE));
            }
            playerInfo.Alliance.AllianceFee = fee;
            playerInfo.Alliance.saveToDatabase();
            return TextCommandResult.Success(Lang.Get("claims:alliane_fee_set_to", fee));
        }
        public static TextCommandResult SetCapitalAlliance(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo))
            {
                return TextCommandResult.Success(Lang.Get("claims:no_such_player_info"));
            }
            if (!playerInfo.HasAlliance())
            {
                return TextCommandResult.Success(Lang.Get("claims:no_alliance"));
            }
            if (!playerInfo.City.Alliance.IsLeader(playerInfo))
            {
                return TextCommandResult.Success(Lang.Get("claims:you_dont_have_right_for_that_command"));
            }

            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Success(Lang.Get("claims:invalid_city_name"));
            }
            if (claims.dataStorage.getCityByName(name, out City city))
            {
                return TextCommandResult.Success(Lang.Get("claims:no_such_city"));
            }
            Alliance alliance = playerInfo.Alliance;
            alliance.MainCity = city;
            alliance.Leader = city.getMayor();           
            alliance.saveToDatabase();
            return TextCommandResult.Success(Lang.Get("claims:capital_changed_to", city.GetPartName(), alliance.GetPartName()));
        }
        public static TextCommandResult SetPrefixAlliance(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo))
            {
                return TextCommandResult.Success(Lang.Get("claims:no_such_player_info"));
            }
            if (!playerInfo.HasAlliance())
            {
                return TextCommandResult.Success(Lang.Get("claims:no_alliance"));
            }
            if (!playerInfo.City.Alliance.IsLeader(playerInfo))
            {
                return TextCommandResult.Success(Lang.Get("claims:you_dont_have_right_for_that_command"));
            }
            
            if (args.Parsers.Count == 0)
            {
                playerInfo.Alliance.Prefix = "";
                playerInfo.Alliance.saveToDatabase();
                return TextCommandResult.Success();
            }

            string prefix = Filter.filterName((string)args.Parsers[0].GetValue());
            if (prefix.Length == 0 || !Filter.checkForBlockedNames(prefix))
            {
                return TextCommandResult.Success(Lang.Get("claims:invalid_name"));
            }
            playerInfo.Alliance.Prefix = prefix;
            playerInfo.Alliance.saveToDatabase();
            return TextCommandResult.Success();
        }
        public static TextCommandResult PrintInviteList(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (!claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo))
            {
                return TextCommandResult.Success(Lang.Get("claims:no_such_player_info"));
            }

            if (!playerInfo.HasAlliance())
            {
                return TextCommandResult.Success(Lang.Get("claims:no_alliance"));
            }
            if (args.Parsers.Count == 0)
            {
                MessageHandler.sendMsgToPlayer(player, playerInfo.Alliance.GetSentInvitations().Count > 0
                                                            ? StringFunctions.getNthPageOf(playerInfo.Alliance.GetSentInvitations(), 1)
                                                            : Lang.Get("claims:no_invitations"));
                return TextCommandResult.Success();
            }

            int page = (int)args.Parsers[0].GetValue();
            MessageHandler.sendMsgToPlayer(player, playerInfo.Alliance.GetSentInvitations().Count > 0
                                                        ? StringFunctions.getNthPageOf(playerInfo.Alliance.GetSentInvitations(), page)
                                                        : Lang.Get("claims:no_invitations"));
            return TextCommandResult.Success();
        }
        /*public static void processConflict(IServerPlayer player, CmdArgs args, TextCommandResult res)
        {
            if (args[0].Equals("declare", StringComparison.OrdinalIgnoreCase))
            {
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    return;
                }
                if (!playerInfo.hasAlliance() || !playerInfo.getAlliance().isLeader(playerInfo))
                {
                    return;
                }
                string name = Filter.filterName(args[1]);
                if (name.Length == 0 || !Filter.checkForBlockedNames(name))
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:invalid_alliance_name"));
                    return;
                }
                claims.dataStorage.getAllianceByName(name, out Alliance targetAlliance);
                if (targetAlliance == null)
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:no_such_alliance"));
                    return;
                }
                Alliance ourAlliance = playerInfo.getAlliance();
                if (ourAlliance.isConquered())
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:you_are_conquared."));
                    return;
                }

                if (ConflictHandler.conflictAlreadyExist(ourAlliance, targetAlliance))
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_already_exists"));
                    return;
                }
                if (targetAlliance.isConquered())
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:target_alliance_already_conquared"));
                    return;
                }
                if (targetAlliance.isNeutral())
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:target_alliance_is_neutral"));
                    return;
                }


                //BOTH SIDES HAVE TO AGREE ON CONFLICT START
                if (Config.Current.AGREE_FOR_CONFLICT.Val)
                {
                    if (ConflictHandler.addConflictLetter(new ConflictLetter(ourAlliance, targetAlliance, TimeFunctions.getEpochSeconds() + TimeFunctions.secondsInAnHour * Config.Current.DELAY_FOR_CONFLICT_ACTIVATED.Val,
                new Thread(new ThreadStart(() =>
                {
                    if (playerInfo == null || !playerInfo.hasAlliance())
                    {
                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:sanity_test_for_new_alliance"));
                        return;
                    }
                    if (ourAlliance.isConquered() || targetAlliance.isConquered())
                    {
                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:some_alliance_is_conquered"));
                        return;
                    }
                    Conflict newConflict = new Conflict("", TimeFunctions.getEpochSeconds().ToString() + ourAlliance.getGuid().Substring(4));
                    ourAlliance.conflictscounter++;
                    targetAlliance.conflictscounter++;
                    foreach (City ourCity in ourAlliance.getCities())
                    {
                        foreach (City targetCity in targetAlliance.getCities())
                        {
                            ourCity.getHostiles().Add(targetCity);
                            ourCity.saveToDatabase();
                        }
                    }
                    foreach (City targetCity in targetAlliance.getCities())
                    {
                        foreach (City ourCity in ourAlliance.getCities())
                        {
                            targetCity.getHostiles().Add(ourCity);
                            targetCity.saveToDatabase();
                        }
                    }

                    claims.dataStorage.tryAddConflict(newConflict);
                    newConflict.setFirstSide(ourAlliance);
                    newConflict.setSecondSide(targetAlliance);
                    newConflict.setConflictState(ConflictState.ACTIVE);
                    targetAlliance.runningConflicts.Add(newConflict);
                    ourAlliance.runningConflicts.Add(newConflict);
                    targetAlliance.saveToDatabase();
                    ourAlliance.saveToDatabase();
                    newConflict.saveToDatabase(false);

                })),
                new Thread(new ThreadStart(() =>
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_denied"));
                    ConflictHandler.removeConflictLetter(ourAlliance, targetAlliance, LetterPurpose.START_CONFLICT);
                })), LetterPurpose.START_CONFLICT
                )))
                    {

                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_letter_sent"));
                        MessageHandler.sendMsgInAlliance(targetAlliance, Lang.Get("claims:alliance_has_sent_conflict_letter", ourAlliance.getPartNameReplaceUnder()));
                        return;
                    }
                    else
                    {
                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_letter_is_duplicate"));
                        return;
                    }
                }

                //ONE SIDE CAN START CONFLICT
                else
                {
                    if (playerInfo == null || !playerInfo.hasAlliance())
                    {
                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:sanity_test_for_new_alliance"));
                        return;
                    }
                    if (ourAlliance.isConquered() || targetAlliance.isConquered())
                    {
                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:some_alliance_is_conquered"));
                        return;
                    }
                    Conflict newConflict = new Conflict("", TimeFunctions.getEpochSeconds().ToString() + ourAlliance.getGuid().Substring(4));
                    ourAlliance.conflictscounter++;
                    targetAlliance.conflictscounter++;
                    foreach (City ourCity in ourAlliance.getCities())
                    {
                        foreach (City targetCity in targetAlliance.getCities())
                        {
                            ourCity.getHostiles().Add(targetCity);
                            ourCity.saveToDatabase();
                        }
                    }
                    foreach (City targetCity in targetAlliance.getCities())
                    {
                        foreach (City ourCity in ourAlliance.getCities())
                        {
                            targetCity.getHostiles().Add(ourCity);
                            targetCity.saveToDatabase();
                        }
                    }

                    claims.dataStorage.tryAddConflict(newConflict);
                    newConflict.setFirstSide(ourAlliance);
                    newConflict.setSecondSide(targetAlliance);
                    newConflict.setConflictState(ConflictState.ACTIVE);
                    targetAlliance.runningConflicts.Add(newConflict);
                    ourAlliance.runningConflicts.Add(newConflict);
                    targetAlliance.saveToDatabase();
                    ourAlliance.saveToDatabase();
                    newConflict.saveToDatabase(false);
                }
            }
            else if (args[0].Equals("revoke", StringComparison.OrdinalIgnoreCase))
            {
                return; //TODO
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    return;
                }
                if (!playerInfo.hasAlliance() || !playerInfo.getAlliance().isLeader(playerInfo))
                {
                    return;
                }
                string name = Filter.filterName(args[1]);
                if (name.Length == 0 || !Filter.checkForBlockedNames(name))
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:invalid_alliance_name"));
                    return;
                }
                claims.dataStorage.getAllianceByName(name, out Alliance targetAlliance);
                if (targetAlliance == null)
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:no_such_alliance"));
                    return;
                }
                Alliance ourAlliance = playerInfo.getAlliance();
                if (ourAlliance.isConquered())
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:you_are_conquared."));
                    return;
                }
                if (!ConflictHandler.conflictAlreadyExist(ourAlliance, targetAlliance))
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_doesnt_exists"));
                    return;
                }

                if (Config.Current.AGREE_FOR_CONFLICT.Val)
                {
                    if (ConflictHandler.addConflictLetter(new ConflictLetter(ourAlliance, targetAlliance, TimeFunctions.getEpochSeconds() + TimeFunctions.secondsInAnHour * Config.Current.DELAY_FOR_CONFLICT_ACTIVATED.Val,
                new Thread(new ThreadStart(() =>
                {


                    Conflict conflictToEnd = ConflictHandler.getConflictWithSides(ourAlliance, targetAlliance);

                    ourAlliance.conflictscounter--;
                    targetAlliance.conflictscounter--;
                    foreach (City ourCity in ourAlliance.getCities())
                    {
                        foreach (City targetCity in targetAlliance.getCities())
                        {
                            ourCity.getHostiles().Remove(targetCity);
                            ourCity.saveToDatabase();
                        }
                    }
                    foreach (City targetCity in targetAlliance.getCities())
                    {
                        foreach (City ourCity in ourAlliance.getCities())
                        {
                            targetCity.getHostiles().Remove(ourCity);
                            targetCity.saveToDatabase();
                        }
                    }
                    claims.getModInstance().getDatabaseHandler().deleteFromDatabaseConflict(conflictToEnd);
                    targetAlliance.saveToDatabase();
                    ourAlliance.saveToDatabase();

                })),
                new Thread(new ThreadStart(() =>
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_ending_denied"));
                })), LetterPurpose.END_CONFLICT
                )))
                    {

                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_letter_sent"));
                        return;
                    }
                    else
                    {
                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_letter_is_duplicate"));
                        return;
                    }
                }
            }
            else if (args[0].Equals("letterssent", StringComparison.OrdinalIgnoreCase))
            {
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    return;
                }
                if (!playerInfo.hasAlliance() || !playerInfo.getAlliance().isLeader(playerInfo))
                {
                    return;
                }
                Alliance alliance = playerInfo.getAlliance();
                if (args.Length == 1)
                {
                    var sentLetters = (ConflictHandler.getSentLettersForAlliance(alliance));
                    MessageHandler.sendMsgToPlayer(player, sentLetters.Count > 0
                                                                ? StringFunctions.getNthPageOf(sentLetters, 1)
                                                                : Lang.Get("claims:no_letters"));
                    return;
                }

                try
                {
                    int page = int.Parse(args[1]);
                    var sentLetters = (ConflictHandler.getSentLettersForAlliance(alliance));
                    MessageHandler.sendMsgToPlayer(player, sentLetters.Count > 0
                                                                ? StringFunctions.getNthPageOf(sentLetters, page)
                                                                : Lang.Get("claims:no_letters"));
                    return;
                }
                catch (FormatException e)
                {
                    return;
                }
            }
            else if (args[0].Equals("lettersreceived", StringComparison.OrdinalIgnoreCase))
            {
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    return;
                }
                if (!playerInfo.hasAlliance() || !playerInfo.getAlliance().isLeader(playerInfo))
                {
                    return;
                }
                Alliance alliance = playerInfo.getAlliance();
                if (args.Length == 1)
                {
                    var receivedLetters = (ConflictHandler.getReceivedLettersForAlliance(alliance));
                    MessageHandler.sendMsgToPlayer(player, receivedLetters.Count > 0
                                                                ? StringFunctions.getNthPageOf(receivedLetters, 1)
                                                                : Lang.Get("claims:no_letters"));
                    return;
                }

                try
                {
                    int page = int.Parse(args[1]);
                    var receivedLetters = (ConflictHandler.getReceivedLettersForAlliance(alliance));
                    MessageHandler.sendMsgToPlayer(player, receivedLetters.Count > 0
                                                                ? StringFunctions.getNthPageOf(receivedLetters, page)
                                                                : Lang.Get("claims:no_letters"));
                    return;
                }
                catch (FormatException e)
                {
                    return;
                }
            }
            else if (args[0].Equals("acceptstart", StringComparison.OrdinalIgnoreCase))
            {
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    return;
                }
                if (!playerInfo.hasAlliance() || !playerInfo.getAlliance().isLeader(playerInfo))
                {
                    return;
                }
                if (args.Length < 2)
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:need_alliance_name"));
                    return;
                }
                string name = Filter.filterName(args[1]);
                if (name.Length == 0 || !Filter.checkForBlockedNames(name))
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:invalid_alliance_name"));
                    return;
                }
                claims.dataStorage.getAllianceByName(name, out Alliance targetAlliance);
                if (targetAlliance == null)
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:no_such_alliance"));
                    return;
                }
                Alliance alliance = playerInfo.getAlliance();
                List<ConflictLetter> lettersReceived = ConflictHandler.getReceivedLettersForAllianceWithPurpose(alliance, LetterPurpose.START_CONFLICT);
                foreach (ConflictLetter it_letter in lettersReceived)
                {
                    if (it_letter.getFrom().Equals(targetAlliance))
                    {
                        MessageHandler.sendMsgInAlliance(alliance, Lang.Get("claims:start_conflict_letter_accepted_from", targetAlliance));
                        MessageHandler.sendMsgInAlliance(targetAlliance, Lang.Get("claims:start_conflict_letter_accepted_from", alliance));
                        claims.getSAPI().Event.RegisterCallback((dt =>
                        {
                            Task.Run(() => it_letter.getOnAccept().Start());
                        }), Config.Current.CONFLICT_START_CAST_TIME.Val * 1000);
                        return;
                    }
                }

            }
            else if (args[0].Equals("denystart", StringComparison.OrdinalIgnoreCase))
            {
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    return;
                }
                if (!playerInfo.hasAlliance() || !playerInfo.getAlliance().isLeader(playerInfo))
                {
                    return;
                }
                string name = Filter.filterName(args[1]);
                if (name.Length == 0 || !Filter.checkForBlockedNames(name))
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:invalid_alliance_name"));
                    return;
                }
                claims.dataStorage.getAllianceByName(name, out Alliance targetAlliance);
                if (targetAlliance == null)
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:no_such_alliance"));
                    return;
                }
                Alliance alliance = playerInfo.getAlliance();
                List<ConflictLetter> lettersReceived = ConflictHandler.getReceivedLettersForAllianceWithPurpose(alliance, LetterPurpose.START_CONFLICT);
                foreach (ConflictLetter it_letter in lettersReceived)
                {
                    if (it_letter.getFrom().Equals(targetAlliance))
                    {
                        claims.getSAPI().Event.RegisterCallback((dt =>
                        {
                            it_letter.getOnDeny().Start();
                        }), 0);
                        return;
                    }
                }
            }
            else if (args[0].Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    return;
                }
                if (!playerInfo.hasAlliance() || !playerInfo.getAlliance().isLeader(playerInfo))
                {
                    return;
                }
                string name = Filter.filterName(args[1]);
                if (name.Length == 0 || !Filter.checkForBlockedNames(name))
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:invalid_alliance_name"));
                    return;
                }
                claims.dataStorage.getAllianceByName(name, out Alliance targetAlliance);
                if (targetAlliance == null)
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:no_such_alliance"));
                    return;
                }
                Alliance ourAlliance = playerInfo.getAlliance();
                if (ourAlliance.isConquered())
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:you_are_conquared."));
                    return;
                }
                if (!ConflictHandler.conflictAlreadyExist(ourAlliance, targetAlliance))
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_doesnt_exist"));
                    return;
                }
                Conflict conflict = ConflictHandler.getConflictWithSides(ourAlliance, targetAlliance);
                if (Config.Current.AGREE_FOR_STOP_CONFLICT.Val)
                {
                    if (ConflictHandler.addConflictLetter(new ConflictLetter(ourAlliance, targetAlliance, TimeFunctions.getEpochSeconds() + TimeFunctions.secondsInAnHour * Config.Current.DELAY_FOR_CONFLICT_ACTIVATED.Val,
                        new Thread(new ThreadStart(() =>
                        {
                            PartDemolition.demolishConflict(conflict);
                            MessageHandler.sendMsgInAlliance(ourAlliance, Lang.Get("claims:conflict_with_was_stopped", targetAlliance));
                            MessageHandler.sendMsgInAlliance(targetAlliance, Lang.Get("claims:conflict_with_was_stopped", ourAlliance));
                            return;
                        })),
                        new Thread(new ThreadStart(() =>
                        {
                            MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_end_letter_denied"));
                            ConflictHandler.removeConflictLetter(ourAlliance, targetAlliance, LetterPurpose.END_CONFLICT);
                        })), LetterPurpose.END_CONFLICT
                )))
                    {
                        MessageHandler.sendMsgInAlliance(ourAlliance, Lang.Get("claims:we_sent_letter_to_stop", targetAlliance));
                        MessageHandler.sendMsgInAlliance(targetAlliance, Lang.Get("claims:conflict_letter_stop_received_from", ourAlliance));
                        return;

                    }
                }
                else
                {
                    MessageHandler.sendMsgInAlliance(ourAlliance, Lang.Get("claims:already_sent_letter_to_stop", targetAlliance));
                }
            }
            else if (args[0].Equals("acceptstop", StringComparison.OrdinalIgnoreCase))
            {
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    return;
                }
                if (!playerInfo.hasAlliance() || !playerInfo.getAlliance().isLeader(playerInfo))
                {
                    return;
                }
                string name = Filter.filterName(args[1]);
                if (name.Length == 0 || !Filter.checkForBlockedNames(name))
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:invalid_alliance_name"));
                    return;
                }
                claims.dataStorage.getAllianceByName(name, out Alliance targetAlliance);
                if (targetAlliance == null)
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:no_such_alliance"));
                    return;
                }
                Alliance alliance = playerInfo.getAlliance();
                List<ConflictLetter> lettersReceived = ConflictHandler.getReceivedLettersForAllianceWithPurpose(alliance, LetterPurpose.END_CONFLICT);
                foreach (ConflictLetter it_letter in lettersReceived)
                {
                    if (it_letter.getFrom().Equals(targetAlliance))
                    {
                        MessageHandler.sendMsgInAlliance(alliance, Lang.Get("claims:stop_letter_accepted_from", targetAlliance));
                        MessageHandler.sendMsgInAlliance(targetAlliance, Lang.Get("claims:we_accepted_stop_letter_from", alliance));
                        claims.getSAPI().Event.RegisterCallback((dt =>
                        {
                            it_letter.getOnAccept().Start();
                        }), Config.Current.CONFLICT_END_CAST_TIME.Val * 1000);
                        return;
                    }
                }
            }
            else if (args[0].Equals("denystop", StringComparison.OrdinalIgnoreCase))
            {
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    return;
                }
                if (!playerInfo.hasAlliance() || !playerInfo.getAlliance().isLeader(playerInfo))
                {
                    return;
                }
                string name = Filter.filterName(args[1]);
                if (name.Length == 0 || !Filter.checkForBlockedNames(name))
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:invalid_alliance_name"));
                    return;
                }
                claims.dataStorage.getAllianceByName(name, out Alliance targetAlliance);
                if (targetAlliance == null)
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:no_such_alliance"));
                    return;
                }
                Alliance alliance = playerInfo.getAlliance();
                List<ConflictLetter> lettersReceived = ConflictHandler.getReceivedLettersForAllianceWithPurpose(alliance, LetterPurpose.END_CONFLICT);
                foreach (ConflictLetter it_letter in lettersReceived)
                {
                    if (it_letter.getFrom().Equals(targetAlliance))
                    {
                        claims.getSAPI().Event.RegisterCallback((dt =>
                        {
                            it_letter.getOnDeny().Start();
                        }), 0);
                        return;
                    }
                }
            }
            else if (args[0].Equals("startriot", StringComparison.OrdinalIgnoreCase))
            {
                claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
                if (playerInfo == null)
                {
                    return;
                }
                if (!playerInfo.hasAlliance() || !playerInfo.getAlliance().isLeader(playerInfo))
                {
                    return;
                }
                Alliance alliance = playerInfo.getAlliance();

                if (alliance.runningConflicts.Count < 1)
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:no_conflicts"));
                    return;
                }

                if (!alliance.isConquered())
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:your_alliance_isnt_conquered"));
                    return;
                }

                Conflict conflict = alliance.runningConflicts.Single();
                if (conflict.getFirstSide().Equals(alliance))
                {
                    if (conflict.getFirstSideRiotCounter() >= Config.Current.RIOT_MAX_COUNT.Val)
                    {
                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:max_riot_counter"));
                        return;
                    }
                    conflict.incFirstSideRiotCounter();
                    conflict.setConflictState(ConflictState.RIOT_BY_FIRST);
                }
                else
                {
                    if (conflict.getSecondSideRiotCounter() >= Config.Current.RIOT_MAX_COUNT.Val)
                    {
                        MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:max_riot_counter"));
                        return;
                    }
                    conflict.incSecondSideRiotCounter();
                    conflict.setConflictState(ConflictState.RIOT_BY_SECOND);
                }
                conflict.saveToDatabase();

            }
        }
        public static void processInfo(IServerPlayer player, CmdArgs args, TextCommandResult res)
        {

        }*/
    }
}
