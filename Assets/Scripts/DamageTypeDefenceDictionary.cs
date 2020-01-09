using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.Collections.Generic
{
    [Serializable]
    public class DamageTypeDefenceTuple : SerializableKeyValuePair<DamageTypes, int>
    {
        public DamageTypeDefenceTuple(DamageTypes item1, int item2) : base(item1, item2) { }
    }

    [Serializable]
    public class DamageTypeDefenceDictionary : SerializableDictionary<DamageTypes, int>
    {
        [SerializeField] private List<DamageTypeDefenceTuple> _pairs = new List<DamageTypeDefenceTuple>();

        protected override List<SerializableKeyValuePair<DamageTypes, int>> _keyValuePairs
        {
            get
            {
                var list = new List<SerializableKeyValuePair<DamageTypes, int>>();
                foreach (var pair in _pairs)
                {
                    list.Add(new SerializableKeyValuePair<DamageTypes, int>(pair.Key, pair.Value));
                }
                return list;
            }

            set
            {
                _pairs.Clear();
                foreach (var kvp in value)
                {
                    _pairs.Add(new DamageTypeDefenceTuple(kvp.Key, kvp.Value));
                }
            }
        }
    }
}
