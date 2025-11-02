using claims.src.auxialiry;
using claims.src.part;
using Vintagestory.API.Common;

namespace claims.src.commands
{
    public class GeneralCommands
    {
        public static void trySetCityPlotColorsAdmin(TextCommandResult tcr, CmdArgs args)
        {
            ColorHandling.tryFindColor(args[1], out int color);

            string cityName = Filter.filterName(args[0]);

            if (cityName.Length == 0 || !Filter.checkForBlockedNames(cityName))
            {
                tcr.StatusMessage = "claims:invalid_city_name";
                return;
            }
            claims.dataStorage.getCityByName(cityName, out City city);
            if (city == null)
            {
                return;
            }
            city.trySetPlotColor(color);
            return;
        }

    }
}
