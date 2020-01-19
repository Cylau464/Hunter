using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using Structures;

[System.Serializable]
public class EnemySpellDictionary : SerializableDictionaryBase<string, EnemySpell> { }

/*
namespace System.Collections.Generic
{
    [Serializable]
    public class EnemySpellTuple : SerializableKeyValuePair<string, EnemySpell>
    {
        public EnemySpellTuple(string item1, EnemySpell item2) : base(item1, item2) { }
    }

    [Serializable]
    public class EnemySpellDictionary : SerializableDictionary<string, EnemySpell>
    {
        [SerializeField] private List<EnemySpellTuple> _pairs = new List<EnemySpellTuple>();

        protected override List<SerializableKeyValuePair<string, EnemySpell>> _keyValuePairs
        {
            get
            {
                var list = new List<SerializableKeyValuePair<string, EnemySpell>>();
                foreach (var pair in _pairs)
                {
                    list.Add(new SerializableKeyValuePair<string, EnemySpell>(pair.Key, pair.Value));
                }
                return list;
            }

            set
            {
                _pairs.Clear();
                foreach (var kvp in value)
                {
                    _pairs.Add(new EnemySpellTuple(kvp.Key, kvp.Value));
                }
            }
        }
    }
}*/
