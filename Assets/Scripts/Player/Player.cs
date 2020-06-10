using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;


public class Player : MonoBehaviour
{
    [SerializeField] bool drawDebugRaycast = true;

    [Header("Movement Properties")]
    [SerializeField] float _speed = 8f;           //Horizontal speed
    //[SerializeField] float crouchSpeedDivisor       = 3f;           //Speed divisor for crouching (Not used now)
    [SerializeField] float _coyoteDuration = .05f;         //Little delay for jump after character start falling down
    [SerializeField] float _climbingSpeed = 70f;          //70 = 20 because 50 it's gravity force

    [Header("Jump Properties")]
    [SerializeField] float _jumpForce = 6.3f;         //Used once after pressing
    [SerializeField] float _jumpHoldForce = 2.2f;         //Used all time if key hold
    [SerializeField] float _jumpHoldDuration = .1f;          //Max duration of holding jump
    //public float hangingJumpForce                 = 15f;	        //Force of wall hanging jump
    [SerializeField] float _maxFallSpeed = -25f;         //Max falling speed 
    [SerializeField] int _extraJumps = 1;            //Count of extra jumps

    [Header("Enviropment Check Properties")]
    [SerializeField] float _footOffset = .3f;          //Offset between foots
    [SerializeField] float _headClearence = .7f;          //Necessary free space above head for stand up or/and jump
    [SerializeField] float _eyeHeight = .2f;			//Height of eye for wall checks
    [SerializeField] float _reachOffset = .7f;          //X offset for wall grabbing
    [SerializeField] float _grabDistance = .4f;          //The reach distance for wall grabs
    [SerializeField] float _groundDistance = .2f;          //Distance to check on ground character or not
    [SerializeField] float _evadingDuration = .35f;         //Duration of evade state
    [SerializeField] float _evadingDistance = 8f;           //Distance of evade
    [SerializeField] float _evadingCooldown = 1f;           //Evade cooldown in sec
    [SerializeField] int _evadingStaminaCosts = 25;           //Spent stamina for using evade
    public int direction { get; private set; } = 1;            //Character direction
    [SerializeField] LayerMask _groundLayer = 1 << 9;       //9 - Platforms layer

    [Header("Attributes Bonus")]
    public AttributesDictionary bonusAttributes = new AttributesDictionary();

    [Header("Hurt Properties")]
    float _curDazedTime = 0f;
    [HideInInspector] public HurtType hurtType;

    [Header("Status Flags")]
    public bool isOnGround;
    public bool isJumping;
    public bool isDoubleJump;
    public bool isHeadBlocked;
    public bool isCrouching;
    public bool isAttacking;
    public bool isEvading;
    public bool isHanging;
    public bool isClimbing;
    bool _isHooked;
    public bool isCast;
    public bool isHealing;
    public bool isHurt;
    public bool isDead;
    public bool canFlip = true;
    bool canHeal;
    bool canAttack;

    PlayerInput _input;
    Rigidbody2D _rigidBody;
    [HideInInspector] public BoxCollider2D bodyCollider;
    SpriteRenderer _sprite;                         //Maybe create property for public get?
    PlayerAttack _attack;
    PlayerAttributes _attributes;
    Transform _playerTransform;
    Hook _hook;
    Transform _hookTransform;
    Rigidbody2D _targetRigidBody;
    PhysicsMaterial2D _physicMaterial;

    int _extraJumpsCount;                          //Current count extra jumps

    float _curCoyoteTime;                          //Current coyote time
    float _playerHeight;                           //Based on vertical collider size
    float _curJumpTime;                            //Current time in jump (Cant be higher max jumpHoldDuration)
    float _curEvadingCooldown;                     //Current evade cooldown time left
    float _curEvadingDuration;                     //Current evade duration time

    Vector2 _colliderStandSize;                    //Collider size for standing position
    Vector2 _colliderStandOffset;                  //Collider offset for standing position
    Vector2 _colliderCrouchSize;                   //Collider size for crouching position
    Vector2 _colliderCrouchOffset;                 //Collider offset for crouching position
    //Vector2 aimDirection;

    // Raycasts for physic check
    RaycastHit2D _blockedCheck;
    RaycastHit2D _ledgeCheck;
    RaycastHit2D _wallCheck;
    RaycastHit2D _climbCheck;
    RaycastHit2D _climbCheck2;

