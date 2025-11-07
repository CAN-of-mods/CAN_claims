using System;
using Newtonsoft.Json;
using Vintagestory.API.MathTools;

namespace claims.src.auxialiry
{
    public class VecJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
            return objectType == typeof(Vec2i);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string)
            {
                return new Vec2i(1, 2);
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
