using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Doodle
{
#if DOODLE_NET35
    public class ObjDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
#else
    public class ObjDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, ICollection, IDictionary, IDeserializationCallback, ISerializable
#endif
        where TValue: class
    {
        public TValue this[TKey key] { get => m_dic[key]; set => m_dic[key] = value; }
        public object this[object key] { get => ((IDictionary)m_dic)[key]; set => ((IDictionary)m_dic)[key] = value; }

        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)m_dic).Keys;

        public ICollection<TValue> Values => m_lst;

        public int Count => m_dic.Count;

        public bool IsReadOnly => ((IDictionary<TKey, TValue>)m_dic).IsReadOnly;

        public bool IsFixedSize => ((IDictionary)m_dic).IsFixedSize;

        public object SyncRoot => ((IDictionary)m_dic).SyncRoot;

        public bool IsSynchronized => ((IDictionary)m_dic).IsSynchronized;

        ICollection IDictionary.Keys => ((IDictionary)m_dic).Keys;

        ICollection IDictionary.Values => m_lst;

#if !DOODLE_NET35
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => ((IReadOnlyDictionary<TKey, TValue>)m_dic).Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => ((IReadOnlyDictionary<TKey, TValue>)m_dic).Values;
#endif
        [Newtonsoft.Json.JsonProperty]
        private readonly Dictionary<TKey, TValue> m_dic = new Dictionary<TKey, TValue>();
        [Newtonsoft.Json.JsonProperty]
        private readonly List<TValue> m_lst = new List<TValue>();

        public void SortValue(Comparison<TValue> comparison)
        {
            m_lst.Sort(comparison);
        }

        public void Add(TKey key, TValue value)
        {
            m_dic.Add(key, value);
            m_lst.Add(value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((IDictionary<TKey, TValue>)m_dic).Add(item);
            m_lst.Add(item.Value);
        }

        public void Add(object key, object value)
        {
            ((IDictionary)m_dic).Add(key, value);
            ((IList)m_lst).Add(value);
        }

        public void Clear()
        {
            m_dic.Clear();
            m_lst.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)m_dic).Contains(item);
        }

        public bool Contains(object key)
        {
            return ((IDictionary)m_dic).Contains(key);
        }

        public bool ContainsKey(TKey key)
        {
            return m_dic.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)m_dic).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            ((IDictionary)m_dic).CopyTo(array, index);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IDictionary<TKey, TValue>)m_dic).GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            if (m_dic.TryGetValue(key, out var value))
            {
                m_dic.Remove(key);
                m_lst.Remove(value);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (m_dic.TryGetValue(item.Key, out var value))
            {
                m_dic.Remove(item.Key);
                m_lst.Remove(value);
                return true;
            }
            return false;
        }

        public void Remove(object key)
        {
            if (((IDictionary)m_dic).Contains(key))
            {
                ((IDictionary)m_dic).Remove(((IDictionary)m_dic)[key]);
                ((IDictionary)m_dic).Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_dic.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<TKey, TValue>)m_dic).GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)m_dic).GetEnumerator();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            m_dic.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            m_dic.OnDeserialization(sender);
        }
    }
}
