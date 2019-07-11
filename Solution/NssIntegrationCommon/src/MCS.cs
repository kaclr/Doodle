using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NssIntegration
{
    public class MCS
    {
        private readonly List<string> m_lstItem = new List<string>();
        private readonly Dictionary<string, int> m_dicItem = new Dictionary<string, int>();

        public MCS(string path)
        {
            Deserialize(path);
        }

        public void Deserialize(string path)
        {
            m_lstItem.Clear();
            m_dicItem.Clear();
            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrEmpty(line)) continue;

                TryAddItem(line);
            }
        }

        public void Serialize(string path)
        {
            using (var f = new StreamWriter(path))
            {
                for (int i = 0; i < m_lstItem.Count; ++i)
                {
                    if (m_dicItem.TryGetValue(m_lstItem[i], out int index) && index == i)
                    {
                        f.WriteLine(m_lstItem[i]);
                    }
                }
            }
        }

        public void ModifyItemByBuildMode(BuildMode buildMode)
        {
            TryDelItem("-define:NSS_DEBUG");
            TryDelItem("-define:NSS_PUBLISH");

            var macroStr = buildMode == BuildMode.Debug ? "-define:NSS_DEBUG" : "-define:NSS_PUBLISH";
            TryAddItem(macroStr);
        }

        public void TryAddItem(string item)
        {
            if (!m_dicItem.ContainsKey(item))
            {
                m_dicItem.Add(item, m_lstItem.Count);
                m_lstItem.Add(item);
            }
        }

        public void TryDelItem(string item)
        {
            if (m_dicItem.TryGetValue(item, out _))
            {
                // 不需要删除list中元素，只有dic中有的元素才是存在的
                m_dicItem.Remove(item);
            }
        }
    }
}
