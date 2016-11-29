using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTRazorPortable
{
    public class ModelStateDictionary : IDictionary<string, ModelState>
    {
        private readonly IDictionary<string, ModelState> _innerDictionary;
        public static string FormFieldKey = "_Form";

        public ModelStateDictionary()
        {
            _innerDictionary = new Dictionary<string, ModelState>(StringComparer.OrdinalIgnoreCase);
        }

        public ModelStateDictionary(ModelStateDictionary dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            _innerDictionary = new CopyOnWriteDictionary<string, ModelState>(dictionary.InnerDictionary,
                                                                             StringComparer.OrdinalIgnoreCase);
        }

        public int Count
        {
            get { return _innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return _innerDictionary.IsReadOnly; }
        }

        public bool IsValid
        {
            get { return Values.All(modelState => modelState.Errors.Count == 0); }
        }

        public ICollection<string> Keys
        {
            get { return _innerDictionary.Keys; }
        }

        public ICollection<ModelState> Values
        {
            get { return _innerDictionary.Values; }
        }

        public ModelState this[string key]
        {
            get
            {
                ModelState value;
                _innerDictionary.TryGetValue(key, out value);
                return value;
            }
            set { _innerDictionary[key] = value; }
        }

        // For unit testing
        internal IDictionary<string, ModelState> InnerDictionary
        {
            get { return _innerDictionary; }
        }

        public void Add(KeyValuePair<string, ModelState> item)
        {
            _innerDictionary.Add(item);
        }

        public void Add(string key, ModelState value)
        {
            _innerDictionary.Add(key, value);
        }

        public void AddModelError(string key, Exception exception)
        {
            GetModelStateForKey(key).Errors.Add(exception);
        }

        public void AddModelError(string key, string errorMessage)
        {
            GetModelStateForKey(key).Errors.Add(errorMessage);
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, ModelState> item)
        {
            return _innerDictionary.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _innerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, ModelState>[] array, int arrayIndex)
        {
            _innerDictionary.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, ModelState>> GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        private ModelState GetModelStateForKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            ModelState modelState;
            if (!TryGetValue(key, out modelState))
            {
                modelState = new ModelState();
                this[key] = modelState;
            }

            return modelState;
        }

        public bool IsValidField(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            // if the key is not found in the dictionary, we just say that it's valid (since there are no errors)
            return DictionaryHelpers.FindKeysWithPrefix(this, key).All(entry => entry.Value.Errors.Count == 0);
        }

        public void Merge(ModelStateDictionary dictionary)
        {
            if (dictionary == null)
            {
                return;
            }

            foreach (var entry in dictionary)
            {
                this[entry.Key] = entry.Value;
            }
        }

        public bool Remove(KeyValuePair<string, ModelState> item)
        {
            return _innerDictionary.Remove(item);
        }

        public bool Remove(string key)
        {
            return _innerDictionary.Remove(key);
        }

        public void SetModelValue(string key, ValueProviderResult value)
        {
            GetModelStateForKey(key).Value = value;
        }

        public bool TryGetValue(string key, out ModelState value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    internal class CopyOnWriteDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _sourceDictionary;
        private readonly IEqualityComparer<TKey> _comparer;
        private IDictionary<TKey, TValue> _innerDictionary;

        public CopyOnWriteDictionary(IDictionary<TKey, TValue> sourceDictionary,
                                     IEqualityComparer<TKey> comparer)
        {
            _sourceDictionary = sourceDictionary;
            _comparer = comparer;
        }

        private IDictionary<TKey, TValue> ReadDictionary
        {
            get
            {
                return _innerDictionary ?? _sourceDictionary;
            }
        }

        private IDictionary<TKey, TValue> WriteDictionary
        {
            get
            {
                if (_innerDictionary == null)
                {
                    _innerDictionary = new Dictionary<TKey, TValue>(_sourceDictionary,
                                                                    _comparer);
                }

                return _innerDictionary;
            }
        }

        public virtual ICollection<TKey> Keys
        {
            get
            {
                return ReadDictionary.Keys;
            }
        }

        public virtual ICollection<TValue> Values
        {
            get
            {
                return ReadDictionary.Values;
            }
        }

        public virtual int Count
        {
            get
            {
                return ReadDictionary.Count;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                return ReadDictionary[key];
            }
            set
            {
                WriteDictionary[key] = value;
            }
        }

        public virtual bool ContainsKey(TKey key)
        {
            return ReadDictionary.ContainsKey(key);
        }

        public virtual void Add(TKey key, TValue value)
        {
            WriteDictionary.Add(key, value);
        }

        public virtual bool Remove(TKey key)
        {
            return WriteDictionary.Remove(key);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return ReadDictionary.TryGetValue(key, out value);
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            WriteDictionary.Add(item);
        }

        public virtual void Clear()
        {
            WriteDictionary.Clear();
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ReadDictionary.Contains(item);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ReadDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return WriteDictionary.Remove(item);
        }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ReadDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal static class DictionaryHelpers
    {
        public static List<KeyValuePair<string, TValue>> FindKeysWithPrefix<TValue>(IDictionary<string, TValue> dictionary, string prefix)
        {
            TValue exactMatchValue;
            if (dictionary.TryGetValue(prefix, out exactMatchValue))
            {
                return new List<KeyValuePair<string, TValue>> { new KeyValuePair<string, TValue> (prefix, exactMatchValue) };
            }

            foreach (var entry in dictionary)
            {
                string key = entry.Key;

                if (key.Length <= prefix.Length)
                {
                    continue;
                }

                if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                char charAfterPrefix = key[prefix.Length];
                switch (charAfterPrefix)
                {
                    case '[':
                    case '.':
                        return new List<KeyValuePair<string, TValue>> { entry };
                }
            }

            return null;
        }

        public static bool DoesAnyKeyHavePrefix<TValue>(IDictionary<string, TValue> dictionary, string prefix)
        {
            return FindKeysWithPrefix(dictionary, prefix).Any();
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue @default)
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            return @default;
        }
    }
}