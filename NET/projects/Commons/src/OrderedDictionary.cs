using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


// Accord Core Library
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright © César Souza, 2009-2017
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace Commons
{
    [Serializable]
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {

        private readonly List<TKey> _list;
        private readonly Dictionary<TKey, TValue> _dictionary;

        public OrderedDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            _list = new List<TKey>();
        }

        public OrderedDictionary(int capacity)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity);
            _list = new List<TKey>(capacity);
        }

        public OrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(comparer);
            _list = new List<TKey>();
        }

        public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
            _list = new List<TKey>();
        }

        public OrderedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
            _list = new List<TKey>();
        }

        public TKey GetKeyByIndex(int index)
        {
            return _list[index];
        }

        public TValue GetValueByIndex(int index)
        {
            return this[GetKeyByIndex(index)];
        }

        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
            set
            {
                _dictionary[key] = value;
                if (!_list.Contains(key))
                    _list.Add(key);
            }
        }

        public ICollection<TKey> Keys
        {
            get { return _list; }
        }

        public ICollection<TValue> Values
        {
            get { return _list.Select(x => _dictionary[x]).ToList(); }
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IDictionary<TKey, TValue>)_dictionary).IsReadOnly; }
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            if (!_list.Contains(key))
                _list.Add(key);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((IDictionary<TKey, TValue>)_dictionary).Add(item);
            if (!_list.Contains(item.Key))
                _list.Add(item.Key);
        }

        public void Clear()
        {
            _dictionary.Clear();
            _list.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)_dictionary).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<TKey, TValue> pair in this)
                array[arrayIndex++] = pair;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (TKey key in _list)
                yield return new KeyValuePair<TKey, TValue>(key, _dictionary[key]);
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.Remove(key))
            {
                _list.Remove(key);
                return true;
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (((IDictionary<TKey, TValue>)_dictionary).Remove(item))
            {
                _list.Remove(item.Key);
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue value) => ((IDictionary<TKey, TValue>)_dictionary).TryGetValue(key, value: out value!);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
