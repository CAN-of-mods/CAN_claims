using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace claims.src
{
    public class Config
    {
        //ECONOMY
        public double NEW_CITY_COST = 150;
        public double CITY_NAME_CHANGE_COST = 20;
        public bool DELETE_CITIZEN_FROM_CITY_IF_DOESN_PAY_FEE = true;

        public double CITY_BASE_CARE = 2;
        public double CITY_MAX_DEBT = 1000;

        public double NEUTRAL_ALLANCE_PAYMENT = 50;
        public bool ADDITIONAL_COST_OF_NO_PVP_PLOT = true;

        public double PLOT_CLAIM_PRICE = 5;
        public double MAX_CITY_FEE = 50;
        public double SUMMON_PAYMENT = 5;
        public double NEW_ALLIANCE_COST = 300;
        public double ALLIANCE_RENAME_COST = 50;
        public double ALLIANCE_MAX_FEE = 50;
        public double ALLIANCE_BASE_CARE = 50;
        //DATABASE
        public string PATH_TO_DB_AND_JSON_FILES = "";
        public string DB_NAME = "claims.db";
        public string MANUALLY_BACKUP_FILE_NAME = "backup_manually_claims.db";
        public string DAILY_BACKUP_FILE_NAME = "backup_daily_claims.db";
        public string HOURLY_BACKUP_FILE_NAME = "backup_hourly_claims.db";
        public string PERMS_FILE_NAME = "claims_permissions.json";
        //CHAT
        public bool USE_MOD_CHAT_WINDOW = true;
        public string CHAT_WINDOW_NAME = "claims";
        public string PREFIX_COLOR_PLAYER = "#00FFFF";
        public string NAME_COLOR_PLAYER = "#FFFFFF";
        public string POSTFIX_COLOR_PLAYER = "#1F920E";
        public string CITY_COLOR_NAME = "#755985";
        public string ALLIANCE_COLOR_NAME = "#218fdc";
        public int MAX_CITIZEN_TITLE_LENGTH = 16;
        public double LOCAL_CHAT_DISTANCE = 100;
        public int ALLIANCE_PREFIX_LENGTH = 3;

        //INVITATIONS
        public int MAX_SENT_INVITATIONS_CITY = 20;
        public int MAX_SENT_INVITATIONS_VILLAGE = 10;
        public bool NEED_AGREE_FOR_CONFLICT = true;
        public int DELAY_FOR_CONFLICT_ACTIVATED = 300;

        public int MAX_RECEIVED_INVITATIONS_CITY = 10;
        public int MAX_RECEIVED_INVITATIONS_PLAYER = 10;

        public int MAX_SENT_INVITATIONS_ALLIANCE = 10;
        public int HOUR_TIMEOUT_INVITATION_TO_ALLIANCE = 2;

        public bool NEED_AGREE_FOR_WAR_RANGES = true;
        public int MIN_WARRANGE_DURATION_MINUTES = 60;
        public int WARRANGE_PER_ALLIANCE = 2;
        public int MIN_RANGE_CELL_DURATION_MINUTES = 30;

        //PLOTGROUPS
        public int PLOT_GROUP_INVITATION_TIMEOUT = 2;
        public int MAX_PLOTS_GROUP_PER_CITY = 5;

        //TIME
        public int HOUR_NEW_DAY_START = 43200;
        public int HOUR_TIMEOUT_INVITATION_CITY = 2;
        public int SECONDS_SUMMON_TIME = 10;
        public int SECONDS_SUMMON_COOLDOWN = 10;
        public bool PVP_DURING_PART_OF_THE_DAY = true;
        public float PVP_TIME_START = 19;
        public float PVP_TIME_END = 6;
        public long MOD_DAY_DURATION_IN_SECONDS = 86400;
        public int SECONDS_ALLIANCE_RENAME_COOLDOWN = 10;
        public int CHECK_FOR_WAR_TO_START_EVERY_N_SECONDS = 600;
        public int CHECK_FOR_WAR_TO_START_CALLBACK_EVERY_N_SECONDS = 960;
        public int FLAG_CAPTURE_DURATION_SECONDS = 60;
        //WAR
        public int MAX_AMOUNT_OF_CAPTURE_FLAGS_ACTIVE = 2;
        public int FLAG_REINFORCEMENT_AMOUNT = 10;
        public int MINIMUM_DAYS_BETWEEN_BATTLES = 3;
        //PATCHES
        public bool FALLING_BLOCKS_TO_CITY_PLOTS_PATCH = true;
        public bool WATER_FLOW_CITY_PLOTS_PATCH = true;

        //DISTANCE
        public int MIN_DISTANCE_FROM_OTHER_CITY_NEW_CITY = 3;
        public bool CAPTURED_PLOTS_DO_NOT_BLOCK_CLAIMS = true;

        //AGREEMENT
        public int AGREEMENT_TIMEOUT_SECONDS = 120;
        public string AGREEMENT_COMMAND = "agree";

        //MOVEMENT
        public int DELTA_TIME_PLAYER_POSITION_CHECK_CLIENT = 500;
        public int DELTA_TIME_PLAYER_POSITION_CHECK = 500;
        public bool PLAYER_MOVEMENT_CANCEL_TELEPORTATION = true;

        //PRISON
        public HashSet<string> BLOCKED_COMMANDS_PRISON = new HashSet<string> { "summon" };
        public double RANSOM_FOR_NO_CITIZEN = 2;
        public double RANSOM_FOR_CITIZEN = 10;
        public double RANSOM_FOR_MAYOR = 20;
        public double RANSOM_FOR_LEADER = 30;
        public double RANSOM_FOR_CHIEF = 5;

        //DEFENCE
        public HashSet<string> PROTECTED_MOB_TYPES = new HashSet<string>{"Bighorn lamb",
            "Bighorn ewe", "Bighorn ram", "Rooster", "Chick", "Hen", "Sow", "Boar", "Piglet" };

        //PLOTS COST
        public double DEFAULT_PLOT_COST = 1;
        public double OUTPOST_PLOT_COST = 150;
        public double TOURNAMENT_PLOT_COST = 3;
        public double CAMP_PLOT_COST = 4;
        public double TEMPLE_PLOT_COST = 5;
        public double FARM_PLOT_COST = 6;
        public double SUMMON_PLOT_COST = 7;
        public double EMBASSY_PLOT_COST = 8;
        public double TAVERN_PLOT_COST = 9;
        public double PLOT_NO_PVP_FLAG_COST = 3;
        public double MAIN_CITYPLOT_COST = 3;
        public double PRISON_PLOT_COST = 3;
        public double EXTRA_PLOT_COST = 30;

     
        //STRINGS
        public int MAX_LENGTH_CITY_INV_MSG = 100;
        public int MAX_LENGTH_CITY_NAME = 40;

        //PRISON
        public int MAX_CELLS_PER_PRISON = 4;

        //SUMMON
        public int SUMMON_MIN_PLAYERS = 0;
        public int SUMMON_HOR_RANGE = 10;
        public int SUMMON_VER_RANGE = 10;
        public bool SUMMON_ALLOWED = true;

        //GENERAL
        public int [] PLOT_BORDERS_COLOR_WILD_PLOT = new int[] { 64, 255, 255, 0 };
        public int [] PLOT_BORDERS_COLOR_OUR_CITY_PLOT = new int[] { 143, 5, 146, 0 };
        public int [] PLOT_BORDERS_COLOR_OTHER_PLOT = new int[] { 16, 49, 158, 0 };
        public int PLOT_SIZE = 16;
        public int MAP_ZONE_SIZE = 512;
        public HashSet<string> BLOCKED_NAMES = new HashSet<string> { };
        public HashSet<string> CITY_PLOTS_COLOR_AVAILABLE_COLORS_GUI = new HashSet<string> { "white", "blue", "red", "orange", "black", "aqua", "yellow", "cyan", "pink", "gold", "indigo", "ivory", "lime", "green", "red", "purple", "silver",
        "violet"};
        public bool NO_ACCESS_WITH_FOR_NOT_CLAIMED_AREA = false;
        public HashSet<int> POSSIBLE_BROKEN_BLOCKS_IN_WILDERNESS = new HashSet<int>();
        public HashSet<int> POSSIBLE_BUILD_BLOCKS_IN_WILDERNESS = new HashSet<int>();
        public HashSet<int> POSSIBLE_USED_BLOCKS_IN_WILDERNESS = new HashSet<int>();
        public HashSet<int> POSSIBLE_BUILD_ITEMS_IN_WILDERNESS = new HashSet<int>();

        public HashSet<string> BLOCK_CODES_CAN_BE_BROKEN_IN_WILDERNESS = new HashSet<string> { "game:ladder-wood-*", "game:woodbucket", "game:water-*-*", "game:firepit-construct*" };
        public HashSet<string> BLOCK_CODES_CAN_BE_BUILD_IN_WILDERNESS = new HashSet<string> { "game:ladder-wood-north" };
        public HashSet<string> BLOCK_CODES_CAN_BE_USED_IN_WILDERNESS = new HashSet<string> { };
        public HashSet<string> ITEMS_CODES_CAN_BE_BUILD_IN_WILDERNESS = new HashSet<string> { "game:drygrass", "game:agedfirewood", "game:firewood" };

        public int[] PLOT_COLORS;

        public int SEND_CITY_UPDATES_EVERY_N_SECONDS = 60;

        //INNER CLAIM
        public int MAX_NUMBER_INNER_CLAIM_PER_TAVERN = 3;

        //CLAIMSEXT
        public bool NEW_CITY_ONLY_BY_ITEM = false;
        public int MAX_NUMBER_TAVERN_PER_CITY = 3;
        public bool SEND_CITY_BANKS_COORDS = true;
        public int ZONE_PLOTS_LENGTH = 32;
        public int ZONE_BLOCKS_LENGTH = 512;

        public int AREA_REGION_SIZE = 512;
        public int AREA_MAP_SIZE = 2000;


        public string SELECTED_ECONOMY_HANDLER = "VIRTUAL_MONEY";
        public string CITY_ACCOUNT_STRING_PREFIX = "#city_";
        public string ALLIANCE_ACCOUNT_STRING_PREFIX = "#alliance_";

        public OrderedDictionary<double, string> COINS_VALUES_TO_CODE = new OrderedDictionary<double, string> { };
        public OrderedDictionary<int, double> ID_TO_COINS_VALUES = new OrderedDictionary<int, double>();

        public static void LoadConfig(ICoreAPI api)
        {
            try
            {
                claims.config = api.LoadModConfig<Config>( "claims.json");
                if (claims.config != null)
                {
                    api.StoreModConfig<Config>(claims.config, "claims.json");
                    return;
                }
                else
                {
                    claims.config = new Config();
                    api.StoreModConfig<Config>(claims.config, "claims.json");
                }
            }
            catch
            {
                if (claims.config == null)
                {
                    claims.config = new Config();
                    api.StoreModConfig<Config>(claims.config, "claims.json");
                    return;
                }
            }
        }
    }

    
}
