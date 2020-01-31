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
    EnemySpellCD knockbackTiming;

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
        { "Long Jump",  new EnemySpell(10f, new Vector2(10f, 5f), 1, 10f, 3f, 1.5f, .75f, new Vector2(2f, 2f), new Vector2(5f, 5f), 1.5f, 5, 7) },
        { "Back Jump",  new EnemySpell(3f, new Vector2(7f, 4f), -1, 12f, 3f, .5f, .5f, new Vector2(2f, 2f), new Vector2(3f, 0f), 1f, 2, 5) },
        { "Ice Breath", new EnemySpell(7f, 8f, 2f, 1f, .5f, new Vector2(10f, 5f), new Vector2(2f, 0f), .3f, 2, .5f) },
        { "Swing Tail", new EnemySpell(5f, 8f, 2f, 1f, .5f, new Vector2(2f, 2f), new Vector2(5f, 2f), 1.5f, 10) },
        { "Ice Spikes", new EnemySpell(11f, 15f, 2f, 1f, 2f, new Vector2(2f, 5f), new Vector2(4f, 2f), 2f, 15) },
        { "Howl",       new EnemySpell(15f, 15f, 3f, 1.5f, 3f, new Vector2(0f, 0f), new Vector2(2f, 0f), .5f, 0, .5f) },
        { "Knockback",  new EnemySpell(2.5f, new Vector2(0f, 5f), 1, 5f, .5f, .5f, 1f, new Vector2(6f, 2f), new Vector2(6f, 6f), 1.5f, 2, 5) },
    };
    bool isSpellCasted;

    [SerializeField] Transform frontLegs = null;
    [SerializeField] Transform head = null;
    [SerializeField] Transform tail = null;
    BoxCollider2D tailCol;
    PolygonCollider2D iceBreathCol;
    Collider2D objectToDamage;

    [SerializeField] float minGlobalCD = .5f;
    float maxDurationWithoutDamage = 10f;       //For Howl and other spells which used if Wolf can't give damage a long time
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

    private void OnGUI()
    {
        GUI.TextField(new Rect(10, 300, 150, 200), "back: " + backJumpTiming.curCooldown + "\nLong: " + longJumpTiming.curCooldown + "\nBreath: " + iceBreathTiming.curCooldown + "\nSpikes: " + iceSpikesTiming.curCooldown + "\nTail: " + swingTailTiming.curCooldown + "\nHowl: " + howlTiming.curCooldown + "\nKnock: " + knockbackTiming.curCooldown);
    }

    new void Update() 
    {
        base.Update();
        Debug.Log("spell: " + spell);


        //Sets two of three spells on cooldown
        if (!target)
        {
            int a = Random.Range(0, 3);

            switch (a)
            {
                case 0:
                    longJumpTiming.curCooldown = mySpells["Long Jump"].cooldown + Time.time;
                    iceBreathTiming.curCooldown = 0f;
                    iceSpikesTiming.curCooldown = mySpells["Ice Spikes"].cooldown + Time.time;
                    break;
                case 1:
                    longJumpTiming.curCooldown = 0f;
                    iceBreathTiming.curCooldown = mySpells["Ice Breath"].cooldown + Time.time;
                    iceSpikesTiming.curCooldown = mySpells["Ice Spikes"].cooldown + Time.time;
                    break;
                case 2:
                    longJumpTiming.curCooldown = mySpells["Long Jump"].cooldown + Time.time;
                    iceBreathTiming.curCooldown = mySpells["Ice Breath"].cooldown + Time.time;
                    iceSpikesTiming.curCooldown = 0f;
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            GameObject _spawner = Resources.Load<GameObject>("Enemies/Elder Wolf/Ice Spike Spawner");
            GameObject _inst = Instantiate(_spawner, myTransform);
            IceSpikesSpawner _iss = _inst.GetComponent<IceSpikesSpawner>();
            _iss.spell = mySpells["Ice Spikes"];
            _iss.player = playerMovement == null ? GameObject.Find("Player").GetComponent<PlayerMovement>() : playerMovement;
        }
    }

    override protected void SwitchSpell()
    {
        //Cast spell if it's possible
        if (curGlobalSpellCD <= Time.time && target != null && !isAttack && !isCast)
        {
            bool spellSelected = false;

            if (knockbackTiming.curCooldown <= Time.time && DistanceToPlayer().x < mySpells["Knockback"].castRange && DistanceToPlayer().y < 2f)
            {
                spell = "Knockback";
                spellSelected = true;
            }
            else
            { 
                if (IsPlayerBehind())
                {
                    //Swing Tail:
                    //  Player behind wolf in cast range
                    if (swingTailTiming.curCooldown <= Time.time)
                    {
                        if (DistanceToPlayer().x <= mySpells["Swing Tail"].castRange && DistanceToPlayer().y < 2f)

                            spell = "Swing Tail";
                    }
                    else return;
                }
                else
                {
                    //Back Jump:
                    //  After take many damage in last seconds and player in front of the wolf Back Jump -> Howl (50% chance)
                    //  Two fast attack combo if one of them hit the player then Back Jump and Ice Breath
                    if (!spellSelected && backJumpTiming.curCooldown <= Time.time)
                    {
                        if ((lastAttack == "Two Fast Attack" && isHitPlayer) ||
                            (damageTakenDPS >= maxHealth / 100 * 5 && Random.Range(0, 100) > 50))
                        {
                            spell = "Back Jump";
                            spellSelected = true;
                        }
                    
                    }
                    //Ice Breath:
                    //  Two fast attack combo if one of them hit the player then Back Jump and Ice Breath
                    //  If one slow attack hit player (and repulse him from wolf)
                    //  After Long Jump if player get caught (50% chance)
                    //  Player in cast range (30% chance)
                    if (!spellSelected && iceBreathTiming.curCooldown <= Time.time)
                    {
                        if ((lastAttack == "Two Fast Attack" && lastSpell == "Back Jump") ||
                             lastAttack == "One Attack" ||
                            (lastSpell  == "Long Jump" && Random.Range(0, 100) > 50) ||
                             DistanceToPlayer().x <= mySpells["Ice Breath"].castRange && Random.Range(0, 100) > 30)
                        {
                            spell = "Ice Breath";
                            spellSelected = true;
                        }
                    }
                    //Long Jump:
                    //  If player on cast range between Long Jump and Ice Spikes
                    //  When player jump (50% chance)
                    if (!spellSelected && longJumpTiming.curCooldown <= Time.time)
                    {
                        if (DistanceToPlayer().x <= mySpells["Long Jump"].castRange ||
                           ((!playerMovement.isOnGround ||
                            playerMovement.isJumping) && Random.Range(0, 100) > 50))
                        {
                            spell = "Long Jump";
                            spellSelected = true;
                        }
                    }
                    //Ice Spikes:
                    //  After Long Jump if player get caught (50% chance)
                    //  Player out in cast range
                    //  Three-attack combo ends with Ice Spikes
                    if (!spellSelected && iceSpikesTiming.curCooldown <= Time.time)
                    {
                        if ((lastSpell == "Long Jump" && playerMovement.hurtType == HurtType.Catch /*&& Random.Range(0, 100) > 50*/) ||
                            DistanceToPlayer().x >= mySpells["Ice Spikes"].castRange ||
                            lastAttack == "Three Attack")
                        {
                            spell = "Ice Spikes";
                            spellSelected = true;
                        }
                    }
                    //Howl:
                    //  After take many damage in last seconds and player in front of the wolf Back Jump -> Howl (50% chance)
                    //  If wolf dont hit player more few seconds
                    if (!spellSelected && howlTiming.curCooldown <= Time.time)
                    {
                        if ((lastSpell == "Back Jump" && damageTakenDPS >= maxHealth / 100 * 5 && Random.Range(0, 100) > 50) ||
                           playerAtributes.timeOfLastTakenDamage + maxDurationWithoutDamage <= Time.time)
                        {
                            spell = "Howl";
                            spellSelected = true;
                        }
                    }
                }

                if(!spellSelected) return;
            }

            if (spell != "None")
            {
                lastSpell = spell;
                curSpellPrepareTime = mySpells[spell].prepareTime + Time.time;
                SwitchState(State.CastSpell);
                spellState = SpellStates.Prepare;
            }
        }
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
            case "Knockback":
                Knockback();
                break;
        }
    }

    void SpellCasted()
    {
        objectToDamage = null;
        isSpellCasted = false;
        curAttackCD = mySpells[spell].globalCD + Time.time;
        spell = "None";
        SwitchState(State.Attack);
    }
    
    void LongJump()
    {
        spellNumber = 1;

        if (isSpellCasted)
        {
            longJumpTiming.curCooldown = mySpells[spell].cooldown + Time.time;      //Переделать переменные кулдауна в один словарь с индексом спелла
            curGlobalSpellCD = (Random.Range(0, 2) == 0 ? mySpells[spell].globalCD : minGlobalCD) + Time.time;
            SpellCasted();
            return;
        }

        if (!frontLegs.gameObject.activeSelf && spellState == SpellStates.Cast)
        {
            EWLongJump _longJump;
            frontLegs.gameObject.SetActive(true);
            _longJump = frontLegs.GetComponent<EWLongJump>();
            _longJump.damageVector = mySpells[spell].damageRange;
            _longJump.damage = mySpells[spell].firstDamage;
        }
    }

    void BackJump()
    {
        spellNumber = 2;

        if (isSpellCasted)
        {
            backJumpTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SpellCasted();
        }
    }

    void SwingTail()
    {
        spellNumber = 3;

        if (isSpellCasted)
        {
            swingTailTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SpellCasted();
        }
    }

    void IceBreath()
    {
        spellNumber = 4;

        if (isSpellCasted)
        {
            iceBreathTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SpellCasted();
        }
    }

    void IceSpikes()
    {
        spellNumber = 5;

        if (isSpellCasted)
        {
            iceSpikesTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SpellCasted();
        }
    }

    void Howl()
    {
        spellNumber = 6;

        if (isSpellCasted)
        {
            howlTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SpellCasted();
        }
    }

    void Knockback()
    {
        spellNumber = 7;

        if(isSpellCasted)
        {
            knockbackTiming.curCooldown = mySpells[spell].cooldown + Time.time;
            curGlobalSpellCD = mySpells[spell].globalCD + Time.time;
            SpellCasted();
        }
    }

    void StartJump()
    {
        rigidBody.AddForce(new Vector2(mySpells[spell].jumpDistance.x * mySpells[spell].jumpDirection * direction, mySpells[spell].jumpDistance.y), ForceMode2D.Impulse);

        if (spell == "Knockback")
            SpellOnceDamage();
    }

    new private void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        /*
        if (frontLegsCol != null)
            Gizmos.DrawWireCube(frontLegs.position, new Vector3(frontLegsCol.size.x * 2f, frontLegsCol.size.y, 0f));*/
    }

    void Landing()
    {
        rigidBody.velocity = Vector2.zero;
        frontLegs.gameObject.SetActive(false);

        objectToDamage = Physics2D.OverlapBox(myTransform.position, mySpells[spell].damageRange, 0f, playerLayer);

        if (objectToDamage != null)
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].lastDamage, HurtType.Repulsion, new Vector2(mySpells[spell].repulseVector.x * direction, mySpells[spell].repulseVector.y), mySpells[spell].dazedTime);

        isSpellCasted = true;
    }

    void SpellOnceDamage()
    {
        switch (spell)
        {
            case "Ice Spikes":
                GameObject _spawner = Resources.Load<GameObject>("Enemies/Elder Wolf/Ice Spike Spawner");
                GameObject _inst = Instantiate(_spawner, myTransform);
                IceSpikesSpawner _iss = _inst.GetComponent<IceSpikesSpawner>();
                _iss.spell = mySpells[spell];
                _iss.player = playerMovement == null ? GameObject.Find("Player").GetComponent<PlayerMovement>() : playerMovement;
                break;
            case "Swing Tail":
                objectToDamage = Physics2D.OverlapBox(tailCol.transform.position, new Vector2(tailCol.size.x, tailCol.size.y), 0, playerLayer);

                if (objectToDamage != null)
                    objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].firstDamage, HurtType.Repulsion, new Vector2(mySpells[spell].repulseVector.x * -direction, mySpells[spell].repulseVector.y), mySpells[spell].dazedTime);

                break;
            case "Howl":
                HowlCirclePulse _hcp = head.GetComponent<HowlCirclePulse>();
                _hcp.spell = mySpells[spell];
                _hcp.StartCoroutine(_hcp.CirclePulse());
                break;
            case "Knockback":
                objectToDamage = Physics2D.OverlapBox(myTransform.position, mySpells[spell].damageRange, 0, playerLayer);

                if (objectToDamage != null)
                    objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].firstDamage, HurtType.Repulsion, new Vector2(mySpells[spell].repulseVector.x * -direction, mySpells[spell].repulseVector.y), mySpells[spell].dazedTime / 3f);

                break;
        }
    }

    void SpellPeriodicDamage()
    {
        if (spell == "Ice Breath")
        {
            if (curSpellDelayBtwDamage <= Time.time)
            {
                objectToDamage = iceBreathCol.GetComponent<IceBreath>().playerCol;

                if (objectToDamage != null)
                { 
                    objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(mySpells[spell].firstDamage, HurtType.Repulsion, new Vector2(mySpells[spell].repulseVector.x * direction, mySpells[spell].repulseVector.y), mySpells[spell].dazedTime);
                    curSpellDelayBtwDamage = mySpells[spell].periodicityDamage + Time.time;
                }
            }
        }
    }

    void SpellEnded()
    {
        isSpellCasted = true;
    }

    override protected void FlipOtherTransforms()
    {
        tail.transform.localPosition = new Vector2(-tail.transform.localPosition.x, tail.transform.localPosition.y);
        frontLegs.transform.localPosition = new Vector2(-frontLegs.transform.localPosition.x, frontLegs.transform.localPosition.y);
    }
}