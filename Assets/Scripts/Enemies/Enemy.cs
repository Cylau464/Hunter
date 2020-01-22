using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public enum SpellStates { None, Prepare, Cast, End }

public class Enemy : MonoBehaviour
{
    [Header("Atributes Properties")]
    [SerializeField] int maxHealth = 5;
    int health;
    [SerializeField] float viewDistance = 10f;
    public DragType dragType = DragType.Draggable;

    [Header("Attack Properties")]
    [SerializeField] protected EnemyComboDictionary combos = new EnemyComboDictionary();
    [SerializeField] protected int damage = 1;
    [SerializeField] ElementDictionary elementDamage = new ElementDictionary()
    {
        { Elements.Fire, 0 },
        { Elements.Earth, 0 },
        { Elements.Wind, 0 },
        { Elements.Water, 0 },
        { Elements.Lightning, 0 },
        { Elements.Primal, 0 }
    };

    public float attackCD = 1.5f;
    [SerializeField] protected float attackDistance = 2f;
    [SerializeField] float attackRangeX = 2f;
    [SerializeField] float attackRangeY = 1f;
    protected float curGlobalSpellCD = 0f;
    protected float curAttackCD = 0f;

    [HideInInspector] public int comboNumber;       //For animations
    [HideInInspector] public int curAttackNumber;   //For switch animations
    EnemyCombo curCombo;

    [Header("Defence Properties")]
    [SerializeField] ElementDictionary elementDefence = new ElementDictionary()
    {
        { Elements.Fire, 0 },
        { Elements.Earth, 0 },
        { Elements.Wind, 0 },
        { Elements.Water, 0 },
        { Elements.Lightning, 0 },
        { Elements.Primal, 0 }
    };
    [SerializeField] DamageTypeDefenceDictionary physicDefence = new DamageTypeDefenceDictionary()
    {
        { DamageTypes.Blunt, 0 },
        { DamageTypes.Chop, 0 },
        { DamageTypes.Slash, 0 },
        { DamageTypes.Thrust, 0 }
    };

