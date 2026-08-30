using System;
using Newtonsoft.Json;

namespace Chimera.Library.Components.Services
{
	internal class NullableBooleanConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(bool).IsAssignableFrom(objectType) || typeof(bool).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			object obj;
			if (reader.TokenType == JsonToken.Null)
			{
				obj = false;
			}
			else
			{
				var flag = false;
				bool.TryParse(reader.Value.ToString(), out flag);
				obj = flag;
			}
			return obj;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteValue(false);
			}
			else
			{
				writer.WriteValue(value);
			}
		}
	}
}
