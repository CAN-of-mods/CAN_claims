using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using claims.src.auxialiry;

namespace claims.src.part.structure.conflict
{
    public class ConflictHandler
    {
        static HashSet<ConflictLetter> conflictLettersList = new HashSet<ConflictLetter>();

        public static void clearAll()
        {
            conflictLettersList.Clear();
        }
        public static bool addConflictLetter(ConflictLetter letter)
        {
            return conflictLettersList.Add(letter);
        }
        public static bool removeConflictLetter(Alliance from, Alliance to, LetterPurpose purpose)
        {
            foreach (var it in conflictLettersList)
            {
                if (it.From.Equals(from) && it.To.Equals(to) && it.Purpose.Equals(purpose))
                {
                    conflictLettersList.Remove(it);
                    return true;
                }
            }
            return false;
        }
        public static bool removeConflictLetter(ConflictLetter conflictLetter)
        {
            if (conflictLettersList.Remove(conflictLetter))
            {
                return true;
            }
            return false;
        }
        public static void updateConflictLetters()
        {
            foreach (ConflictLetter letter in conflictLettersList)
            {
                long now = TimeFunctions.getEpochSeconds();
                if (letter.TimeStampExpire < now)
                {
                    //TODO
                    conflictLettersList.Remove(letter);
                }
            }
        }
        public static bool conflictAlreadyExist(Alliance firstSide, Alliance secondSide)
        {
            foreach (Conflict conflict in claims.dataStorage.conflicts)
            {
                if ((conflict.First.Equals(firstSide) && conflict.Second.Equals(secondSide)) ||
                    ((conflict.First.Equals(secondSide) && conflict.Second.Equals(firstSide))))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool TryGetConflictWithSides(Alliance firstSide, Alliance secondSide, out Conflict conflict)
        {
            foreach (Conflict it in claims.dataStorage.conflicts)
            {
                if (it.First.Equals(firstSide) && it.Second.Equals(secondSide) ||
                    (it.First.Equals(secondSide) && it.Second.Equals(firstSide)))
                {
                    conflict = it;
                    return true;
                }
            }
            conflict = null;
            return false;
        }
        public static List<Conflict> GetAllConflictsForAlliance(Alliance alliance)
        {
            List<Conflict> sentLetters = new List<Conflict>();

            foreach (var it in claims.dataStorage.conflicts)
            {
                if (it.First.Equals(alliance) || it.Second.Equals(alliance))
                {
                    sentLetters.Add(it);
                }
            }
            return sentLetters;
        }
        public static List<ConflictLetter> GetAllLettersForAlliance(Alliance alliance)
        {
            List<ConflictLetter> sentLetters = new List<ConflictLetter>();

            foreach (var it in conflictLettersList)
            {
                if (it.From.Equals(alliance) || it.To.Equals(alliance))
                {
                    sentLetters.Add(it);
                }
            }
            return sentLetters;
        }
        public static List<ConflictLetter> getSentLettersForAlliance(Alliance alliance)
        {
            List<ConflictLetter> sentLetters = new List<ConflictLetter>();

            foreach (var it in conflictLettersList)
            {
                if (it.From.Equals(alliance))
                {
                    sentLetters.Add(it);
                }
            }
            return sentLetters;
        }
        public static List<ConflictLetter> getReceivedLettersForAlliance(Alliance alliance)
        {
            List<ConflictLetter> sentLetters = new List<ConflictLetter>();

            foreach (var it in conflictLettersList)
            {
                if (it.To.Equals(alliance))
                {
                    sentLetters.Add(it);
                }
            }
            return sentLetters;
        }

        public static List<ConflictLetter> getReceivedLettersForAllianceWithPurpose(Alliance alliance, LetterPurpose purpose)
        {
            List<ConflictLetter> sentLetters = new List<ConflictLetter>();

            foreach (var it in conflictLettersList)
            {
                if (it.To.Equals(alliance) && it.Purpose.Equals(purpose))
                {
                    sentLetters.Add(it);
                }
            }
            return sentLetters;
        }

        public static bool TryGetConflictLetter(Alliance first, Alliance second, LetterPurpose purpose, out ConflictLetter letter)
        {
            foreach (var it in conflictLettersList)
            {
                if ((it.From.Equals(first) && it.To.Equals(second) && it.Purpose.Equals(purpose)) ||
                    (it.From.Equals(second) && it.To.Equals(first) && it.Purpose.Equals(purpose)))
                {
                    letter = it;
                    return true;
                }
            }
            letter = null;
            return false;
        }
        public static bool TryGetConflictLetter(string guid, out ConflictLetter letter)
        {
            foreach (var it in conflictLettersList)
            {
                if (it.Guid.Equals(guid))
                {
                    letter = it;
                    return true;
                }
            }
            letter = null;
            return true;
        }
        public static bool GuidIsFree(Guid guid)
        {
            foreach(var it in conflictLettersList)
            {
                if (it.Guid.Equals(guid))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
