using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;

namespace claims.src.part.structure
{
    public class PrisonCellInfo
    {
        public HashSet<PlayerInfo> prisonedPlayers = new HashSet<PlayerInfo>();
        public Vec3i spawnPostion { get; set; }
        public HashSet<string> playerNames = new HashSet<string>();
        public PrisonCellInfo(Vec3i pos)
        {
            spawnPostion = pos;
        }
        public PrisonCellInfo()
        {

        }
        public bool AddPlayer(PlayerInfo player)
        {
            return prisonedPlayers.Add(player) && playerNames.Add(player.GetPartName());
        }
        public HashSet<PlayerInfo> getPlayerInfos()
        {
            return prisonedPlayers;
        }
        public HashSet<string> GetPlayersNames()
        {
            HashSet<string> names = new HashSet<string>();
            foreach (PlayerInfo playerInfo in prisonedPlayers)
            {
                names.Add(playerInfo.GetPartName());
            }
            return names;
        }
        public List<string> GetPlayersGuids()
        {
            List<string> names = new List<string>();
            foreach (PlayerInfo playerInfo in prisonedPlayers)
            {
                names.Add(playerInfo.Guid);
            }
            return names;
        }
        public Vec3i getSpawnPosition()
        {
            return spawnPostion;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(JsonSerializer.Serialize(new { spawnPostion.X, spawnPostion.Y, spawnPostion.Z, players = GetPlayersGuids() }));
            //sb.Append(spawnPostion.X.ToString()).Append(",").Append(spawnPostion.Y.ToString()).Append(",").Append(spawnPostion.Z.ToString());
            //sb.Append(":");
            //sb.Append(JsonSerializer.Serialize(GetPlayersGuids()));
            /*foreach(PlayerInfo playerInfo in prisonedPlayers)
            {
                sb.Append(playerInfo.Guid);
                if(!playerInfo.Equals(prisonedPlayers.Last()))
                {
                    sb.Append(',');
                }
            }*/
            return sb.ToString();
        }
        public void fromString(string input)
        {
            //coords:list of uid
            string [] splited =  input.Split(':');

            string [] spawn = splited[0].Split(',');

            /*while(true)
            {
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(input);
                    var p = ((JsonElement)dict["X"]).GetInt32();
                    spawnPostion = new Vec3i(((JsonElement)dict["X"]).GetInt32(), ((JsonElement)dict["Y"]).GetInt32(), ((JsonElement)dict["Z"]).GetInt32());
                    List<string> tmp = ((JsonElement)dict["players"]).EnumerateArray().Select(p => p.GetString()).ToList();
                    //spawnPostion = JsonSerializer.Deserialize<Vec3i>(splited[0]);
                }
                catch { }
            }*/
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(input);
            var p = ((JsonElement)dict["X"]).GetInt32();
            spawnPostion = new Vec3i(((JsonElement)dict["X"]).GetInt32(), ((JsonElement)dict["Y"]).GetInt32(), ((JsonElement)dict["Z"]).GetInt32());
            List<string> tmp = ((JsonElement)dict["players"]).EnumerateArray().Select(p => p.GetString()).ToList();
            foreach (string uid in tmp)
            {
                if (uid.Length == 0)
                    continue;
                if (claims.dataStorage.getPlayerByUid(uid, out PlayerInfo player))
                    prisonedPlayers.Add(player);
            }
            /*spawnPostion = JsonSerializer.Deserialize<Vec3i>(splited[0]);
                //new Vec3i(int.Parse(spawn[0]), int.Parse(spawn[1]), int.Parse(spawn[2]));
            if(splited[1].Length == 0)
            {
                return;
            }
            var guids = JsonSerializer.Deserialize<List<string>>(splited[1]);
            //string [] uids = splited[1].Split(',');
            foreach (string uid in guids) 
            {
                if (uid.Length == 0)
                    continue;
                if(claims.dataStorage.getPlayerByUid(uid, out PlayerInfo player))
                    prisonedPlayers.Add(player);
            }*/
        }
    }
}
