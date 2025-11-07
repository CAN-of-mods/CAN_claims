using Newtonsoft.Json;

namespace claims.src.auxialiry
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AllianceLevelInfo
    {
        [JsonProperty]
        public int AdditionalAmountOfPlots { get; set; }
        [JsonProperty]
        public int MaxCampsAmount { get; set; }
        [JsonProperty]
        public int UnconditionalPayment { get; set; }
        public AllianceLevelInfo(int additionalAmountOfPlots, int maxCampsAmount, int unconditionalPayment)
        {
            AdditionalAmountOfPlots = additionalAmountOfPlots;
            MaxCampsAmount = maxCampsAmount;
            UnconditionalPayment = unconditionalPayment;
        }
    }
}
