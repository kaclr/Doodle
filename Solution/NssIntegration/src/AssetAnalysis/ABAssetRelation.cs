using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NssIntegration
{
    using Doodle;

    public enum DiffType
    {
        Add,
        Del,
        Mod,
    }

    public class ABAssetRelation
    {
        [JsonProperty]
        private readonly Dictionary<string, ABInfo> m_abInfos = new Dictionary<string, ABInfo>();
        [JsonProperty]
        private readonly Dictionary<string, AssetInfo> m_assetInfos = new Dictionary<string, AssetInfo>();

        private string[] ignoreAssets = new string[0];

        public void SetIgnoreAssets(params string[] ignoreAssets)
        {
            this.ignoreAssets = ignoreAssets;
        }

        public ABInfo GetAB(string abPath)
        {
            return m_abInfos.GetValueOrCreate(abPath, () => new ABInfo(abPath));
        }

        public IEnumerable<ABInfo> EnumABs()
        {
            return m_abInfos.Values;
        }

        public IEnumerable<AssetInfo> EnumAssets()
        {
            return m_assetInfos.Values;
        }

        public List<AssetInfo> SortAssets(Comparison<AssetInfo> comparison)
        {
            var lst = new List<AssetInfo>(m_assetInfos.Values);
            lst.Sort(comparison);
            return lst;
        }

        public void Parse(string pathAssetInfoData, string pathBundleNodeData, string nssUnityProjDir = null, List<string> abWhiteList = null)
        {
            if (PathUtil.GetPathState(pathAssetInfoData) != PathState.File)
                throw new ArgumentException($"pathAssetInfoData '{pathAssetInfoData}' is not exists!");
            if (PathUtil.GetPathState(pathBundleNodeData) != PathState.File)
                throw new ArgumentException($"pathBundleNodeData '{pathBundleNodeData}' is not exists!");

            // 反序列化 AssetInfoData.json
            Logger.Log($"Loading '{pathAssetInfoData}'...");
            Dictionary<string, Dictionary<string, object>> dicAssetInfoData = null;
            try
            {
                using (StreamReader f = File.OpenText(pathAssetInfoData))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    dicAssetInfoData = (Dictionary<string, Dictionary<string, object>>)jsonSerializer.Deserialize(f, typeof(Dictionary<string, Dictionary<string, object>>));
                }
            }
            catch (Exception e)
            {
                throw new NssIntegrationException($"Deserialize AssetInfoData.json failed:\n", e);
            }

            // 反序列化 BundleNodeData.json
            Logger.Log($"Loading '{pathBundleNodeData}'...");
            Dictionary<string, Dictionary<string, object>> dicBundleNodeData = null;
            try
            {
                using (StreamReader f = File.OpenText(pathBundleNodeData))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    dicBundleNodeData = (Dictionary<string, Dictionary<string, object>>)jsonSerializer.Deserialize(f, typeof(Dictionary<string, Dictionary<string, object>>));
                }
            }
            catch (Exception e)
            {
                throw new NssIntegrationException($"Deserialize BundleNodeData.json failed:\n", e);
            }

            Dictionary<string, string> mainAsset2AB = new Dictionary<string, string>();

            // 得到每个ab包中包含了哪些MainAsset
            Logger.Log($"Parsing ab's main assets...");
            foreach (var pair in dicBundleNodeData)
            {
                string abName = pair.Key;
                if (abWhiteList != null && abWhiteList.IndexOf(abName) < 0)
                {// 有白名单，且不在
                    continue;
                }

                Logger.VerboseLog(abName);

                try
                {
                    Dictionary<string, object> dicInfo = pair.Value;

                    var abInfo = GetAB(abName);
                    long bSize = (long)pair.Value["FileSize"];
                    abInfo.SetKBSize(bSize == -1 ? 0 : (double)bSize / 1024);

                    foreach (var mainAssetJToken in (JArray)dicInfo["Assets"])
                    {
                        var assetPath = mainAssetJToken.Value<string>();

                        if (Array.Find(ignoreAssets, suffix => assetPath.EndsWith(suffix)) != null)
                        {// 过滤忽略
                            continue;
                        }

                        //var dicAssetInfo = dicAssetInfoData[assetPath];
                        //if ((((int)((long)dicAssetInfo["Flag"]) >> 3) & 1) == 1)
                        //{// 被删除的
                        //    continue;
                        //}


                        if (!string.IsNullOrEmpty(nssUnityProjDir) && !File.Exists(Path.Combine(nssUnityProjDir, assetPath)))
                        {// 文件不存在
                            continue;
                        }

                        var mainAsset = m_assetInfos.GetValueOrCreate(assetPath, () => new AssetInfo(assetPath, dicAssetInfoData[assetPath]));
                        abInfo.AddMainAsset(mainAsset);

                        // 对应meta
                        var metaPath = assetPath + ".meta";
                        var mainAssetMeta = m_assetInfos.GetValueOrCreate(metaPath, () => new AssetInfo(metaPath, dicAssetInfoData[assetPath]));
                        abInfo.AddMainAsset(mainAssetMeta);

                        // 存放MainAsset路径和它所在的ab包名字的关系，方面后面索引
                        mainAsset2AB.Add(mainAsset.path, abInfo.name);


                        Logger.VerboseLog($"   {mainAssetJToken.Value<string>()}");
                    }
                }
                catch (Exception e)
                {
                    throw new NssIntegrationException($"Parsing '{abName}' failed, in BundleNodeData.json:\n", e);
                }
            }

            // 获取每个ab中MainAsset的依赖Asset，条件：不是其他ab的MainAsset，并且这个asset不在UI相关的ab包中
            Logger.Log($"Parsing ab's dep assets...");
            foreach (var abInfo in m_abInfos.Values)
            {
                Logger.VerboseLog($"{abInfo.name}");

                foreach (var mainAsset in abInfo.EnumMainAssets())
                {
                    if (Path.GetExtension(mainAsset.path) == ".meta")
                        continue;

                    var dicInfo = dicAssetInfoData[mainAsset.path];

                    foreach (var depAssetJToken in (JArray)dicInfo["DependenciesList"])
                    {
                        var depAssetPath = depAssetJToken.Value<string>();

                        if (Array.Find(ignoreAssets, suffix => depAssetPath.EndsWith(suffix)) != null)
                        {// 过滤忽略
                            continue;
                        }

                        if (!string.IsNullOrEmpty(nssUnityProjDir) && !File.Exists(Path.Combine(nssUnityProjDir, depAssetPath)))
                        {// 文件不存在
                            continue;
                        }

                        try
                        {
                            //Logger.Log($"       {depAssetPath}", false);
                            if (!mainAsset2AB.TryGetValue(depAssetPath, out string depAssetABName) 
                                || depAssetABName == abInfo.name 
                                || depAssetABName.StartsWith("UI") 
                                || depAssetABName.StartsWith("Atlas")
                                || depAssetABName.StartsWith("Font"))
                            {// 此依赖Asset不是一个MainAsset，或它是自己ab包中的MainAsset，或者是ui相关ab（ui相关资源冗余在了很多ab包中）

                                abInfo.AddDepAsset(m_assetInfos.GetValueOrCreate(depAssetPath, () => new AssetInfo(depAssetPath, dicAssetInfoData[depAssetPath])));
                                abInfo.AddDepAsset(m_assetInfos.GetValueOrCreate(depAssetPath, () => new AssetInfo($"{depAssetPath}.meta", dicAssetInfoData[$"{depAssetPath}.meta"])));
                            }

                        }
                        catch (Exception e)
                        {
                            throw new NssIntegrationException($"Parsing ab '{abInfo.name}' failed, mainAsset: {mainAsset.path}, depAssetPath: {depAssetPath}\n", e);
                        }
                    }
                }
            }
        }

    }
}
