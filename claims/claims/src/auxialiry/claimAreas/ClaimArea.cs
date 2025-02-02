using claims.src.auxialiry.innerclaims;
using claims.src.network.packets;
using claims.src.perms;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;

namespace claims.src.auxialiry.claimAreas
{
    [ProtoContract]
    public class ClaimArea
    {
        /*
         { 0, "use"},
         { 1, "build" },
         { 2, "attack" }};
          3  destroy*/
        [ProtoMember(1)]
        public Vec3i pos1;
        [ProtoMember(2)]
        public Vec3i pos2;
        [ProtoMember(3)]
        public bool[] permissionsFlags;
        [ProtoMember(4)]
        public bool[] settingFlags;
        [ProtoMember(5)]
        public string areaName = "";
        public int MinX => Math.Min(pos1.X, pos2.X);

        public int MinY => Math.Min(pos1.Y, pos2.Y);

        public int MinZ => Math.Min(pos1.Z, pos2.Z);

        public int MaxX => Math.Max(pos1.X, pos2.X);

        public int MaxY => Math.Max(pos1.Y, pos2.Y);

        public int MaxZ => Math.Max(pos1.Z, pos2.Z);
        public bool Intersects(ClaimArea with)
        {
            if (with.MaxX < MinX || with.MinX > MaxX)
            {
                return false;
            }

            if (with.MaxY < MinY || with.MinY > MaxY)
            {
                return false;
            }

            if (with.MaxZ >= MinZ)
            {
                return with.MinZ <= MaxZ;
            }

            return false;
        }
        public bool Contains(Vec3d pos)
        {
            if (pos.X >= (double)MinX && pos.X < (double)MaxX && pos.Y >= (double)MinY && pos.Y < (double)MaxY && pos.Z >= (double)MinZ)
            {
                return pos.Z < (double)MaxZ;
            }

            return false;
        }

        public bool Contains(int x, int y, int z)
        {
            if (x >= MinX && x < MaxX && y >= MinY && y < MaxY && z >= MinZ)
            {
                return z < MaxZ;
            }

            return false;
        }

        public bool Contains(BlockPos pos)
        {
            if (pos.X >= MinX && pos.X <= MaxX && pos.Y >= MinY && pos.Y <= MaxY && pos.Z >= MinZ)
            {
                return pos.Z < MaxZ;
            }

            return false;
        }

        public ClaimArea()
        {

        }
        public ClaimArea(string name, Vec3i pos1, Vec3i pos2, bool[] permissionsFlags, bool[] settingFlags)
        {
            this.areaName = name;
            this.pos1 = pos1;
            this.pos2 = pos2;

            this.permissionsFlags[0] = permissionsFlags[0];
            this.permissionsFlags[1] = permissionsFlags[1];
            this.permissionsFlags[2] = permissionsFlags[2];
            this.permissionsFlags[3] = permissionsFlags[3];

            this.settingFlags[0] = settingFlags[0];
            this.settingFlags[1] = settingFlags[1];
            this.settingFlags[2] = settingFlags[2];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(pos1.X).Append(",").Append(pos1.Y).Append(",").Append(pos1.Z).Append(":").Append(pos2.X).Append(",").Append(pos2.Y).Append(",").Append(pos2.Z).Append(":").Append(permissionsFlags[0] ? "1" : "0").Append(",").
                Append(permissionsFlags[1] ? "1" : "0").Append(",").Append(permissionsFlags[2] ? "1" : "0").Append(":");
            return sb.ToString();
        }
        public string GetAreaInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("pvp: ").Append(settingFlags[(int)EnumCANClaimSettingFlag.PVP] ? "off" : "on").Append("| ");
            stringBuilder.Append("fire: ").Append(settingFlags[(int)EnumCANClaimSettingFlag.FIRE] ? "off" : "on").Append("| ");
            stringBuilder.Append("blast: ").Append(settingFlags[(int)EnumCANClaimSettingFlag.BLAST] ? "off" : "on").Append("\n");
            stringBuilder.Append("use: ").Append(permissionsFlags[(int)EnumCANPlotAccessFlags.Use] ? "on" : "off").Append("\n");
            stringBuilder.Append("build: ").Append(permissionsFlags[(int)EnumCANPlotAccessFlags.Build] ? "on" : "off").Append("\n");
            stringBuilder.Append("break: ").Append(permissionsFlags[(int)EnumCANPlotAccessFlags.Break] ? "on" : "off").Append("\n");
            stringBuilder.Append("attack animals: ").Append(permissionsFlags[(int)EnumCANPlotAccessFlags.Attack] ? "on" : "off").Append("\n");
            stringBuilder.Append("\n");
            return stringBuilder.ToString();
        }

    }
}