    const float _smallAmount = .05f;               //A small amount used for hanging position and something else

    public AnimationClip castAnim = null;         //Set here default cast animation

    enum State { Null, Idle, Move, Attack, Jump, Fall, Hurt, Hang, Evade, Climb, Cast, Heal, Crouch, Dead };
    State currentState;

    void Start()
    {
        //Get all references
        _input = GetComponent<PlayerInput>();
        _rigidBody = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _attack = GetComponent<PlayerAttack>();
        _attributes = GetComponent<PlayerAttributes>();
        _playerTransform = GetComponent<Transform>();
        _hook = GetComponentInChildren<Hook>();
        _physicMaterial = bodyCollider.sharedMaterial;

        _playerHeight = bodyCollider.size.y;

        _colliderStandSize = bodyCollider.size;
        _colliderStandOffset = bodyCollider.offset;

        //Decrease collider size and offset for this strange values. Need to change they on variables with percent, not numbers
        _colliderCrouchSize = new Vector2(bodyCollider.size.x, .85f);
        _colliderCrouchOffset = new Vector2(bodyCollider.offset.x, -.17f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            Time.timeScale = 0.1f;
        if (Input.GetKeyDown(KeyCode.V))
            Time.timeScale = 1f;
        if (Input.GetKeyDown(KeyCode.T))
            Time.timeScale = 0.01f;
        /*
        //If hook is not throwing or pulling
        if (input.hook && hook.throwCoroutine == null)
        {
            //Value 10f in vector3 compensates for the Z position of the camera. Need to get this value from the camera (-Input.mousePosition.z)
            hook.throwCoroutine = hook.StartCoroutine(hook.Throw(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f))));
        }*/
        //Reset variables when character landed
        if (isOnGround)
        {
            _curCoyoteTime = Time.time + _coyoteDuration;
            _extraJumpsCount = _extraJumps + (bonusAttributes.ContainsKey(BonusAttributes.JumpCount) ? (int)bonusAttributes[BonusAttributes.JumpCount] : 0);
        }

        //aimDirection = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);

        //if (Mathf.Sign(aimDirection.x) != direction)
        //    FlipCharacterDirection();
    }

    void FixedUpdate()
    {
        if (currentState == State.Dead) return;

        if (currentState != State.Hurt)
            PhysicsCheck();

        switch(currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Move:
                Move();
                break;
            case State.Jump:
                Jump();
                break;
            case State.Evade:
                Evade();
                break;
            case State.Crouch:
                Crouch();
                break;
            case State.Climb:
                Climb();
                break;
            case State.Cast:
                //Cast();
                break;
            case State.Attack:
                Attack();
                break;
            case State.Hang:
                Hanging();
                break;
            case State.Heal:
                Heal();
                break;
            case State.Hurt:
                Hurt();
                break;
        }
        
        //{
            
        //    Healing();
        //    GroundMovement();
        //    AirMovement();
        //}

        if (isAttacking && !isOnGround && _attack.attackState != AttackState.End && _attack.attackType != AttackTypes.TopDown)
        {
            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, 0f);

            if (!isOnGround)
                _rigidBody.gravityScale = 0;
        }
        else if (_rigidBody.gravityScale != 1 && !isHurt)
            _rigidBody.gravityScale = 1;
    }

    void SwitchState(State newState)
    {
        isJumping = isDoubleJump = isCrouching = isAttacking = isEvading = isHanging = isClimbing = isCast = isHealing = isHurt = isDead = false;
        canFlip = true;
        _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        bodyCollider.size = _colliderStandSize;
        bodyCollider.offset = _colliderStandOffset;
        _attributes.isInvulnerable = false;
        canHeal = false;
        currentState = newState;
    }