    [Header("Speed Properties")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 4f;

    [Header("Patrol Properties")]
    [SerializeField] float minWaitTime = 1f;
    [SerializeField] float maxWaitTime = 4f;
    [SerializeField] float patrolDistance = 10f;

    bool pathPassed = true;

    //[Header("Hurt Properties")]
    //[SerializeField] float dazedTime = .5f;
    //float curDazedTime;

    [Header("State Flags")]
    public bool isPatrol;
    public bool isChase;
    public bool isAttack;
    public bool isCast;
    public bool isHurt;
    public bool isDead;

    [SerializeField] float spriteBlinkingDuration = 0.6f;
    [SerializeField] float spriteBlinkingPeriod = 0.1f;
    float blinkingTimer = 0f;
    float blinkingPeriod = 0f;

    bool spriteBlinkingEnabled;

    protected enum State { Null, Patrol, Chase, Attack, CastSpell, Hurt, Dead };
    protected State currentState;

    protected Transform myTransform;
    protected Rigidbody2D rigidBody;
    protected Collider2D target;
    protected LayerMask playerLayer = 1 << 10;       //10 - player layer
    Transform hookTransform;
    Transform myHookTarget;
    PlayerMovement playerMovement;
    SpriteRenderer sprite;
    Vector2 startPos;

    public int direction { get; protected set; } = 1;

    //Patrol variables
    float pathDestination  = 0f;   //Point to move around start position
    float patrolDelay      = 0f;   //Delay before start next path

    int dir                = 0;    //Local var direction

    [Header("Spell Properties")]
    [HideInInspector] public int spellNumber;        //Needful for animator

    protected float curSpellPrepareTime;
    protected float curSpellCastTime;

    protected string spell = "None";

    public SpellStates spellState = SpellStates.None;

    protected void Awake()
    {
        health         = maxHealth;
        currentState   = State.Patrol;
        myTransform    = GetComponent<Transform>();
        rigidBody      = GetComponent<Rigidbody2D>();
        sprite         = GetComponent<SpriteRenderer>();
        startPos       = myTransform.position;

        foreach(Transform t in transform)
        {
            if(t.tag == "Hook Target")
                myHookTarget = t;
        }
    }

    protected void Update()
    {
        if (health <= 0 && currentState != State.Dead)
        {
            target = null;
            SwitchState(State.Dead);
        }

        //Blinking after take damage
        if(spriteBlinkingEnabled)
            SpriteBlinkingEffect();
    }

    protected void FixedUpdate()
    {
        if(currentState == State.Dead) return;

        CheckPlayer();

        switch (currentState)
        {
            case State.Patrol:
                isPatrol = true;
                Patrol();
                break;
            case State.Chase:
                isChase = true;
                Chase();
                break;
            case State.Attack:
                //isAttack = true;
                Attack();
                break;
            case State.CastSpell:
                isCast = true;         
                CastSpell();
                break;
            case State.Hurt:
                isHurt = true;
                Hurt();
                break;
            case State.Dead:
                //isDead = true;
                //Invoke("Dead", 3f);
                //currentState = State.Null;
                break;
        }
    }

    protected void SwitchState(State newState)
    {
        isPatrol    = false;
        isChase     = false;
        isAttack    = false;
        isCast      = false;
        isHurt      = false;
        isDead      = false;

        spellNumber = 0;
        spellState = SpellStates.None;
        currentState = newState;

        if (newState == State.Dead)
        {
            isDead = true;
            Invoke("Dead", 3f);
        }
    }

    protected void Patrol()
    {
        //If character reached destination...
        if (pathPassed)
        {
            //...he is looking for a new point to move. 
            //New point should not be closer than 2.5 if distance randomly selected to max and current pos is too close to it
            if((pathDestination - transform.position.x) * dir <= 2.5)
            //if (Mathf.Abs((startPos.x + patrolDistance * dir) - transform.position.x) < 2.5f || (pathDestination - transform.position.x) * dir <= 0)
            {
                dir = (int) Mathf.Sign(Random.Range(-1, 1));
                pathDestination = Random.Range(patrolDistance / 3, patrolDistance) * dir + startPos.x;
            }
            else
            {
                if (direction != dir)
                    FlipCharacter(false);

                pathPassed = false;
            }
        }
        else
        {
            //Move forward until reach destination
            if ((pathDestination - transform.position.x) * direction > 0)
                rigidBody.velocity = new Vector2(patrolSpeed * direction, rigidBody.velocity.y);
            else
            {
                //rigidBody.velocity = Vector2.Zero; Check need it or not
                //Random select waiting time
                if (patrolDelay == 0f)
                    patrolDelay = Random.Range(minWaitTime, maxWaitTime) + Time.time;
                //Waiting
                else if (patrolDelay <= Time.time)
                {
                    patrolDelay = 0f;
                    pathPassed = true;
                }
            }
        }
    }

    protected void Chase()
    {
        //Flip enemy towards the player
        if(Mathf.Sign(target.transform.position.x - transform.position.x) != direction)
            FlipCharacter(false);

        //Chase while the player in view area
        if (DistanceToPlayer() <= 20f)
        {
            //Move towards the player until the attack distance is reached...
            if ((target.transform.position.x - transform.position.x - attackDistance * direction) * direction > 0)
                rigidBody.velocity = new Vector2(chaseSpeed * direction, rigidBody.velocity.y);
            else
                //...and attack when it reached
                SwitchState(State.Attack);
        }
        else
        {
            SwitchState(State.Patrol);
            target = null;
        }
    }

    protected void Attack()
    {
        if(curAttackCD <= Time.time && !isAttack)
        {
            if (target != null)
            {
                int _random = Random.Range(0, 100);
                int _chance = 0;
                int _serNumber = 0;         //Combo number

                foreach(KeyValuePair<string, EnemyCombo> combo in combos)
                {
                    if (_random < combo.Value.chance + _chance)
                    {
                        //If player too far
                        if (DistanceToPlayer() > attackDistance + combo.Value.attackRange)
                        {
                            SwitchState(State.Chase);
                            return;
                        }

                        curAttackNumber = 0;
                        comboNumber = _serNumber;
                        curCombo = combo.Value;
                        Debug.Log("combo " + comboNumber);
                        break;
                    }

                    _serNumber++;
                    _chance += combo.Value.chance;
                }

                //Flip enemy towards the player
                if (Mathf.Sign(target.transform.position.x - transform.position.x) != direction)
                    FlipCharacter(false);

                isAttack = true;
            }
            else
            {
                SwitchState(State.Patrol);
            }
        }
    }

    protected virtual void CastSpell()
    {

    }

    protected void Hurt()
    {
        /*
        if (hookTransform != null)
            transform.position = hookTransform.position - myHookTarget.localPosition;
        else if (curDazedTime <= Time.time)
            SwitchState(State.Chase);*/
    }
    
    public void HookOn(Transform hook)
    {
        hookTransform = hook;
        SwitchState(State.Hurt);
    }

    public void HookOff(float dazedTime)
    {
        hookTransform = null;
        //curDazedTime = Time.time + dazedTime;
    }

    protected void Dead()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(int damage, DamageTypes damageType, Element element /*, bool crit*/)
    {
        if (currentState == State.Dead) return;

        Debug.Log("BEFORE damage " + damage + " element " + element.value);
        element.value = element.value - Mathf.CeilToInt(element.value / 100f * elementDefence[element.element]);    //Calculation of defence from elements
        damage = damage - Mathf.CeilToInt(damage / 100f * physicDefence[damageType]);                               //Calculation of physical defence
        health -= damage + element.value;
        spriteBlinkingEnabled = true;
        Debug.Log("POST damage " + damage + " element " + element.value);
        /*if(crit)
            currentState = State.Hurt;*/
        TakeDamageEffects(damage + element.value);
    }

    void TakeDamageEffects(int damage)
    {
        GameObject damageText = Resources.Load<GameObject>("DamageNumber");
        damageText = Instantiate(damageText, transform);
        damageText.GetComponent<DamageNumber>().damage = damage;
    }

    void SpriteBlinkingEffect()
    {
        //Timer of total blinking duration
        blinkingTimer += Time.deltaTime;
        if(blinkingTimer >= spriteBlinkingDuration)
        { 
            spriteBlinkingEnabled = false;
            blinkingTimer = 0f;
            sprite.color = Color.white;
            return;
        }
        //Timer of blinking tact
        blinkingPeriod += Time.deltaTime;
        if(blinkingPeriod >= spriteBlinkingPeriod)
        {
            blinkingPeriod = 0f;/*
            if(sprite.color == Color.white)
                sprite.color = Color.red;
            else
                sprite.color = Color.white;*/
            sprite.color = sprite.color == Color.white ? Color.red : Color.white;
        }
    }

    void GiveDamage()
    {
        curAttackCD = Time.time + attackCD;       //Start cooldown of attack

        Collider2D _objectToDamage = Physics2D.OverlapBox(transform.position, new Vector2(attackRangeX, attackRangeY), 0, playerLayer);

        if (_objectToDamage != null)
            _objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(damage, HurtType.Repulsion);
    }

    void EndOfAttack()
    {
        isAttack = false;
    }

    void FlipCharacter(bool onlyDir)
    {
        direction = -direction;
        
        //If need to invert not only direction
        if(!onlyDir)
        {
            //Chech direction and flipX are syncronized or not
            if(!sprite.flipX && direction == -1)
                sprite.flipX = true;
            else if(sprite.flipX && direction == 1)
                sprite.flipX = false;
        }
    }

    void CheckPlayer()
    {
        if (playerMovement != null && playerMovement.isDead)
        {
            target = null;
            return;
        }

        //Find the player in area of view
        if (Physics2D.OverlapCircle(transform.position, viewDistance, 1 << 10) && target == null)
        {
            target = Physics2D.OverlapCircle(transform.position, viewDistance, 1 << 10);
            playerMovement = target.GetComponent<PlayerMovement>();
        }

        if(target && isPatrol)
            SwitchState(State.Chase);
    }

    protected bool IsPlayerBehind()
    {
        if(Mathf.Sign(myTransform.position.x - target.transform.position.x) * direction < 0)
            return false;
        else
            return true;
    }

    protected float DistanceToPlayer()
    {
        return Mathf.Abs(target.transform.position.x - transform.position.x);
    }
}