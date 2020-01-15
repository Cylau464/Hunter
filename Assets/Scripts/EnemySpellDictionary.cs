using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.Collections.Generic
{
    [Serializable]
    public class EnemySpellTuple : SerializableKeyValuePair<SpellEnum, EnemySpell>
    {
        public EnemySpellTuple(SpellEnum item1, EnemySpell item2) : base(item1, item2) { }
    }

    [Serializable]
    public class EnemySpellDictionary : SerializableDictionary<SpellEnum, EnemySpell>
    {
        [SerializeField] private List<EnemySpellTuple> _pairs = new List<EnemySpellTuple>();

        protected override List<SerializableKeyValuePair<SpellEnum, EnemySpell>> _keyValuePairs
        {
            get
            {
                var list = new List<SerializableKeyValuePair<SpellEnum, EnemySpell>>();
                foreach (var pair in _pairs)
                {
                    list.Add(new SerializableKeyValuePair<SpellEnum, EnemySpell>(pair.Key, pair.Value));
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
}