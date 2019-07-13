using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NssIntegration
{
    using Doodle;

    public class BuildConfig
    {
        private Dictionary<string, object> m_dic;

        private string m_path;

        public BuildConfig(string path)
        {
            if (!File.Exists(path)) throw new NotFileException(path, nameof(path));

            m_path = path;

            using (StreamReader f = new StreamReader(path))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                m_dic = (Dictionary<string, object>)jsonSerializer.Deserialize(f, typeof(Dictionary<string, object>));
            }
        }

        public T Get<T>(string key)
        {
            if (m_dic == null || !m_dic.TryGetValue(key, out object value))
            {
                throw new NssIntegrationException($"{nameof(BuildConfig)} '{m_path}'中没有key为'{key}'的配置！");
            }

            if (!typeof(T).IsAssignableFrom(value.GetType()))
            {
                throw new NssIntegrationException($"'{key}'的值类型为'{value.GetType()}'不能转换为类型'{typeof(T)}'！");
            }

            return (T)value;
        }
    }
}
