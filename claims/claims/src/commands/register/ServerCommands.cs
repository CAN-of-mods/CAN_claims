using claims.src.messages;
using claims.src.rights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace claims.src.commands.register
{
    public class ServerCommands
    {
       
        public static void RegisterCommands(ICoreServerAPI sapi)
        {
            var parsers = sapi.ChatCommands.Parsers;

            RegisterCityCommands(parsers, sapi);
            RegisterCitizenCommands(parsers, sapi);
            RegisterPlotCommands(parsers, sapi);
            RegisterCAdminCommands(parsers, sapi);
            RegisterClaimAreaCommands(parsers, sapi);

            sapi.ChatCommands.Create("cc").HandleWith(commands.ClaimsChatCommands.onCommandCC)
             .RequiresPlayer().RequiresPrivilege(Privilege.chat);

            sapi.ChatCommands.Create("lc").HandleWith(commands.ClaimsChatCommands.onCommandLC)
             .RequiresPlayer().RequiresPrivilege(Privilege.chat);

            sapi.ChatCommands.Create("gc").HandleWith(commands.ClaimsChatCommands.onCommandGC)
              .RequiresPlayer().RequiresPrivilege(Privilege.chat);

            sapi.ChatCommands.Create("agree").HandleWith(commands.agreementCommand.onCommand)
              .RequiresPlayer().RequiresPrivilege(Privilege.chat);

            sapi.ChatCommands.Create("accept").HandleWith(commands.AcceptCommand.onCommand)
              .RequiresPlayer().RequiresPrivilege(Privilege.chat)
              .WithArgs(parsers.OptionalWord("cityName"));

            sapi.ChatCommands.Create("deny").HandleWith(commands.DenyCommand.onCommand)
              .RequiresPlayer().RequiresPrivilege(Privilege.chat)
              .WithArgs(parsers.OptionalWord("cityName"));

            sapi.ChatCommands.Create("plotsgroupaccept").HandleWith(commands.AcceptCommand.onAcceptPlotGroup)
              .RequiresPlayer().RequiresPrivilege(Privilege.chat)
              .WithArgs(parsers.OptionalWord("groupName"));

            sapi.ChatCommands.Create("plotsgroupleave").HandleWith(commands.AcceptCommand.onLeavePlotGroup)
              .RequiresPlayer().RequiresPrivilege(Privilege.chat)
              .WithArgs(parsers.OptionalWord("cityName:groupName"));
        }
        public static void RegisterCityCommands(CommandArgumentParsers parsers, ICoreServerAPI sapi)
        {
            sapi.ChatCommands.Create("city")
                .RequiresPlayer().RequiresPrivilege(Privilege.chat).WithAlias("c")
                    /////////
                     .BeginSub("create")
                        .HandleWith(commands.CityCommand.CreateNewCity)
                        .WithAlias("new")
                        .WithDesc("Create a new city.")
                        .WithArgs(parsers.Word("cityName"))
                     .EndSub()
                     /////////
                     .BeginSub("delete")
                        .HandleWith(commands.CityCommand.DeleteCity)
                        .WithDesc("Remove the city.")
                     .EndSub()                   
                    /////////
                    .BeginSub("claim")
                        .WithDesc("Claim new plot for the city.")
                        .WithPreCondition((TextCommandCallingArgs args) => {                           
                            if (args.Caller.Player is IServerPlayer player)
                            {
                                if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_CLAIM_PLOT }))
                                {
                                    return TextCommandResult.Success();
                                }
                                else
                                {
                                    return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));                                   
                                }

                            }
                            return TextCommandResult.Error("");
                        })
                        .HandleWith(commands.CityCommand.ClaimCityPlot)
                    .EndSub()
                    /////////
                    .BeginSub("unclaim")
                         .WithPreCondition((TextCommandCallingArgs args) => {
                             if (args.Caller.Player is IServerPlayer player)
                             {
                                 if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_UNCLAIM_PLOT }))
                                 {
                                     return TextCommandResult.Success();
                                 }
                                 else
                                 {
                                     return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                 }

                             }
                             return TextCommandResult.Error("");
                         })
                        .WithDesc("Unclaim city's plot.")
                        .HandleWith(commands.CityCommand.UnclaimCityPlot)
                    .EndSub()                  
                    /////////
                    .BeginSub("extraplot")
                        .WithDesc("Buy an extra plot.")
                        .WithPreCondition((TextCommandCallingArgs args) => {
                            if (args.Caller.Player is IServerPlayer player)
                            {
                                if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_BUY_EXTRA_PLOT }))
                                {
                                    return TextCommandResult.Success();
                                }
                                else
                                {
                                    return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                }

                            }
                            return TextCommandResult.Error("");
                        })
                        .HandleWith(commands.CityCommand.ProcessExtraPlot)
                    .EndSub()
                    /////////
                    .BeginSub("outpost")
                        .WithPreCondition((TextCommandCallingArgs args) => {
                            if (args.Caller.Player is IServerPlayer player)
                            {
                                if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_BUY_OUTPOST }))
                                {
                                    return TextCommandResult.Success();                                   
                                }
                                else
                                {
                                    return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                }

                            }
                            return TextCommandResult.Error("");
                        })
                        .HandleWith(commands.CityCommand.ClaimOutpost)
                        .WithDesc("Buy outpost.")
                    .EndSub()
                    /////////
                    .BeginSub("here")
                        .WithDesc("Show info about city on this plot.")
                        .HandleWith(commands.CityCommand.CityHere)
                    .EndSub()
                    /////////
                    .BeginSub("info")
                        .WithDesc("Show info about city.")
                        .WithPreCondition((TextCommandCallingArgs args) => {
                            if (args.Caller.Player is IServerPlayer player)
                            {
                                if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_INFO }))
                                {
                                    return TextCommandResult.Success();
                                }
                                else
                                {
                                    return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                }
                            }
                            return TextCommandResult.Error("");
                        })
                        .HandleWith(commands.CityCommand.CityInfo)
                        .WithArgs(parsers.Word("cityName"))
                    .EndSub()
                    /////////
                    .BeginSub("invite")
                        .WithDesc("Invite a player to the city.")
                        .WithPreCondition((TextCommandCallingArgs args) => {
                            if (args.Caller.Player is IServerPlayer player)
                            {
                                if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_INVITE }))
                                {
                                    return TextCommandResult.Success();
                                }
                                else
                                {
                                    return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                }
                            }
                            return TextCommandResult.Error("");
                        })
                        .WithArgs(parsers.Word("playerName"))
                        .HandleWith(commands.CityCommand.InviteToCity)
                    .EndSub()
                    /////////
                    .BeginSub("kick")
                        .HandleWith(commands.CityCommand.CityKick)
                         .WithPreCondition((TextCommandCallingArgs args) => {
                             if (args.Caller.Player is IServerPlayer player)
                             {
                                 if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_KICK }))
                                 {
                                     return TextCommandResult.Success();
                                 }
                                 else
                                 {
                                     return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                 }
                             }
                             return TextCommandResult.Error("");
                         })
                        .WithDesc("Kick a player from the city.")
                        .WithArgs(parsers.Word("playerName"))
                    .EndSub()
                    /////////
                    .BeginSub("leave")
                         .WithDesc("Leave the city.")
                         .HandleWith(CityCommand.CityLeave)
                     .EndSub()
                      ////////
                      .BeginSub("uninvite")
                         .WithDesc("Delete invitation for player.")
                         .WithPreCondition((TextCommandCallingArgs args) => {
                             if (args.Caller.Player is IServerPlayer player)
                             {
                                 if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_UNINVITE }))
                                 {
                                     return TextCommandResult.Success();
                                 }
                                 else
                                 {
                                     return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                 }
                             }
                             return TextCommandResult.Error("");
                         })
                         .WithArgs(parsers.Word("playerName"))
                         .HandleWith(commands.CityCommand.UninviteToCity)
                     .EndSub()
                     ////////
                     .BeginSub("list")
                         .WithDesc("List all cities.")
                         .HandleWith(commands.CityCommand.ProcessListCities)
                     .EndSub()
                     /////////
                     .BeginSub("set")
                         .WithDesc("Setters for city.")
                             .BeginSub("permissions")
                                 .WithDesc("Change city permissions.")
                                 .WithPreCondition((TextCommandCallingArgs args) => {
                                      if (args.Caller.Player is IServerPlayer player)
                                      {
                                          if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SET_PLOT_ACCESS_PERMISSIONS }))
                                          {
                                              return TextCommandResult.Success();
                                          }
                                          else
                                          {
                                              return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                          }
                                      }
                                      return TextCommandResult.Error("");
                                  })
                                 .WithAlias("p")
                                 .HandleWith(commands.CityCommand.CitySetPermissions)
                                 .IgnoreAdditionalArgs()
                             .EndSub()
                             .BeginSub("info")
                                 .WithDesc("City info of setters.")
                                 .HandleWith(commands.CityCommand.CitySetInfo)
                             .EndSub()
                             .BeginSub("name")
                                 .WithDesc("Set city name.")
                                 .WithPreCondition((TextCommandCallingArgs args) => {
                                      if (args.Caller.Player is IServerPlayer player)
                                      {
                                          if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SET_NAME, EnumPlayerPermissions.CITY_SET_ALL }))
                                          {
                                              return TextCommandResult.Success();
                                          }
                                          else
                                          {
                                              return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                          }
                                      }
                                      return TextCommandResult.Error("");
                                  })
                                 .WithArgs(parsers.Word("cityName"))
                                 .HandleWith(commands.CityCommand.SetCityName)
                             .EndSub()
                             .BeginSub("open")
                                 .WithDesc("Set city open/close.")
                                 .WithPreCondition((TextCommandCallingArgs args) => {
                                      if (args.Caller.Player is IServerPlayer player)
                                      {
                                          if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SET_OPEN_STATE, EnumPlayerPermissions.CITY_SET_ALL }))
                                          {
                                              return TextCommandResult.Success();
                                          }
                                          else
                                          {
                                              return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                          }
                                      }
                                      return TextCommandResult.Error("");
                                  })
                                 .WithArgs(parsers.WordRange("state", "on", "off"))
                                 .HandleWith(commands.CityCommand.CitySetOpen)
                             .EndSub()
                             .BeginSub("fee")
                                 .WithDesc("Set city fee.")
                                 .WithPreCondition((TextCommandCallingArgs args) => {
                                     if (args.Caller.Player is IServerPlayer player)
                                     {
                                         if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SET_GLOBAL_FEE, EnumPlayerPermissions.CITY_SET_ALL }))
                                         {
                                             return TextCommandResult.Success();
                                         }
                                         else
                                         {
                                             return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                         }
                                     }
                                     return TextCommandResult.Error("");
                                 })
                                 .WithArgs(parsers.Int("fee"))
                                 .HandleWith(commands.CityCommand.CitySetFee)
                             .EndSub()
                             .BeginSub("mayor")
                                 .WithDesc("Set city mayor")
                                 .WithArgs(parsers.Word("newMayorName"))
                                 .HandleWith(commands.CityCommand.CitySetMayor)
                             .EndSub()
                             .BeginSub("invmsg")
                                 .WithDesc("Set city's invite msg.")
                                 .WithPreCondition((TextCommandCallingArgs args) => {
                                     if (args.Caller.Player is IServerPlayer player)
                                     {
                                         if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SET_ALL, EnumPlayerPermissions.CITY_SET_INV_MSG }))
                                         {
                                             return TextCommandResult.Success();
                                         }
                                         else
                                         {
                                             return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                         }
                                     }
                                     return TextCommandResult.Error("");
                                 })
                                 .WithArgs(parsers.OptionalAll("invMsg"))
                                 .HandleWith(commands.CityCommand.CitySetInvMsg)
                             .EndSub()
                             .BeginSub("pvp")
                                 .WithDesc("Set pvp state.")
                                 .WithPreCondition((TextCommandCallingArgs args) => {
                                      if (args.Caller.Player is IServerPlayer player)
                                      {
                                          if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SET_ALL, EnumPlayerPermissions.CITY_SET_PVP }))
                                          {
                                              return TextCommandResult.Success();
                                          }
                                          else
                                          {
                                              return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                          }
                                      }
                                      return TextCommandResult.Error("");
                                  })
                                 .WithArgs(parsers.WordRange("state", "on", "off"))
                                 .HandleWith(commands.CityCommand.CitySetPvP)
                             .EndSub()
                             .BeginSub("fire")
                                 .WithDesc("Set fire spread state.")
                                 .WithPreCondition((TextCommandCallingArgs args) => {
                                      if (args.Caller.Player is IServerPlayer player)
                                      {
                                          if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SET_ALL, EnumPlayerPermissions.CITY_SET_FIRE }))
                                          {
                                              return TextCommandResult.Success();
                                          }
                                          else
                                          {
                                              return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                          }
                                      }
                                      return TextCommandResult.Error("");
                                  })
                                 .WithArgs(parsers.WordRange("state", "on", "off"))
                                 .HandleWith(commands.CityCommand.CitySetFire)
                             .EndSub()
                             .BeginSub("blast")
                                 .WithDesc("Set blast permission state.")
                                 .WithPreCondition((TextCommandCallingArgs args) => {
                                     if (args.Caller.Player is IServerPlayer player)
                                     {
                                         if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SET_ALL, EnumPlayerPermissions.CITY_SET_BLAST }))
                                         {
                                             return TextCommandResult.Success();
                                         }
                                         else
                                         {
                                             return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                         }
                                     }
                                     return TextCommandResult.Error("");
                                 })
                                 .WithArgs(parsers.WordRange("state", "on", "off"))
                                 .HandleWith(commands.CityCommand.CitySetBlast)
                             .EndSub()
                             .BeginSub("title")
                                 .WithDesc("Set citizen's title.")
                                 .WithPreCondition((TextCommandCallingArgs args) => {
                                      if (args.Caller.Player is IServerPlayer player)
                                      {
                                          if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SET_ALL, EnumPlayerPermissions.CITY_SET_OTHERS_PREFIX }))
                                          {
                                              return TextCommandResult.Success();
                                          }
                                          else
                                          {
                                              return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                          }
                                      }
                                      return TextCommandResult.Error("");
                                  })
                                 .HandleWith(commands.CityCommand.CitySetCitizenPrefix)
                                 .WithArgs(parsers.OptionalAll("playerName title"))
                             .EndSub()
                             .BeginSub("color")
                                 .WithArgs(parsers.Word("color"))
                                 .HandleWith(commands.CityCommand.CitySetPlotsColor)
                             .IgnoreAdditionalArgs()
                             .EndSub()
                             .BeginSub("colorint")
                                 .WithArgs(parsers.Word("colorint"))
                                 .HandleWith(commands.CityCommand.CitySetPlotsColorInt)
                             .IgnoreAdditionalArgs()
                             .EndSub()
                      .EndSub()
                      /////////
                      .BeginSub("invitesent")
                         .WithDesc("List all invitation city has sent.")
                          .WithPreCondition((TextCommandCallingArgs args) => {
                              if (args.Caller.Player is IServerPlayer player)
                              {
                                  if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.SHOW_INVITES_SENT }))
                                  {
                                      return TextCommandResult.Success();
                                  }
                                  else
                                  {
                                      return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                  }
                              }
                              return TextCommandResult.Error("");
                          })
                         .HandleWith(commands.CityCommand.ShowInvitesSent)
                         .WithArgs(parsers.OptionalInt("page"))
                     .EndSub()
                      ////////
                     .BeginSub("rank")
                            .BeginSub("list")
                              .WithPreCondition((TextCommandCallingArgs args) => {
                                  if (args.Caller.Player is IServerPlayer player)
                                  {
                                      if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SHOW_RANK_OTHERS }))
                                      {
                                          return TextCommandResult.Success();
                                      }
                                      else
                                      {
                                          return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                      }
                                  }
                                  return TextCommandResult.Error("");
                              })
                             .HandleWith(commands.CityCommand.CityRankList)
                             .WithDesc("List player's ranks.")
                             .WithArgs(parsers.OptionalWord("playerName"))
                         .EndSub()
                         .BeginSub("add")
                             .WithPreCondition((TextCommandCallingArgs args) => {
                                 if (args.Caller.Player is IServerPlayer player)
                                 {
                                     if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_SET_RANK }))
                                     {
                                         return TextCommandResult.Success();
                                     }
                                     else
                                     {
                                         return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                     }
                                 }
                                 return TextCommandResult.Error("");
                             })
                             .HandleWith(commands.CityCommand.CityRankAdd)
                             .WithDesc("Add rank to player.")
                             .WithArgs(parsers.OptionalAll("rank playerName"))
                         .EndSub()
                         .BeginSub("remove")
                               .WithPreCondition((TextCommandCallingArgs args) => {
                                   if (args.Caller.Player is IServerPlayer player)
                                   {
                                       if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_REMOVE_RANK }))
                                       {
                                           return TextCommandResult.Success();
                                       }
                                       else
                                       {
                                           return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                       }
                                   }
                                   return TextCommandResult.Error("");
                               })
                             .HandleWith(commands.CityCommand.CityRankRemove)
                             .WithDesc("Remove rank from player.")
                             .WithArgs(parsers.OptionalAll("rank playerName"))
                         .EndSub()
                     .EndSub()
                     /////////
                     .BeginSub("join")
                         .HandleWith(commands.CityCommand.CityJoin)
                         .WithDesc("Join an open city.")
                         .WithArgs(parsers.Word("cityName"))
                     .EndSub()
                     /////////
                     .BeginSub("prison")
                         .BeginSub("list")
                             .WithDesc("List all prison cells in the city.")
                              .WithPreCondition((TextCommandCallingArgs args) => {
                                  if (args.Caller.Player is IServerPlayer player)
                                  {
                                      if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_PRISON_ALL, EnumPlayerPermissions.CITY_PRISON_LIST }))
                                      {
                                          return TextCommandResult.Success();
                                      }
                                      else
                                      {
                                          return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                      }
                                  }
                                  return TextCommandResult.Error("");
                              })
                             .HandleWith(commands.CityCommand.PrisonList)
                         .EndSub()
                         .BeginSub("addcell")
                             .WithDesc("Add new cell in the prison plot.")
                             .WithPreCondition((TextCommandCallingArgs args) => {
                                 if (args.Caller.Player is IServerPlayer player)
                                 {
                                     if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_PRISON_ALL, EnumPlayerPermissions.CITY_PRISON_ADD_CELL }))
                                     {
                                         return TextCommandResult.Success();
                                     }
                                     else
                                     {
                                         return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                     }
                                 }
                                 return TextCommandResult.Error("");
                             })
                             .HandleWith(commands.CityCommand.AddPrisonCell)
                         .EndSub()
                         .BeginSub("removecell")
                              .WithPreCondition((TextCommandCallingArgs args) => {
                                  if (args.Caller is IServerPlayer player)
                                  {
                                      if (commands.CityCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.CITY_PRISON_ALL, EnumPlayerPermissions.CITY_PRISON_REMOVE_CELL }))
                                      {
                                          return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                      }
                                      else
                                      {
                                          return TextCommandResult.Success();
                                      }

                                  }
                                  return TextCommandResult.Error("");
                              })
                             .WithDesc("Remove prison cell by number.")
                             .HandleWith(commands.CityCommand.RemovePrisonCell)
                             .WithArgs(parsers.Int("prisonCellNumber"))
                         .EndSub()
                     .EndSub()
                /////////
                ;


            /*                 
                                               
                     /////////
                     .BeginSub("summonlist")
                         .WithDesc("Get summon points of the city.")
                         .HandleWith(commands.CityCommand.processSummonList)
                     .EndSub()
                     /////////
                     .BeginSub("criminal")
                         .BeginSub("list")
                             .WithDesc("List all criminals.")
                             .HandleWith(commands.CityCommand.processCityCriminalList)
                         .EndSub()
                         .BeginSub("add")
                             .WithDesc("Add new criminal.")
                             .HandleWith(commands.CityCommand.processCityCriminalAdd)
                             .WithArgs(parsers.Word("playerName"))
                         .EndSub()
                         .BeginSub("remove")
                             .WithDesc("Remove criminal from the list.")
                             .HandleWith(commands.CityCommand.processCityCriminalRemove)
                             .WithArgs(parsers.Word("playerName"))
                         .EndSub()
                     .EndSub()
                     /////////    
                     
                     /////////
                     .BeginSub("summon")
                         .BeginSub("set")
                             .WithDesc("Set a new summon point.")
                             .HandleWith(commands.CityCommand.processSummonSet)
                         .EndSub()
                         .BeginSub("list")
                             .WithDesc("List of all summon points.")
                             .HandleWith(commands.CityCommand.processSummonList)
                         .EndSub()
                         .BeginSub("use")
                             .WithDesc("Teleport on selected point.")
                             .HandleWith(commands.CityCommand.processSummonTeleport)
                             .WithArgs(parsers.OptionalInt("summonPointNumber"))
                         .EndSub()
                     .EndSub()
                     /////////
                     .BeginSub("plotsgroup")
                         .BeginSub("create")
                             .WithArgs(parsers.Word("groupName"))
                             .HandleWith(commands.CityCommand.PlotsGroupCreate)
                             .WithDesc("Create new plotsgroup")
                         .EndSub()
                         .BeginSub("delete")
                             .HandleWith(commands.CityCommand.PlotsGroupDelete)
                             .WithDesc("Remove plotsgroup")
                             .WithArgs(parsers.Word("groupName"))
                         .EndSub()
                         .BeginSub("list")
                             .HandleWith(commands.CityCommand.PlotsGroupList)
                             .WithDesc("List plotsgroups")
                         .EndSub()
                         .BeginSub("listplayers")
                             .HandleWith(commands.CityCommand.PlotsGroupListPlayers)
                             .WithDesc("List players in the plotsgroup")
                             .WithArgs(parsers.Word("groupName"))
                         .EndSub()
                         .BeginSub("add")
                             .HandleWith(commands.CityCommand.PlotsGroupAddTo)
                             .WithDesc("Add player to the plotsgroup")
                             .IgnoreAdditionalArgs()
                         .EndSub()
                         .BeginSub("unadd")
                             .WithDesc("")
                             .HandleWith(commands.CityCommand.PlotsGroupUnaddTo)
                         .EndSub()
                         .BeginSub("kick")
                             .WithDesc("Kick player from the plotsgroup")
                             .HandleWith(commands.CityCommand.PlotsGroupKickFrom)
                             .WithArgs(parsers.All("groupName playerName"))
                         .EndSub()
                         .BeginSub("plotadd")
                             .HandleWith(commands.CityCommand.processPlotAdd)
                             .WithDesc("Add plot to the plotsgroup")
                             .WithArgs(parsers.Word("groupName"))
                         .EndSub()
                         .BeginSub("plotremove")
                             .HandleWith(commands.CityCommand.processPlotRemove)
                             .WithDesc("Remove plot from the plotsgroup")
                             .WithArgs(parsers.Word("plotgroup"))
                         .EndSub()
                         .BeginSub("set")
                             .HandleWith(commands.CityCommand.PlotsGroupSet)
                             .WithArgs(parsers.All("p groupname permgroup permtype"))
                         .EndSub()
                     .EndSub()
                 ;*/
        }
        public static void RegisterCitizenCommands(CommandArgumentParsers parsers, ICoreServerAPI sapi)
        {
            sapi.ChatCommands.Create("citizen")
                  .WithAlias("ci")
                  .RequiresPrivilege(Privilege.chat)
                  .BeginSub("info")
                       .HandleWith(commands.CitizenCommand.CitizenInfo)
                       .WithArgs(parsers.Word("playerName"))
                       .WithDesc("Print info about selected player.")
                   .EndSub()
                   /////////
                    .BeginSub("prices")
                       .HandleWith(commands.CitizenCommand.CitizenPrices)
                       .WithDesc("List prices.")
                   .EndSub()
                    /////////
                    .BeginSub("nextdaytimer")
                       .WithAlias("ndt")
                       .HandleWith(commands.CitizenCommand.NextDayTimer)
                       .WithDesc("Show time until a next mod's day.")
                   .EndSub()
                    /////////
                    .BeginSub("invitelist")
                       .HandleWith(commands.CitizenCommand.InvitesList)
                       .WithDesc("List all invites to cities you got.")
                       .WithArgs(parsers.OptionalInt("pageNumber"))
                   .EndSub()
                    /////////
                    .BeginSub("friend")
                       .BeginSub("info")
                           .HandleWith(commands.CitizenCommand.FriendsList)
                           .WithDesc("Show list of friends")
                       .EndSub()
                       .BeginSub("add")
                           .HandleWith(commands.CitizenCommand.FriendAdd)
                           .WithDesc("Add new friend")
                           .WithArgs(parsers.Word("playerName"))
                       .EndSub()
                       .BeginSub("remove")
                           .HandleWith(commands.CitizenCommand.FriendRemove)
                           .WithDesc("Remove friend")
                           .WithArgs(parsers.Word("playerName"))
                       .EndSub()
                   .EndSub()
                   /////////
                   
                   ;
            /*sapi.ChatCommands.Create("citizen")
                  .RequiresPrivilege(Privilege.chat)
                   .BeginSub("fees")
                       .HandleWith(commands.CitizenCommand.processFee)
                       .WithDesc("Show fee state.")
                       .WithAlias("feeState")
                   .EndSub()

                   .BeginSub("prices")
                       .HandleWith(commands.CitizenCommand.processCitizenPrices)
                       .WithDesc("Prices list")
                   .EndSub()
                   .BeginSub("prison")
                       .BeginSub("info")
                           .HandleWith(commands.CitizenCommand.prisonInfo)
                           .WithDesc("Show prison prices")
                       .EndSub()
                       .BeginSub("payout")
                           .HandleWith(commands.CitizenCommand.processPrisonPayout)
                           .WithDesc("Prison handle")
                       .EndSub()
                   .EndSub()
                  
                   .BeginSub("set")
                       .BeginSub("info")
                           .HandleWith(commands.CitizenCommand.citizenSetInfo)
                           .WithDesc("Show player's perms for friends")
                       .EndSub()
                       .BeginSub("permissions")
                           .WithAlias("p")
                           .HandleWith(commands.CitizenCommand.SetPerm)
                           .WithArgs(parsers.All("permGroup permType on/off"))
                       .EndSub()
                   .EndSub();*/
        }
        public static void RegisterPlotCommands(CommandArgumentParsers parsers, ICoreServerAPI sapi)
        {
            sapi.ChatCommands.Create("plot")
               .RequiresPrivilege(Privilege.chat)
                   .BeginSub("here")
                       .HandleWith(commands.PlotCommand.HereInfo)
                       .WithDesc("List info about plot you stand on.")
                   .EndSub()
                   /////////
                   .BeginSub("borders")
                       .WithArgs(parsers.WordRange("on/off", "on", "off"))
                       .WithDesc("Select borders of plot you currently stand in.")
                       .HandleWith(commands.PlotCommand.plotBorders)
                   .EndSub()
                   /////////
                   .BeginSub("claim")
                       .HandleWith(commands.PlotCommand.PlotClaim)
                       .WithDesc("Buy plot you current stand on.(Ownership by a player)")
                   .EndSub()
                   /////////
                   .BeginSub("unclaim")
                       .HandleWith(commands.PlotCommand.PlotUnclaim)
                       .WithDesc("Unclaim plot you stand on.(Remove ownership)")
                   .EndSub()
                    /////////
                    .BeginSub("set")
                       .BeginSub("permissions")
                            .WithPreCondition((TextCommandCallingArgs args) => {
                                if (args.Caller.Player is IServerPlayer player)
                                {
                                    if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS, EnumPlayerPermissions.PLOT_SET_PLOT_ACCESS_PERMISSIONS  }))
                                    {
                                        return TextCommandResult.Success();
                                    }
                                    else
                                    {
                                        return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                    }

                                }
                                return TextCommandResult.Error("");
                            })
                           .WithAlias("p")
                           .HandleWith(commands.PlotCommand.SetPermissions)
                           .WithDesc("Set plot permissions")
                           .IgnoreAdditionalArgs()
                       .EndSub()
                       .BeginSub("pvp")
                            .WithPreCondition((TextCommandCallingArgs args) => {
                                if (args.Caller.Player is IServerPlayer player)
                                {
                                    if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS, EnumPlayerPermissions.PLOT_SET_PVP }))
                                    {
                                        return TextCommandResult.Success();
                                    }
                                    else
                                    {
                                        return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                    }

                                }
                                return TextCommandResult.Error("");
                            })
                           .HandleWith(commands.PlotCommand.SetPvp)
                           .WithDesc("Pvp state")
                           .WithArgs(parsers.WordRange("state", "on", "off"))
                       .EndSub()
                       .BeginSub("fire")
                           .WithPreCondition((TextCommandCallingArgs args) => {
                               if (args.Caller.Player is IServerPlayer player)
                               {
                                   if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS, EnumPlayerPermissions.PLOT_SET_FIRE }))
                                   {
                                       return TextCommandResult.Success();
                                   }
                                   else
                                   {
                                       return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                   }

                               }
                               return TextCommandResult.Error("");
                           })
                           .HandleWith(commands.PlotCommand.SetFire)
                           .WithDesc("Firespread state")
                           .WithArgs(parsers.WordRange("state", "on", "off"))
                       .EndSub()
                       .BeginSub("blast")
                           .WithPreCondition((TextCommandCallingArgs args) => {
                               if (args.Caller.Player is IServerPlayer player)
                               {
                                   if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS, EnumPlayerPermissions.PLOT_SET_BLAST }))
                                   {
                                       return TextCommandResult.Success();
                                   }
                                   else
                                   {
                                       return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                   }

                               }
                               return TextCommandResult.Error("");
                           })
                           .HandleWith(commands.PlotCommand.SetBlast)
                           .WithDesc("Blast state")
                           .WithArgs(parsers.WordRange("state", "on", "off"))
                       .EndSub()
                       .BeginSub("name")
                           .WithPreCondition((TextCommandCallingArgs args) => {
                               if (args.Caller.Player is IServerPlayer player)
                               {
                                   if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS, EnumPlayerPermissions.PLOT_SET_NAME }))
                                   {
                                       return TextCommandResult.Success();
                                   }
                                   else
                                   {
                                       return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                   }

                               }
                               return TextCommandResult.Error("");
                           })
                           .HandleWith(commands.PlotCommand.SetName)
                           .WithDesc("Set plot's name")
                           .WithArgs(parsers.Word("plotName"))
                       .EndSub()
                       .BeginSub("fee")
                           .WithPreCondition((TextCommandCallingArgs args) => {
                               if (args.Caller.Player is IServerPlayer player)
                               {
                                   if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS, EnumPlayerPermissions.PLOT_SET_FEE }))
                                   {
                                       return TextCommandResult.Success();
                                   }
                                   else
                                   {
                                       return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                   }

                               }
                               return TextCommandResult.Error("");
                           })
                           .HandleWith(commands.PlotCommand.SetFee)
                           .WithDesc("Set plot fee")
                           .WithArgs(parsers.Int("fee"))
                       .EndSub()
                       .BeginSub("type")
                            .WithPreCondition((TextCommandCallingArgs args) => {
                                if (args.Caller.Player is IServerPlayer player)
                                {
                                    if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS, EnumPlayerPermissions.PLOT_SET_TYPE }))
                                    {
                                        return TextCommandResult.Success();
                                    }
                                    else
                                    {
                                        return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                    }

                                }
                                return TextCommandResult.Error("");
                            })
                           .HandleWith(commands.PlotCommand.SetType)
                           .WithDesc("Set plot type")
                           .WithArgs(parsers.Word("plotType"))
                       .EndSub()
               .EndSub()
               /////////
               .BeginSub("fs")
                    .WithPreCondition((TextCommandCallingArgs args) => {
                        if (args.Caller.Player is IServerPlayer player)
                        {
                            if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS, EnumPlayerPermissions.PLOT_SET_FS }))
                            {
                                return TextCommandResult.Success();
                            }
                            else
                            {
                                return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                            }

                        }
                        return TextCommandResult.Error("");
                    })
                   .HandleWith(commands.PlotCommand.SetForSale)
                   .WithArgs(parsers.Int("price"))
                   .WithDesc("Set plot for sale")
                   .WithAlias("forsale")
               .EndSub()
                /////////
                .BeginSub("nfs")
                .WithPreCondition((TextCommandCallingArgs args) => {
                    if (args.Caller.Player is IServerPlayer player)
                    {
                        if (BaseCommand.CheckForPlayerPermissions(player, new EnumPlayerPermissions[] { EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS, EnumPlayerPermissions.PLOT_SET_NFS }))
                        {
                            return TextCommandResult.Success();
                        }
                        else
                        {
                            return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                        }

                    }
                    return TextCommandResult.Error("");
                })
                   .HandleWith(commands.PlotCommand.SetNotForSale)
                   .WithDesc("Remove plot from sale")
                   .WithAlias("notforsale")
               .EndSub()
                   ;

            /*              
               .BeginSub("innerclaim")
                   .IgnoreAdditionalArgs()
                   .HandleWith(commands.plotCommand.processInnerClaim)
               .EndSub()
               .BeginSub("plotmsgs")
                   .WithArgs(parsers.WordRange("state", "0", "1", "2", "3"))
                   .WithDesc("Show nothing - 0, only msgs - 1, only hud - 2, both - 3")
                   .HandleWith(commands.plotCommand.plotMsgs)
               .EndSub()
              
               ;*/
        }
        public static void RegisterCAdminCommands(CommandArgumentParsers parsers, ICoreServerAPI sapi)
        {
            sapi.ChatCommands.Create("cadmin")
                .RequiresPlayer().RequiresPrivilege(Privilege.controlserver)
                .BeginSub("nday")
                     .WithPreCondition((TextCommandCallingArgs args) => {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.CAdminCommand.triggerNextDay)
                    .WithDesc("Trigger new day events")
                .EndSub()
                .BeginSub("nhour")
                     .WithPreCondition((TextCommandCallingArgs args) => {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.CAdminCommand.triggerNextHour)
                    .WithDesc("Trigger new hour events")
                .EndSub()
                .BeginSub("plot")
                    .BeginSub("set")
                       .BeginSub("permissions")
                             .WithAlias("p")
                               .WithPreCondition((TextCommandCallingArgs args) => {

                                   if (args.Caller.Player is IServerPlayer player)
                                   {
                                       if (!player.Role.Code.Equals("admin"))
                                       {
                                           return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                       }
                                       return TextCommandResult.Success();

                                   }
                                   return TextCommandResult.Error("");
                               })
                             .HandleWith(commands.CAdminCommand.plotPermissions)
                             .IgnoreAdditionalArgs()
                        .EndSub()
                        .BeginSub("pvp")
                             .WithPreCondition((TextCommandCallingArgs args) => {

                                 if (args.Caller.Player is IServerPlayer player)
                                 {
                                     if (!player.Role.Code.Equals("admin"))
                                     {
                                         return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                     }
                                     return TextCommandResult.Success();

                                 }
                                 return TextCommandResult.Error("");
                             })
                            .HandleWith(commands.CAdminCommand.plotPvp)
                            .WithDesc("Pvp state")
                            .WithArgs(parsers.WordRange("state", "on", "off"))
                        .EndSub()
                        .BeginSub("blast")
                             .WithPreCondition((TextCommandCallingArgs args) => {

                                 if (args.Caller.Player is IServerPlayer player)
                                 {
                                     if (!player.Role.Code.Equals("admin"))
                                     {
                                         return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                     }
                                     return TextCommandResult.Success();

                                 }
                                 return TextCommandResult.Error("");
                             })
                            .HandleWith(commands.CAdminCommand.plotBlast)
                            .WithDesc("Blast state")
                            .WithArgs(parsers.WordRange("state", "on", "off"))
                        .EndSub()
                        .BeginSub("fire")
                             .WithPreCondition((TextCommandCallingArgs args) => {

                                 if (args.Caller.Player is IServerPlayer player)
                                 {
                                     if (!player.Role.Code.Equals("admin"))
                                     {
                                         return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                     }
                                     return TextCommandResult.Success();

                                 }
                                 return TextCommandResult.Error("");
                             })
                            .HandleWith(commands.CAdminCommand.plotFire)
                            .WithDesc("Fire state")
                            .WithArgs(parsers.WordRange("state", "on", "off"))
                        .EndSub()
                    .EndSub()
                    .BeginSub("type")
                        .WithPreCondition((TextCommandCallingArgs args) => {

                            if (args.Caller.Player is IServerPlayer player)
                            {
                                if (!player.Role.Code.Equals("admin"))
                                {
                                    return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                }
                                return TextCommandResult.Success();

                            }
                            return TextCommandResult.Error("");
                        })
                        .HandleWith(commands.CAdminCommand.plotType)
                        .WithArgs(parsers.Word("type"))
                    .EndSub()
                    .BeginSub("fee")
                        .WithPreCondition((TextCommandCallingArgs args) => {

                            if (args.Caller.Player is IServerPlayer player)
                            {
                                if (!player.Role.Code.Equals("admin"))
                                {
                                    return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                }
                                return TextCommandResult.Success();

                            }
                            return TextCommandResult.Error("");
                        })
                        .HandleWith(commands.CAdminCommand.plotFee)
                        .WithArgs(parsers.Int("fee"))
                    .EndSub()
                    .BeginSub("fs")
                         .WithPreCondition((TextCommandCallingArgs args) => {

                             if (args.Caller.Player is IServerPlayer player)
                             {
                                 if (!player.Role.Code.Equals("admin"))
                                 {
                                     return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                 }
                                 return TextCommandResult.Success();

                             }
                             return TextCommandResult.Error("");
                         })
                        .HandleWith(commands.CAdminCommand.plotForSale)
                        .WithArgs(parsers.Int("price"))
                    .EndSub()
                .EndSub()
                .BeginSub("city")
                    .BeginSub("new")
                         .WithPreCondition((TextCommandCallingArgs args) => {

                             if (args.Caller.Player is IServerPlayer player)
                             {
                                 if (!player.Role.Code.Equals("admin"))
                                 {
                                     return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                 }
                                 return TextCommandResult.Success();

                             }
                             return TextCommandResult.Error("");
                         })
                        .HandleWith(commands.CAdminCommand.cCreateCity)
                        .WithArgs(parsers.Word("cityName"))
                        .WithDesc("New city creation")
                    .EndSub()
                    .BeginSub("delete")
                        .WithPreCondition((TextCommandCallingArgs args) => {

                            if (args.Caller.Player is IServerPlayer player)
                            {
                                if (!player.Role.Code.Equals("admin"))
                                {
                                    return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                }
                                return TextCommandResult.Success();

                            }
                            return TextCommandResult.Error("");
                        })
                        .HandleWith(commands.CAdminCommand.cityDelete)
                        .WithDesc("City remove")
                        .WithArgs(parsers.Word("cityName"))
                    .EndSub()
                    .BeginSub("claim")
                        .WithPreCondition((TextCommandCallingArgs args) => {

                            if (args.Caller.Player is IServerPlayer player)
                            {
                                if (!player.Role.Code.Equals("admin"))
                                {
                                    return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                }
                                return TextCommandResult.Success();

                            }
                            return TextCommandResult.Error("");
                        })
                        .HandleWith(commands.CAdminCommand.cityClaim)
                        .WithDesc("Claim plot for city")
                        .WithArgs(parsers.Word("cityName"))
                    .EndSub()
                    .BeginSub("unclaim")
                         .WithPreCondition((TextCommandCallingArgs args) => {

                             if (args.Caller.Player is IServerPlayer player)
                             {
                                 if (!player.Role.Code.Equals("admin"))
                                 {
                                     return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                 }
                                 return TextCommandResult.Success();

                             }
                             return TextCommandResult.Error("");
                         })
                        .HandleWith(commands.CAdminCommand.cityUnclaim)
                        .WithDesc("Unclaim city plot")
                        .WithArgs(parsers.Word("cityName"))
                    .EndSub()
                    .BeginSub("kick")
                          .WithPreCondition((TextCommandCallingArgs args) => {

                              if (args.Caller.Player is IServerPlayer player)
                              {
                                  if (!player.Role.Code.Equals("admin"))
                                  {
                                      return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                  }
                                  return TextCommandResult.Success();

                              }
                              return TextCommandResult.Error("");
                          })
                        .HandleWith(commands.CAdminCommand.cCityPlayerKick)
                        .WithDesc("Kick player from city")
                        .IgnoreAdditionalArgs()
                    .EndSub()
                    .BeginSub("add")
                          .WithPreCondition((TextCommandCallingArgs args) => {

                              if (args.Caller.Player is IServerPlayer player)
                              {
                                  if (!player.Role.Code.Equals("admin"))
                                  {
                                      return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                  }
                                  return TextCommandResult.Success();

                              }
                              return TextCommandResult.Error("");
                          })
                        .HandleWith(commands.CAdminCommand.cCityPlayerAdd)
                        .WithDesc("Add player to the city")
                        .IgnoreAdditionalArgs()
                    .EndSub()
                    .BeginSub("set")
                        .BeginSub("name")
                            .WithPreCondition((TextCommandCallingArgs args) => {

                                if (args.Caller.Player is IServerPlayer player)
                                {
                                    if (!player.Role.Code.Equals("admin"))
                                    {
                                        return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                    }
                                    return TextCommandResult.Success();

                                }
                                return TextCommandResult.Error("");
                            })
                            .HandleWith(commands.CAdminCommand.cCitySetName)
                            .WithArgs(parsers.Word("cityName"), parsers.Word("newName"))
                        .EndSub()
                        .BeginSub("pvp")
                            .WithPreCondition((TextCommandCallingArgs args) => {

                                if (args.Caller.Player is IServerPlayer player)
                                {
                                    if (!player.Role.Code.Equals("admin"))
                                    {
                                        return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                    }
                                    return TextCommandResult.Success();

                                }
                                return TextCommandResult.Error("");
                            })
                            .HandleWith(commands.CAdminCommand.citySetPvp)
                            .WithArgs(parsers.Word("cityName"), parsers.Word("state"))
                        .EndSub()
                        .BeginSub("fire")
                            .WithPreCondition((TextCommandCallingArgs args) => {

                                if (args.Caller.Player is IServerPlayer player)
                                {
                                    if (!player.Role.Code.Equals("admin"))
                                    {
                                        return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                    }
                                    return TextCommandResult.Success();

                                }
                                return TextCommandResult.Error("");
                            })
                            .HandleWith(commands.CAdminCommand.citySetFire)
                            .WithArgs(parsers.Word("cityName"), parsers.Word("state"))
                        .EndSub()
                        .BeginSub("blast")
                            .WithPreCondition((TextCommandCallingArgs args) => {

                                if (args.Caller.Player is IServerPlayer player)
                                {
                                    if (!player.Role.Code.Equals("admin"))
                                    {
                                        return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                    }
                                    return TextCommandResult.Success();

                                }
                                return TextCommandResult.Error("");
                            })
                            .HandleWith(commands.CAdminCommand.citySetBlast)
                            .WithArgs(parsers.Word("cityName"), parsers.Word("state"))
                        .EndSub()
                        .BeginSub("open")
                            .WithPreCondition((TextCommandCallingArgs args) => {

                                if (args.Caller.Player is IServerPlayer player)
                                {
                                    if (!player.Role.Code.Equals("admin"))
                                    {
                                        return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                    }
                                    return TextCommandResult.Success();

                                }
                                return TextCommandResult.Error("");
                            })
                            .HandleWith(commands.CAdminCommand.citySetOpenClosed)
                            .WithArgs(parsers.Word("cityName"), parsers.Word("state"))
                        .EndSub()
                        .BeginSub("technical")
                             .WithPreCondition((TextCommandCallingArgs args) => {

                                 if (args.Caller.Player is IServerPlayer player)
                                 {
                                     if (!player.Role.Code.Equals("admin"))
                                     {
                                         return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                     }
                                     return TextCommandResult.Success();

                                 }
                                 return TextCommandResult.Error("");
                             })
                            .HandleWith(commands.CAdminCommand.citySetTechnical)
                            .WithArgs(parsers.Word("cityName"), parsers.Word("state"))
                        .EndSub()
                        .BeginSub("fee")
                             .WithPreCondition((TextCommandCallingArgs args) => {

                                 if (args.Caller.Player is IServerPlayer player)
                                 {
                                     if (!player.Role.Code.Equals("admin"))
                                     {
                                         return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                     }
                                     return TextCommandResult.Success();

                                 }
                                 return TextCommandResult.Error("");
                             })
                            .HandleWith(commands.CAdminCommand.citySetFee)
                            .WithArgs(parsers.Word("cityName"), parsers.Int("fee"))
                        .EndSub()
                        .BeginSub("mayor")
                             .WithPreCondition((TextCommandCallingArgs args) => {

                                 if (args.Caller.Player is IServerPlayer player)
                                 {
                                     if (!player.Role.Code.Equals("admin"))
                                     {
                                         return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                                     }
                                     return TextCommandResult.Success();

                                 }
                                 return TextCommandResult.Error("");
                             })
                            .HandleWith(commands.CAdminCommand.citySetMayor)
                            .WithArgs(parsers.Word("cityName"), parsers.Word("newMayorName"))
                        .EndSub()
                    .EndSub()
                .EndSub()
                /*.BeginSub("claimscolor")
                    .HandleWith(commands.CAdminCommand.processClaimsColor)
                    .IgnoreAdditionalArgs()
                .EndSub()*/
                .BeginSub("world")
                    .WithPreCondition((TextCommandCallingArgs args) => {

                        if (args.Caller.Player is IServerPlayer player)
                        {
                            if (!player.Role.Code.Equals("admin"))
                            {
                                return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                            }
                            return TextCommandResult.Success();

                        }
                        return TextCommandResult.Error("");
                    })
                    .HandleWith(commands.CAdminCommand.cWorldCommands)
                    .IgnoreAdditionalArgs()
                .EndSub()
                /*.BeginSub("gb")
                    .HandleWith(commands.CAdminCommand.cGlobalBank)
                    .IgnoreAdditionalArgs()
                .EndSub()*/
                .BeginSub("backup")
                    .WithPreCondition((TextCommandCallingArgs args) => {

                        if (args.Caller.Player is IServerPlayer player)
                        {
                            if (!player.Role.Code.Equals("admin"))
                            {
                                return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                            }
                            return TextCommandResult.Success();

                        }
                        return TextCommandResult.Error("");
                    })
                    .HandleWith(commands.CAdminCommand.processBackup)
                .EndSub()
                ;
        }     
        public static void RegisterClaimAreaCommands(CommandArgumentParsers parsers, ICoreServerAPI sapi)
        {
            sapi.ChatCommands.Create("claimarea")
                .RequiresPlayer().RequiresPrivilege(Privilege.controlserver)
                .BeginSub("new")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.ClaimAreaCommands.NewArea)
                    .WithDesc("Start a new claim area")
                .EndSub()
                .BeginSub("start")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.ClaimAreaCommands.StartPoint)
                    .WithDesc("Select start point of claim area")
                .EndSub()
                .BeginSub("end")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.ClaimAreaCommands.EndPoint)
                    .WithDesc("Select end point of claim area")
                .EndSub()
                .BeginSub("save")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.ClaimAreaCommands.SaveArea)
                    .WithArgs(parsers.Word("areaName"))
                    .WithDesc("Save claim area by selected start/end")
                .EndSub()
                .BeginSub("list")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.ClaimAreaCommands.ListAreas)
                    .WithArgs(parsers.Int("pageNume"))
                    .WithDesc("List page of claim areas")
                .EndSub()
                /*.BeginSub("showid")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.ClaimAreaCommands.ShowAreaById)
                    .WithDesc("Trigger new day events")
                .EndSub()*/
                 .BeginSub("showhere")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.ClaimAreaCommands.ShowAreaHere)
                    .WithDesc("Show borders of area claim on player's position")
                .EndSub()
                .BeginSub("delete")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.ClaimAreaCommands.DeleteAreaName)
                    .WithArgs(parsers.Word("areaName"))
                    .WithDesc("Delete claim area by it's name")
                .EndSub()
                .BeginSub("deletenum")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                     .WithArgs(parsers.Int("areaNum"))
                    .HandleWith(commands.ClaimAreaCommands.DeleteAreaNumber)
                    .WithDesc("Delete claim area by it's number in the list")
                .EndSub()
                .BeginSub("setaccess")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                    .HandleWith(commands.ClaimAreaCommands.SetAreaBlockAccess)
                    .WithArgs(parsers.WordRange("flag", "build","use","attack","break"), parsers.WordRange("state", "on", "off"))
                    .WithDesc("Set acces flag - build, use, attack, break - on/off")
                .EndSub()
                .BeginSub("setflag")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                     .WithArgs(parsers.WordRange("flag", "pvp", "fire", "blast"), parsers.WordRange("state", "on", "off"))
                    .HandleWith(commands.ClaimAreaCommands.SetAreaBlockAccess)
                    .WithDesc("Set flag - pvp, fire, blast - on/off")
                .EndSub().BeginSub("info")
                     .WithPreCondition((TextCommandCallingArgs args) =>
                     {

                         if (args.Caller.Player is IServerPlayer player)
                         {
                             if (!player.Role.Code.Equals("admin"))
                             {
                                 return TextCommandResult.Error(Lang.Get("claims:you_dont_have_right_for_that_command"));
                             }
                             return TextCommandResult.Success();

                         }
                         return TextCommandResult.Error("");
                     })
                     .WithArgs(parsers.Word("areaName"))
                    .HandleWith(commands.ClaimAreaCommands.ShowAreaInfoHere)
                    .WithDesc("Show info about claim area by it's name")
                .EndSub();
        }
    }
}
