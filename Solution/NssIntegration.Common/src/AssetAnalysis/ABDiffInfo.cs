using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    public class ABDiffInfo
    {
        public enum DiffType
        {
            Add,
            Del,
            Mod,
        }

        public string name
        {
            get;
            set;
        }

        public double kbSize
        {
            get;
            set;
        }

        public List<KeyValuePair<string, DiffType>> diffAssets
        {
            get
            {
                return m_diffAssets;
            }
        }

        private readonly List<KeyValuePair<string, DiffType>> m_diffAssets = new List<KeyValuePair<string, DiffType>>();

        public void AddDiffAsset(DiffType diffType, string assetPath)
        {
            m_diffAssets.Add(new KeyValuePair<string, DiffType>(assetPath, diffType));
        }

        public IEnumerable<KeyValuePair<string, DiffType>> EnumDiffAssets()
        {
            return m_diffAssets;
        }

        public int GetModCount()
        {
            int count = 0;
            m_diffAssets.ForEach(diffAsset =>
            {
                if (diffAsset.Value == DiffType.Mod)
                {
                    ++count;
                }
            });
            return count;
        }

        public int GetAddCount()
        {
            int count = 0;
            m_diffAssets.ForEach(diffAsset =>
            {
                if (diffAsset.Value == DiffType.Add)
                {
                    ++count;
                }
            });
            return count;
        }

        public int GetDelCount()
        {
            int count = 0;
            m_diffAssets.ForEach(diffAsset =>
            {
                if (diffAsset.Value == DiffType.Del)
                {
                    ++count;
                }
            });
            return count;
        }
    }
}
