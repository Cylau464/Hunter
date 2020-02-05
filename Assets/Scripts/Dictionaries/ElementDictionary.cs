using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using Enums;

[System.Serializable]
public class ElementDictionary : SerializableDictionaryBase<Elements, int> { }
/*
namespace System.Collections.Generic
{
    [Serializable]
    public class ElementTuple : SerializableKeyValuePair<Elements, int>
    {
        public ElementTuple(Elements item1, int item2) : base(item1, item2) { }
    }

    [Serializable]
    public class ElementDictionary : SerializableDictionary<Elements, int>
    {
        [SerializeField] private List<ElementTuple> _pairs = new List<ElementTuple>();

        protected override List<SerializableKeyValuePair<Elements, int>> _keyValuePairs
        {
            get
            {
                var list = new List<SerializableKeyValuePair<Elements, int>>();
                foreach (var pair in _pairs)
                {
                    list.Add(new SerializableKeyValuePair<Elements, int>(pair.Key, pair.Value));
                }
                return list;
            }

            set
            {
                _pairs.Clear();
                foreach (var kvp in value)
                {
                    _pairs.Add(new ElementTuple(kvp.Key, kvp.Value));
                }
            }
        }
    }
}*/
