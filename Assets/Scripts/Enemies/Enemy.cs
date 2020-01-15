using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Atributes Properties")]
    [SerializeField] int maxHealth = 5;
    int health;
    [SerializeField] float viewDistance = 10f;
    public DragType dragType = DragType.Draggable;

    /*public Element[] elementDefense = {
        new Element("Fire", Elements.Fire, 0),
        new Element("Wind", Elements.Wind, 0),
        new Element("Earth", Elements.Earth, 0),
        new Element("Water", Elements.Water, 0),
        new Element("Lightning", Elements.Lightning, 0),
        new Element("Primal", Elements.Primal, 0)
    };*/

    [Header("Attack Properties")]
    [SerializeField] protected int damage = 1;
    public ElementDictionary elementDamage = new ElementDictionary();
    /*public Element[] elementDamage = {
        new Element("Fire", Elements.Fire, 0),
        new Element("Wind", Elements.Wind, 0),
        new Element("Earth", Elements.Earth, 0),
        new Element("Water", Elements.Water, 0),
    };*/
    [SerializeField] float attackDistance = 2f;
    //[SerializeField] float attackDuration = 1f;
    [SerializeField] float attackRangeX = 2f;
    [SerializeField] float attackRangeY = 1f;
    public float attackCD = 1.5f;
    float curAttackCD = 0f;
    protected float curGlobalSpellCD = 0f;

    [Header("Defence Properties")]
    public ElementDictionary elementDefence = new ElementDictionary();
    public DamageTypeDefenceDictionary physicDefence = new DamageTypeDefenceDictionary();

    [Header("Speed Properties")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 4f;
    
    [Header("Patrol Properties")]
    [SerializeField] float minWaitTime = 1f;
    [SerializeField] float maxWaitTime = 4f;
    [SerializeField] float patrolDistance = 10f;
    bool pathPassed = true;

    [Header("Hurt Properties")]
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
    [SerializeField] float spriteBlinkingPeriod   = 0.1f;
    float blinkingTimer = 0f;
    float blinkingPeriod = 0f;

    bool spriteBlinkingEnabled;

    protected enum State { Null, Patrol, Chase, Attack, CastSpell, Hurt, Dead };
    protected State currentState;
    Transform myTransform;
    Transform hookTransform;
    Transform myHookTarget;
    protected Rigidbody2D rigidBody;
    SpriteRenderer sprite;
    Vector2 startPos;
    Collider2D target;

    protected int direction = 1;

    //Patrol variables
    float pathDestination  = 0f;   //Point to move around start position
    float patrolDelay      = 0f;   //Delay before start next path
    int dir                = 0;    //Local var direction

    public int spellNumber;        //Needful for animator

    void Start()
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
        if(health <= 0 && currentState != State.Dead && currentState != State.Null)
            SwitchState(State.Dead);

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
                isDead = true;
                Invoke("Dead", 3f);
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
        currentState = newState;
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

        if (Mathf.Abs(target.transform.position.x - transform.position.x) <= 20f)
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
            //If player too far
            if (target != null)
            {
                if (Mathf.Abs(target.transform.position.x - transform.position.x) > attackDistance)
                {
                    SwitchState(State.Chase);
                    return;
                }


                //Flip enemy towards the player
                if (Mathf.Sign(target.transform.position.x - transform.position.x) != direction)
                    FlipCharacter(false);
            }

            isAttack = true;
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
        LayerMask playerLayer = 1 << 10;       //10 - player layer

        Collider2D objectToDamage = Physics2D.OverlapBox(transform.position, new Vector2(attackRangeX, attackRangeY), 0, playerLayer);

        if (objectToDamage != null)
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(damage);
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
        //Find the player in area of view
        target = target ?? Physics2D.OverlapCircle(transform.position, viewDistance, 1 << 10);

        if(target && isPatrol)
            SwitchState(State.Chase);
    }

    bool IsPlayerBehind()
    {
        if(Mathf.Sign(myTransform.position.x - target.transform.position.x) * direction < 0)
            return false;
        else
            return true;
    }
}

[System.Serializable]
public struct EnemySpell
{
    [Header("General vars")]
    public float delayAfterCast;
    public float curDelay;
    public float cooldown;
    public float globalCD;
    public float curCooldown;
    public float curGlobalCD;

    public float CurCooldown { set { curCooldown = value; } }

    [Header("Attack spells")]
    public float damageRangeX;
    public float damageRangeY;

    public int damage;

    [Header("Spells with jumps")]
    public float jumpDistance;
    public float jumpHeight;

    //Сделать для всех полей свойства

    //For spells with jumps
    public EnemySpell(float jumpDistance, float jumpHeight, float delayAfterCast, float cooldown, float globalCD, float damageRangeX, float damageRangeY, int damage)
    {
        this.jumpDistance   = jumpDistance;
        this.jumpHeight     = jumpHeight;
        this.delayAfterCast = delayAfterCast;
        this.cooldown       = cooldown;
        this.globalCD       = globalCD;
        this.damageRangeX   = damageRangeX;
        this.damageRangeY   = damageRangeY;
        this.damage         = damage;
        //this.spell          = spell;

        curCooldown         = 0f;
        curGlobalCD         = 0f;
        curDelay            = 0f;
    }
    /*
    //For other spells
    public EnemySpell()
    {

    }*/
}