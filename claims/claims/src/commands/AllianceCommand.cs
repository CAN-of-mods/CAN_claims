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
using claims.src.part.structure.conflict;

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
            if (alliance.IsLeader(playerInfo))
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
            UsefullPacketsSend.AddToQueueAllianceInfoUpdate(alliance.Guid, new Dictionary<string, object> { { "value", alliance.Guid } }, EnumPlayerRelatedInfo.NEW_ALLIANCE_ALL);
            UsefullPacketsSend.AddToQueueCityInfoUpdate(city.Guid, EnumPlayerRelatedInfo.OWN_ALLIANCE_REMOVE);
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
            UsefullPacketsSend.AddToQueueAllianceInfoUpdate(alliance.Guid, new Dictionary<string, object> { { "value", alliance.Guid } }, EnumPlayerRelatedInfo.NEW_ALLIANCE_ALL);
            UsefullPacketsSend.AddToQueueCityInfoUpdate(city.Guid, new Dictionary<string, object> { { "value", alliance.Guid } }, EnumPlayerRelatedInfo.OWN_ALLIANCE_REMOVE);
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
            long timeStamp = TimeFunctions.getEpochSeconds() + claims.config.HOUR_TIMEOUT_INVITATION_TO_ALLIANCE * 60;
            if (InvitationHandler.addNewInvite(new Invitation(alliance, city, timeStamp,
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
                    UsefullPacketsSend.AddToQueueAllianceInfoUpdate(alliance.Guid, new Dictionary<string, object> { { "value", alliance.Guid } }, EnumPlayerRelatedInfo.NEW_ALLIANCE_ALL);

                })),
                new Thread(new ThreadStart(() =>
                {
                    MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:disagrre_with_invitation_to_alliance", playerInfo.GetPartName(), city.GetPartName()));
                }))
                )))
            {
                UsefullPacketsSend.AddToQueueCityInfoUpdate(city.Guid,
                    new Dictionary<string, object> { { "value", new ClientToAllianceInvitationCellElement(alliance.GetPartName(), alliance.Guid, timeStamp) } },
                    EnumPlayerRelatedInfo.TO_ALLIANCE_INVITE_ADD);
                MessageHandler.sendMsgInCity(city, Lang.Get("claims:your_city_was_invited_to_alliance", alliance.GetPartName()));
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
            UsefullPacketsSend.AddToQueueAllianceInfoUpdate(playerInfo.Alliance.Guid, new Dictionary<string, object> { { "value", playerInfo.Alliance.Guid } }, EnumPlayerRelatedInfo.NEW_ALLIANCE_ALL);
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
        public static TextCommandResult DeclareConflict(TextCommandCallingArgs args)
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
            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Error(Lang.Get("claims:invalid_alliance_name"));
            }
            if (!claims.dataStorage.GetAllianceByName(name, out Alliance targetAlliance))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_alliance"));
            }

            Alliance ourAlliance = playerInfo.Alliance;

            if (ConflictHandler.conflictAlreadyExist(ourAlliance, targetAlliance))
            {
                return TextCommandResult.Success(Lang.Get("claims:conflict_already_exists"));
            }
            if (targetAlliance.Neutral)
            {
                return TextCommandResult.Success(Lang.Get("claims:target_alliance_is_neutral"));
            }
            //BOTH SIDES HAVE TO AGREE ON CONFLICT START
            if (claims.config.NEED_AGREE_FOR_CONFLICT)
            {
                long timestamp = TimeFunctions.getEpochSeconds() + claims.config.DELAY_FOR_CONFLICT_ACTIVATED;
                string newConflictGuid = ConflictLetter.GetUnusedGuid().ToString();
                if (ConflictHandler.addConflictLetter(new ConflictLetter(ourAlliance, targetAlliance, LetterPurpose.START_CONFLICT, timestamp,
                        new Thread(new ThreadStart(() =>
                        {
                            if (playerInfo == null || !playerInfo.HasAlliance())
                            {
                                MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:sanity_test_for_new_alliance"));
                                return;
                            }

                            if(!ConflictHandler.TryGetConflictLetter(ourAlliance, targetAlliance, LetterPurpose.START_CONFLICT, out var letter))
                            {
                                MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:no_letter_found"));
                                return;
                            }
                            
                            Conflict newConflict = new Conflict("", newConflictGuid);
                            foreach (City ourCity in ourAlliance.Cities)
                            {
                                foreach (City targetCity in targetAlliance.Cities)
                                {
                                    ourCity.HostileCities.Add(targetCity);
                                    ourCity.saveToDatabase();
                                }
                            }
                            foreach (City targetCity in targetAlliance.Cities)
                            {
                                foreach (City ourCity in ourAlliance.Cities)
                                {
                                    targetCity.HostileCities.Add(ourCity);
                                    targetCity.saveToDatabase();
                                }
                            }

                            claims.dataStorage.TryAddConflict(newConflict);
                            ConflictHandler.removeConflictLetter(letter);

                            UsefullPacketsSend.AddToQueueAllianceInfoUpdate(ourAlliance.Guid, 
                                new Dictionary<string, object> { { "value", letter.Guid } }, EnumPlayerRelatedInfo.ALLIANCE_LETTER_REMOVE);
                            UsefullPacketsSend.AddToQueueAllianceInfoUpdate(targetAlliance.Guid, 
                                new Dictionary<string, object> { { "value", letter.Guid } }, EnumPlayerRelatedInfo.ALLIANCE_LETTER_REMOVE);

                            newConflict.First = ourAlliance;
                            newConflict.Second = targetAlliance;
                            newConflict.StartedBy = ourAlliance;
                            newConflict.State = ConflictState.CREATED;
                            newConflict.TimeStampStarted = TimeFunctions.getEpochSeconds();

                            UsefullPacketsSend.AddToQueueAllianceInfoUpdate(ourAlliance.Guid,
                                new Dictionary<string, object> { { "value", new ClientConflictCellElement(newConflict.GetPartName(),
                                newConflict.First.GetPartName(), newConflict.Second.GetPartName(), newConflict.First.GetPartName(),
                                newConflict.State, newConflict.Guid, newConflict.WarRanges, newConflict.TimeStampStarted) } }, EnumPlayerRelatedInfo.ALLIANCE_CONFLICT_ADD);
                            UsefullPacketsSend.AddToQueueAllianceInfoUpdate(targetAlliance.Guid,
                                new Dictionary<string, object> { { "value", new ClientConflictCellElement(newConflict.GetPartName(),
                                newConflict.First.GetPartName(), newConflict.Second.GetPartName(), newConflict.First.GetPartName(),
                                newConflict.State, newConflict.Guid, newConflict.WarRanges, newConflict.TimeStampStarted) } }, EnumPlayerRelatedInfo.ALLIANCE_CONFLICT_ADD);

                            targetAlliance.RunningConflicts.Add(newConflict);
                            ourAlliance.RunningConflicts.Add(newConflict);
                            targetAlliance.saveToDatabase();
                            ourAlliance.saveToDatabase();
                            newConflict.saveToDatabase(false);
                            MessageHandler.SendMsgInAlliance(targetAlliance, Lang.Get("claims:conflict_created_with", targetAlliance.getPartNameReplaceUnder()));
                        })),
                        new Thread(new ThreadStart(() =>
                        {
                            MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_denied"));
                            ConflictHandler.removeConflictLetter(ourAlliance, targetAlliance, LetterPurpose.START_CONFLICT);
                        }))
                        , newConflictGuid.ToString())))
                {
                    ConflictHandler.TryGetConflictLetter(newConflictGuid.ToString(), out ConflictLetter conflictLetter);
                    UsefullPacketsSend.AddToQueueAllianceInfoUpdate(targetAlliance.Guid, new Dictionary<string, object> { { "value", 
                            new ClientConflictLetterCellElement(conflictLetter.From.GetPartName(), conflictLetter.From.Guid.ToString(),
                            conflictLetter.To.GetPartName(), conflictLetter.To.Guid.ToString(),
                            conflictLetter.Purpose, conflictLetter.TimeStampExpire, conflictLetter.Guid) } }, EnumPlayerRelatedInfo.ALLIANCE_LETTER_ADD);
                    UsefullPacketsSend.AddToQueueAllianceInfoUpdate(ourAlliance.Guid, new Dictionary<string, object> { { "value", new ClientConflictLetterCellElement(
                            conflictLetter.From.GetPartName(), conflictLetter.From.Guid.ToString(),
                            conflictLetter.To.GetPartName(), conflictLetter.To.Guid.ToString(),
                            conflictLetter.Purpose, conflictLetter.TimeStampExpire, conflictLetter.Guid) } }, EnumPlayerRelatedInfo.ALLIANCE_LETTER_ADD);
                    MessageHandler.SendMsgInAlliance(targetAlliance, Lang.Get("claims:alliance_has_sent_conflict_letter", ourAlliance.getPartNameReplaceUnder()));
                    return TextCommandResult.Success(Lang.Get("claims:conflict_letter_sent"));
                }
                else
                {
                    return TextCommandResult.Success(Lang.Get("claims:conflict_letter_is_duplicate"));
                }
            }

            //ONE SIDE CAN START CONFLICT
            else
            {
                if (playerInfo == null || !playerInfo.HasAlliance())
                {
                    return TextCommandResult.Success(Lang.Get("claims:sanity_test_for_new_alliance"));
                }

                Conflict newConflict = new Conflict("", Alliance.GetUnusedGuid());

                foreach (City ourCity in ourAlliance.Cities)
                {
                    foreach (City targetCity in targetAlliance.Cities)
                    {
                        ourCity.HostileCities.Add(targetCity);
                        ourCity.saveToDatabase();
                    }
                }
                foreach (City targetCity in targetAlliance.Cities)
                {
                    foreach (City ourCity in ourAlliance.Cities)
                    {
                        targetCity.HostileCities.Add(ourCity);
                        targetCity.saveToDatabase();
                    }
                }

                claims.dataStorage.TryAddConflict(newConflict);
                newConflict.First = ourAlliance;
                newConflict.StartedBy = ourAlliance;
                newConflict.Second = targetAlliance;
                newConflict.State = ConflictState.CREATED;
                targetAlliance.RunningConflicts.Add(newConflict);
                ourAlliance.RunningConflicts.Add(newConflict);
                targetAlliance.saveToDatabase();
                ourAlliance.saveToDatabase();
                newConflict.saveToDatabase(false);
                return TextCommandResult.Success(Lang.Get("claims:conflict_created_with", targetAlliance.getPartNameReplaceUnder()));
            }
        }
        public static TextCommandResult RevokeConflict(TextCommandCallingArgs args)
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
            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Error(Lang.Get("claims:invalid_alliance_name"));
            }
            if (!claims.dataStorage.GetAllianceByName(name, out Alliance targetAlliance))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_alliance"));
            }

            Alliance ourAlliance = playerInfo.Alliance;

            if (ConflictHandler.conflictAlreadyExist(ourAlliance, targetAlliance))
            {
                return TextCommandResult.Success(Lang.Get("claims:conflict_already_started"));
            }
                      
            var conflictLettersList = ConflictHandler.getReceivedLettersForAllianceWithPurpose(targetAlliance, LetterPurpose.START_CONFLICT);
            ConflictLetter foundLetter = null;
            foreach(var it in conflictLettersList)
            {
                if(it.To.Equals(targetAlliance))
                {
                    foundLetter = it;
                    break;
                }
            }

            if(foundLetter == null)
            {
                return TextCommandResult.Success(Lang.Get("claims:conflict_letter_doesnt_exist"));
            }
            UsefullPacketsSend.AddToQueueAllianceInfoUpdate(targetAlliance.Guid, new Dictionary<string, object> { { "value", foundLetter.Guid } }, EnumPlayerRelatedInfo.ALLIANCE_LETTER_REMOVE);
            UsefullPacketsSend.AddToQueueAllianceInfoUpdate(ourAlliance.Guid, new Dictionary<string, object> { { "value", foundLetter.Guid } }, EnumPlayerRelatedInfo.ALLIANCE_LETTER_REMOVE);
            ConflictHandler.removeConflictLetter(foundLetter);
            return TextCommandResult.Success(Lang.Get("claims:conflict_declaration_removed", targetAlliance.getPartNameReplaceUnder()));          
        }
        public static TextCommandResult AcceptStartConflict(TextCommandCallingArgs args)
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

            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Error(Lang.Get("claims:invalid_alliance_name"));
            }
            if (!claims.dataStorage.GetAllianceByName(name, out Alliance targetAlliance))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_alliance"));
            }

            Alliance ourAlliance = playerInfo.Alliance;

            if (ConflictHandler.conflictAlreadyExist(ourAlliance, targetAlliance))
            {
                return TextCommandResult.Success(Lang.Get("claims:conflict_already_exists"));
            }

            if(!ConflictHandler.TryGetConflictLetter(ourAlliance, targetAlliance, LetterPurpose.START_CONFLICT, out var letter))
            {
                return TextCommandResult.Success(Lang.Get("claims:conflict_letter_doesnt_exist"));
            }

            letter.OnAccept.Start();
            return TextCommandResult.Success();
        }
        public static TextCommandResult DenyStartConflict(TextCommandCallingArgs args)
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

            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Error(Lang.Get("claims:invalid_alliance_name"));
            }
            if (!claims.dataStorage.GetAllianceByName(name, out Alliance targetAlliance))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_alliance"));
            }

            Alliance ourAlliance = playerInfo.Alliance;

            if (ConflictHandler.conflictAlreadyExist(ourAlliance, targetAlliance))
            {
                return TextCommandResult.Success(Lang.Get("claims:conflict_already_exists"));
            }

            if (!ConflictHandler.TryGetConflictLetter(ourAlliance, targetAlliance, LetterPurpose.START_CONFLICT, out var letter))
            {
                return TextCommandResult.Success(Lang.Get("claims:conflict_letter_doesnt_exist"));
            }
            letter.OnDeny.Start();
            return TextCommandResult.Success();
        }
        public static TextCommandResult OfferStopConflict(TextCommandCallingArgs args)
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
            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Error(Lang.Get("claims:invalid_alliance_name"));
            }
            if (!claims.dataStorage.GetAllianceByName(name, out Alliance targetAlliance))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_alliance"));
            }

            Alliance ourAlliance = playerInfo.Alliance;

            if (!ConflictHandler.conflictAlreadyExist(ourAlliance, targetAlliance))
            {
                return TextCommandResult.Success(Lang.Get("claims:no_conflict_found"));
            }

            if (ConflictHandler.TryGetConflictLetter(ourAlliance, targetAlliance, LetterPurpose.END_CONFLICT, out var letter))
            {
                return TextCommandResult.Success(Lang.Get("claims:end_conflict_letter_exist"));
            }

            //BOTH SIDES HAVE TO AGREE ON CONFLICT START
            if (claims.config.NEED_AGREE_FOR_CONFLICT)
            {
                long timestamp = TimeFunctions.getEpochSeconds() + claims.config.DELAY_FOR_CONFLICT_ACTIVATED;
                Guid newConflictGuid = ConflictLetter.GetUnusedGuid();
                if (ConflictHandler.addConflictLetter(new ConflictLetter(ourAlliance, targetAlliance, LetterPurpose.END_CONFLICT, timestamp,
                        new Thread(new ThreadStart(() =>
                        {
                            if (playerInfo == null || !playerInfo.HasAlliance())
                            {
                                MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:sanity_test_for_new_alliance"));
                                return;
                            }
                            
                            if(!ConflictHandler.TryGetConflictWithSides(ourAlliance, targetAlliance, out Conflict conflict))
                            {
                                MessageHandler.SendMsgInAlliance(targetAlliance, Lang.Get("claims:conflict_not_found"));
                                return;
                            }
                            PartDemolition.DemolishConflict(conflict);
                            MessageHandler.SendMsgInAlliance(targetAlliance, Lang.Get("claims:conflict_stopped_with", targetAlliance.getPartNameReplaceUnder()));
                        })),
                        new Thread(new ThreadStart(() =>
                        {
                            MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:conflict_stop_denied"));
                            ConflictHandler.removeConflictLetter(ourAlliance, targetAlliance, LetterPurpose.START_CONFLICT);
                        })),
                        newConflictGuid.ToString()
                        )))
                {
                    MessageHandler.SendMsgInAlliance(targetAlliance, Lang.Get("claims:alliance_has_sent_conflict_letter", ourAlliance.getPartNameReplaceUnder()));
                    return TextCommandResult.Success(Lang.Get("claims:conflict_letter_sent"));
                }
                else
                {
                    return TextCommandResult.Success(Lang.Get("claims:conflict_letter_is_duplicate"));
                }
            }

            //ONE SIDE CAN START CONFLICT
            else
            {
                if (playerInfo == null || !playerInfo.HasAlliance())
                {
                    return TextCommandResult.Success(Lang.Get("claims:sanity_test_for_new_alliance"));
                }

                if (!ConflictHandler.TryGetConflictWithSides(ourAlliance, targetAlliance, out Conflict conflict))
                {
                    return TextCommandResult.Success(Lang.Get("claims:conflict_not_found"));
                }
                PartDemolition.DemolishConflict(conflict);
                return TextCommandResult.Success(Lang.Get("claims:conflict_stopped_with", targetAlliance.getPartNameReplaceUnder()));
            }
        }
        public static TextCommandResult AcceptStopConflict(TextCommandCallingArgs args)
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

            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Error(Lang.Get("claims:invalid_alliance_name"));
            }
            if (!claims.dataStorage.GetAllianceByName(name, out Alliance targetAlliance))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_alliance"));
            }

            Alliance ourAlliance = playerInfo.Alliance;

            if (!ConflictHandler.conflictAlreadyExist(ourAlliance, targetAlliance))
            {
                return TextCommandResult.Success(Lang.Get("claims:no_conflict_found"));
            }

            if (!ConflictHandler.TryGetConflictLetter(ourAlliance, targetAlliance, LetterPurpose.END_CONFLICT, out var letter))
            {
                return TextCommandResult.Success(Lang.Get("claims:conflict_letter_doesnt_exist"));
            }

            letter.OnAccept.Start();
            return TextCommandResult.Success();
        }
        public static TextCommandResult DenyStopConflict(TextCommandCallingArgs args)
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

            string name = Filter.filterName((string)args.Parsers[0].GetValue());
            if (name.Length == 0 || !Filter.checkForBlockedNames(name))
            {
                return TextCommandResult.Error(Lang.Get("claims:invalid_alliance_name"));
            }
            if (!claims.dataStorage.GetAllianceByName(name, out Alliance targetAlliance))
            {
                return TextCommandResult.Error(Lang.Get("claims:no_such_alliance"));
            }

            Alliance ourAlliance = playerInfo.Alliance;

            if (!ConflictHandler.conflictAlreadyExist(ourAlliance, targetAlliance))
            {
                return TextCommandResult.Success(Lang.Get("claims:no_conflict_found"));
            }

            if (!ConflictHandler.TryGetConflictLetter(ourAlliance, targetAlliance, LetterPurpose.END_CONFLICT, out var letter))
            {
                return TextCommandResult.Success(Lang.Get("claims:conflict_letter_doesnt_exist"));
            }

            letter.OnDeny.Start();
            return TextCommandResult.Success();
        }
    }
}
