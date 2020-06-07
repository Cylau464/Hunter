using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class PlayerSpells : MonoBehaviour
{
    [SerializeField] GameObject spellListPrefab = null;
    [HideInInspector] public SpellBar spellBar = null;

    SpellsList spellList;
    PlayerInput input;
    [HideInInspector] public PlayerMovement movement;
    [HideInInspector] public PlayerAttributes attributes;
    PlayerAttack attack;
    Animator anim;
    Spell curSpell;
    [HideInInspector] public int attackCount;
    Rigidbody2D rigidBody;
    [HideInInspector] public Transform myTransform;
    [SerializeField] ChargingBar chargingBar = null;

    Dictionary<SpellTitles, SpellState> spellState = new Dictionary<SpellTitles, SpellState>();
    [HideInInspector] public SpellTitles curSpellTitle = default;
    [HideInInspector] public int cellIndex;
    [HideInInspector] public Dictionary<SpellTitles, int> attackNumber = new Dictionary<SpellTitles, int>();
    bool forceThePlayer;
    public Dictionary<SpellTitles, float> startMovePosition = new Dictionary<SpellTitles, float>(); // player position on X coor before start spell
    [HideInInspector] public float movePosition;
    float forceStartPos;
    float startCastTime;
    [HideInInspector] public float chargingDamageMultiplier;
    //int directionOfPrevForce; // Right now used only for Void Shift
    //GameObject spellPrefab;

    void Start()
    {
        myTransform = transform;
        spellList = spellListPrefab.GetComponent<SpellsList>();
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();
        attack = GetComponent<PlayerAttack>();
        attributes = GetComponent<PlayerAttributes>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!movement.isCast && spellState.ContainsKey(curSpellTitle) && spellState[curSpellTitle] == SpellState.Cast)
        {
            if (movement.isHurt)
                SpellCastCanceled(curSpellTitle);

            return;
        }

        if (curSpell != null && curSpell.spell.useType == SpellUseTypes.Charging && input.spellKeyUp[cellIndex] && spellState.ContainsKey(curSpellTitle) && spellState[curSpellTitle] == SpellState.Cast)
        {
            CancelInvoke("NextAnimation");
            NextAnimation();
            chargingDamageMultiplier = (Time.time - startCastTime) / curSpell.spell.castTime;
            chargingBar.gameObject.SetActive(false);
        }

        if (!movement.isEvading && !movement.isClimbing && !movement.isHanging &&
           input.lastInputs.Count > 0 && (input.lastInputs[0] == InputsEnum.FirstSpell || input.lastInputs[0] == InputsEnum.SecondSpell || input.lastInputs[0] == InputsEnum.ThirdSpell) &&
           attack.attackState != AttackState.Damage)
        {
            float _curCooldown = 0f;
            int _cellIndex = 0;

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

            SpellTitles _spellTitle = spellBar.spellUI[_cellIndex].title;

            if ((spellState.ContainsKey(_spellTitle) && spellState[_spellTitle] != SpellState.Waiting && movement.isCast) ||
                (!spellState.ContainsKey(_spellTitle) && movement.isCast) ||
                (spellState.ContainsKey(_spellTitle) && movement.isCast && _cellIndex != cellIndex))
            {
                return;
            }

            curSpellTitle = _spellTitle;
            cellIndex = _cellIndex;

            if (spellState.ContainsKey(curSpellTitle) && spellState[curSpellTitle] == SpellState.Waiting)
            {
                curSpell = spellList.spells[curSpellTitle].GetComponent<Spell>();
                attackCount = curSpell.spell.attackCount;
                movement.CastSpell(curSpell.spell.attackAnimations[attackNumber[curSpellTitle]]);
                input.lastInputs.RemoveAt(0);
                NextAnimation();
            }
            else
            {
                //// Check to enough energy to use spell
                //if (attributes.EnergyPoints < cellIndex + 1)
                //{
                //    input.lastInputs.RemoveAt(0);
                //    Debug.Log("Not enough energy to use spell!");
                //    return;
                //}

                _curCooldown = spellBar.spellUI[cellIndex].curCooldown.ContainsKey(curSpellTitle) ? spellBar.spellUI[cellIndex].curCooldown[curSpellTitle] : 0f;
                input.lastInputs.RemoveAt(0);

                if (_curCooldown <= Time.time && curSpellTitle != SpellTitles.FreeSlot)
                {
                    spellState.Add(spellBar.spellUI[cellIndex].title, SpellState.Cast);
                    spellBar.spellUI[cellIndex].ChangeState(SpellState.Cast);
                    curSpell = spellList.spells[curSpellTitle].GetComponent<Spell>();
                    attackCount = curSpell.spell.attackCount;
                    movement.CastSpell(curSpell.spell.castAnimation);
                    Invoke("NextAnimation", curSpell.spell.castTime);
                    AudioManager.PlayCastSpellAudio(curSpell.spell.audioCast);
                    attackNumber.Add(curSpellTitle, 0);
                    startCastTime = Time.time;

                    if (curSpell.spell.useType == SpellUseTypes.Charging)
                        chargingBar.SetChargingTime(curSpell.spell.castTime);
                }
                else
                    Debug.Log("SPELL ON CD"); // Play CD sound
            }
        }
    }

    private void FixedUpdate()
    {
        if(forceThePlayer)
        {
            float _moveDistance = Mathf.Abs(movePosition - myTransform.position.x);
            int _moveMultiplier = _moveDistance < .5f ? 0 : (int)Mathf.Sign(Mathf.Floor(_moveDistance * movement.direction));
            rigidBody.velocity = new Vector2(_moveMultiplier * Mathf.Abs(movePosition - forceStartPos)/*curSpell.spell.forceDirection[attackNumber[curSpellTitle] - 1].x*/ / curSpell.spell.timeBtwAttack[attackNumber[curSpellTitle] - 1], curSpell.spell.forceDirection[attackNumber[curSpellTitle] - 1].y);

            if (_moveMultiplier == 0)
                forceThePlayer = false;
        }
    }

    void NextAnimation()
    {
        if (!movement.isCast)
            return;

        //if(attackNumber[curSpellTitle] <= 0)
        //    attributes.EnergyPoints -= cellIndex + 1;

        AudioManager.StopPlayerAudio();
        attributes.isInvulnerable = true;
        spellState[curSpellTitle] = SpellState.Active;
        spellBar.spellUI[cellIndex].ChangeState(SpellState.Active);
        attackNumber[curSpellTitle]++;
        forceThePlayer = false;
        rigidBody.velocity = Vector2.zero;

        curSpell.NextAnimation(this);

        if (attackNumber[curSpellTitle] <= attackCount)
            movement.NextSpellAttack(curSpell.spell.attackAnimations[attackNumber[curSpellTitle] - 1]);
        else
        {
            SpellEnd(curSpellTitle);
        }
    }

    void ForceTheCharacter(int attackNumber)
    {
        this.attackNumber[curSpellTitle] = attackNumber;
        forceThePlayer = true;
        movePosition = myTransform.position.x + curSpell.spell.forceDirection[attackNumber - 1].x * movement.direction;
        forceStartPos = myTransform.position.x;

        curSpell.ForceTheCharacter(this);
    }

    void GiveSpellDamage(int attackNumber)
    {
        this.attackNumber[curSpellTitle] = attackNumber;
        attackNumber--;
        GameObject _inst;
        int _targetLayer = 12;
        PlayerSpell _spell = curSpell.spell;
        Vector2 _boxPosition = Vector2.zero;

        switch (curSpell.spell.useType)
        {
            case SpellUseTypes.Single:
                Invoke("NextAnimation", _spell.timeBtwAttack[attackNumber]); // Start next attack after delay
                break;
            case SpellUseTypes.Charging:
                chargingDamageMultiplier = chargingDamageMultiplier > 0f ? chargingDamageMultiplier : 1f;
                Invoke("NextAnimation", _spell.timeBtwAttack[attackNumber]); // Start next attack after delay
                break;
            case SpellUseTypes.Multiuse:
                StartCoroutine(TimeToNextUseSpell(curSpellTitle, curSpell.spell.timeToUse));
                Invoke("SetSpellToWaitingState", _spell.timeBtwAttack[attackNumber]);
                break;
        }

        switch (_spell.type)
        {
            case SpellTypes.Melee:
                _boxPosition = attack.weapon.position;
                _inst = Instantiate(attack.damageBox, transform);
                DamageBox _damageBox = _inst.GetComponent<DamageBox>();
                _damageBox.GetParameters(_spell.damage[attackNumber], _spell.damageType[attackNumber], _spell.element, _boxPosition, _spell.damageRange[attackNumber], _spell.timeBtwAttack[attackNumber], _spell.audioImpact, _targetLayer, attributes, true);
                break;
            case SpellTypes.MeleeAOE:
                _boxPosition = myTransform.position;
                _inst = Instantiate(_spell.spellPrefab, _boxPosition, _spell.spellPrefab.transform.rotation, myTransform);
                _inst.GetComponent<SpellDamageAOE>().SetParameters(_spell, curSpellTitle, attackNumber);
                break;
            case SpellTypes.Range:
                _boxPosition = attack.weapon.position;
                curSpell.GiveSpellDamage(this, _boxPosition);
                break;
            case SpellTypes.RangeAOE:
                _boxPosition = new Vector2(attack.weapon.position.x + _spell.castDistance[attackNumber], attack.weapon.position.y);
                break;
        }
    }

    void SetSpellToWaitingState()
    {
        forceThePlayer = false;
        spellState[curSpellTitle] = SpellState.Waiting;
        spellBar.spellUI[cellIndex].ChangeState(SpellState.Waiting);

        if (attackNumber.ContainsKey(curSpellTitle) && attackNumber[curSpellTitle] >= attackCount)
        {
            SpellEnd(curSpellTitle);
            return;
        }

        if (input.lastInputs.Count > 0)
        {
            int _cellIndex = 0;

            switch (input.lastInputs[0])
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

            // Multiuse spell continue used
            if (cellIndex == _cellIndex)
                return;
        }

        movement.SpellCastEnd();
    }

    void SpellEnd(SpellTitles spellTitle)
    {
        curSpell.SpellEnd();

        startMovePosition.Remove(spellTitle);
        movePosition = default;
        attackNumber.Remove(spellTitle);
        attributes.isInvulnerable = false;
        movement.SpellCastEnd();
        spellBar.spellUI[cellIndex].SetCooldown(spellTitle);
        spellState.Remove(spellTitle);
        spellBar.spellUI[cellIndex].ChangeState(SpellState.None);
        chargingDamageMultiplier = 0f;
    }

    void SpellCastCanceled(SpellTitles spellTitle)
    {
        spellState.Remove(spellTitle);
        attackNumber.Remove(spellTitle);
        spellBar.spellUI[cellIndex].ChangeState(SpellState.None);
        chargingBar.gameObject.SetActive(false);
    }

    IEnumerator TimeToNextUseSpell(SpellTitles spellTitle, float time)
    {
        yield return new WaitForSeconds(time);

        if (spellState.ContainsKey(spellTitle))
        {
            while (spellState.ContainsKey(spellTitle) && spellState[spellTitle] == SpellState.Active)
                yield return new WaitForEndOfFrame();

            SpellEnd(spellTitle);
        }
    }
}

