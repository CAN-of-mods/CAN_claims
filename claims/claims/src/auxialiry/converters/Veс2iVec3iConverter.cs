using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Vintagestory.API.MathTools;

namespace claims.src.auxialiry.converters
{
    public class Veс2iVec3iConverter : JsonConverter<Dictionary<Vec2i, Vec3i>>
    {
        public override void WriteJson(JsonWriter writer, Dictionary<Vec2i, Vec3i> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key.ToString()); // ключ как строка
                serializer.Serialize(writer, kvp.Value);
            }
            writer.WriteEndObject();
        }

        public override Dictionary<Vec2i, Vec3i> ReadJson(JsonReader reader, Type objectType, Dictionary<Vec2i, Vec3i> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new Dictionary<Vec2i, Vec3i>();

            var jo = JObject.Load(reader);
            foreach (var property in jo.Properties())
            {
                Vec2i key = new Vec2i();
                int.TryParse(property.Name.Split('/')[0], out int x);
                int.TryParse(property.Name.Split('/')[1], out int z);
                key.X = x;
                key.Y = z;
                Vec3i value = property.Value.ToObject<Vec3i>(serializer);
                result[key] = value;
            }

            return result;
        }
    }
}
