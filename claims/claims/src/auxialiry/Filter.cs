using System.Text.RegularExpressions;

namespace claims.src.auxialiry
{
    public static class Filter
    {
        public static string filterName(string inputString)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9-_]");
            return rgx.Replace(inputString, "");
        }
        public static string filterNameWithSpaces(string inputString)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9-_ ]");
            return rgx.Replace(inputString, "");
        }
        public static bool checkForBlockedNames(string inputString)
        {
            if(Settings.blockedNames.Contains(inputString))
            {
                return false;
            }
            return true;
        }
    }
}
