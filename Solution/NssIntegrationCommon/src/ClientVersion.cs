using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NssIntegration
{
    public class ClientVersion
    {
        [JsonProperty]
        public string VerType { get; set; }
        [JsonProperty]
        public int MajorVersion { get; set; }
        [JsonProperty]
        public int MinorVersion { get; set; }
        [JsonProperty]
        public int FixVersion { get; set; }
        [JsonProperty]
        public int BuildVersion { get; set; }
        [JsonProperty]
        public string VerLine { get; set; }

        [JsonIgnore]
        public string path { get; private set; }

        public static string GetVersionFileName(BuildTarget buildTarget)
        {
            var platName = buildTarget.ToString();
            if (buildTarget == BuildTarget.iOS)
            {
                platName = "iPhone";
            }
            return $"Version_{platName}.json";
        }

        public static ClientVersion New(string versionJsonPath)
        {
            using (StreamReader f = new StreamReader(versionJsonPath))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                var version = (ClientVersion)jsonSerializer.Deserialize(f, typeof(ClientVersion));
                version.path = versionJsonPath;
                return version;
            }
        }

        public static ClientVersion New(string versionDir, BuildTarget buildTarget)
        {
            return New(Path.Combine(versionDir, GetVersionFileName(buildTarget)));
        }

        public void Serialize()
        {
            using (StreamWriter f = new StreamWriter(path))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(f, this);
            }
        }

        public override string ToString()
        {
            return $"{MajorVersion}.{MinorVersion}.{FixVersion}.{BuildVersion}";
        }
    }
}
