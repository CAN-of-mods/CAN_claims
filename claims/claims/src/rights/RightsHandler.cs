using claims.src.auxialiry;
using claims.src.messages;
using claims.src.part;
using claims.src.rights;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;

namespace claims.src
{
    public class RightsHandler
    {
        public static HashSet<EnumPlayerPermissions> GetPermsByGroup(string group)
        {
            if (PlayerPermissionsByGroups.TryGetValue(group, out HashSet<EnumPlayerPermissions> list))
            {
                return list;
            }
            return null;
        }
        static Dictionary<string, HashSet<EnumPlayerPermissions>> PlayerPermissionsByGroups = new Dictionary<string, HashSet<EnumPlayerPermissions>>();        
        private static Dictionary<string, HashSet<EnumPlayerPermissions>> getDefaultRankPermsDict()
        {
            Dictionary<string, HashSet<EnumPlayerPermissions>> outDict = new Dictionary<string, HashSet<EnumPlayerPermissions>>
            {
                 { "DEFAULT", new HashSet<EnumPlayerPermissions> 
                    { 
                        EnumPlayerPermissions.PLOT_CLAIM,
                        EnumPlayerPermissions.PLOT_UNCLAIM,
                        EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS,
                        EnumPlayerPermissions.CITY_HERE,
                        EnumPlayerPermissions.CITY_INFO

                    } 
                 },

                 { "MAYOR", new HashSet<EnumPlayerPermissions>
                    {
                        EnumPlayerPermissions.CITY_CLAIM_PLOT,
                        EnumPlayerPermissions.CITY_UNCLAIM_PLOT,
                        EnumPlayerPermissions.CITY_UNINVITE,
                        EnumPlayerPermissions.CITY_INVITE,
                        EnumPlayerPermissions.CITY_KICK,
                        EnumPlayerPermissions.CITY_SET_ALL,
                        EnumPlayerPermissions.PLOT_SET_ALL_CITY_PLOTS,
                        EnumPlayerPermissions.CITY_CRIMINAL_ALL,
                        EnumPlayerPermissions.CITY_PRISON_ALL,
                        EnumPlayerPermissions.CITY_SET_OTHERS_PREFIX,
                        EnumPlayerPermissions.CITY_SHOW_RANK_OTHERS,
                        EnumPlayerPermissions.CITY_SET_RANK,
                        EnumPlayerPermissions.CITY_REMOVE_RANK,
                        EnumPlayerPermissions.CITY_SET_PLOTS_COLOR,
                        EnumPlayerPermissions.CITY_SEE_BALANCE,
                        EnumPlayerPermissions.CITY_BUY_EXTRA_PLOT,
                        EnumPlayerPermissions.CITY_REMOVE_CRIMINAL,
                        EnumPlayerPermissions.CITY_ADD_CRIMINAL,
                        EnumPlayerPermissions.CITY_SET_SUMMON
                    }
                },

                { "CITY_ASSISTANT", new HashSet<EnumPlayerPermissions>
                    {
                        EnumPlayerPermissions.CITY_CLAIM_PLOT,
                        EnumPlayerPermissions.CITY_UNCLAIM_PLOT,
                        EnumPlayerPermissions.CITY_KICK,
                        EnumPlayerPermissions.CITY_INVITE,
                        EnumPlayerPermissions.CITY_SET_PLOTS_COLOR,
                        EnumPlayerPermissions.CITY_SEE_BALANCE
                    }
                },

            };
            return outDict;
        }
        public static bool ExistCityRank(string val)
        {
            foreach(var it in PlayerPermissionsByGroups)
            {
                if(it.Key.StartsWith("CITY_"))
                {
                    string withoutPrefix = it.Key.Substring(5);
                    if(val.Equals(withoutPrefix) || val.ToLower().Equals(withoutPrefix.ToLower()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static List<string> GetCityRanks()
        {
            var list = new List<string>();
            foreach (var it in PlayerPermissionsByGroups)
            {
                if (it.Key.StartsWith("CITY_"))
                {
                    list.Add(it.Key.Substring(5));
                }
            }
            return list;
        }
        public static void reapplyRights(PlayerInfo playerInfo)
        {
            IServerPlayer player = claims.sapi.World.PlayerByUid(playerInfo.Guid) as IServerPlayer;
            if(player == null)
            {
                return;
            }
            playerInfo.PlayerPermissionsHandler.ClearPermissions();
            if (PlayerPermissionsByGroups.TryGetValue("DEFAULT", out HashSet<EnumPlayerPermissions> strangerPerms))
            {
                playerInfo.PlayerPermissionsHandler.AddPermissions(strangerPerms);

            }
            City city = playerInfo.City;
            if (city != null) 
            {
                if (city.isMayor(playerInfo))
                {
                    if (PlayerPermissionsByGroups.TryGetValue("MAYOR", out HashSet<EnumPlayerPermissions> mayorPerms))
                    {
                        playerInfo.PlayerPermissionsHandler.AddPermissions(mayorPerms);
                    }
                }
                foreach (string str in playerInfo.getCityTitles())
                {
                    if (PlayerPermissionsByGroups.TryGetValue("CITY_" + str, out HashSet<EnumPlayerPermissions> titlePerms))
                    {
                        playerInfo.PlayerPermissionsHandler.AddPermissions(titlePerms);
                    }
                }
            }
            UsefullPacketsSend.AddToQueuePlayerInfoUpdate(playerInfo.Guid, gui.playerGui.structures.EnumPlayerRelatedInfo.PLAYER_PERMISSIONS);
        }
        public static HashSet<string> playersRights(string playerUID)
        {
            return (claims.sapi.World.PlayerByUid(playerUID) as IServerPlayer).ServerData.PermaPrivileges;
        }
        public class StringEnumConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType.IsEnum;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.String)
                {
                    string enumString = reader.Value.ToString();
                    if (Enum.IsDefined(objectType, enumString))
                    {
                        return Enum.Parse(objectType, enumString);
                    }
                    else
                    {
                        throw new JsonSerializationException($"Unknown enum value: {enumString}");
                    }
                }
                throw new JsonSerializationException("Expected string token");
            }
        }
        public static void readOrCreateRightPerms()
        {
            string h = Directory.GetCurrentDirectory();
            MessageHandler.sendErrorMsg(h);
            string filePath;
            if (claims.config.PATH_TO_DB_AND_JSON_FILES.Length == 0)
            {
                filePath = @"" + Path.Combine(GamePaths.ModConfig, claims.config.PERMS_FILE_NAME);
            }
            else
            {
                filePath = @"" + Path.Combine(claims.config.PATH_TO_DB_AND_JSON_FILES, claims.config.PERMS_FILE_NAME);
            }
            string json;
            if (File.Exists(filePath))
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    json = r.ReadToEnd();
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter());
                    PlayerPermissionsByGroups = JsonConvert.DeserializeObject<Dictionary<string, HashSet<EnumPlayerPermissions>>>(json, settings);
                }
            }
            else
            {
                Dictionary<string, HashSet<EnumPlayerPermissions>> ranksPerms = getDefaultRankPermsDict();
                using (StreamWriter r = new StreamWriter(filePath))
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter());
                    settings.Formatting = Formatting.Indented;
                    string b = JsonConvert.SerializeObject(ranksPerms, settings);
                    r.WriteLine(b);
                    PlayerPermissionsByGroups = ranksPerms;
                }
            }
        }
        public static bool hasRight(IServerPlayer player, string right)
        {
            return player.ServerData.PermaPrivileges.Contains(right) 
                || player.WorldData.CurrentGameMode == Vintagestory.API.Common.EnumGameMode.Creative;
        }
        public void initRightsDict()
        {
            readOrCreateRightPerms();
        }
        public static void clearAll()
        {
            //rightsByGroupDict.Clear();
        }
    }
}
