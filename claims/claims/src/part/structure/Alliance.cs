using claims.src.delayed.invitations;
using claims.src.part.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent;

namespace claims.src.part.structure
{
    public class Alliance : Part, ISender, IGetStatus, ICooldown
    {
        public string MoneyAccountName => claims.config.CITY_ACCOUNT_STRING_PREFIX + Guid;
        PlayerInfo leader;
        List<Invitation> listSentInvitations = new List<Invitation>();
        public List<City> cities { get; set; } = new List<City>(); 
        public City mainCity { get; set; }
        public List<Alliance> hostiles { get; set; } = new List<Alliance>();
        public List<Alliance> comradAlliancies { get; set; } = new List<Alliance>();
        public int allianceFee = 0;
        public bool neutral { get; set; } = false;
        public Alliance(string val, string guid) : base(val, guid)
        {
        }
        public override bool saveToDatabase(bool update = true)
        {
            return claims.getModInstance().getDatabaseHandler().saveAlliance(this, update);
        }
        public List<Invitation> getSentInvitations()
        {
            throw new NotImplementedException();
        }

        public void deleteSentInvitation(Invitation invitation)
        {
            throw new NotImplementedException();
        }

        public void addSentInvitation(Invitation invitation)
        {
            throw new NotImplementedException();
        }

        public int getMaxSentInvitations()
        {
            
            throw new NotImplementedException();
        }

        public string getNameSender()
        {
            throw new NotImplementedException();
        }

        public List<string> getStatus(PlayerInfo forPlayer)
        {
            throw new NotImplementedException();
        }
    }
}
