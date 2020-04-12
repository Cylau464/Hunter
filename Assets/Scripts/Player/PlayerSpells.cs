using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class PlayerSpells : MonoBehaviour
{
    [SerializeField] GameObject spellListPrefab = null;
    [SerializeField] SpellBar spellBar = null;

    SpellsList spellList;
    PlayerInput input;
    PlayerMovement movement;
    PlayerAttributes attributes;
    PlayerAttack attack;
    Animator anim;
    Spell curSpell;
    Rigidbody2D rigidBody;

    [HideInInspector] public int attackNumber;

    void Start()
    {
        spellList = spellListPrefab.GetComponent<SpellsList>();
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();
        attack = GetComponent<PlayerAttack>();
        attributes = GetComponent<PlayerAttributes>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(!movement.isEvading && !movement.isClimbing && !movement.isHanging && !movement.isCast &&
           input.lastInputs.Count > 0 && (input.lastInputs[0] == InputsEnum.FirstSpell || input.lastInputs[0] == InputsEnum.SecondSpell || input.lastInputs[0] == InputsEnum.ThirdSpell) &&
           attack.attackState != AttackState.Damage)
        {
            int _cellIndex = 0;
            SpellTitles _spellTitle = default;
            float _curCooldown = 0f;

            switch(input.lastInputs[0])
            {
                case InputsEnum.FirstSpell:
                    _cellIndex = 0;
                    break;
                case InputsEnum.SecondSpell:
                    _cellIndex = 1;
                    break;
                case InputsEnum.ThirdSpell:
                    _cellIndex = 2;
                    break;
            }

            if(attributes.EnergyPoints < _cellIndex + 1)
            {
                input.lastInputs.RemoveAt(0);
                Debug.Log("Not enough energy to use spell!");
                return;
            }

            _spellTitle = spellBar.spellUI[_cellIndex].title;
            _curCooldown = spellBar.spellUI[_cellIndex].curCooldown.ContainsKey(_spellTitle) ? spellBar.spellUI[_cellIndex].curCooldown[_spellTitle] : 0f;
            input.lastInputs.RemoveAt(0);
            
            if (_curCooldown <= Time.time && _spellTitle != SpellTitles.FreeSlot)
            {
                attributes.EnergyPoints -= _cellIndex + 1;
                curSpell = spellList.spells[_spellTitle].GetComponent<Spell>();
                spellBar.spellUI[_cellIndex].SetCooldown(_spellTitle);
                movement.CastSpell(curSpell.spell.castAnimation);
                Invoke("NextAnimation", curSpell.spell.castTime);
            }
            else
                Debug.Log("SPELL ON CD");//Play CD sound
        }
    }

    void NextAnimation()
    {
        if (!movement.isCast) return;

        attributes.isInvulnerable = true;
        attackNumber++;
        rigidBody.velocity = Vector2.zero;

        switch (curSpell.title)
        {
            case SpellTitles.VoidShift:
                if (attackNumber == 2)
                {
                    movement.canFlip = true;
                    movement.FlipCharacterDirection();
                    movement.canFlip = false;
                }
                break;
        }

        if (attackNumber <= curSpell.spell.attackCount)
            movement.NextSpellAttack(curSpell.spell.attackAnimations[attackNumber - 1]);
        else
        {
            attackNumber = 0;
            attributes.isInvulnerable = false;
            movement.SpellCastEnd();
        }
    }

    void ForceTheCharacter(int attackNumber)
    {
        rigidBody.AddForce(new Vector2(curSpell.spell.forceDirection[attackNumber - 1].x * movement.direction, curSpell.spell.forceDirection[attackNumber - 1].y), ForceMode2D.Impulse);
        Debug.Log("FORCE");
    }

    void GiveSpellDamage(int attackNumber)
    {
        this.attackNumber = attackNumber;
        attackNumber--;
        GameObject _inst = Instantiate(attack.damageBox, transform);
        DamageBox _damageBox = _inst.GetComponent<DamageBox>();
        int _targetLayer = 12;
        PlayerSpell _spell = curSpell.spell;
        Vector2 _boxPosition = Vector2.zero;

        Invoke("NextAnimation", _spell.timeBtwAttack[attackNumber]); // Start next attack after delay

        switch (_spell.type)
        {
            case SpellTypes.Melee:
                _boxPosition = attack.weapon.position;
                break;
            case SpellTypes.MeleeAOE:
                _boxPosition = transform.position;
                break;
            case SpellTypes.Range:
                _boxPosition = attack.weapon.position;
                break;
            case SpellTypes.RangeAOE:
                _boxPosition = new Vector2(attack.weapon.position.x + _spell.castDistance[attackNumber], attack.weapon.position.y);
                break;
        }

        _damageBox.GetParameters(_spell.damage[attackNumber], _spell.damageType[attackNumber], _spell.element, _boxPosition, _spell.damageRange[attackNumber], .3f, null, _targetLayer, attributes);
    }
}

