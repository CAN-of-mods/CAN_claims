using claims.src.network.packets;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.Server;

namespace claims.src.auxialiry.claimAreas
{
    public class ClaimAreaHandler
    {
        HashSet<ClaimArea> AreaClaims;
        Dictionary<long, HashSet<ClaimArea>> AreaClaimsPerZone;
        Dictionary<string, TemporaryArea> PlayerCreatingArea;
        bool sideServer = false;
        public ClaimAreaHandler(bool sideServer = false)
        {
            AreaClaims = new HashSet<ClaimArea>();
            AreaClaimsPerZone = new Dictionary<long, HashSet<ClaimArea>>();
            PlayerCreatingArea = new Dictionary<string, TemporaryArea>();
            this.sideServer = sideServer;
        }
        public bool AddAreaClaim(ClaimArea newAreaClaim)
        {
            int indexX_1 = newAreaClaim.pos1.X / claims.config.AREA_REGION_SIZE;
            int indexX_2 = newAreaClaim.pos2.X / claims.config.AREA_REGION_SIZE;          
            if (indexX_1 > indexX_2) 
            {
                int tmp = indexX_1;
                indexX_1 = indexX_2;
                indexX_2 = tmp;
            }

            int indexZ_1 = newAreaClaim.pos1.Z/ claims.config.AREA_REGION_SIZE;
            int indexZ_2 = newAreaClaim.pos2.Z / claims.config.AREA_REGION_SIZE;
            if (indexZ_1 > indexZ_2)
            {
                int tmp = indexZ_1;
                indexZ_1 = indexZ_2;
                indexZ_2 = tmp;
            }
            bool intersectsWithExisting = false;
            List<long> regionsList = new List<long>();
            for (int i = indexX_1; i <= indexX_2; i++)
            {
                for(int j = indexZ_1; j <= indexZ_2; j++)
                {
                    long index2d = MapRegionIndex2D(i, j);
                    if(AreaClaimsPerZone.ContainsKey(index2d))
                    {
                        foreach (var olderAreaClaims in AreaClaimsPerZone[index2d])
                        {
                            if(olderAreaClaims.Intersects(newAreaClaim))
                            {
                                intersectsWithExisting = true;
                                break;
                            }
                        }
                    }
                    regionsList.Add(index2d);
                }
            }
            if(intersectsWithExisting)
            {
                return false;
            }
            AreaClaims.Add(newAreaClaim);
            foreach (var it in regionsList) 
            {
                if(AreaClaimsPerZone.ContainsKey(it))
                {
                    AreaClaimsPerZone[it].Add(newAreaClaim);
                }
                else
                {
                    AreaClaimsPerZone.Add(it, new HashSet<ClaimArea> { newAreaClaim });
                }
            }
            if (this.sideServer)
            {
                foreach (var player in claims.sapi.World.AllOnlinePlayers)
                {
                    claims.serverChannel.SendPacket(new ClaimAreasPacket()
                    {
                        type = ClaimAreasPacketEnum.Add,
                        claims = new List<ClaimArea> { newAreaClaim }

                    }, (IServerPlayer)player);
                }
            }
            SaveIntoMap();
            return true;
        }
        public bool RemoveAreaClaim(ClaimArea oldAreaClaim)
        {
            int indexX_1 = oldAreaClaim.pos1.X / claims.config.AREA_REGION_SIZE;
            int indexX_2 = oldAreaClaim.pos2.X / claims.config.AREA_REGION_SIZE;
            if (indexX_1 > indexX_2)
            {
                int tmp = indexX_1;
                indexX_1 = indexX_2;
                indexX_2 = tmp;
            }

            int indexZ_1 = oldAreaClaim.pos1.Z / claims.config.AREA_REGION_SIZE;
            int indexZ_2 = oldAreaClaim.pos2.Z / claims.config.AREA_REGION_SIZE;
            if (indexZ_1 > indexZ_2)
            {
                int tmp = indexZ_1;
                indexZ_1 = indexZ_2;
                indexZ_2 = tmp;
            }
            List<long> regionsList = new List<long>();
            for (int i = indexX_1; i <= indexX_2; i++)
            {
                for (int j = indexZ_1; j <= indexZ_2; j++)
                {
                    long index2d = MapRegionIndex2D(i, j);
                    if (AreaClaimsPerZone.ContainsKey(index2d))
                    {
                        AreaClaimsPerZone[index2d].Remove(oldAreaClaim);
                    }
                }
            }
            AreaClaims.Remove(oldAreaClaim);
            if (this.sideServer)
            {
                foreach (var player in claims.sapi.World.AllOnlinePlayers)
                {
                    claims.serverChannel.SendPacket(new ClaimAreasPacket()
                    {
                        type = ClaimAreasPacketEnum.Remove,
                        claims = new List<ClaimArea> { oldAreaClaim }

                    }, (IServerPlayer)player);
                }
            }
            SaveIntoMap();
            return true;
        }
        public EnumAreaClaimsWorldAccessResponse CheckAccess(IPlayer player, BlockSelection blockSel, EnumCANBlockAccessFlags accessType)
        {
            long regionNumber = MapRegionIndex2D((int)player.Entity.Pos.X / claims.config.AREA_REGION_SIZE, (int)player.Entity.Pos.Z / claims.config.AREA_REGION_SIZE);
            if (AreaClaimsPerZone.ContainsKey(regionNumber))
            {
                foreach (ClaimArea area in this.AreaClaimsPerZone[regionNumber])
                {
                    if (area.Contains(blockSel.Position))
                    {
                        if(area.permissionsFlags[(int)accessType])
                        {
                            return EnumAreaClaimsWorldAccessResponse.GrantedByFlag;
                        }
                        else
                        {
                            return EnumAreaClaimsWorldAccessResponse.DeniedByFlag;
                        }
                    }
                }
            }
            return EnumAreaClaimsWorldAccessResponse.AreaNotFound;
        }
        public bool CheckSettingFlag(IPlayer player, BlockSelection blockSel, EnumCANClaimSettingFlag settingFlag)
        {
            long regionNumber = MapRegionIndex2D((int)player.Entity.Pos.X / claims.config.AREA_REGION_SIZE, (int)player.Entity.Pos.Z / claims.config.AREA_REGION_SIZE);
            if (AreaClaimsPerZone.ContainsKey(regionNumber))
            {
                foreach (ClaimArea area in this.AreaClaimsPerZone[regionNumber])
                {
                    if (area.Contains(blockSel.Position))
                    {
                        return area.settingFlags[(int)settingFlag];
                    }
                }
            }
            return false;
        }
        private long MapRegionIndex2D(int regionX, int regionZ)
        {
            return (long)regionZ * (long)claims.config.AREA_MAP_SIZE + regionX;
        }
        public class TemporaryArea
        {
            public Vec3i start;
            public Vec3i end;
            public TemporaryArea() { }
        }
        public bool TryGetCurrentArea(string playerUID, out TemporaryArea temporaryArea)
        {
            if(PlayerCreatingArea.TryGetValue(playerUID, out temporaryArea))
            {
                return true;
            }
            return false;
        }
        public void CreateNewItemCurrentArea(string playerUID)
        {
            PlayerCreatingArea.Add(playerUID, new TemporaryArea());
        }
        public bool SetPos(string playerUID, Vec3i coord, bool start = true)
        {
            if(TryGetCurrentArea(playerUID, out var temporaryArea))
            {
                if (start)
                {
                    temporaryArea.start = coord;
                }
                else
                {
                    temporaryArea.end = coord;
                }
                return true;
            }
            return false;
        }
        public bool SaveArea(string playerUID, string newAreaName)
        {
            if (!TryGetCurrentArea(playerUID, out var temporaryArea))
            {
                return false;              
            }
            if(temporaryArea.start == null || temporaryArea.end == null)
            {
                return false;
            }
            ClaimArea claimArea = new ClaimArea(newAreaName, temporaryArea.start, temporaryArea.end, new bool[] { false, false, false, false}, new bool[] { false, false, false });
            return AddAreaClaim(claimArea);
        }
        public bool TryGetArea(Vec3d coords, out ClaimArea claimArea)
        {
            long regionNumber = MapRegionIndex2D((int)coords.X / claims.config.AREA_REGION_SIZE, (int)coords.Z / claims.config.AREA_REGION_SIZE);
            if (AreaClaimsPerZone.ContainsKey(regionNumber))
            {
                foreach (ClaimArea area in this.AreaClaimsPerZone[regionNumber])
                {
                    if (area.Contains(coords))
                    {
                        claimArea = area;
                        return true;
                    }
                }
            }
            claimArea = null;
            return false;
        }
        public bool RemoveAreaHere(Vec3d coords)
        {
            if(TryGetArea(coords, out ClaimArea claimArea))
            {
                return RemoveAreaClaim(claimArea);
            }
            return false;
        }
        public List<ClaimArea> GetPageAreas(int start, int end)
        {
            List<ClaimArea> claims = new List<ClaimArea>();
            int counter = 0;
            foreach (ClaimArea area in this.AreaClaims.ToArray())
            {
                if (counter >= start && counter < end)
                {
                    claims.Add(area);
                }
                counter++;
            }
            return claims;
        }
        public bool GetAreaByNumber(int number, out ClaimArea claimArea)
        {
            if(this.AreaClaims.ToArray().Count() >= number)
            {
                claimArea = this.AreaClaims.ToArray()[number];
                return true;
            }
            claimArea = null;
            return false;
        }
        public bool GetAreaByName(string name, out ClaimArea claimArea)
        {
            foreach(var it in AreaClaims)
            {
                if(it.areaName.Equals(name))
                {
                    claimArea = it;
                    return true;
                }
            }
            claimArea = null;
            return false;
        }
        public string SerializedData()
        {
            return JsonConvert.SerializeObject(AreaClaims);
        }
        public bool DeserializeData(string data)
        {
            var areas = JsonConvert.DeserializeObject<HashSet<ClaimArea>>(data);
            foreach (var area in areas)
            {
                this.AddAreaClaim(area);
            }
            return true;
        }
        public bool SaveIntoMap()
        {
            claims.sapi.WorldManager.SaveGame.StoreData<string>("claimareas", SerializedData());          
            return true;
        }
        public void InitAreasHashSet(List<ClaimArea> claimAreas)
        {
            foreach(var it in  claimAreas)
            {
                AddAreaClaim(it);
            }
        }
        public void UpdateClaimArea(ClaimArea claimArea)
        {

            if(GetAreaByName(claimArea.areaName, out var oldClaimArea))
            {
                for (int i = 0; i < claimArea.settingFlags.Length; i++)
                {
                    oldClaimArea.settingFlags[i] = claimArea.settingFlags[i];
                }
                for (int i = 0; i < claimArea.permissionsFlags.Length; i++)
                {
                    oldClaimArea.permissionsFlags[i] = claimArea.permissionsFlags[i];
                }
            }
        }
        public HashSet<ClaimArea> GetAllClaimAreas()
        {
            return AreaClaims;
        }
    }
}
