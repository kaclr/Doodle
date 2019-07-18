using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Doodle
{
    public class RepeateDo
    {
        public int maxTimes { get; set; }
        public string finalFailStr { get; set; }

        private readonly Dictionary<Type, List<string>> m_dicIgnoreException = new Dictionary<Type, List<string>>();

        public RepeateDo(int maxTimes)
        {
            this.maxTimes = maxTimes;
        }

        public void IgnoreException<T>(string regexPattern = null)
        {
            if (!m_dicIgnoreException.TryGetValue(typeof(T), out List<string> patterns))
            {
                patterns = new List<string>();
                m_dicIgnoreException.Add(typeof(T), patterns);
            }

            if (!string.IsNullOrEmpty(regexPattern))
                patterns.Add(regexPattern);
        }

        public void Do(Action action)
        {
            Do(() =>
            {
                action();
                return true;
            });
        }

        public void Do(Func<bool> action)
        {
            for (int i = 0; i < maxTimes; ++i)
            {
                try
                {
                    if (action())
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    if (m_dicIgnoreException.TryGetValue(e.GetType(), out var patterns))
                    {
                        if (patterns.Count == 0)
                        {
                            Logger.ErrorLog($"{nameof(RepeateDo)} fail {i + 1} times: {e.Message}");
                            continue;
                        }

                        foreach (var pattern in patterns)
                        {
                            if (Regex.IsMatch(e.Message, pattern))
                            {
                                Logger.ErrorLog($"{nameof(RepeateDo)} fail {i + 1} times: {e.Message}");
                                continue;
                            }
                        }
                    }

                    throw e;
                }

                return;
            }

            throw new DoodleException(finalFailStr == null ? $"{nameof(RepeateDo)} failed in the end, after {maxTimes} times!" : finalFailStr);
        }
    }
}
