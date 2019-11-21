using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Atributes Properties")]
    [SerializeField] int _maxHealth = 5;
    int _health;
    [SerializeField] float _viewDistance = 10f;

    [Header("Attack properties")]
    [SerializeField] int _damage = 1;
    [SerializeField] float _attackDistance = 2f;
    //[SerializeField] float _attackDuration = 1f;
    [SerializeField] float _attackRangeX = 2f;
    [SerializeField] float _attackRangeY = 1f;
    public float attackCD = 1.5f;
    float _attackCD = 0f;

    [Header("Speed Properties")]
    [SerializeField] float _patrolSpeed = 2f;
    [SerializeField] float _chaseSpeed = 4f;
    
    [Header("Patrol Properties")]
    [SerializeField] float _minWaitTime = 1f;
    [SerializeField] float _maxWaitTime = 4f;
    [SerializeField] float _patrolDistance = 10f;
    bool _pathPassed = true;

    [Header("Hurt Properties")]
    float _dazedTime = .5f;

    [Header("State Flags")]
    public bool isPatrol;
    public bool isChase;
    public bool isAttack;
    public bool isHurt;
    public bool isDead;

    [SerializeField] float _spriteBlinkingDuration = 0.6f;
    [SerializeField] float _spriteBlinkingPeriod   = 0.1f;
    float blinkingTimer = 0f;
    float blinkingPeriod = 0f;
    bool _spriteBlinkingEnabled;

    enum State { Null, Patrol, Chase, Attack, Hurt, Dead };
    State _currentState;
    Transform _transform;
    Transform _hook;
    Rigidbody2D _rigidBody;
    SpriteRenderer _sprite;
    Vector2 _startPos;
    Collider2D _target;
    DragType dragType = DragType.Draggable;

    int direction = 1;

    //Patrol variables
    float _pathDestination  = 0f;   //Point to move around start position
    float _patrolDelay      = 0f;   //Delay before start next path
    int _dir                = 0;    //Local var direction

    void Start()
    {
        _health         = _maxHealth;
        _currentState   = State.Patrol;
        _transform      = GetComponent<Transform>();
        _rigidBody      = GetComponent<Rigidbody2D>();
        _sprite         = GetComponent<SpriteRenderer>();
        _startPos       = transform.position;
    }

    void Update()
    {
        if(_health <= 0 && _currentState != State.Dead && _currentState != State.Null)
            SwitchState(State.Dead);

        //Blinking after take damage
        if(_spriteBlinkingEnabled)
            SpriteBlinkingEffect();
    }

    void FixedUpdate()
    {
        CheckPlayer();

        switch (_currentState)
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
            case State.Hurt:
                isHurt = true;
                Hurt();
                break;
            case State.Dead:
                isDead = true;
                Invoke("Dead()", 3f);
                _currentState = State.Null;
                break;
        }
    }

    void SwitchState(State newState)
    {
        isPatrol    = false;
        isChase     = false;
        isAttack    = false;
        isHurt      = false;
        isDead      = false;

        _currentState = newState;
    }

    void Patrol()
    {
        //If character reached destination...
        if (_pathPassed)
        {
            //...he is looking for a new point to move. 
            //New point should not be closer than 2.5 if distance randomly selected to max and current pos is too close to it
            if((_pathDestination - _transform.position.x) * _dir <= 2.5)
            //if (Mathf.Abs((_startPos.x + _patrolDistance * _dir) - _transform.position.x) < 2.5f || (_pathDestination - _transform.position.x) * _dir <= 0)
            {
                _dir = (int) Mathf.Sign(Random.Range(-1, 1));
                _pathDestination = Random.Range(_patrolDistance / 3, _patrolDistance) * _dir + _startPos.x;
            }
            else
            {
                if (direction != _dir)
                    FlipCharacter(false);

                _pathPassed = false;
            }
        }
        else
        {
            //Move forward until reach destination
            if ((_pathDestination - _transform.position.x) * direction > 0)
                _rigidBody.velocity = new Vector2(_patrolSpeed * direction, _rigidBody.velocity.y);
            else
            {
                //_rigidBody.velocity = Vector2.Zero; Check need it or not
                //Random select waiting time
                if (_patrolDelay == 0f)
                    _patrolDelay = Random.Range(_minWaitTime, _maxWaitTime) + Time.time;
                //Waiting
                else if (_patrolDelay <= Time.time)
                {
                    _patrolDelay = 0f;
                    _pathPassed = true;
                }
            }
        }
    }

    void Chase()
    {
        //Flip enemy towards the player
        if(Mathf.Sign(_target.transform.position.x - _transform.position.x) != direction)
            FlipCharacter(false);

        if (Mathf.Abs(_target.transform.position.x - _transform.position.x) <= 20f)
        {
            //Move towards the player until the attack distance is reached...
            if ((_target.transform.position.x - _transform.position.x - _attackDistance * direction) * direction > 0)
                _rigidBody.velocity = new Vector2(_chaseSpeed * direction, _rigidBody.velocity.y);
            else
                //...and attack when it reached
                SwitchState(State.Attack);
        }
        else
        {
            SwitchState(State.Patrol);
            _target = null;
        }
    }

    void Attack()
    {
        if(_attackCD <= Time.time && !isAttack)
        {
            //If player too far
            if (_target != null)
            {
                if (Mathf.Abs(_target.transform.position.x - _transform.position.x) > _attackDistance)
                {
                    SwitchState(State.Chase);
                    return;
                }


                //Flip enemy towards the player
                if (Mathf.Sign(_target.transform.position.x - _transform.position.x) != direction)
                    FlipCharacter(false);
            }

            isAttack = true;
        }
    }

    void Hurt()
    {
        _transform.position = _hook ?? _hook.transform.position;

        if(_dazedTime <= Time.time)
            SwitchState(State.Chase);
    }

    public void HookOn(Transform hook)
    {
        _hook = hook;
        SwitchState(State.Hurt);
    }

    public void HookOff(float dazedTime)
    {
        _hook = null;
        _dazedTime = Time.time + dazedTime;
    }

    void Dead()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(int damage /*, bool crit*/)
    {
        _health -= damage;
        _spriteBlinkingEnabled = true;

        /*if(crit)
            _currentState = State.Hurt;*/
    }

    void SpriteBlinkingEffect()
    {
        //Timer of total blinking duration
        blinkingTimer += Time.deltaTime;
        if(blinkingTimer >= _spriteBlinkingDuration)
        {
            _spriteBlinkingEnabled = false;
            blinkingTimer = 0f;
            _sprite.color = Color.white;
            return;
        }
        //Timer of blinking tact
        blinkingPeriod += Time.deltaTime;
        if(blinkingPeriod >= _spriteBlinkingPeriod)
        {
            blinkingPeriod = 0f;
            if(_sprite.color == Color.white)
                _sprite.color = Color.red;
            else
                _sprite.color = Color.white;
        }
    }

    void GiveDamage()
    {
        _attackCD = Time.time + attackCD;       //Start cooldown of attack
        LayerMask _playerLayer = 1 << 10;       //10 - player layer

        Collider2D objectToDamage = Physics2D.OverlapBox(_transform.position, new Vector2(_attackRangeX, _attackRangeY), 0, _playerLayer);

        if (objectToDamage != null)
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(_damage);
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
            if(!_sprite.flipX && direction == -1)
                _sprite.flipX = true;
            else if(_sprite.flipX && direction == 1)
                _sprite.flipX = false;
        }
    }

    void CheckPlayer()
    {
        _target = _target ?? Physics2D.OverlapCircle(_transform.position, _viewDistance, 1 << 10);

        if(_target && isPatrol)
            SwitchState(State.Chase);
    }
}