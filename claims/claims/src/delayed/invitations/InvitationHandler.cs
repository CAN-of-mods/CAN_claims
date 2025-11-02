using System.Collections.Generic;
using claims.src.auxialiry;
using claims.src.part.interfaces;

namespace claims.src.delayed.invitations
{
    public class InvitationHandler
    {
        static List<Invitation> invites = new List<Invitation>();

        public static void findAndDeleteOverdueInvitations()
        {
            long timestampNow = TimeFunctions.getEpochSeconds();
            foreach (Invitation invitation in invites.ToArray())
            {
                if (invitation.getTimeStamp() < timestampNow)
                {
                    invitation.getReceiver().deleteReceivedInvitation(invitation);
                    invitation.getSender().deleteSentInvitation(invitation);
                    invites.Remove(invitation);
                }
            }
        }

        public List<Invitation> getAllInvitations()
        {
            return invites;
        }
        public static bool addNewInvite(Invitation invitation)
        {
            foreach (var it in invites)
            {
                if (it.getSender().Equals(invitation.getSender()) && it.getReceiver().Equals(invitation.getReceiver()))
                {
                    return false;
                }
            }
            if (invitation.getSender().GetSentInvitations().Count >= invitation.getSender().getMaxSentInvitations())
            {
                return false;
            }
            if (invitation.getReceiver().getReceivedInvitations().Count >= invitation.getReceiver().getMaxReceivedInvitations())
            {
                return false;
            }
            invites.Add(invitation);
            invitation.getReceiver().addReceivedInvitation(invitation);
            invitation.getSender().addSentInvitation(invitation);
            return true;
        }
        public static bool removeInvitationIfExists(ISender sender, IReceiver receiver)
        {
            foreach (var it in invites)
            {
                if (it.getSender().Equals(sender) && it.getReceiver().Equals(receiver))
                {
                    it.getSender().GetSentInvitations().Remove(it);
                    it.getReceiver().getReceivedInvitations().Remove(it);
                    invites.Remove(it);
                    return true;
                }
            }
            return false;
        }
        public static List<Invitation> getInvitesForReceiver(IReceiver receiver)
        {
            List<Invitation> outInvitations = new List<Invitation>();
            foreach (var it in invites)
            {
                if (it.getReceiver().Equals(receiver))
                {
                    outInvitations.Add(it);
                }
            }
            return outInvitations;
        }
        public static void deleteAllInvitationsForReceiver(IReceiver receiver)
        {
            foreach (var it in invites.ToArray())
            {
                if (it.getReceiver() == receiver)
                {
                    it.getSender().deleteSentInvitation(it);
                    invites.Remove(it);
                }
            }
        }
        public static void deleteAllInvitationsForSender(ISender sender)
        {
            foreach (var it in invites.ToArray())
            {
                if (it.getSender() == sender)
                {
                    it.getReceiver().deleteReceivedInvitation(it);
                    invites.Remove(it);
                }
            }
        }
    }
}