    void PhysicsCheck()
    {
        if (direction != Mathf.Sign(_input.horizontal) && _input.horizontal != 0)
            FlipCharacterDirection();

        //Start by assuming the player isn't on the ground and the head isn't blocked
        isOnGround = false;
        isHeadBlocked = false;

        //Cast rays for the left and right foot
        RaycastHit2D leftCheck = Raycast(new Vector2(-_footOffset, -_playerHeight / 2f), Vector2.down, _groundDistance);
        RaycastHit2D rightCheck = Raycast(new Vector2(_footOffset, -_playerHeight / 2f), Vector2.down, _groundDistance);

        if ((leftCheck || rightCheck) && !isJumping)
            isOnGround = true;

        //Check free space above the character (for crouch)
        RaycastHit2D headCheck = Raycast(Vector2.zero, Vector2.up, _headClearence);

        if (headCheck)
            isHeadBlocked = true;

        //Determine the direction of the wall grab attempt
        Vector2 grabDir = new Vector2(direction, 0f);

        //Cast three rays to look for a wall grab
        _blockedCheck = Raycast(new Vector2(_footOffset * direction, _playerHeight / 2f), grabDir, _grabDistance);
        _ledgeCheck = Raycast(new Vector2(_reachOffset * direction, _playerHeight / 2f), Vector2.down, _grabDistance);
        _wallCheck = Raycast(new Vector2(_footOffset * direction, _eyeHeight), grabDir, _grabDistance);
        _climbCheck = Raycast(new Vector2(_footOffset * direction, -_playerHeight / 2f), grabDir, _grabDistance);
        _climbCheck2 = Raycast(new Vector2(_footOffset * direction, -_playerHeight / 4f), grabDir, _grabDistance);

        //Jump
        if (_input.jumpPressed && _attack.attackState != AttackState.Damage && !isHealing && !isClimbing && !isHanging && (isOnGround || _curCoyoteTime > Time.time || _extraJumpsCount > 0))
        {
            if(currentState != State.Jump)
                SwitchState(State.Jump);
        }

        //If the player is off the ground AND is not hanging AND is falling AND
        //found a ledge AND found a wall AND the grab is NOT blocked...
        if (!isOnGround && !isHanging && !isEvading && !isAttacking && !isClimbing &&
            _rigidBody.velocity.y <= 0f && _ledgeCheck && _wallCheck && !_blockedCheck)
        {
            SwitchState(State.Hang);
        }

        //Evading
        if (_input.lastInputs.Contains(InputsEnum.Evade) && _attack.attackState != AttackState.Damage && !isEvading && !isCast && !isHealing && _curEvadingCooldown <= Time.time)
        {
            SwitchState(State.Evade);
        }

        //Clear up all evade inputs from the inputs list if CD has not yet passed or not enough stamina
        if (_curEvadingCooldown > Time.time || _attributes.Stamina < _evadingStaminaCosts)
            _input.lastInputs.RemoveAll(x => x == InputsEnum.Evade);

        //Climbing
        if (!isEvading && !isClimbing && !isAttacking && (_climbCheck || _climbCheck2) && !_wallCheck && _input.horizontal != 0)
        {
            int numColliders = 10;
            Collider2D[] colliders = new Collider2D[numColliders];
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(1 << 9); //9 - platform layer
            int colliderCount = bodyCollider.OverlapCollider(contactFilter, colliders);

            if (colliderCount > 0)
            {
                _rigidBody.velocity = Vector2.zero;
                SwitchState(State.Climb);
            }
        }

        // Heal
        if(_input.healingPotionPressed && canHeal && _attributes.curHealingPotionCount > 0 && _attributes.curHealingDelay < Time.time)
        {
            SwitchState(State.Heal);
        }

        if (_input.lastInputs.Count > 0 && _input.attackPressed && canAttack)
            SwitchState(State.Attack);
    }

    float Movement()
    {
        if (_input.crouchHeld && isOnGround)
            SwitchState(State.Crouch);

        float xVelocity;

        xVelocity = (_speed * _attributes.speedDivisor + (bonusAttributes.ContainsKey(BonusAttributes.Speed) ? bonusAttributes[BonusAttributes.Speed] : 0)) * _input.horizontal / _attack.weaponMass;
        _rigidBody.velocity = new Vector2(xVelocity, _rigidBody.velocity.y);

        // Fixed max fall speed
        if (_rigidBody.velocity.y < _maxFallSpeed)
            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, _maxFallSpeed);

