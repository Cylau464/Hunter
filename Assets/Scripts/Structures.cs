using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

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
        public Element elementDamage;
        public Effects effect;
        public float effectValue;

        [Header("Spells with jumps")]
        public Vector2 jumpDistance;
        public int jumpDirection;       //-1 = back, 1 = forward

        //Сделать для всех полей свойства

        /// <summary>
        /// For spells with jumps
        /// </summary>
        public EnemySpell(float castRange, Vector2 jumpDistance, int jumpDirection, float cooldown, float globalCD, float prepareTime, float castTime, Vector2 damageRange, Vector2 repulseVector, float dazedTime, int firstDamage, int lastDamage, Element elementDamage = new Element(), Effects effect = Effects.None, float effectValue = 0f)
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
            this.elementDamage      = elementDamage;
            this.effect             = effect;
            this.effectValue        = effectValue;

            //Not used
            periodicityDamage = 0f;
        }

        /// <summary>
        /// For spells with once damage
        /// </summary>
        public EnemySpell(float castRange, float cooldown, float globalCD, float prepareTime, float castTime, Vector2 damageRange, Vector2 repulseVector, float dazedTime, int damage, Element elementDamage = new Element(), Effects effect = Effects.None, float effectValue = 0f)
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
            this.elementDamage      = elementDamage;
            this.effect             = effect;
            this.effectValue        = effectValue;

            //Not used
            lastDamage = 0;
            jumpDirection = 0;
            periodicityDamage = 0f;
            jumpDistance = Vector2.zero;
        }

        /// <summary>
        /// For spells with periodic damage
        /// </summary>
        public EnemySpell(float castRange, float cooldown, float globalCD, float prepareTime, float castTime, Vector2 damageRange, Vector2 repulseVector, float dazedTime, int damage, float periodicityDamage, Element elementDamage = new Element(), Effects effect = Effects.None, float effectValue = 0f)
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
            this.elementDamage      = elementDamage;
            this.periodicityDamage  = periodicityDamage;
            this.effect             = effect;
            this.effectValue        = effectValue;

            //Not used
            lastDamage = 0;
            jumpDirection = 0;
            jumpDistance = Vector2.zero;
        }
    }

    /// <summary>
    /// Stores attacks properties
    /// </summary>
    [Serializable]
    public struct EnemyCombo
    {
        public int attackCount;
        public int chance;
        public int[] damage;
        public float[] timeBtwAttack;
        public float[] dazedTime;
        public float attackCD;
        public float attackRange;
        public Vector2[] repulseDistantion;
        public WeaponAttackType attackType;
        public Element element;

        /// <summary>
        /// For melee attacks. In arrays indicate values in order
        /// </summary>
        public EnemyCombo(int attackCount, WeaponAttackType attackType, int chance, int[] damage, float[] timeBtwAttack, float[] dazedTime, float attackCD, Vector2[] repulseDistantion, Element element = new Element())
        {
            this.attackCount = attackCount;
            this.attackType = attackType;
            this.chance = chance;
            this.damage = damage;
            this.timeBtwAttack = timeBtwAttack;
            this.dazedTime = dazedTime;
            this.attackCD = attackCD;
            this.repulseDistantion = repulseDistantion;
            this.element = element;

            attackRange = 0f;
        }

        /// <summary>
        /// For range attacks. In arrays indicate values in order
        /// </summary>
        public EnemyCombo(int attackCount, WeaponAttackType attackType, int chance, int[] damage, float[] timeBtwAttack, float[] dazedTime, float attackCD, float attackRange, Vector2[] repulseDistantion, Element element = new Element())
        {
            this.attackCount = attackCount;
            this.attackType = attackType;
            this.chance = chance;
            this.damage = damage;
            this.timeBtwAttack = timeBtwAttack;
            this.dazedTime = dazedTime;
            this.attackCD = attackCD;
            this.repulseDistantion = repulseDistantion;
            this.attackRange = attackRange;
            this.element = element;
        }
    }

    //public struct EnemySpellCD
    //{
    //    public float curCooldown;
    //    public float curDelay;

    //    public EnemySpellCD(float curCooldown, float curDelay)
    //    {
    //        this.curCooldown = curCooldown;
    //        this.curDelay = curDelay;
    //    }
    //}

    /// <summary>
    /// Stores element properties
    /// </summary>
    [Serializable]
    public struct Element
    {
        [HideInInspector] public string title;
        public Elements element;
        public int value;
        public Color color;

        public Element(string title, Elements element, int value)
        {
            this.title = title;
            this.element = element;
            this.value = value;

            switch (element)
            {
                case Elements.Fire:
                    color = Color.cyan;
                    break;
                case Elements.Earth:
                    color = new Color(.71f, .33f, .1f);
                    break;
                case Elements.Ice:
                    color = new Color(.36f, .56f, .7f);
                    break;
                case Elements.Lightning:
                    color = Color.blue;
                    break;
                case Elements.Primal:
                    color = Color.magenta;
                    break;
                case Elements.Wind:
                    color = Color.green;
                    break;
                default:
                    color = Color.black;
                    break;
            }
        }
    }

    [Serializable]
    public struct Ultimate
    {
        [HideInInspector] public string title;
        
        [Header("General Properties")]
        public WeaponUltimate type;

        [Header("Passive Ultimate")]
        public AtributesDictionary atributes;

        /// <summary>
        /// Passive ultimates
        /// </summary>
        public Ultimate(string title, WeaponUltimate type, AtributesDictionary atributes)
        {
            this.title = title;
            this.type = type;
            this.atributes = atributes;
        }
    }

    [Serializable]
    public struct Effect
    {
        public int maxStackCount;
        public float stackDuration;
        public List<float> curStackDuration;
        public float value;
        public float effectPeriod;
        
        /// <summary>
        /// 2 parameters - continuous effect, 3 parameters - period effect
        /// </summary>
        public Effect(int maxStackCount, float stackDuration, float effectPeriod = 0f)
        {
            this.maxStackCount = maxStackCount;
            this.stackDuration = stackDuration;
            this.effectPeriod = effectPeriod;
            value = 0f;
            curStackDuration = new List<float>(maxStackCount);
        }
    }
}
