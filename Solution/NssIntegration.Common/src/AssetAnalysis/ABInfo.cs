﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NssIntegration
{
    public class ABInfo
    {
        private static bool ExistsAsset(ABInfo abInfo, string assetPath, string nssUnityProj)
        {
            return abInfo.m_dicTotalAsset.TryGetValue(assetPath, out AssetInfo assetInfo);
        }

        [JsonProperty]
        public string name { get; private set; }
        [JsonProperty]
        public double kbSize { get; private set; }

        [JsonProperty]
        private readonly Dictionary<string, AssetInfo> m_dicMainAsset = new Dictionary<string, AssetInfo>();
        [JsonProperty]
        private readonly List<AssetInfo> m_lstTotalAsset = new List<AssetInfo>();
        [JsonProperty]
        private readonly Dictionary<string, AssetInfo> m_dicTotalAsset = new Dictionary<string, AssetInfo>();

        private MD5CryptoServiceProvider m_md5 = new MD5CryptoServiceProvider();

        public ABInfo() { }

        public ABInfo(string name)
        {
            this.name = name;
            this.kbSize = -1;
        }

        public ABDiffInfo GenDiff(ABInfo other, string myNssUnityProj, string otherNssUnityProj)
        {
            ABDiffInfo abDiffInfo = new ABDiffInfo()
            {
                name = name,
                kbSize = kbSize,
            };

            foreach (var myAssetInfo in m_lstTotalAsset)
            {
                var assetPath = myAssetInfo.path;
                if (!ExistsAsset(this, assetPath, myNssUnityProj))
                {
                    continue;
                }

                if (!ExistsAsset(other, assetPath, otherNssUnityProj))
                {// 自己有，别人没有，为新增
                    abDiffInfo.AddDiffAsset(ABDiffInfo.DiffType.Add, assetPath);
                }
                else
                {// 都有，检查是否修改
                    var myAssetLocalPath = Path.Combine(myNssUnityProj, assetPath);
                    var otherAssetLocalPath = Path.Combine(otherNssUnityProj, assetPath);

                    if (CheckLocalAssetChange(myAssetLocalPath, otherAssetLocalPath))
                    {
                        abDiffInfo.AddDiffAsset(ABDiffInfo.DiffType.Mod, assetPath);
                    }
                }
            }

            foreach (var otherAssetInfo in other.m_lstTotalAsset)
            {
                var assetPath = otherAssetInfo.path;

                if (!ExistsAsset(other, assetPath, otherNssUnityProj))
                {
                    continue;
                }

                if (!ExistsAsset(this, assetPath, myNssUnityProj))
                {// 别人有，自己没有，为删除
                    abDiffInfo.AddDiffAsset(ABDiffInfo.DiffType.Del, otherAssetInfo.path);
                }
            }
            return abDiffInfo;
        }

        public void SetKBSize(double kbSize)
        {
            if (this.kbSize != -1 && this.kbSize != kbSize)
            {
                throw new Exception($"AB '{name}' 的大小有多个！");
            }

            this.kbSize = kbSize;
        }

        public void AddMainAsset(AssetInfo mainAsset)
        {
            if (!m_dicTotalAsset.ContainsKey(mainAsset.path))
            {
                m_dicTotalAsset.Add(mainAsset.path, mainAsset);
                m_lstTotalAsset.Add(mainAsset);

                m_dicMainAsset.Add(mainAsset.path, mainAsset);

                mainAsset.AddAB(this);
            }
        }

        public void AddDepAsset(AssetInfo depAsset)
        {
            if (!m_dicTotalAsset.ContainsKey(depAsset.path))
            {
                m_dicTotalAsset.Add(depAsset.path, depAsset);
                m_lstTotalAsset.Add(depAsset);

                depAsset.AddAB(this);
            }
        }

        public void DeleteAsset(AssetInfo asset)
        {
            if (m_dicTotalAsset.ContainsKey(asset.path))
            {
                m_dicTotalAsset.Remove(asset.path);
                m_lstTotalAsset.RemoveAt(m_lstTotalAsset.FindIndex(assetInfo => assetInfo.path == asset.path));

                if (m_dicMainAsset.ContainsKey(asset.path))
                    m_dicMainAsset.Remove(asset.path);
            }
        }

        public IEnumerable<AssetInfo> EnumMainAssets()
        {
            return m_dicMainAsset.Values;
        }

        public IEnumerable<AssetInfo> EnumTotalAssets()
        {
            return m_lstTotalAsset;
        }

        private bool CheckLocalAssetChange(string localAssetPath1, string localAssetPath2)
        {
            return GetMD5(localAssetPath1) != GetMD5(localAssetPath2);
        }

        private string GetMD5(string path)
        {
            using (var f = new FileStream(path, FileMode.Open))
            {
                var srcMd5Bytes = m_md5.ComputeHash(f);
                StringBuilder sc = new StringBuilder();
                for (int i = 0; i < srcMd5Bytes.Length; i++)
                {
                    sc.Append(srcMd5Bytes[i].ToString("x2"));
                }
                return sc.ToString();
            }
        }

        
    }
}
