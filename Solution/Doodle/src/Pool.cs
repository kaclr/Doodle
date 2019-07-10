using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public class Pool<TKey, TValue>
        where TValue: class, new()
    {
        [JsonProperty]
        private readonly Dictionary<TKey, TValue> m_dic;
        [JsonProperty]
        private readonly List<TValue> m_lst;

        private Func<TKey, TValue> m_onNewValue;

        public Pool()
        {
            m_dic = new Dictionary<TKey, TValue>();
            m_lst = new List<TValue>();
        }

        public Pool(IEqualityComparer<TKey> comparer)
        {
            m_dic = new Dictionary<TKey, TValue>(comparer);
        }

        public void OnNewValue(Func<TKey, TValue> onNewValue)
        {
            m_onNewValue = onNewValue;
        }

        public TValue Get(TKey key)
        {
            TValue value = null;
            if (!m_dic.TryGetValue(key, out value))
            {
                if (m_onNewValue != null)
                    value = m_onNewValue(key);
                else
                    value = new TValue();
                m_dic.Add(key, value);
                m_lst.Add(value);
            }
            return value;
        }

        public IEnumerable<TValue> EnumValues()
        {
            return m_lst;
        }

        public void SortValues(Comparison<TValue> comparison)
        {
            m_lst.Sort(comparison);
        }
    }
}