        return xVelocity;
        ////Decrease move speed on crouch
        //if (isCrouching)
        //    xVelocity /= crouchSpeedDivisor;
    }

    void Idle()
    {
        canAttack = true;
        canHeal = true;
        float xVelocity = Movement();

        if (xVelocity != 0f)
            SwitchState(State.Move);
    }

    void Move()
    {
        canAttack = true;
        canHeal = true;
        float xVelocity = Movement();

        if (xVelocity == 0f)
            SwitchState(State.Idle);
    }

    void Crouch()
    {
        canAttack = true;
        canHeal = true;

        if (!isCrouching)
        {
            bodyCollider.size = _colliderCrouchSize;
            bodyCollider.offset = _colliderCrouchOffset;
            _rigidBody.velocity = Vector2.zero;
            isCrouching = true;
        }
        else
        {
            if (!_input.crouchHeld)
            {
                if (!isOnGround)
                    SwitchState(State.Fall);
                else
                    SwitchState(State.Idle);
            }
        }
    }

    void Jump()
    {
        canAttack = true;
        Movement();

        if (!isJumping)
        {
            if (!isOnGround)
            {
                _extraJumpsCount--;
                isDoubleJump = true;
                AudioManager.PlayDoubleJumpAudio();
            }
            else
            {
                isJumping = true;
                AudioManager.PlayJumpAudio();
            }

            _curJumpTime = Time.time + _jumpHoldDuration;

            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, 0f);
            _rigidBody.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
        }
        else
        {
            if (_input.jumpHeld)
                _rigidBody.AddForce(new Vector2(0f, _jumpHoldForce), ForceMode2D.Impulse);

            if (_curJumpTime <= Time.time)
            {
                SwitchState(State.Fall);
            }
            else if(_input.jumpPressed && _extraJumpsCount > 0)
            {
                isJumping = false;
            }
        }
    }

    void Fall()
    {
        canAttack = true;
        Movement();

        if (isOnGround)
        {
            //Skip stand up animation
            if (_input.crouchHeld)
                SwitchState(State.Crouch);
            else
                SwitchState(State.Idle);
        }
    }

    void Climb()
    {
        if (!isClimbing)
        {
            isClimbing = true;
            canFlip = false;
        }
        else 
        {
            _rigidBody.AddForce(Vector2.up * _climbingSpeed, ForceMode2D.Force);

            if (!_climbCheck && !_climbCheck2)
            {
                _rigidBody.velocity = Vector2.zero;
                _rigidBody.AddForce(new Vector2(5f * direction, 0f), ForceMode2D.Impulse);
                SwitchState(State.Idle);
            }
        }
    }

    void Evade()
    {
        if (!isEvading)
        {
            _input.lastInputs.Clear();       //Clear all last inputs
            isEvading = true;
            canFlip = false;
            _attributes.isInvulnerable = true;
            _curEvadingDuration = Time.time + _evadingDuration;
            _rigidBody.velocity = Vector2.zero;
            bodyCollider.size = _colliderCrouchSize;
            bodyCollider.offset = _colliderCrouchOffset;
            _attributes.Stamina -= _evadingStaminaCosts - (int)bonusAttributes[BonusAttributes.EvadeCosts];

            _rigidBody.AddForce(new Vector2((_evadingDistance + bonusAttributes[BonusAttributes.EvadeDistance]) * _attributes.speedDivisor * direction, 0f), ForceMode2D.Impulse);

            AudioManager.PlayEvadeAudio();
        }
        else if(_curEvadingDuration <= Time.time)
        {
            _curEvadingCooldown = Time.time + _evadingCooldown;
            
            //Skip stand up animation
            if (_input.crouchHeld && isOnGround)
                SwitchState(State.Crouch);
            else
                SwitchState(State.Idle);
        }
        else if(_input.jumpPressed && _extraJumpsCount > 0)
        {
            _curEvadingCooldown = Time.time + _evadingCooldown;
            SwitchState(State.Jump);
        }
    }

    void Hanging()
    {
        if (!isHanging)
        {
            //...we have a ledge grab. Record the current position...
            Vector3 pos = _playerTransform.position;
            //...move the distance to the wall (minus a small amount)...
            pos.x += _wallCheck.distance * direction;
            //...move the player down to grab onto the ledge...
            pos.y -= _ledgeCheck.distance;
            //...apply this position to the platform...
            _playerTransform.position = pos;
            //...set the rigidbody to static...
            _rigidBody.bodyType = RigidbodyType2D.Static;
            //...finally, set isHanging to true
            isHanging = true;
            canFlip = false;
        }
        else 
        {
            if (_input.crouchPressed)
            {
                SwitchState(State.Fall);
            }
            else if (_input.evade)
            {
                SwitchState(State.Evade);
            }
            else if (_input.jumpPressed)
            {
                if (Mathf.Sign(_input.horizontal) == direction || _input.horizontal == 0)
                    SwitchState(State.Climb);
                else
                    SwitchState(State.Jump);
            }
        }
    }

    void Heal()
    {
        //_attributes.curHealingDelay CHECK THIS


        if (!isHealing)
        {
            _rigidBody.velocity = Vector2.zero;
            isHealing = true;
            // Healing applies from animation from attributes script
        }
        else if(!_input.healingPotionHeld || _attributes.curHealingPotionCount <= 0)
        {
            // Play animation which switch up a state
            _attributes.AnimationHealingEnd(true);
        }

        // Repeat audio clip
        if (!AudioManager.current.playerSource.isPlaying)
            AudioManager.PlayHealingAudio();
    }

    void HealingEnd()
    {
        _attributes.AnimationHealingEnd(false);
        SwitchState(State.Idle);
    }

    void Attack()
    {
        if(!isAttacking)
        {
            isAttacking = true;
        }
    }

    void Hurt()
    {
        if (_curDazedTime != 0f && _curDazedTime <= Time.time)
        {
            _curDazedTime = 0f;
            hurtType = HurtType.None;
            _targetRigidBody = null;
            bodyCollider.sharedMaterial = _physicMaterial;

            if (_rigidBody.gravityScale == 0f)
                _rigidBody.gravityScale = 1f;

            SwitchState(State.Idle);
        }
        else
        {
            isHurt = true;
            //SpellCastEnd();
            bodyCollider.sharedMaterial = null;
            canFlip = false;
            _attributes.AnimationHealingEnd(false);

            //Follow for catch source
            if (hurtType == HurtType.Catch)
            {
                _rigidBody.velocity = _targetRigidBody.velocity;
            }
            else if (hurtType == HurtType.Stun)
            {
                _rigidBody.velocity = Vector2.zero;
                _rigidBody.gravityScale = 0f;
            }
        }
    }

    public void GetCaught(HurtType hurtType, Rigidbody2D targetRigidBody)
    {
        this.hurtType = hurtType;
        _targetRigidBody = targetRigidBody;

        SwitchState(State.Hurt);
    }

    public void Repulse(Vector2 repulseDistantion, float dazedTime)
    {
        _rigidBody.velocity = Vector2.zero;
        hurtType = HurtType.Repulsion;
        _rigidBody.AddForce(repulseDistantion, ForceMode2D.Impulse);
        _curDazedTime = dazedTime + Time.time;

        SwitchState(State.Hurt);
    }

    public void Stunned(float dazedTime)
    {
        _curDazedTime = dazedTime + Time.time;

        SwitchState(State.Hurt);
    }

    public void CastSpell(AnimationClip castAnim)
    {
        this.castAnim = castAnim;
        _rigidBody.velocity = Vector2.zero;
        SwitchState(State.Cast);
        canFlip = false;
        isCast = true;
    }

    public void NextSpellAttack(AnimationClip castAnim)
    {
        this.castAnim = castAnim;
    }

    public void SpellCastEnd()
    {
        SwitchState(State.Idle);
    }

    public void FlipCharacterDirection()
    {
        if (!canFlip)
            return;

        direction *= -1;
        _sprite.flipX = !_sprite.flipX;
    }

    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length)
    {
        //If ray used without layer
        return Raycast(offset, rayDirection, length, _groundLayer);
    }

    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
    {
        Vector2 pos = _playerTransform.position;
        //Cast ray from char+offset to direction on set length
        RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length, mask);

        if (drawDebugRaycast)
        {
            //If ray takes hit from object on setted layer color be a red, if not - cyan
            Color color = hit ? Color.red : Color.cyan;

            Debug.DrawRay(pos + offset, rayDirection * length, color);
        }

        return hit;
    }
}