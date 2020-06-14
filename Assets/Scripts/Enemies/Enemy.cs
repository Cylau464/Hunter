using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using Structures;
using Enums;
using System.Linq;

public class Enemy : MonoBehaviour
{
    [Header("Attributes Properties")]
    [SerializeField] protected int maxHealth = 100;
    int health;
    [SerializeField] protected int maxStamina = 100;
    int stamina;
    [SerializeField] float restoreStaminaDelay = 2f;
    float curRestoreStaminaDelay;
    [SerializeField] int restoreStaminaValue = 5;
    protected int Stamina
    { 
        set
        {
            if (value < stamina)
            {
                if (value <= 0)
                {
                    Stun();
                }

                curRestoreStaminaDelay = restoreStaminaDelay + Time.time;
            }
            else if (value > maxStamina)
            {
                value = maxStamina;
            }

            stamina = value;
        }

        get { return stamina; }
    }

    [SerializeField] float viewDistance = 20f;
    public DragType dragType = DragType.Draggable;

    [Header("Attack Properties")]
    [SerializeField] GameObject damageBox = null;
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
    [HideInInspector] public bool isHitPlayer;
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
    public bool isStun;
    public bool isDead;

    [SerializeField] float spriteBlinkingDuration = 0.6f;
    [SerializeField] float spriteBlinkingPeriod = 0.1f;
    float blinkingTimer = 0f;
    float blinkingPeriod = 0f;

    bool spriteBlinkingEnabled;

    protected enum State { Null, Patrol, Chase, Attack, CastSpell, Hurt, Stun, Dead };
    protected State currentState;

    protected Transform myTransform;
    protected Rigidbody2D rigidBody;
    Collider2D bodyCollider;
    protected float colliderWidth;
    protected Collider2D target;
    protected LayerMask playerLayer = 1 << 10;       //10 - player layer
    Transform hookTransform;
    Transform myHookTarget;
    protected PlayerMovement playerMovement;
    protected PlayerAttributes playerAttributes;
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

    public EnemySpellStates spellState = EnemySpellStates.None;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource = null;
    [SerializeField] AudioClip[] stepClips = null;
    [SerializeField] AudioClip impactClip = null;
    [SerializeField] AudioClip stunClip = null;

    //UI
    [SerializeField] Transform statusBarTransform = null;
    StatusBar statusBar;

    protected void Awake()
    {
        health              = maxHealth;
        Stamina             = maxStamina;
        currentState        = State.Patrol;
        myTransform         = GetComponent<Transform>();
        bodyCollider        = GetComponent<Collider2D>();
        rigidBody           = GetComponent<Rigidbody2D>();
        sprite              = GetComponent<SpriteRenderer>();
        startPos            = myTransform.position;
        playerAttributes     = GameObject.FindWithTag("Player").GetComponent<PlayerAttributes>();

        statusBarTransform.TryGetComponent(out StatusBar _bar);
        statusBar = _bar;
        statusBar.maxHealth = maxHealth;
        statusBar.HealthChange(health);

        foreach (Transform t in transform)
        {
            if(t.tag == "Hook Target")
                myHookTarget = t;
        }

        if (bodyCollider is PolygonCollider2D)
        {
            PolygonCollider2D _col = (PolygonCollider2D)bodyCollider;

            foreach (Vector2 point in _col.points)
            {
                colliderWidth = point.x > colliderWidth ? point.x : colliderWidth;
            }
        }
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if(bodyCollider != null)
            Gizmos.DrawWireCube(new Vector2(transform.position.x + attackRange.x * 1.5f * direction, transform.position.y + attackRange.y / 2f), new Vector2(attackRange.x + curCombo.attackRange, attackRange.y));
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

        //Stamina restore
        if(curRestoreStaminaDelay <= Time.time && stamina < maxStamina)
        {
            Stamina += restoreStaminaValue;
            curRestoreStaminaDelay = Time.time + 1f;
        }
    }

    protected void FixedUpdate()
    {
        if(currentState == State.Dead) return;

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
            case State.Stun:
                isStun = true;
                Stunned();
                break;
            case State.Dead:
                //isDead = true;
                //Invoke("Dead", 3f);
                //currentState = State.Null;
                break;
        }

