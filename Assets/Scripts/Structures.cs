using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structures
{
    /// <summary>
    /// Stores spell properties
    /// </summary>
    [Serializable]
    public struct EnemySpell
    {
        [Header("General Properties")]
        //public float delayAfterCast;
        public float cooldown;
        public float globalCD;
        public float prepareTime;
        public float castTime;
        public float castRange;

        [Header("Attack Properties")]
        public float periodicityDamage;
        public float dazedTime;
        public Vector2 repulseVector;
        public Vector2 damageRange;

        public int firstDamage;
        public int lastDamage;

        [Header("Spells with jumps")]
        public Vector2 jumpDistance;
        public int jumpDirection;       //-1 = back, 1 = forward

        //Сделать для всех полей свойства

        /// <summary>
        /// For spells with jumps
        /// </summary>
        public EnemySpell(float castRange, Vector2 jumpDistance, int jumpDirection, float cooldown, float globalCD, float prepareTime, float castTime, Vector2 damageRange, Vector2 repulseVector, float dazedTime, int firstDamage, int lastDamage)
        {
            this.castRange          = castRange;
            this.jumpDistance       = jumpDistance;
            this.jumpDirection      = jumpDirection;
            //this.delayAfterCast = delayAfterCast;
            this.cooldown           = cooldown;
            this.globalCD           = globalCD;
            this.prepareTime        = prepareTime;
            this.castTime           = castTime;
            this.damageRange        = damageRange;
            this.repulseVector      = repulseVector;
            this.dazedTime          = dazedTime;
            this.firstDamage        = firstDamage;
            this.lastDamage         = lastDamage;

            //Not used
            periodicityDamage = 0f;
        }

        /// <summary>
        /// For spells with once damage
        /// </summary>
        public EnemySpell(float castRange, float cooldown, float globalCD, float prepareTime, float castTime, Vector2 damageRange, Vector2 repulseVector, float dazedTime, int damage)
        {
            this.castRange          = castRange;
            this.cooldown           = cooldown;
            this.globalCD           = globalCD;
            this.prepareTime        = prepareTime;
            this.castTime           = castTime;
            this.damageRange        = damageRange;
            this.repulseVector      = repulseVector;
            this.dazedTime          = dazedTime;
            firstDamage             = damage;

            //Not used
            lastDamage = 0;
            jumpDirection = 0;
            periodicityDamage = 0f;
            jumpDistance = Vector2.zero;
        }

        /// <summary>
        /// For spells with periodic damage
        /// </summary>
        public EnemySpell(float castRange, float cooldown, float globalCD, float prepareTime, float castTime, Vector2 damageRange, Vector2 repulseVector, float dazedTime, int damage, float periodicityDamage)
        {
            this.castRange          = castRange;
            this.cooldown           = cooldown;
            this.globalCD           = globalCD;
            this.prepareTime        = prepareTime;
            this.castTime           = castTime;
            this.damageRange        = damageRange;
            this.repulseVector      = repulseVector;
            this.dazedTime          = dazedTime;
            firstDamage             = damage;
            this.periodicityDamage  = periodicityDamage;

            //Not used
            lastDamage = 0;
            jumpDirection = 0;
            jumpDistance = Vector2.zero;
        }
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

    /// <summary>
    /// Stores element properties
    /// </summary>
    [Serializable]
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
