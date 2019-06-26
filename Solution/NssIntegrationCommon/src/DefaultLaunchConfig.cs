using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NssIntegrationCommon
{
    public class DefaultLaunchConfig
    {
        private Dictionary<string, string> m_dic;

        public string this[string key]
        {
            get
            {
                return m_dic[key];
            }
            set
            {
                m_dic[key] = value;
            }
        }

        public DefaultLaunchConfig() { }

        public DefaultLaunchConfig(string path)
        {
            Deserialize(path);
        }

        public void Deserialize(string path)
        {
            if (!File.Exists(path)) throw new ArgumentException($"路径'{path}'下不存在文件！", nameof(path));

            m_dic = new Dictionary<string, string>();

            string pattern = "^(.*?)=(.*?)$";
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var m = Regex.Match(line, pattern);
                if (!m.Success)
                {
                    throw new Exception($"'{path}'是一个非法的DefaultLaunchConfig！");
                }

                m_dic.Add(m.Groups[1].Value, m.Groups[2].Value);
            }
        }

        public void Serialize(string path)
        {
            var lines = new List<string>();
            foreach (var pair in m_dic)
            {
                lines.Add($"{pair.Key}={pair.Value}");
            }

            File.WriteAllLines(path, lines.ToArray());
        }
    }
}