        CheckPlayer();
        CheckOtherEnemies(); // Check other enemies to push them away so that they dont stack
    }

    protected void SwitchState(State newState)
    {
        if (lastSpell == "Knockback" && isCast)
            FlipCharacter(false);

        isPatrol    = false;
        isChase     = false;
        isAttack    = false;
        isCast      = false;
        isHurt      = false;
        isStun      = false;
        isDead      = false;

        spellNumber = 0;
        spellState = EnemySpellStates.None;
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
        //Chase while the player in view area
        if (target)
        {
            //Flip enemy towards the player
            if (Mathf.Sign(target.transform.position.x - transform.position.x) != direction)
                FlipCharacter(false);

            //Move towards the player until the attack distance is reached...
            if (DistanceToPlayer().x > attackRange.x + curCombo.attackRange)
                rigidBody.velocity = new Vector2(chaseSpeed * direction, rigidBody.velocity.y);
            else
                //...and attack when it reached
                SwitchState(State.Attack);
        }
        else
        {
            SwitchState(State.Patrol);
            //target = null;
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
                            if (DistanceToPlayer().x > attackRange.x + combo.Value.attackRange || DistanceToPlayer().x <= (attackRange.x + combo.Value.attackRange) / 2f)
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

                //Flip enemy towards the player
                if (Mathf.Sign(target.transform.position.x - transform.position.x) != direction)
                    FlipCharacter(false);
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

    public int TakeDamage(int damage, DamageTypes damageType, Element element, int staminaDamage = 0 /*, bool crit*/)
    {
        if (currentState == State.Dead) return default;

        //Debug.Log("BEFORE damage " + damage + " element " + element.value);
        element.value = element.value - Mathf.CeilToInt(element.value / 100f * elementDefence[element.element]);    //Calculation of defence from elements
        damage = damage - Mathf.CeilToInt(damage / 100f * physicDefence[damageType]);                               //Calculation of physical defence
        health -= damage + element.value;
        damageTaken.Add(new Dictionary<float, int>());
        damageTaken.Last().Add(Time.time, damage + element.value);
        Stamina -= staminaDamage;

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

        return damage + element.value;
    }

    void DamageText(int damage)
    {
        GameObject damageText = Resources.Load<GameObject>("UI/DamageNumber");
        damageText = Instantiate(damageText, transform);
        damageText.GetComponent<DamageNumber>().GetParameters(damage, myTransform);
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
        GameObject _inst = Instantiate(damageBox, transform);
        DamageBox _damageBox = _inst.GetComponent<DamageBox>();
        int targetLayer = 10;
        float _dbLifeTime = curCombo.timeBtwAttack[curAttackNumber] > 0f ? curCombo.timeBtwAttack[curAttackNumber] : .1f;
        _damageBox.GetParameters(curCombo.damage[curAttackNumber], curCombo.element, new Vector2(transform.position.x + attackRange.x * 1.5f * direction, transform.position.y + attackRange.y / 2f), attackRange, _dbLifeTime, impactClip, targetLayer, curCombo.dazedTime[curAttackNumber], curCombo.repulseDistantion[curAttackNumber], this);
        PlayAttackAudio(curCombo.attackClips[curAttackNumber]);

        if (curCombo.attackCount <= curAttackNumber + 1)
            curAttackCD = Time.time + curCombo.attackCD;                        //Cooldown of attack
        else
        {
            curAttackCD = Time.time + curCombo.timeBtwAttack[curAttackNumber];  //Cooldown between combo attacks
            increaseAttackNumber = true;
        }
    }

    void EndOfAttack()
    {
        //If combo is ended
        if (curCombo.attackCount <= curAttackNumber + 1)
            isAttack = false;
    }

    protected void FlipCharacter(bool onlyDir)
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
        if (playerMovement != null)
        {
            if (playerMovement.isDead)
                target = null;
        }
        else
        {
            //Find the player in area of view
            if (Physics2D.OverlapCircle(transform.position, viewDistance, 1 << 10) && target == null)
            { 
                target = Physics2D.OverlapCircle(transform.position, viewDistance, 1 << 10);
                playerMovement = target.GetComponent<PlayerMovement>();
            }

            if (target && isPatrol)
                SwitchState(State.Chase);
        }
    }

    void CheckOtherEnemies()
    {
        Collider2D _collider;
        Rigidbody2D _rigidBody;
        float _checkDistance = .5f;

        if(_collider = Physics2D.OverlapCircle(transform.position, _checkDistance, 1 << 12))
        {
            _rigidBody = _collider.attachedRigidbody;
            Vector2 _distanceToEnemy = DistanceToOtherEnemy(_collider.transform.position);

            if (_distanceToEnemy.x < _checkDistance)
            {
                _rigidBody.AddForce(Vector2.right * _distanceToEnemy * Mathf.Sign(_distanceToEnemy.x), ForceMode2D.Impulse);
            }
        }
    }

    Vector2 DistanceToOtherEnemy(Vector2 otherPosition)
    {
        return otherPosition - (Vector2) myTransform.position;
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
        float x = Mathf.Abs(target.transform.position.x - transform.position.x);
        float y = target.transform.position.y - transform.position.y;
        return new Vector2(x, y);
    }

    void PlayStepsAudio()
    {
        if (audioSource == null)
            return;

        audioSource.pitch = 1f;
        audioSource.clip = stepClips[Random.Range(0, stepClips.Length)];
        audioSource.volume = .25f;
        audioSource.Play();
    }

    void PlayAttackAudio(AudioClip clip)
    {
        if (audioSource == null)
            return;

        audioSource.pitch = Random.Range(.5f, .8f);
        audioSource.PlayOneShot(clip);
    }

    void PlayStunAudio()
    {
        if (audioSource == null)
            return;

        audioSource.clip = stunClip;
        audioSource.Play();
    }

    void Stun()
    {
        PlayStunAudio();
        SwitchState(State.Stun);
    }

    void Stunned()
    {
        if (curRestoreStaminaDelay <= Time.time)
        {
            SwitchState(State.Chase);
            Stamina = maxStamina;
        }
    }
}