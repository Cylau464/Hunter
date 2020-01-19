using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class ElderWolf : Enemy
{
    EnemySpellCD longJumpTiming;
    EnemySpellCD backJumpTiming;
    EnemySpellCD swingTailTiming;
    EnemySpellCD iceBreathTiming;

    [Header("Spell Properties")]
    [SerializeField] EnemySpellDictionary mySpells = new EnemySpellDictionary()
    {
        { "Long Jump", new EnemySpell(10f, 5f, 1, 10f, 2f, 1.5f, .75f, 2f, 2f, new Vector2(5f, 5f), 1.5f, 5, 7) },
        { "Back Jump", new EnemySpell(0f, 0f, 1, 0f, 0f, 0f, 0f, 0f, 0f, Vector2.zero, 0f, 0, 0) },
        { "Ice Breath", new EnemySpell(0f, 0f, 1, 0f, 0f, 0f, 0f, 0f, 0f, Vector2.zero, 0f, 0, 0) },
        { "Swing Tail", new EnemySpell(0f, 0f, 1, 0f, 0f, 0f, 0f, 0f, 0f, Vector2.zero, 0f, 0, 0) }
    };
    bool isSpellCasted;
    bool isPlayerCaught;      //Was the player caught

    string spell = "None";

    [SerializeField] Transform frontLegs = null;
    BoxCollider2D frontLegsCol;
    Collider2D objectToDamage;

    float spellCastRerandomDelay = 3f;
    float curSpellCastRerandomDelay;

    new void Start()
    {
        base.Start();

        frontLegsCol = frontLegs.GetComponent<BoxCollider2D>();
    }

    new void Update() 
    {
        base.Update();
    }

    new void FixedUpdate() 
    {
        int _rand = 101;

        if (curSpellCastRerandomDelay <= Time.time)
        {
            _rand = Random.Range(1, 101);
            curSpellCastRerandomDelay = spellCastRerandomDelay + Time.time;
        }

        Debug.Log(_rand);
        //Cast spell with 33% chance if it's possible
        if (curGlobalSpellCD <= Time.time && target != null && !isAttack && !isCast && _rand <= 33)
        {
            if (IsPlayerBehind())
                spell = "Swing Tail";
            else
            {
                if (DistanceToPlayer() <= 5f && backJumpTiming.curCooldown <= Time.time)
                    spell = "Back Jump";
                else if (DistanceToPlayer() <= 7f && iceBreathTiming.curCooldown <= Time.time)
                    spell = "Ice Breath";
                else if (DistanceToPlayer() <= mySpells["Long Jump"].jumpDistance && longJumpTiming.curCooldown <= Time.time)
                    spell = "Long Jump";
                else return;
            }

            //rigidBody.velocity = Vector2.zero;
            curSpellPrepareTime = mySpells[spell].prepareTime + Time.time;
            SwitchState(State.CastSpell);
            spellState = SpellStates.Prepare;
        }

        base.FixedUpdate();
    }
    
    override protected void CastSpell()
    {
        switch(spell)
        {
            case "None":
                Debug.LogError("Enemy try cast spell with none value. Enemy ID: " + gameObject.GetInstanceID());
                break;
            case "Long Jump":
                LongJump();
                break;
            case "Back Jump":
                BackJump();
                break;
            case "Swing Tail":
                SwingTail();
                break;
            case "Ice Breath":
                IceBreath();
                break;
        }
    }
    
    void LongJump()
    {
        spellNumber = 1;

        if (spellState == SpellStates.Prepare && curSpellPrepareTime <= Time.time)
        {
            spellState = SpellStates.Cast;
            curSpellCastTime = mySpells[spell].castTime + Time.time;
        }

        if (spellState == SpellStates.Cast && curSpellCastTime <= Time.time)
            spellState = SpellStates.End;

        if (objectToDamage == null)
            objectToDamage = Physics2D.OverlapBox(frontLegs.position, new Vector2(frontLegsCol.size.x, frontLegsCol.size.y), 0f, playerLayer);
        //If player collided with front legs
        else if (!isPlayerCaught && spellState != SpellStates.Prepare)
        {
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].firstDamage, HurtTypesEnum.Catch, frontLegs);
            isPlayerCaught = true;
        }

        if (isSpellCasted)
        {
            objectToDamage = null;
            isPlayerCaught = false;
            isSpellCasted = false;
            curAttackCD = attackCD + Time.time;
            //longJumpTiming.curDelay = mySpells[(Enemy.SpellEnum) spell].delayAfterCast + Time.time;
            longJumpTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SwitchState(State.Attack);
        }
    }

    void BackJump()
    {
        spellNumber = 2;

        //Wait a delay and then switch state
        if(isSpellCasted/* && backJumpTiming.curDelay != 0 && backJumpTiming.curDelay <= Time.time*/)
        {
            objectToDamage = null;
            isPlayerCaught = false;
            isSpellCasted = false;
            curAttackCD = attackCD + Time.time;
            //backJumpTiming.curDelay = mySpells[(Enemy.SpellEnum) spell].delayAfterCast + Time.time;
            backJumpTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SwitchState(State.Attack);
        }
    }

    void StartJump()
    {
        rigidBody.AddForce(new Vector2(mySpells[spell].jumpDistance * mySpells[spell].jumpDirection * direction, mySpells[spell].jumpHeight), ForceMode2D.Impulse);
    }

    void Landing()
    {
        rigidBody.velocity = Vector2.zero;
        //Give AOE damage
        objectToDamage = Physics2D.OverlapBox(frontLegs.position, new Vector2(frontLegsCol.size.x * 2f, frontLegsCol.size.y), 0f, playerLayer);

        if (objectToDamage != null)
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].lastDamage, HurtTypesEnum.Repulsion, mySpells[spell].repulseVector, mySpells[spell].dazedTime);
        
        isSpellCasted = true;
    }

    //void JumpDamage()
    //{
    //    mySpells[spell].curDelay = Time.time + mySpells[spell].delayAfterCast;
    //    LayerMask playerLayer = 1 << 10;       //10 - player layer
    //    Collider2D objectToDamage = Physics2D.OverlapBox(transform.position, new Vector2(mySpells[spell].damageRangeX, mySpells[spell].damageRangeY), 0, playerLayer);

    //    if (objectToDamage != null)
    //        objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].damage);
    //}

    void SwingTail()
    {
        spellNumber = 3;
    }

    void IceBreath()
    {
        spellNumber = 4;
    }
}