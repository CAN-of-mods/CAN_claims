using Newtonsoft.Json;

namespace claims.src.auxialiry
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CityLevelInfo
    {
        [JsonProperty]
        public int AmountOfPlots { get; set; }
        [JsonProperty]
        public int UnconditionalPayment { get; set; }
        [JsonProperty]
        public int SummonPlots { get; set; }
        [JsonProperty]
        public int Maxextrachunksbought { get; set; }
        public CityLevelInfo(int amountOfPlots, int outGo, int summonPlots, int maxextrachunksbought)
        {
            AmountOfPlots = amountOfPlots;
            UnconditionalPayment = outGo;
            SummonPlots = summonPlots;
            Maxextrachunksbought = maxextrachunksbought;
        }
    }
}
