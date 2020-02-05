using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enums
{
    //------------ Weapon enums ------------
    //Physics types of damage
    public enum DamageTypes { Slash, Chop, Thrust, Blunt }
    public enum Elements { Fire, Ice, Wind, Earth, Lightning, Primal }
    public enum WeaponAttackType { Melee, Range }
    public enum WeaponType { Sword, Lance, Staff, Hammer, Bow, Scythe, Gloves }
    public enum WeaponUltimate { Spell, Charging, Buff, Passive }
    public enum BonusAtributes { JumpCount, EvadeCosts, EvadeDistance, Speed }

    //------------ Player enums ------------
    public enum HurtType { None, Repulsion, Catch };
    public enum AttackTypes { NotAttacking, Light, Strong, Joint, TopDown }
    public enum AttackState { Free, Start, Damage, End }
    public enum InputsEnum { Evade, StrongAttack, LightAttack, JointAttack, TopDownAttack }

    //------------ Enemy enums ------------
    public enum SpellStates { None, Prepare, Cast, End }
    public enum DragType { None, Draggable, NotDraggable };
    public enum DefenseTypes { Slash, Chop, Thrust, Blunt };

    
}