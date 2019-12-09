using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElderWolf : Enemy
{
    enum Spell { None, LongJump, BackJump, SwingTail, IceBreath };
    Spell spell = Spell.None;

    object[] spells = {  };

    [SerializeField] float longJumpDistance = 10f;
    [SerializeField] float longJumpHeight = 5f;
    [SerializeField] float longJumpDelay = 1.5f;                      //Delay after jump
    float curLongJumpDelay;
    [SerializeField] float longJumpCD = 7f;
    float curLongJumpCD;
    [SerializeField] float longJumpRangeX = 5f;
    [SerializeField] float longJumpRangeY = 2f;

    [SerializeField] float backJumpDistance = 6f;
    [SerializeField] float backJumpHeight = 4f;
    [SerializeField] float backJumpDelay = .5f;                      //Delay after jump
    float curBackJumpDelay;
    [SerializeField] float backJumpCD = 10f;
    float curBackJumpCD;
    [SerializeField] float backJumpRangeX = 3f;
    [SerializeField] float backJumpRangeY = 2f;

    override protected void CastSpell()
    {
        switch(spell)
        {
            case Spell.None:
                Debug.LogError("Enemy try cast spell with none value. Enemy ID: " + gameObject.GetInstanceID());
                break;
            case Spell.LongJump:
                LongJump();
                break;
            case Spell.BackJump:
                BackJump();
                break;
            case Spell.SwingTail:
                SwingTail();
                break;
            case Spell.IceBreath:
                IceBreath();
                break;
        }
    }

    void LongJump()
    {
        spellNumber = 1;
        
        //Wait a delay and then switch state
        if(curLongJumpDelay != 0 && curLongJumpDelay <= Time.time)
        {
            SwitchState(State.Chase);
            curLongJumpDelay = 0;
        }
    }

    void BackJump()
    {
        spellNumber = 2;

        //Wait a delay and then switch state
        if(curBackJumpDelay != 0 && curBackJumpDelay <= Time.time)
        {
            SwitchState(State.Chase);
            curBackJumpDelay = 0;
        }
    }

    void StartJump()
    {
        if(spell == Spell.LongJump)
            rigidBody.AddForce(new Vector2(longJumpDistance * direction, longJumpHeight), ForceMode2D.Impulse);
        else if(spell == Spell.BackJump)
            rigidBody.AddForce(new Vector2(backJumpDistance * -direction, backJumpHeight), ForceMode2D.Impulse);
    }

    void JumpDamage(Spell usedSpell)
    {
        foreach()
        if(spell == Spell.LongJump)
        {
            curLongJumpDelay = Time.time + longJumpDelay;
            curLongJumpCD = Time.time + longJumpCD;
        }
        else if(spell == Spell.BackJump)
        {
            curBackJumpDelay = Time.time + backJumpDelay;
            curBackJumpCD = Time.time + backJumpCD;
        }

        LayerMask playerLayer = 1 << 10;       //10 - player layer

        Collider2D objectToDamage = Physics2D.OverlapBox(transform.position, new Vector2(1, 1), 0, playerLayer);

        if (objectToDamage != null)
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(damage);

    }

    void SwingTail()
    {
        spellNumber = 3;
    }

    void IceBreath()
    {
        spellNumber = 4;
    }
}