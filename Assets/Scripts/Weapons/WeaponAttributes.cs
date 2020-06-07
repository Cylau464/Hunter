using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class WeaponAttributes : MonoBehaviour
{
    [Header("Main Attributes")]
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

    [Header("Additional Attributes")]
    public float attackRangeX               = 1.2f;
    public float attackRangeY               = 1f;
    public float lightAttackForce           = 1f;
    public float strongAttackForce          = 2f;
    public float jointAttackForce           = 2.5f;
    public float topDownAttackForce         = 10f;
    public float mass = 1f;                    //It multiplier character move speed and jump height. 1 = 100% speed and height

    [Header("Shell Properties")]
    public float shellSpeed                 = 0f;
    public float shellFlyTime               = 1f;

    [Header("Dictionaries of Types")]
    public DamageTypeOfAttackDictionary damageTypesOfAttacks = new DamageTypeOfAttackDictionary()
    {
        { AttackTypes.Light, DamageTypes.Slash },
        { AttackTypes.Strong, DamageTypes.Slash },
        { AttackTypes.Joint, DamageTypes.Thrust },
        { AttackTypes.TopDown, DamageTypes.Slash }
    };
    public WeaponAttackType weaponAttackType = WeaponAttackType.Melee;
    public WeaponType type = WeaponType.Sword;
    public Ultimate weaponUltimate = new Ultimate("Evade Bonus", WeaponUltimate.Passive, new AttributesDictionary() {
        { BonusAttributes.EvadeCosts, 3 },
        { BonusAttributes.EvadeDistance, 3 }
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

    //[Header("Spells")]
    //[SerializeField] GameObject[] spellPrefabs = new GameObject[3];
    //[HideInInspector] public Spell[] spells = new Spell[3];
    //[SerializeField] SpellBar spellBar = null;

    [Header("Weapon Icon")]
    public Sprite icon;
    StatusBar statusBar;

    [Header("Audio Clips")]
    public AudioClip[] lightSwingClips;
    public AudioClip[] strongSwingClips;
    public AudioClip[] jointSwingClips;
    public AudioClip[] lightAttackClips;
    public AudioClip[] strongAttackClips;
    public AudioClip[] jointAttackClips;
    public AudioClip impactClip;

    void Start()
    {
        GameObject[] _go = GameObject.FindGameObjectsWithTag("PlayerUI");

        foreach (GameObject go in _go)
        {
            if (go.TryGetComponent(out StatusBar _sb) && statusBar == null)
                statusBar = _sb;
        }

        statusBar.WeaponIconSet(icon, gameObject.tag);

        //for (int i = 0; i < spellPrefabs.Length; i++)
        //{
        //    spells[i] = spellPrefabs[i].GetComponent<Spell>();
        //}

        //if (gameObject.tag == "Second Weapon")
        //    gameObject.SetActive(false);
        //else
        //    spellBar.SetSpells(spells);
    }
}
