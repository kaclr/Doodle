using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Doodle;
using Doodle.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NssIntegration;

namespace Test
{
    class TestConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TestData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class TestData
    {

    }

    class Program
    {
        
        static void Main(string[] args)
        {
            Logger.verbosity = Verbosity.Verbose;

            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new TestConverter());
            var o = jsonSerializer.Deserialize(new StreamReader("D:\\Jieji\\NssUnityProj\\Version\\Version_iPhone.json"), typeof(TestData));
        }
    }
}
