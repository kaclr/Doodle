using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NssIntegration
{
    public class ClientVersion
    {
        private const string VER_PATTERN = @"(\d+)\.(\d+)\.(\d+)\.(\d+)";

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

        public static bool IsVersionValid(string version)
        {
            return Regex.Match(version, VER_PATTERN).Success;
        }

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

        public static void ModifyVersion(string versionJsonPath, string newVersion)
        {
            var cv = New(versionJsonPath);
            cv.Modify(newVersion);
            cv.Serialize();
        }

        public void Modify(string newVersion)
        {
            var mo = Regex.Match(newVersion, VER_PATTERN);
            if (!mo.Success)
            {
                throw new ArgumentException($"'{newVersion}' is not a valid version!", nameof(newVersion));
            }

            MajorVersion = int.Parse(mo.Groups[1].Value);
            MinorVersion = int.Parse(mo.Groups[2].Value);
            FixVersion = int.Parse(mo.Groups[3].Value);
            BuildVersion = int.Parse(mo.Groups[4].Value);
        }

        public void Serialize()
        {
            using (StreamWriter f = new StreamWriter(path))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Formatting = Formatting.Indented;
                jsonSerializer.Serialize(f, this);
            }
        }

        public override string ToString()
        {
            return $"{MajorVersion}.{MinorVersion}.{FixVersion}.{BuildVersion}";
        }
    }
}
