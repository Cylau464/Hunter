using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class WeaponAtributes : MonoBehaviour
{
    [Header("Main Atributes")]
    public string title = "Weapon";
    public int lightAttackDamage            = 1;
    public int strongAttackDamage           = 2;
    public int jointAttackDamage            = 3;
    public int topDownAttackDamage          = 4;
    public int lightAttackStaminaCosts      = 10;
    public int strongAttackStaminaCosts     = 15;
    public int jointAttackStaminaCosts      = 20;
    public int topDownAttackStaminaCosts    = 15;
    public float lightAttackSpeed           = .4f;
    public float strongAttackSpeed          = .6f;
    public float jointAttackSpeed           = .7f;
    public float topDownAttackSpeed         = .8f;

    [Header("Additional Atributes")]
    public float attackRangeX               = 1.2f;
    public float attackRangeY               = 1f;
    public float lightAttackForce           = 1f;
    public float strongAttackForce          = 2f;
    public float jointAttackForce           = 2.5f;
    public float topDownAttackForce         = 10f;
    public float shellSpeed                 = 0f;
    public float weaponMass                 = 1f;                    //It multiplier character move speed and jump height. 1 = 100% speed and height
    public float speedDivisorL              = 1f;                    //Movespeed divisor light attack
    public float speedDivisorS              = 1f;                    //Movespeed divisor strong attack
    public float speedDivisorJ              = 1f;                    //Movespeed divisor joint attack
    public float speedDivisorTD             = 1f;                    //Movespeed divisor top-down attack

    [Header("Dictionaries of Types")]
    public DamageTypeOfAttackDictionary damageTypesOfAttacks = new DamageTypeOfAttackDictionary()
    {
        { AttackTypes.Light, DamageTypes.Slash },
        { AttackTypes.Strong, DamageTypes.Slash },
        { AttackTypes.Joint, DamageTypes.Thrust },
        { AttackTypes.TopDown, DamageTypes.Slash }
    };
    public WeaponAttackType weaponAttackType = WeaponAttackType.Melee;
    public WeaponType weaponType = WeaponType.Sword;
    public Ultimate weaponUltimate = new Ultimate("Evade Bonus", WeaponUltimate.Passive, new AtributesDictionary() {
        { BonusAtributes.EvadeCosts, 3 },
        { BonusAtributes.EvadeDistance, 3 }
    });

    public GameObject shellPrefab;

    [Header("Elements")]
    public ElementsOfAttacksDictionary elements = new ElementsOfAttacksDictionary()
    {
        { AttackTypes.Light, new Element("Fire", Elements.Fire, 0) },
        { AttackTypes.Strong, new Element("Fire", Elements.Fire, 0) },
        { AttackTypes.Joint, new Element("Fire", Elements.Fire, 0) },
        { AttackTypes.TopDown, new Element("Wind", Elements.Wind, 0) }
    };

    [Header("Weapon Icon")]
    public Sprite icon;
    StatusBar statusBar;

    void Start()
    {
        GameObject[] _go = GameObject.FindGameObjectsWithTag("UI");

        foreach (GameObject go in _go)
        {
            if (go.TryGetComponent(out StatusBar _sb) && statusBar == null)
                statusBar = _sb;
        }

        statusBar.WeaponIconSet(icon, gameObject.tag);

        if (gameObject.tag == "Second Weapon")
            gameObject.SetActive(false);  
    }
}
