using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public DamageTypes damageType = DamageTypes.Slash;
    public WeaponType weaponType = WeaponType.Melee;
    public GameObject shellPrefab;

    public Element[] elements = {
        new Element("Fire", Elements.Fire, 0),
        new Element("Wind", Elements.Wind, 0),
        new Element("Earth", Elements.Earth, 0),
        new Element("Water", Elements.Water, 0)
    };

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

public enum Elements { Fire, Water, Wind, Earth }

public enum WeaponType { Melee, Range, Magic }

[System.Serializable]
public struct Element
{
    public string title;
    public Elements element;
    public int value;

    public Element(string title, Elements element, int value)
    {
        this.title = title;
        this.element = element;
        this.value = value;
    }
}
