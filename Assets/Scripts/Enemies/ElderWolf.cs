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
    new enum SpellEnum { None, LongJump, BackJump, SwingTail, IceBreath };
    SpellEnum spell = SpellEnum.None;
    //string spell = "None";
    public EnemySpellDictionary mySpells = new EnemySpellDictionary();
    /*Dictionary<SpellEnum, EnemySpell> mySpells = new Dictionary<SpellEnum, EnemySpell>
    {
        { SpellEnum.LongJump, new EnemySpell(10f, 5f, 1.5f, 7f, 5f, 2f, 2) },
        { SpellEnum.BackJump, new EnemySpell(-6f, 4f, .5f, 10f, 3f, 2f, 1) },
        { SpellEnum.SwingTail, new EnemySpell() },
        { SpellEnum.IceBreath, new EnemySpell() }
    };*/

    bool isSpellCasted;
    bool isPlayerCaught;      //Was the player caught

    Transform frontLegs;
    BoxCollider2D frontLegsCol;
    Collider2D objectToDamage;

    new void Start()
    {
        base.Start();

        frontLegs = myTransform.Find("Front Legs");
        frontLegsCol = frontLegs.GetComponent<BoxCollider2D>();
    }

    new void Update() 
    {
        base.Update();

        if (Input.GetKeyDown("J"))
        mySpells.Remove((Enemy.SpellEnum) SpellEnum.LongJump);//or mySpells.Remove(SpellEnum.LongJump);


    }

    new void FixedUpdate() 
    {
        base.FixedUpdate();

        //Cast spell with 33% chance if it's possible
        if(curGlobalSpellCD <= Time.time && target != null && !isAttack && Random.Range(0, 2) == 2)
        {
            if(IsPlayerBehind())
                spell = SpellEnum.SwingTail;
            else
            {
                if (DistanceToPlayer() <= 5f && backJumpTiming.curCooldown <= Time.time)
                    spell = SpellEnum.BackJump;
                else if (DistanceToPlayer() <= 15f && iceBreathTiming.curCooldown <= Time.time)
                    spell = SpellEnum.IceBreath;
                else if (DistanceToPlayer() <= mySpells[(Enemy.SpellEnum) SpellEnum.LongJump].jumpDistance && longJumpTiming.curCooldown <= Time.time)
                    spell = SpellEnum.LongJump;
                else return;
            }

            SwitchState(State.CastSpell);
        }
    }
    
    override protected void CastSpell()
    {
        switch(spell)
        {
            case SpellEnum.None:
                Debug.LogError("Enemy try cast spell with none value. Enemy ID: " + gameObject.GetInstanceID());
                break;
            case SpellEnum.LongJump:
                LongJump();
                break;
            case SpellEnum.BackJump:
                BackJump();
                break;
            case SpellEnum.SwingTail:
                SwingTail();
                break;
            case SpellEnum.IceBreath:
                IceBreath();
                break;
        }
    }
    
    void LongJump()
    {
        spellNumber = 1;
        //Повесить начало прыжка (StartJump()) на начало анимации
        if (objectToDamage == null)
            objectToDamage = Physics2D.OverlapBox(frontLegs.position, new Vector2(frontLegsCol.size.x, frontLegsCol.size.y), 0f, playerLayer);
        //If player collided with front legs
        else if (!isPlayerCaught)
        {
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[(Enemy.SpellEnum)spell].firstDamage, HurtTypesEnum.Catch, frontLegs);
            isPlayerCaught = true;
        }

        if (isSpellCasted)
        {
            objectToDamage = null;
            isPlayerCaught = false;
            curAttackCD = attackCD + Time.time;
            //longJumpTiming.curDelay = mySpells[(Enemy.SpellEnum) spell].delayAfterCast + Time.time;
            longJumpTiming.curCooldown = mySpells[(Enemy.SpellEnum) spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[(Enemy.SpellEnum) spell].globalCD + Time.time;
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
            curAttackCD = attackCD + Time.time;
            //backJumpTiming.curDelay = mySpells[(Enemy.SpellEnum) spell].delayAfterCast + Time.time;
            backJumpTiming.curCooldown = mySpells[(Enemy.SpellEnum) spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[(Enemy.SpellEnum)spell].globalCD + Time.time;
            SwitchState(State.Attack);
        }
    }

    void StartJump()
    {
        rigidBody.AddForce(new Vector2(mySpells[(Enemy.SpellEnum) spell].jumpDistance * mySpells[(Enemy.SpellEnum)spell].jumpDirection, mySpells[(Enemy.SpellEnum)spell].jumpHeight), ForceMode2D.Impulse);
    }

    void Landing()
    {
        //Calling from end of animation
        //Give AOE damage
        objectToDamage = Physics2D.OverlapBox(frontLegs.position, new Vector2(frontLegsCol.size.x * 2f, frontLegsCol.size.y), 0f, playerLayer);

        if (objectToDamage != null)
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[(Enemy.SpellEnum)spell].lastDamage, HurtTypesEnum.Repulsion, mySpells[(Enemy.SpellEnum)spell].repulseVector, mySpells[(Enemy.SpellEnum)spell].dazedTime);
        
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