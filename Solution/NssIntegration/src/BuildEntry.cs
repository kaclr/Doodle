using Doodle;
using Doodle.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NssIntegration
{
    internal static class BuildEntry
    {
        private static void Build(
            [NssConceptConfiguration(NssConcept.NssUnityProj)] string nssUnityProj,
            [NssConceptConfiguration(NssConcept.BuildTarget)] BuildTarget buildTarget,
            [ParameterConfiguration("是否使用唯一的构建版本号", optionTemplate = "-uniqueVersion")] bool uniqueVersion,
            [NssConceptConfiguration(NssConcept.ToolsDir, optionTemplate = "-toolsDir")] string toolsDir = null,
            [NssConceptConfiguration(NssConcept.BuildConfig, optionTemplate = "-buildConfig")] string buildConfigPath = "Config/BuildConfig.json",
            [NssConceptConfiguration(NssConcept.IFS, optionTemplate = "-ifs")] string ifsPath = null,
            [NssConceptConfiguration(NssConcept.BuildMode, optionTemplate = "-buildMode")] BuildMode buildMode = BuildMode.Debug,
            [NssConceptConfiguration(NssConcept.VerLine, optionTemplate = "-verLine")] VerLine verLine = VerLine.DB,
            [NssConceptConfiguration(NssConcept.PackageType, optionTemplate = "-packageType")] PackageType packageType = PackageType.Normal,
            [NssConceptConfiguration(NssConcept.BuildOption, optionTemplate = "-buildOption")] BuildOption buildOption = BuildOption.None,
            [ParameterConfiguration("输出目录", optionTemplate = "-outputDir")] string outputDir = "Output"
            )
        {
            if (string.IsNullOrEmpty(toolsDir))
            {
                toolsDir = Path.Combine(nssUnityProj, "../Tools");
            }

            outputDir = Path.GetFullPath(outputDir);
            DirUtil.CreateEmptyDir(outputDir);

            var buildConfig = new BuildConfig(buildConfigPath);
            var unityExePath = NssConceptHelper.CheckConceptThrow<string>(NssConcept.UnityExe, buildConfig.Get<string>("UnityExe"));
            var svnRev = -1;
            try
            {
                svnRev = SvnUtil.GetSvnInfo(nssUnityProj).lastChangedRev;
            }
            catch { }

            string versionJsonPath = null;
            if (uniqueVersion)
            {
                versionJsonPath = BuildProcedure.PrepareUniqueVersionJson(buildTarget, Path.Combine(nssUnityProj, "Version"));
            }
            else
            {
                versionJsonPath = Path.Combine(nssUnityProj, "Version", ClientVersion.GetVersionFileName(buildTarget));
            }

            BuildAppResult buildAppResult = null;
            if (buildTarget == BuildTarget.Android)
            {
                var sdkRoot = buildConfig.Get<string>("AndroidSDKRoot");
                var ndkRoot = buildConfig.Get<string>("AndroidNDKRoot");
                var jdkRoot = buildConfig.Get<string>("JDKRoot");

                buildAppResult = BuildProcedure.BuildApk(unityExePath, nssUnityProj, buildMode, true, sdkRoot, ndkRoot, jdkRoot,
                    versionJsonPath, verLine, packageType, buildOption, toolsDir, outputDir, svnRev);

                //buildAppResult = new BuildAppResult()
                //{
                //    appPath = "H:\\branches\\H_trunk\\NssUnityProj\\Output\\NSS_Debug_Test_DB_1.14.0.22330_349712.apk",
                //    appVersion = "1.14.0.22330",
                //    defaultTDir = "Test",
                //};
            }
            else
            {
                throw new NotImplementedException();
            }
            
            if (File.Exists(ifsPath))
            {
                Logger.Log($"使用输入的IFS文件：{ifsPath}...");

                ifsPath = IFSUtil.ModifyIFSVersion(ifsPath, buildTarget, buildAppResult.appVersion);
            }
            else
            {
                throw new NotImplementedException();
            }

            BuildProcedure.AssemblyApk(buildAppResult.appPath, 
                Path.Combine(outputDir, NssHelper.GetStandardAppName(buildTarget, buildMode, verLine, buildAppResult.defaultTDir, buildAppResult.appVersion, svnRev)), 
                ifsPath);
        }

        private static void UnpackIFS(string ifsPath, string outDir)
        {
            IFSUtil.UnpackIFS(ifsPath, outDir);
        }

        private static void ParseABAssetRelation(
            string pathAssetInfoData1, 
            string pathBundleNodeData1,
            string pathAssetInfoData2,
            string pathBundleNodeData2,
            string outAsset2ABInfo,
            string outModAsset2ABInfoByCount,
            string outModAsset2ABInfoByKB,
            [ParameterConfiguration("是否收集cs脚本信息", optionTemplate = "-considerScript")] bool considerScript)
        {
            Logger.Log("分析ABAssetRelation1...");
            ABAssetRelation abAssetRelation1 = new ABAssetRelation();
            if (!considerScript)
            {
                abAssetRelation1.SetIgnoreAssets(".cs");
            }
            abAssetRelation1.Parse(pathAssetInfoData1, pathBundleNodeData1);

            Logger.Log("分析ABAssetRelation2...");
            ABAssetRelation abAssetRelation2 = new ABAssetRelation();
            if (!considerScript)
            {
                abAssetRelation2.SetIgnoreAssets(".cs");
            }
            abAssetRelation2.Parse(pathAssetInfoData2, pathBundleNodeData2);

            var sizeOrderAssets = abAssetRelation1.SortAssets((assetInfo1, assetInfo2) =>
            {
                return (int)(assetInfo2.abTotalKBSize - assetInfo1.abTotalKBSize);
            });

            Logger.Log($"输出outAsset2ABInfo -> '{outAsset2ABInfo}'...");
            using (StreamWriter f = new StreamWriter(outAsset2ABInfo))
            {
                sizeOrderAssets.ForEach(assetInfo => f.WriteLine($"{assetInfo.path}|{assetInfo.abTotalKBSize}|{assetInfo.abCount}"));
            }

            // 做资源diff
            List<AssetDiffInfo> assetDiffInfos = new List<AssetDiffInfo>();
            foreach (var ab1 in abAssetRelation1.EnumABs())
            {
                var ab2 = abAssetRelation2.GetAB(ab1.name);
                if (ab2 == null)
                    continue;

                var diff = ab1.GenDiff(ab2);

                foreach (var assetDiffInfo in diff.assetDiffInfos)
                {
                    if (assetDiffInfo.diffType != DiffType.Mod) continue;

                    if (assetDiffInfos.FindIndex(innerAssetDiffInfo => innerAssetDiffInfo.assetInfo.path == assetDiffInfo.assetInfo.path) < 0)
                    {// 防止重复
                        assetDiffInfos.Add(assetDiffInfo);
                    }
                }
            }

            // 数量排序
            Logger.Log($"输出outModAsset2ABInfoByCount -> '{outModAsset2ABInfoByCount}'...");
            assetDiffInfos.Sort((assetDiff1, assetDiff2) =>
            {
                return assetDiff2.assetInfo.abCount - assetDiff1.assetInfo.abCount;
            });
            using (StreamWriter f = new StreamWriter(outModAsset2ABInfoByCount))
            {
                assetDiffInfos.ForEach(assetDiffInfo => f.WriteLine($"{assetDiffInfo.assetInfo.path}|{assetDiffInfo.assetInfo.abTotalKBSize}|{assetDiffInfo.assetInfo.abCount}"));
            }

            // 大小排序
            Logger.Log($"输出outModAsset2ABInfoByKB -> '{outModAsset2ABInfoByKB}'...");
            assetDiffInfos.Sort((assetDiff1, assetDiff2) =>
            {
                return (int)(assetDiff2.assetInfo.abTotalKBSize - assetDiff1.assetInfo.abTotalKBSize);
            });
            using (StreamWriter f = new StreamWriter(outModAsset2ABInfoByKB))
            {
                assetDiffInfos.ForEach(assetDiffInfo => f.WriteLine($"{assetDiffInfo.assetInfo.path}|{assetDiffInfo.assetInfo.abTotalKBSize}|{assetDiffInfo.assetInfo.abCount}"));
            }

            //using (StreamWriter f = new StreamWriter(outfile))
            //{
            //    assetDiffInfos.ForEach(assetDiffInfo => f.WriteLine(assetDiffInfo.assetInfo.path));
            //}
        }
    }
}
