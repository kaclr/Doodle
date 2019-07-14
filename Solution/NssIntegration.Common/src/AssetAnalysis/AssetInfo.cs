using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    public class AssetInfo
    {
        [JsonProperty]
        public string path
        {
            get;
            private set;
        }

        [JsonProperty]
        public double kbSize
        {
            get;
            set;
        }

        public int abCount
        {
            get
            {
                return dicAB.Count;
            }
        }

        public double abTotalKBSize
        {
            get
            {
                double kb = 0;
                foreach (var ab in dicAB.Values)
                {
                    kb += ab.kbSize;
                }
                return kb;
            }
        }

        /// <summary>
        /// Asset所在的所有ab
        /// </summary>
        [JsonProperty]
        private readonly Dictionary<string, ABInfo> dicAB = new Dictionary<string, ABInfo>();

        public AssetInfo()
        {
        }

        public AssetInfo(string path)
        {
            this.path = path;
        }

        public void AddAB(ABInfo ab)
        {
            dicAB.Add(ab.name, ab);
        }

        public IEnumerable<ABInfo> EnumABs()
        {
            return dicAB.Values;
        }
    }
}
