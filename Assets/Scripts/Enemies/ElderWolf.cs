using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElderWolf : Enemy
{
    //enum SpellEnum { None, LongJump, BackJump, SwingTail, IceBreath };
    //SpellEnum spell = SpellEnum.None;
    string spell = "None";
    public EnemySpellDictionary mySpells = new EnemySpellDictionary();
    /*Dictionary<SpellEnum, EnemySpell> mySpells = new Dictionary<SpellEnum, EnemySpell>
    {
        { SpellEnum.LongJump, new EnemySpell(10f, 5f, 1.5f, 7f, 5f, 2f, 2) },
        { SpellEnum.BackJump, new EnemySpell(-6f, 4f, .5f, 10f, 3f, 2f, 1) },
        { SpellEnum.SwingTail, new EnemySpell() },
        { SpellEnum.IceBreath, new EnemySpell() }
    };*/

    new void Update() 
    {
        base.Update();

        if (Input.GetKeyDown("J"))
        mySpells.Remove("Long Jump");//or mySpells.Remove(SpellEnum.LongJump); DebugRemoveList<EnemySpellDictionary>(ref mySpells);


    }

    new void FixedUpdate() 
    {
        base.FixedUpdate();

        //if(curGlobalSpellCD <= Time.time && )
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
        
        //Wait a delay and then switch state
        if(mySpells[spell].curDelay != 0 && mySpells[spell].curDelay <= Time.time)
        {
            mySpells[spell].CurCooldown = Time.time + mySpells[spell].cooldown;
            mySpells[spell].curDelay = 0;
            mySpells[spell].
            SwitchState(State.Chase);
        }
    }

    void BackJump()
    {
        spellNumber = 2;

        //Wait a delay and then switch state
        if(mySpells[spell].curDelay != 0 && mySpells[spell].curDelay <= Time.time)
        {
            mySpells[spell].curCooldown = Time.time + mySpells[spell].cooldown;
            mySpells[spell].curDelay = 0;
            SwitchState(State.Chase);
        }
    }

    void StartJump()
    {
        rigidBody.AddForce(new Vector2(mySpells[spell].jumpDistance * direction, mySpells[spell].jumpHeight), ForceMode2D.Impulse);
    }

    void JumpDamage()
    {
        mySpells[spell].curDelay = Time.time + mySpells[spell].delayAfterCast;
        LayerMask playerLayer = 1 << 10;       //10 - player layer
        Collider2D objectToDamage = Physics2D.OverlapBox(transform.position, new Vector2(mySpells[spell].damageRangeX, mySpells[spell].damageRangeY), 0, playerLayer);

        if (objectToDamage != null)
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].damage);
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