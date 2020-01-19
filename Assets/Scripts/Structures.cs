using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structures
{
    [System.Serializable]
    public struct EnemySpell
    {
        [Header("General Properties")]
        //public float delayAfterCast;
        //public float hurtDuration;
        public float cooldown;
        public float globalCD;
        public float prepareTime;
        public float castTime;

        [Header("Attack Properties")]
        public float damageRangeX;
        public float damageRangeY;
        public float dazedTime;
        public Vector2 repulseVector;

        public int firstDamage;
        public int lastDamage;

        [Header("Spells with jumps")]
        public float jumpDistance;
        public float jumpHeight;
        public int jumpDirection;       //-1 = left, 1 = right

        //Сделать для всех полей свойства

        //For spells with jumps
        public EnemySpell(float jumpDistance, float jumpHeight, int jumpDirection, float cooldown, float globalCD, float prepareTime, float castTime, float damageRangeX, float damageRangeY, Vector2 repulseVector, float dazedTime, int firstDamage, int lastDamage)
        {
            this.jumpDistance = jumpDistance;
            this.jumpHeight = jumpHeight;
            this.jumpDirection = jumpDirection;
            //this.delayAfterCast = delayAfterCast;
            this.cooldown = cooldown;
            this.globalCD = globalCD;
            this.prepareTime = prepareTime;
            this.castTime = castTime;
            this.damageRangeX = damageRangeX;
            this.damageRangeY = damageRangeY;
            this.repulseVector = repulseVector;
            this.dazedTime = dazedTime;
            this.firstDamage = firstDamage;
            this.lastDamage = lastDamage;
            //this.spell          = spell;
        }
        /*
        //For other spells
        public EnemySpell()
        {

        }*/
    }

    public struct EnemySpellCD
    {
        public float curCooldown;
        public float curDelay;

        public EnemySpellCD(float curCooldown, float curDelay)
        {
            this.curCooldown = curCooldown;
            this.curDelay = curDelay;
        }
    }

    [System.Serializable]
    public struct Element
    {
        [HideInInspector] public string title;
        public Elements element;
        public int value;

        public Element(string title, Elements element, int value)
        {
            this.title = title;
            this.element = element;
            this.value = value;
        }
    }
}
