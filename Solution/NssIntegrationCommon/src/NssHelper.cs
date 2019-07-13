using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NssIntegration
{
    public enum NssConcept
    {
        NssUnityProj,
        UnityExe,
    }

    public static class NssHelper
    {
        private class ConceptRecord
        {
            public Type type;
            public Func<object, bool> Check;
        }

        private static readonly Dictionary<NssConcept, ConceptRecord> m_dicConceptRecord = new Dictionary<NssConcept, ConceptRecord>();

        static NssHelper()
        {
            m_dicConceptRecord.Add(NssConcept.NssUnityProj, new ConceptRecord()
            {
                type = typeof(string),
                Check = value =>
                {
                    var path = (string)value;
                    return !Directory.Exists(Path.Combine(path, "Assets")) || !Directory.Exists(Path.Combine(path, "Version"));
                }
            });

            m_dicConceptRecord.Add(NssConcept.UnityExe, new ConceptRecord()
            {
                type = typeof(string),
                Check = value =>
                {
                    var path = (string)value;
                    return Path.GetFileNameWithoutExtension(path) == "Unity";
                }
            });
        }

        public static bool CheckConcept<T>(NssConcept concept, T value)
        {
            if (!m_dicConceptRecord.TryGetValue(concept, out ConceptRecord conceptRecord))
            {
                throw new NssIntegrationException($"'{concept}'是未注册的{nameof(NssConcept)}");
            }

            if (conceptRecord.type.IsAssignableFrom(typeof(T)))
            {
                throw new NssIntegrationException($"'{concept}'类型不匹配，需要的类型'{conceptRecord.type}'，输入的类型'{typeof(T)}'！");
            }

            return conceptRecord.Check(value);
        }

        public static T CheckConceptThrow<T>(NssConcept concept, T value)
        {
            if (!m_dicConceptRecord.TryGetValue(concept, out ConceptRecord conceptRecord))
            {
                throw new NssIntegrationException($"'{concept}'是未注册的{nameof(NssConcept)}");
            }

            if (conceptRecord.type.IsAssignableFrom(typeof(T)))
            {
                throw new NssIntegrationException($"'{concept}'类型不匹配，需要的类型'{conceptRecord.type}'，输入的类型'{typeof(T)}'！");
            }

            if (!conceptRecord.Check(value))
            {
                throw new NssIntegrationException($"'{value}'不是一个'{concept}'！");
            }
            return value;
        }

        public static string GetStandardIFSName(BuildTarget buildTarget, string version)
        {
            return $"{buildTarget}_{version}.ifs";
        }

        public static string GetStandardAppName(
            BuildTarget buildTarget,
            BuildMode buildMode,
            VerLine verLine,
            string defaultTDir,
            string version,
            int svnRev)
        {
            var name = $"NSS_{buildMode}_{verLine}_{version}_{svnRev}";
            if (buildTarget == BuildTarget.Android)
            {
                return $"{name}.apk";
            }
            else if (buildTarget == BuildTarget.iOS)
            {
                return $"{name}.ipa";
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
