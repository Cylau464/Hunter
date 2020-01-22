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
    EnemySpellCD iceSpikesTiming;

    [Header("Spell Properties")]
    [SerializeField] EnemySpellDictionary mySpells = new EnemySpellDictionary()
    {
        { "Long Jump", new EnemySpell(10f, new Vector2(10f, 5f), 1, 10f, 3f, 1.5f, .75f, new Vector2(2f, 2f), new Vector2(5f, 5f), 1.5f, 5, 7) },
        { "Back Jump", new EnemySpell(3f, new Vector2(7f, 4f), -1, 12f, 3f, .5f, .5f, new Vector2(2f, 2f), new Vector2(3f, 0f), 1f, 2, 5) },
        { "Ice Breath", new EnemySpell(7f, 8f, 2f, 1f, .5f, new Vector2(10f, 5f), new Vector2(2f, 0f), .3f, 2, .5f) },
        { "Swing Tail", new EnemySpell(5f, 8f, 2f, 1f, .5f, new Vector2(2f, 2f), new Vector2(5f, 2f), 1.5f, 10) },
        { "Ice Spikes", new EnemySpell(11f, 15f, 2f, 1f, 2f, new Vector2(2f, 5f), new Vector2(4f, 2f), 2f, 15) }
    };
    bool isSpellCasted;
    bool isPlayerCaught;      //Was the player caught

    [SerializeField] Transform frontLegs = null;
    BoxCollider2D frontLegsCol;
    BoxCollider2D tailCol;
    PolygonCollider2D iceBreathCol;
    Collider2D objectToDamage;

    float spellCastRerandomDelay = 3f;
    float curSpellCastRerandomDelay;
    float curSpellDelayBtwDamage;

    new void Awake()
    {
        base.Awake();

        foreach (Transform child in myTransform)
        {
            if (child.name == "Tail")
                tailCol = child.GetComponent<BoxCollider2D>();

            if (child.name == "Ice Breath")
            {
                iceBreathCol = child.GetComponent<PolygonCollider2D>();
                child.GetComponent<IceBreath>().colliderSize = mySpells["Ice Breath"].damageRange;
            }
        }
    }

    void Start()
    {
        //base.Start();

        frontLegsCol = frontLegs.GetComponent<BoxCollider2D>();
    }

    new void Update() 
    {
        base.Update();
        Debug.Log("spell: " + spellNumber);
    }

    new void FixedUpdate() 
    {
        int _rand = 101;

        if (curSpellCastRerandomDelay <= Time.time)
        {
            _rand = Random.Range(1, 101);
            curSpellCastRerandomDelay = spellCastRerandomDelay + Time.time;
        }

        //Cast spell with 33% chance if it's possible
        if (curGlobalSpellCD <= Time.time && target != null && !isAttack && !isCast && _rand <= 33)
        {
            if (IsPlayerBehind())
            {
                if (DistanceToPlayer() <= 5 && swingTailTiming.curCooldown <= Time.time)
                    spell = "Swing Tail";
                else return;
            }
            else
            {
                if (DistanceToPlayer() <= mySpells["Back Jump"].castRange && backJumpTiming.curCooldown <= Time.time)
                    spell = "Back Jump";
                else if (DistanceToPlayer() <= mySpells["Ice Breath"].castRange && iceBreathTiming.curCooldown <= Time.time)
                    spell = "Ice Breath";
                else if (DistanceToPlayer() <= mySpells["Long Jump"].castRange && longJumpTiming.curCooldown <= Time.time)
                    spell = "Long Jump";
                else if (DistanceToPlayer() >= mySpells["Ice Spikes"].castRange && iceSpikesTiming.curCooldown <= Time.time)
                    spell = "Ice Spikes";
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
        if (spellState == SpellStates.Prepare && curSpellPrepareTime <= Time.time)
        {
            spellState = SpellStates.Cast;
            curSpellCastTime = mySpells[spell].castTime + Time.time;
        }

        if (spellState == SpellStates.Cast && curSpellCastTime <= Time.time)
            spellState = SpellStates.End;

        switch (spell)
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
            case "Ice Spikes":
                IceSpikes();
                break;
        }
    }
    
    void LongJump()
    {
        spellNumber = 1;

        if (objectToDamage == null)
            objectToDamage = Physics2D.OverlapBox(frontLegs.position, new Vector2(frontLegsCol.size.x, frontLegsCol.size.y), 0f, playerLayer);
        //If player collided with front legs
        else if (!isPlayerCaught && spellState != SpellStates.Prepare)
        {
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].firstDamage, HurtType.Catch, frontLegs);
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

        if (isSpellCasted)
        {
            objectToDamage = null;
            isSpellCasted = false;
            curAttackCD = attackCD + Time.time;
            //backJumpTiming.curDelay = mySpells[(Enemy.SpellEnum) spell].delayAfterCast + Time.time;
            backJumpTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SwitchState(State.Attack);
        }
    }

    void SwingTail()
    {
        spellNumber = 3;

        if (isSpellCasted)
        {
            objectToDamage = null;
            isSpellCasted = false;
            curAttackCD = attackCD + Time.time;
            //backJumpTiming.curDelay = mySpells[(Enemy.SpellEnum) spell].delayAfterCast + Time.time;
            backJumpTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SwitchState(State.Attack);
        }
    }

    void IceBreath()
    {
        spellNumber = 4;

        if (isSpellCasted)
        {
            objectToDamage = null;
            isSpellCasted = false;
            curAttackCD = attackCD + Time.time;
            //backJumpTiming.curDelay = mySpells[(Enemy.SpellEnum) spell].delayAfterCast + Time.time;
            backJumpTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SwitchState(State.Attack);
        }
    }

    void IceSpikes()
    {
        spellNumber = 5;

        if (isSpellCasted)
        {
            objectToDamage = null;
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
        rigidBody.AddForce(new Vector2(mySpells[spell].jumpDistance.x * mySpells[spell].jumpDirection * direction, mySpells[spell].jumpDistance.y), ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
       Gizmos.color = Color.red;
        if (frontLegsCol != null)
        Gizmos.DrawWireCube(frontLegs.position, new Vector3(frontLegsCol.size.x * 2f, frontLegsCol.size.y, 0f));
    }

    void Landing()
    {
        rigidBody.velocity = Vector2.zero;

        objectToDamage = Physics2D.OverlapBox(frontLegs.position, new Vector2(frontLegsCol.size.x * 2f, frontLegsCol.size.y), 0f, playerLayer);

        if (objectToDamage != null)
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].lastDamage, HurtType.Repulsion, mySpells[spell].repulseVector, mySpells[spell].dazedTime);

        isSpellCasted = true;
    }

    void SpellOnceDamage()
    {
        objectToDamage = Physics2D.OverlapBox(tailCol.transform.position, new Vector2(tailCol.size.x, tailCol.size.y), 0, playerLayer);

        if (objectToDamage != null)
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].firstDamage, HurtType.Repulsion, new Vector2(mySpells[spell].repulseVector.x * -direction, mySpells[spell].repulseVector.y), mySpells[spell].dazedTime);
    }

    void SpellPeriodicDamage()
    {
        if(curSpellDelayBtwDamage <= Time.time)
        {
            objectToDamage = iceBreathCol.GetComponent<IceBreath>().playerCol;

            if (objectToDamage != null)
                objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].firstDamage, HurtType.Repulsion, new Vector2(mySpells[spell].repulseVector.x * direction, mySpells[spell].repulseVector.y), mySpells[spell].dazedTime);

            curSpellDelayBtwDamage = mySpells[spell].periodicityDamage + Time.time;
        }
    }

    void SpellEnded()
    {
        isSpellCasted = true;
    }
}