using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NssIntegration
{
    public static class NssConceptHelper
    {
        private static readonly Dictionary<NssConcept, NssConceptRecord> m_dicConceptRecord = new Dictionary<NssConcept, NssConceptRecord>();

        static NssConceptHelper()
        {
            m_dicConceptRecord.Add(NssConcept.NssUnityProj, new NssConceptRecord<string>()
            {
                info = "客户端Unity工程的根目录的路径",
                checker = value =>
                {
                    if (!Directory.Exists(Path.Combine(value, "Assets")) || !Directory.Exists(Path.Combine(value, "Version")))
                    {
                        return $"路径'{value}'不是合法的客户端Unity工程根目录！";
                    }
                    return null;
                }
            });

            m_dicConceptRecord.Add(NssConcept.UnityExe, new NssConceptRecord<string>()
            {
                info = "Unity引擎的可执行文件",
                checker = value =>
                {
                    if (Path.GetFileNameWithoutExtension(value) != "Unity")
                    {
                        return $"路径'{value}'不是一个合法的Unity引擎可执行文件的路径！";
                    }
                    return null;
                }
            });

            m_dicConceptRecord.Add(NssConcept.ToolsDir, new NssConceptRecord<string>()
            {
                info = "客户端工程中的Tools目录的路径",
                checker = value =>
                {
                    if (!Directory.Exists(Path.Combine(value, "BuildTools")))
                    {
                        return $"路径'{value}'不是一个合法的客户端工程中的Tools目录！";
                    }
                    return null;
                }
            });

            m_dicConceptRecord.Add(NssConcept.BuildTarget, new NssConceptRecord<BuildTarget>()
            {
                info = "构建平台",
            });

            m_dicConceptRecord.Add(NssConcept.BuildConfig, new NssConceptRecord<string>()
            {
                info = "构建配置json文件的路径",
            });

            m_dicConceptRecord.Add(NssConcept.IFS, new NssConceptRecord<string>()
            {
                info = "IFS文件的路径",
            });

            m_dicConceptRecord.Add(NssConcept.BuildMode, new NssConceptRecord<BuildMode>()
            {
                info = "构建模式",
            });

            m_dicConceptRecord.Add(NssConcept.VerLine, new NssConceptRecord<VerLine>()
            {
                info = "版本线",
            });

            m_dicConceptRecord.Add(NssConcept.PackageType, new NssConceptRecord<PackageType>()
            {
                info = "包类型",
            });

            m_dicConceptRecord.Add(NssConcept.BuildOption, new NssConceptRecord<BuildOption>()
            {
                info = "构建额外选项",
            });
        }

        public static string GetConceptInfo(NssConcept nssConcept)
        {
            if (!m_dicConceptRecord.TryGetValue(nssConcept, out NssConceptRecord nssConceptRecord))
            {
                throw new NssIntegrationException($"'{nssConcept}'是未注册的{nameof(NssConcept)}");
            }
            return nssConceptRecord.info;
        }

        public static string CheckConcept(NssConcept nssConcept, object value)
        {
            if (value == null) return null;

            if (!m_dicConceptRecord.TryGetValue(nssConcept, out NssConceptRecord nssConceptRecord))
            {
                throw new NssIntegrationException($"'{nssConcept}'是未注册的{nameof(NssConcept)}");
            }

            if (!nssConceptRecord.type.IsAssignableFrom(value.GetType()))
            {
                throw new NssIntegrationException($"'{nssConcept}'类型不匹配，需要的类型'{nssConceptRecord.type}'，输入的类型'{value.GetType()}'！");
            }

            var checkerField = nssConceptRecord.GetType().GetField("checker", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var checker = checkerField.GetValue(nssConceptRecord);
            var ret = ((Delegate)checker).DynamicInvoke(value);

            return (string)ret;
        }

        public static string CheckConcept<T>(NssConcept nssConcept, T value)
        {
            if (!m_dicConceptRecord.TryGetValue(nssConcept, out NssConceptRecord nssConceptRecord))
            {
                throw new NssIntegrationException($"'{nssConcept}'是未注册的{nameof(NssConcept)}");
            }

            if (!nssConceptRecord.type.IsAssignableFrom(typeof(T)))
            {
                throw new NssIntegrationException($"'{nssConcept}'类型不匹配，需要的类型'{nssConceptRecord.type}'，输入的类型'{typeof(T)}'！");
            }

            return ((NssConceptRecord<T>)nssConceptRecord).checker(value);
        }

        public static T CheckConceptThrow<T>(NssConcept nssConcept, T value)
        {
            if (!m_dicConceptRecord.TryGetValue(nssConcept, out NssConceptRecord nssConceptRecord))
            {
                throw new NssIntegrationException($"'{nssConcept}'是未注册的{nameof(NssConcept)}");
            }

            if (!nssConceptRecord.type.IsAssignableFrom(typeof(T)))
            {
                throw new NssIntegrationException($"'{nssConcept}'类型不匹配，需要的类型'{nssConceptRecord.type}'，输入的类型'{typeof(T)}'！");
            }

            var error = ((NssConceptRecord<T>)nssConceptRecord).checker(value);
            if (error != null)
            {
                throw new NssIntegrationException(error);
            }
            return value;
        }
    }
}
