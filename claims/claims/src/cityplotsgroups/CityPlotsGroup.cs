using System.Collections.Generic;
using claims.src.part;
using claims.src.perms;

namespace claims.src
{
    public class CityPlotsGroup: Part
    {
        public List<PlayerInfo> PlayersList {  get; set; }
        //USED ONLY CITIZEN
        public PermsHandler PermsHandler {  get; set; }
        public double PlotsGroupFee {  get; set; }
        public City City {  get; set; }
        public CityPlotsGroup(string val, string guid) : base(val, guid)
        {
            PlayersList = new List<PlayerInfo>();
            PermsHandler = new PermsHandler();
            PlotsGroupFee = 0;
        }
        public bool HasFee()
        {
            return PlotsGroupFee > 0;
        }    
        public override bool saveToDatabase(bool update = true)
        {
            return claims.getModInstance().getDatabaseHandler().saveCityPlotGroup(this, update);
        }
    }
}
