using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using claims.src.rights;

namespace claims.src.part.structure
{
    public class CustomCityRank
    {
        public string Name { get; set; }
        public HashSet<EnumPlayerPermissions> Permissions { get; set; } = new();
        public HashSet<string> CitizensNames { get; set; } = new();
        public override bool Equals(object obj)
        {
            if (obj is CustomCityRank)
            {
                return this.Name == ((CustomCityRank)obj).Name;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
