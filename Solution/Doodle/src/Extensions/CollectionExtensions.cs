using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public static class CollectionExtensions
    {
        public static TValue GetValueOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TValue> onCreateValue)
        {
            if (!dic.TryGetValue(key, out var value))
            {
                value = onCreateValue();
                dic.Add(key, value);
            }
            return value;
        }
    }
}
