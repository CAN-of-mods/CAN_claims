using claims.src.auxialiry;
using System;
using System.Collections.Generic;

namespace claims.src.part.structure.union
{
    public class UnionHander
    {
        static HashSet<UnionLetter> unionLettersList = new HashSet<UnionLetter>();

        public static void clearAll()
        {
            unionLettersList.Clear();
        }
        public static bool addUnionLetter(UnionLetter letter)
        {
            return unionLettersList.Add(letter);
        }
        public static bool removeUnionLetter(Alliance from, Alliance to)
        {
            foreach (var it in unionLettersList)
            {
                if ((it.From.Equals(from) && it.To.Equals(to)) ||
                    (it.From.Equals(to) && it.To.Equals(from)))
                {
                    unionLettersList.Remove(it);
                    return true;
                }
            }
            return false;
        }
        public static bool removeUnionLetter(UnionLetter conflictLetter)
        {
            if (unionLettersList.Remove(conflictLetter))
            {
                return true;
            }
            return false;
        }
        public static void updateUnionLetters()
        {
            foreach (UnionLetter letter in unionLettersList)
            {
                long now = TimeFunctions.getEpochSeconds();
                if (letter.TimeStampExpire < now)
                {
                    //TODO
                    unionLettersList.Remove(letter);
                }
            }
        }
        public static bool unionAlreadyExist(Alliance firstSide, Alliance secondSide)
        {
            if(firstSide.ComradAlliancies.Contains(secondSide))
            {
                return true;
            }
            return false;
        }
        public static List<UnionLetter> GetAllLettersForAlliance(Alliance alliance)
        {
            List<UnionLetter> sentLetters = new List<UnionLetter>();

            foreach (var it in unionLettersList)
            {
                if (it.From.Equals(alliance) || it.To.Equals(alliance))
                {
                    sentLetters.Add(it);
                }
            }
            return sentLetters;
        }
        public static List<UnionLetter> getSentLettersForAlliance(Alliance alliance)
        {
            List<UnionLetter> sentLetters = new List<UnionLetter>();

            foreach (var it in unionLettersList)
            {
                if (it.From.Equals(alliance))
                {
                    sentLetters.Add(it);
                }
            }
            return sentLetters;
        }
        public static List<UnionLetter> getReceivedLettersForAlliance(Alliance alliance)
        {
            List<UnionLetter> sentLetters = new List<UnionLetter>();

            foreach (var it in unionLettersList)
            {
                if (it.To.Equals(alliance))
                {
                    sentLetters.Add(it);
                }
            }
            return sentLetters;
        }
        public static bool TryGetUnionLetter(Alliance first, Alliance second, out UnionLetter letter)
        {
            foreach (var it in unionLettersList)
            {
                if ((it.From.Equals(first) && it.To.Equals(second)) ||
                    (it.From.Equals(second) && it.To.Equals(first)))
                {
                    letter = it;
                    return true;
                }
            }
            letter = null;
            return false;
        }
        public static bool TryGetUnionLetter(string guid, out UnionLetter letter)
        {
            foreach (var it in unionLettersList)
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
            foreach (var it in unionLettersList)
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
