using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.Collections.Generic
{
    [Serializable]
    public class DamageTypeofAttackTuple : SerializableKeyValuePair<AttackTypes, DamageTypes>
    {
        public DamageTypeofAttackTuple(AttackTypes item1, DamageTypes item2) : base(item1, item2) { }
    }

    [Serializable]
    public class DamageTypeOfAttackDictionary : SerializableDictionary<AttackTypes, DamageTypes>
    {
        [SerializeField] private List<DamageTypeofAttackTuple> _pairs = new List<DamageTypeofAttackTuple>();

        protected override List<SerializableKeyValuePair<AttackTypes, DamageTypes>> _keyValuePairs
        {
            get
            {
                var list = new List<SerializableKeyValuePair<AttackTypes, DamageTypes>>();
                foreach (var pair in _pairs)
                {
                    list.Add(new SerializableKeyValuePair<AttackTypes, DamageTypes>(pair.Key, pair.Value));
                }
                return list;
            }

            set
            {
                _pairs.Clear();
                foreach (var kvp in value)
                {
                    _pairs.Add(new DamageTypeofAttackTuple(kvp.Key, kvp.Value));
                }
            }
        }
    }
}