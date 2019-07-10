using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NssIntegration
{
    using Doodle;

    static class AssetAnalysisUtils
    {
        public static Dictionary<string, double> ParseAssetSize(string pathBuildABLog)
        {
            if (PathUtil.GetPathState(pathBuildABLog) != PathState.File)
                throw new ArgumentException($"{nameof(pathBuildABLog)} '{pathBuildABLog}' is not a file!");

            var dicAssetTotalSize = new Dictionary<string, double>();
            var pattern = "^ (.*?) kb\\s.*?%\\s(Assets.*)$";

            using (StreamReader f = File.OpenText(pathBuildABLog))
            {
                while (true)
                {
                    var line = f.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    var match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        var kb = double.Parse(match.Groups[1].Value);
                        var assetPath = match.Groups[2].Value;

                        if (dicAssetTotalSize.TryGetValue(assetPath, out double curKB))
                        {
                            // 取最大值
                            if (kb > curKB)
                                dicAssetTotalSize[assetPath] = kb;
                        }
                        else
                        {
                            dicAssetTotalSize.Add(assetPath, kb);
                        }
                    }
                }
            }

            return dicAssetTotalSize;
        }
    }
}
