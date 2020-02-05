using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using Structures;
using Enums;
using System.Linq;

public class Enemy : MonoBehaviour
{
    [Header("Atributes Properties")]
    [SerializeField] protected int maxHealth = 100;
    int health;
    [SerializeField] float viewDistance = 20f;
    public DragType dragType = DragType.Draggable;

    [Header("Attack Properties")]
    [SerializeField] protected EnemyComboDictionary combos = new EnemyComboDictionary();
    [SerializeField] ElementDictionary elementDamage = new ElementDictionary()
    {
        { Elements.Fire, 0 },
        { Elements.Earth, 0 },
        { Elements.Wind, 0 },
        { Elements.Ice, 0 },
        { Elements.Lightning, 0 },
        { Elements.Primal, 0 }
    };

    public float attackCD = 1.5f;
    [SerializeField] protected Vector2 attackRange;
    protected float curGlobalSpellCD = 0f;
    protected float curAttackCD = 0f;
    protected string lastAttack;
    protected bool isHitPlayer;
    bool increaseAttackNumber;
    [HideInInspector] public int comboNumber;       //For animations
    [HideInInspector] public int curAttackNumber;   //For switch animations
    EnemyCombo curCombo;

    [Header("Defence Properties")]
    [SerializeField] ElementDictionary elementDefence = new ElementDictionary()
    {
        { Elements.Fire, 0 },
        { Elements.Earth, 0 },
        { Elements.Wind, 0 },
        { Elements.Ice, 0 },
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
    BoxCollider2D bodyCollider;
    protected Collider2D target;
    protected LayerMask playerLayer = 1 << 10;       //10 - player layer
    Transform hookTransform;
    Transform myHookTarget;
    protected PlayerMovement playerMovement;
    protected PlayerAtributes playerAtributes;
    protected SpriteRenderer sprite;
    Vector2 startPos;

    public int direction { get; protected set; } = 1;

    //Patrol variables
    float pathDestination  = 0f;   //Point to move around start position
    float patrolDelay      = 0f;   //Delay before start next path

    int dir                = 0;    //Local var direction of patrol

    [Header("Spell Properties")]
    [HideInInspector] public int spellNumber;        //Needful for animator
    /// <summary>
    /// Damage taken for the last times and time when it been
    /// </summary>
    //protected Dictionary<float, int> damageTaken = new Dictionary<float, int>();
    protected List<Dictionary<float, int>> damageTaken = new List<Dictionary<float, int>>();
    //protected ListDictionary damageTaken = new ListDictionary();
    /// <summary>
    /// Time of taken damage and duration for calculate DPS
    /// </summary>
    protected float damageTakenDPS;
    protected float delayOfRemoveOldTakenDamage = 2f;
    protected float curSpellPrepareTime;
    protected float curSpellCastTime;

    protected string spell = "None";
    protected string lastSpell;

    public SpellStates spellState = SpellStates.None;

    //UI
    [SerializeField] Transform statusBarTransform = null;
    StatusBar statusBar;

    protected void Awake()
    {
        health              = maxHealth;
        currentState        = State.Patrol;
        myTransform         = GetComponent<Transform>();
        bodyCollider        = GetComponent<BoxCollider2D>();
        rigidBody           = GetComponent<Rigidbody2D>();
        sprite              = GetComponent<SpriteRenderer>();
        startPos            = myTransform.position;
        playerAtributes     = GameObject.FindWithTag("Player").GetComponent<PlayerAtributes>();

        statusBar = statusBarTransform.GetComponent<StatusBar>();
        statusBar.maxHealth = maxHealth;
        statusBar.HealthChange(health);

        foreach (Transform t in transform)
        {
            if(t.tag == "Hook Target")
                myHookTarget = t;
        }
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if(bodyCollider != null)
            Gizmos.DrawWireCube(new Vector2(transform.position.x +(bodyCollider.size.x / 2f + attackRange.x / 2f) * direction, transform.position.y + attackRange.y / 2f), attackRange);
    }

    protected void Update()
    {
        if (health <= 0 && currentState != State.Dead)
        {
            target = null;
            SwitchState(State.Dead);
        }

        if(!isCast && !isAttack)
        {
            isHitPlayer = false;
            lastAttack = null;
            lastSpell = null;
        }

        //Remove first list element if he's was added too long
        if (damageTaken.Count < 0 && System.Convert.ToSingle(damageTaken.First().Keys) + delayOfRemoveOldTakenDamage <= Time.time)
            damageTaken.RemoveAt(0);
        
        //Blinking after take damage
        if(spriteBlinkingEnabled)
            SpriteBlinkingEffect();
    }

    protected void FixedUpdate()
    {
        if(currentState == State.Dead) return;

        CheckPlayer();
        SwitchSpell();

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
        if (DistanceToPlayer().x <= viewDistance)
        {
            //Move towards the player until the attack distance is reached...
            if (Mathf.Abs(target.transform.position.x - transform.position.x - attackRange.x * direction) > attackRange.x)
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
        if (curAttackCD <= Time.time)
        {
            if (!isAttack)
            {
                if (target != null)
                {
                    int _random = Random.Range(0, 100);
                    int _chance = 0;
                    int _comboNumber = 0;

                    foreach (KeyValuePair<string, EnemyCombo> combo in combos)
                    {
                        if (_random < combo.Value.chance + _chance)
                        {
                            //If player too far
                            if (DistanceToPlayer().x - attackRange.x * direction > attackRange.x + combo.Value.attackRange)
                            {
                                SwitchState(State.Chase);
                                return;
                            }

                            curAttackNumber = 0;
                            comboNumber = _comboNumber;
                            curCombo = combo.Value;
                            lastAttack = combo.Key;
                            break;
                        }

                        _comboNumber++;
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
            else if (increaseAttackNumber)
            {
                curAttackNumber++;      //This will switch the animation to the next attack
                increaseAttackNumber = false;
            }
        }
    }

    protected virtual void CastSpell()
    {

    }

    protected virtual void SwitchSpell()
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

        //Debug.Log("BEFORE damage " + damage + " element " + element.value);
        element.value = element.value - Mathf.CeilToInt(element.value / 100f * elementDefence[element.element]);    //Calculation of defence from elements
        damage = damage - Mathf.CeilToInt(damage / 100f * physicDefence[damageType]);                               //Calculation of physical defence
        health -= damage + element.value;
        damageTaken.Add(new Dictionary<float, int>());
        damageTaken.Last().Add(Time.time, damage + element.value);

        //Calculate duration between first taken damage and last, then devide all taken damage on it
        if (damageTaken.Count > 1)
            damageTakenDPS = damageTaken.Sum(x => x.Values.Single() / (damageTaken.Last().Keys.Single() - damageTaken.First().Keys.Single()));
        else
            damageTakenDPS = damageTaken.First().Values.Single();

        spriteBlinkingEnabled = true;
        //Debug.Log("POST damage " + damage + " element " + element.value);
        /*if(crit)
            currentState = State.Hurt;*/
        statusBar.HealthChange(health);
        DamageText(damage + element.value);
    }

    void DamageText(int damage)
    {
        GameObject damageText = Resources.Load<GameObject>("DamageNumber");
        damageText = Instantiate(damageText, transform);
        damageText.GetComponent<DamageNumber>().damage = damage;
        damageText.GetComponent<DamageNumber>().target = myTransform;
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
            blinkingPeriod = 0f;
            sprite.color = sprite.color == Color.white ? Color.red : Color.white;
        }
    }

    void GiveDamage()
    {
        if (curCombo.attackCount <= curAttackNumber + 1)
            curAttackCD = Time.time + curCombo.attackCD;                        //Cooldown of attack
        else
        {
            curAttackCD = Time.time + curCombo.timeBtwAttack[curAttackNumber];  //Cooldown between combo attacks
            increaseAttackNumber = true;
        }

        Collider2D _objectToDamage = Physics2D.OverlapBox(new Vector2(transform.position.x + (bodyCollider.size.x / 2f + attackRange.x / 2f) * direction, transform.position.y + attackRange.y / 2f), attackRange, 0, playerLayer); //Need to create child object "Attack Point"

        if (_objectToDamage != null)
        {
            _objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(curCombo.damage[curAttackNumber], HurtType.Repulsion, new Vector2(curCombo.repulseDistantion[curAttackNumber].x * direction, curCombo.repulseDistantion[curAttackNumber].y), curCombo.dazedTime[curAttackNumber], curCombo.element);
            isHitPlayer = true;
        }
    }

    void EndOfAttack()
    {
        //If combo is ended
        if (curCombo.attackCount <= curAttackNumber + 1)
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

            FlipOtherTransforms();
        }
    }

    protected virtual void FlipOtherTransforms()
    {

    }

    protected void CheckPlayer()
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

    protected Vector2 DistanceToPlayer()
    {
        //return Mathf.Abs(target.transform.position.x - transform.position.x - attackRange.x * direction);
        float _x = Mathf.Abs(target.transform.position.x - transform.position.x);
        float _y = target.transform.position.y - transform.position.y;
        return new Vector2(_x, _y);
    }
}