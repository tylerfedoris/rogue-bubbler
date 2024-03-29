﻿using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new();
        [SerializeField] private List<TValue> values = new();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var (key, value) in this)
            {
                keys.Add(key);
                values.Add(value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (var i = 0; i < keys.Count; i++)
            {
                Add(keys[i], values[i]);
            }
        }
    }
}