
using claims.src.part;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace claims.src.cityplotsgroups
{
    public class CityPlotsGroupInvitation
    {
        public City Sender {  get; set; }
        public PlayerInfo Receiver {  get; set; }
        public long TimeStampFinished {  get; set; }
        Thread onAccept;
        Thread onReject;
        public string GroupName {  get; set; }
        public CityPlotsGroupInvitation(City sender, PlayerInfo receive, long timeStampFinished, Thread onAccept, Thread onReject, string groupName)
        {
            this.Sender = sender;
            this.Receiver = receive;
            this.TimeStampFinished = timeStampFinished;
            this.onAccept = onAccept;
            this.onReject = onReject;
            this.GroupName = groupName;
        }
        public void reject()
        {
            CityPlotsGroupInvitationsHandler.RemoveInvitation(this);
            this.onReject.Start();
        }
        public void accept()
        {
            CityPlotsGroupInvitationsHandler.RemoveInvitation(this);
            Task.Run(() => this.onAccept.Start());
        }      
        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (obj is not CityPlotsGroupInvitation)
                return false;

            return this.Receiver == ((CityPlotsGroupInvitation)obj).Receiver && this.Sender == ((CityPlotsGroupInvitation)obj).Sender;
        }
        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + Receiver.GetHashCode();
            hash = (hash * 7) + Sender.GetHashCode();
            return hash;
        }
    }
}
