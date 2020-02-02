using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class WeaponAtributes : MonoBehaviour
{
    [Header("Main Atributes")]
    public string title = "Weapon";
    public int lightAttackDamage = 1;
    public int strongAttackDamage = 2;
    public int jointAttackDamage = 3;
    public float lightAttackSpeed = .4f;
    public float strongAttackSpeed = .6f;
    public float jointAttackSpeed = .7f;

    [Header("Additional Atributes")]
    public float attackRangeX = 1.2f;
    public float attackRangeY = 1f;
    public float lightAttackForce = 1f;
    public float strongAttackForce = 2f;
    public float jointAttackForce = 2.5f;
    public float shellSpeed = 0f;
    public float weaponMass = 1f;                       //It multiplier character move speed and jump height. 1 = 100% speed and height
    public float speedDivisorL = 1f;                    //Movespeed divisor light attack
    public float speedDivisorS = 1f;                    //Movespeed divisor strong attack
    public float speedDivisorJ = 1f;                    //Movespeed divisor joint attack

    [Header("Dictionaries of Types")]
    public DamageTypeOfAttackDictionary damageTypesOfAttacks = new DamageTypeOfAttackDictionary()
    {
        { AttackTypes.Light, DamageTypes.Slash },
        { AttackTypes.Strong, DamageTypes.Slash },
        { AttackTypes.Joint, DamageTypes.Thrust }
    };
    public DamageTypes[] damageType = { DamageTypes.Slash };
    public WeaponAttackType weaponAttackType = WeaponAttackType.Melee;
    public WeaponType weaponType = WeaponType.Sword;

    public GameObject shellPrefab;

    [Header("Elements")]
    public Element element = new Element("Fire", Elements.Fire, 0);
    public Element elementJA = new Element("Wind", Elements.Wind, 0);

    void Start()
    {

    }

    void Update()
    {
        //public string title { get; private set; } = "Weapon";
        //public int damage { get; private set; } = 5;
    }
}

//Physics types of damage
public enum DamageTypes { Slash, Chop, Thrust, Blunt }

public enum Elements { Fire, Ice, Wind, Earth, Lightning, Primal }

public enum WeaponAttackType { Melee, Range }

public enum WeaponType { Sword, Lance, Staff, Hammer, Bow, Scythe, Gloves }
