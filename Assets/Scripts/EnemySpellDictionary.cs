using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

namespace System.Collections.Generic
{
    [Serializable]
    public class EnemySpellTuple : SerializableKeyValuePair<Enemy.SpellEnum, EnemySpell>
    {
        public EnemySpellTuple(Enemy.SpellEnum item1, EnemySpell item2) : base(item1, item2) { }
    }

    [Serializable]
    public class EnemySpellDictionary : SerializableDictionary<Enemy.SpellEnum, EnemySpell>
    {
        [SerializeField] private List<EnemySpellTuple> _pairs = new List<EnemySpellTuple>();

        protected override List<SerializableKeyValuePair<Enemy.SpellEnum, EnemySpell>> _keyValuePairs
        {
            get
            {
                var list = new List<SerializableKeyValuePair<Enemy.SpellEnum, EnemySpell>>();
                foreach (var pair in _pairs)
                {
                    list.Add(new SerializableKeyValuePair<Enemy.SpellEnum, EnemySpell>(pair.Key, pair.Value));
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