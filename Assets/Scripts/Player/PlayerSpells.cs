using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

class PlayerSpells : MonoBehaviour
{
    [SerializeField] GameObject spellListPrefab = null;
    [SerializeField] SpellBar spellBar = null;

    SpellsList spellList;
    PlayerInput input;
    PlayerMovement movement;
    PlayerAtributes atributes;
    PlayerAttack attack;

    void Start()
    {
        spellList = spellListPrefab.GetComponent<SpellsList>();
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();
        attack = GetComponent<PlayerAttack>();
    }

    void Update()
    {
        if(!movement.isEvading && !movement.isClimbing && !movement.isHanging &&
           input.lastInputs.Count > 0 && (input.lastInputs[0] == InputsEnum.FirstSpell || input.lastInputs[0] == InputsEnum.SecondSpell || input.lastInputs[0] == InputsEnum.ThirdSpell) &&
           attack.attackState != AttackState.Damage)
        {
            SpellsTitle _spellTitle = default;

            switch(input.lastInputs[0])
            {
                case InputsEnum.FirstSpell:
                    _spellTitle = spellBar.spellTitle[0];
                    break;
                case InputsEnum.SecondSpell:
                    _spellTitle = spellBar.spellTitle[1];
                    break;
                case InputsEnum.ThirdSpell:
                    _spellTitle = spellBar.spellTitle[2];
                    break;
            }

            input.lastInputs.RemoveAt(0);
            CastSpell(_spellTitle);
        }
    }

    void CastSpell(SpellsTitle spellTitle)
    {
        Instantiate(spellList.spells[spellTitle], transform);
    }
}

