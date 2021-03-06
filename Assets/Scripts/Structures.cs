﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Structures
{
    /// <summary>
    /// Player spell properties
    /// </summary>
    [Serializable]
    public struct PlayerSpell
    {
        public int attackCount;
        public int[] damage;
        public float[] timeBtwAttack;
        public DamageTypes[] damageType;
        public SpellTypes type;
        public SpellUseTypes useType;
        public float timeToUse;
        //[Range(1, 3)] public int energyCost;
        public float castTime;
        public float cooldown;
        public Vector2[] damageRange;
        public Element element;
        public Sprite icon;
        public Sprite waitingIcon;
        public GameObject particle;
        public GameObject spellPrefab;  // Some GO like a shell or mark
        public AnimationClip castAnimation;
        public AnimationClip[] attackAnimations;
        public AudioClip audioCast;
        public AudioClip[] audioAttacks;
        public AudioClip audioImpact;

        [Header("Movement Properties")]
        public Vector2[] forceDirection;

        [Header("Range Properties")]
        public float[] castDistance;
        public float[] shellLifeTime;
        public AudioClip[] shellClip;

        public PlayerSpell(int attackCount, int[] damage, float[] timeBtwAttack, DamageTypes[] damageType, SpellTypes type, SpellUseTypes useType, float timeToUse/*, int energyCost*/, float castTime, float cooldown, Vector2[] damageRange, Element element, Vector2[] forceDirection, float[] castDistance, float[] shellLifeTime)
        {
            this.attackCount = attackCount;
            this.damage = damage;
            this.timeBtwAttack = timeBtwAttack;
            this.damageType = damageType;
            this.type = type;
            //this.energyCost = energyCost;
            this.castTime = castTime;
            this.cooldown = cooldown;
            this.damageRange = damageRange;
            this.element = element;
            this.forceDirection = forceDirection;
            this.castDistance = castDistance;
            this.timeToUse = timeToUse;
            this.useType = useType;
            this.shellLifeTime = shellLifeTime;

            icon = null;
            waitingIcon = null;
            castAnimation = null;
            attackAnimations = null;
            audioCast = null;
            audioImpact = null;
            audioAttacks = null;
            particle = null;
            spellPrefab = null;
            shellClip = null;
        }
    }

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
        public Effect effect;

        [Header("Spells with jumps")]
        public Vector2 jumpDistance;
        public int jumpDirection;       //-1 = back, 1 = forward

        [Header("Audio")]
        public AudioClip spellClip;
        public AudioClip jumpClip;
        public AudioClip landingClip;

        /// <summary>
        /// For spells with jumps
        /// </summary>
        public EnemySpell(float castRange, Vector2 jumpDistance, int jumpDirection, float cooldown, float globalCD, float prepareTime, float castTime, Vector2 damageRange, Vector2 repulseVector, float dazedTime, int firstDamage, int lastDamage, Element elementDamage = new Element(), Effect effect = new Effect())
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

            //Not used
            periodicityDamage = 0f;
            spellClip = null;
            jumpClip = null;
            landingClip = null;
        }

        /// <summary>
        /// For spells with once damage
        /// </summary>
        public EnemySpell(float castRange, float cooldown, float globalCD, float prepareTime, float castTime, Vector2 damageRange, Vector2 repulseVector, float dazedTime, int damage, Element elementDamage = new Element(), Effect effect = new Effect())
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

            //Not used
            lastDamage = 0;
            jumpDirection = 0;
            periodicityDamage = 0f;
            jumpDistance = Vector2.zero;
            spellClip = null;
            jumpClip = null;
            landingClip = null;
        }

        /// <summary>
        /// For spells with periodic damage
        /// </summary>
        public EnemySpell(float castRange, float cooldown, float globalCD, float prepareTime, float castTime, Vector2 damageRange, Vector2 repulseVector, float dazedTime, int damage, float periodicityDamage, Element elementDamage = new Element(), Effect effect = new Effect())
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

            //Not used
            lastDamage = 0;
            jumpDirection = 0;
            jumpDistance = Vector2.zero;
            spellClip = null;
            jumpClip = null;
            landingClip = null;
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
        public AudioClip[] attackClips;

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
            attackClips = null;
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

            attackClips = null;
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
                    color = new Color(.91f, .33f, .1f);
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
        public AttributesDictionary attributes;

        /// <summary>
        /// Passive ultimates
        /// </summary>
        public Ultimate(string title, WeaponUltimate type, AttributesDictionary attributes)
        {
            this.title = title;
            this.type = type;
            this.attributes = attributes;
        }
    }

    [Serializable]
    public struct Effect
    {
        public Effects effect;
        public int stacksCount;
        public int maxStacksCount;
        public float stackDuration;
        public List<float> curStackDuration;
        public float value;
        public float effectPeriod;
        public Color color;
        
        /// <summary>
        /// Common effect parameters. 2 parameters - continuous effect, 3 parameters - period effect
        /// </summary>
        public Effect(Effects effect, int maxStacksCount, float stackDuration, float value, float effectPeriod = 0f)
        {
            this.maxStacksCount  = maxStacksCount;
            this.stackDuration  = stackDuration;
            this.effectPeriod   = effectPeriod;
            this.effect         = effect;
            this.value          = value;

            stacksCount         = 0;
            curStackDuration    = new List<float>(maxStacksCount);

            switch (effect)
            {
                case Effects.Burning:
                    color = new Color(.97f, .4f, .18f);
                    break;
                case Effects.Root:
                    color = new Color(.71f, .33f, .1f);
                    break;
                case Effects.Freeze:
                    color = new Color(.36f, .56f, .7f);
                    break;
                case Effects.Poison:
                    color = Color.green;
                    break;
                default:
                    color = Color.black;
                    break;
            }
        }

        /// <summary>
        /// Used for set effect
        /// </summary>
        public Effect(Effects effect, int stacksCount)
        {
            this.effect = effect;
            this.stacksCount = stacksCount;

            value = 0f;
            maxStacksCount = 0;
            stackDuration = 0f;
            effectPeriod = 0f;
            curStackDuration = new List<float>();

            switch (effect)
            {
                case Effects.Burning:
                    color = new Color(.97f, .4f, .18f);
                    break;
                case Effects.Root:
                    color = new Color(.71f, .33f, .1f);
                    break;
                case Effects.Freeze:
                    color = new Color(.36f, .56f, .7f);
                    break;
                case Effects.Poison:
                    color = Color.green;
                    break;
                default:
                    color = Color.black;
                    break;
            }
        }

        /// <summary>
        /// Used for set effect with one stack
        /// </summary>
        public Effect(Effects effect, float effectDuration)
        {
            this.effect = effect;
            stackDuration = effectDuration;
            stacksCount = 1;

            value = 0f;
            maxStacksCount = 0;
            effectPeriod = 0f;
            curStackDuration = new List<float>();

            switch (effect)
            {
                case Effects.Burning:
                    color = new Color(.97f, .4f, .18f);
                    break;
                case Effects.Root:
                    color = new Color(.71f, .33f, .1f);
                    break;
                case Effects.Freeze:
                    color = new Color(.36f, .56f, .7f);
                    break;
                case Effects.Poison:
                    color = Color.green;
                    break;
                default:
                    color = Color.black;
                    break;
            }
        }
    }
}
