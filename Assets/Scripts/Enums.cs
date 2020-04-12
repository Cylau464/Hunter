using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enums
{
    //------------ Weapon enums ------------
    //Physics types of damage
    public enum DamageTypes { Slash, Chop, Thrust, Blunt }
    public enum Elements { None, Fire, Ice, Wind, Earth, Lightning, Primal }
    public enum WeaponAttackType { Melee, Range }
    public enum WeaponType { Sword, Lance, Staff, Hammer, Bow, Scythe, Gloves }
    public enum WeaponUltimate { Spell, Charging, Buff, Passive }
    public enum BonusAttributes { JumpCount, EvadeCosts, EvadeDistance, Speed }
    public enum Effects { Freeze, Burning, Poison, Bleeding, Root }

    //------------ Player enums ------------
    public enum HurtType { None, Repulsion, Catch, Stun };
    public enum AttackTypes { NotAttacking, Light, Strong, Joint, TopDown }
    public enum AttackState { Free, Start, Damage, End }
    public enum InputsEnum { Evade, StrongAttack, LightAttack, JointAttack, TopDownAttack, FirstSpell, SecondSpell, ThirdSpell }
    public enum SpellTitles { FreeSlot, CircleSmash, FireSpike, IcePunch, VoidShift }
    public enum SpellTypes { MeleeAOE, Melee, RangeAOE, Range }

    //------------ Enemy enums ------------
    public enum EnemySpellStates { None, Prepare, Cast, End, Destroy }
    public enum DragType { None, Draggable, NotDraggable };
    public enum DefenseTypes { Slash, Chop, Thrust, Blunt };
}