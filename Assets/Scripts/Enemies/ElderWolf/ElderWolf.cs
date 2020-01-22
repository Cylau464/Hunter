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
    EnemySpellCD howlTiming;

    /*
    combos = new EnemyComboDictionary()
    {
        { "One Attack", new EnemyCombo(1, WeaponAttackType.Melee, 30, new int[] { 3 }, new float[] { 0f }, new float[] { .2f }, 2f, new Vector2[] { new Vector2(5f, 0f) }) },
        { "Two Fast Attack", new EnemyCombo(2, WeaponAttackType.Melee, 25, new int[] { 2, 2 }, new float[] { .2f, 0f }, new float[] { .2f, .2f }, 2f, new Vector2[] { new Vector2(1f, 0f), new Vector2(1f, 0f) }) },
        { "Two Slow Attack", new EnemyCombo(2, WeaponAttackType.Melee, 25, new int[] { 3, 3 }, new float[] { .4f, 0f }, new float[] { .3f, .3f }, 3f, new Vector2[] { new Vector2(1f, 0f), new Vector2(3f, 0f) }) },
        { "Three Attack", new EnemyCombo(3, WeaponAttackType.Melee, 20, new int[] { 2, 2, 4 }, new float[] { .2f, .3f, 0f }, new float[] { .2f, .2f, .4f }, 4f, new Vector2[] { new Vector2(1f, 0f), new Vector2(2f, 0f), new Vector2(4f, 0f) }) },
    };*/

    [Header("Spell Properties")]
    [SerializeField] EnemySpellDictionary mySpells = new EnemySpellDictionary()
    {
        { "Long Jump", new EnemySpell(10f, new Vector2(10f, 5f), 1, 10f, 3f, 1.5f, .75f, new Vector2(2f, 2f), new Vector2(5f, 5f), 1.5f, 5, 7) },
        { "Back Jump", new EnemySpell(3f, new Vector2(7f, 4f), -1, 12f, 3f, .5f, .5f, new Vector2(2f, 2f), new Vector2(3f, 0f), 1f, 2, 5) },
        { "Ice Breath", new EnemySpell(7f, 8f, 2f, 1f, .5f, new Vector2(10f, 5f), new Vector2(2f, 0f), .3f, 2, .5f) },
        { "Swing Tail", new EnemySpell(5f, 8f, 2f, 1f, .5f, new Vector2(2f, 2f), new Vector2(5f, 2f), 1.5f, 10) },
        { "Ice Spikes", new EnemySpell(11f, 15f, 2f, 1f, 2f, new Vector2(2f, 5f), new Vector2(4f, 2f), 2f, 15) },
        { "Howl", new EnemySpell(11f, 15f, 2f, 1f, 2f, new Vector2(2f, 5f), new Vector2(4f, 2f), 2f, 15) }
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
        int _rand = 100;

        if (curSpellCastRerandomDelay <= Time.time)
        {
            _rand = Random.Range(0, 100);
            curSpellCastRerandomDelay = spellCastRerandomDelay + Time.time;
        }

        //Cast spell with 33% chance if it's possible
        if (curGlobalSpellCD <= Time.time && target != null && !isAttack && !isCast && _rand <= 32)
        {
            if (IsPlayerBehind())
            {
                if (DistanceToPlayer() <= mySpells["Swing Tail"].castRange && swingTailTiming.curCooldown <= Time.time)
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
            case "Howl":
                Howl();
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
            swingTailTiming.curCooldown = mySpells[spell].cooldown + Time.time;
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
            iceBreathTiming.curCooldown = mySpells[spell].cooldown + Time.time;
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
            iceSpikesTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SwitchState(State.Attack);
        }
    }

    void Howl()
    {
        spellNumber = 6;

        if (isSpellCasted)
        {
            objectToDamage = null;
            isSpellCasted = false;
            curAttackCD = attackCD + Time.time;
            //backJumpTiming.curDelay = mySpells[(Enemy.SpellEnum) spell].delayAfterCast + Time.time;
            howlTiming.curCooldown = mySpells[spell].cooldown + Time.time;
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

    //Attacks:
    //  One slow attack
    //  Two fast attacks 
    //  Two normal attacks
    //  Three attacks
    //Patterns:
    //Back Jump:
    //  After take many damage in last seconds and player in front of the wolf (50% chance)
    //  Two fast attack combo if one of them hit the player then Back Jump and Ice Breath
    //Long Jump:
    //  If player on cast range between Long Jump and Ice Spikes
    //  When player jump (50% chance)
    //Ice Breath:
    //  Two fast attack combo if one of them hit the player then Back Jump and Ice Breath
    //  If one slow attack hit player (and repulse him from wolf)
    //  After Long Jump if player get caught (50% chance)
    //  Player in cast range (30% chance)
    //Swing Tail:
    //  Player behind wolf in cast range
    //Ice Spikes:
    //  After Long Jump if player get caught (50% chance)
    //  Player out in cast range
    //  Three-attack combo ends with Ice Spikes
    //Howl:
    //  After take many damage in last seconds (50% chance if player in front of the wolf)
    //  If wolf dont hit player more few seconds
}