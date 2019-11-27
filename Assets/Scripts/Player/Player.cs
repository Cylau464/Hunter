using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Idle, Move, Jump, Crouch, Attack, Climb, Hang, Hooked, Hurt, Dead };

public class Player : MonoBehaviour
{
    [SerializeField] bool drawDebugRaycast = true;

    [Header("Movement Properties")]
    [SerializeField] float speed = 8f;                        //Horizontal speed
    [SerializeField] float crouchSpeedDivisor = 3f;           //Speed divisor for crouching (Not used now)
    [SerializeField] float coyoteDuration = .05f;             //Little delay for jump after character start falling down
    [SerializeField] float climbingSpeed = 70f;               //70 = 20 because 50 it's gravity force

    [Header("Jump Properties")]
    [SerializeField] float jumpForce = 6.3f;                  //Used once after pressing
    [SerializeField] float jumpHoldForce = 2.2f;              //Used all time if key hold
    [SerializeField] float jumpHoldDuration = .1f;            //Max duration of holding jump
    //public float hangingJumpForce = 15f;	        //Force of wall hanging jump
    [SerializeField] float maxFallSpeed = -25f;               //Max falling speed 
    [SerializeField] int extraJumps = 1;                      //Count of extra jumps

    [Header("Enviropment Check Properties")]
    [SerializeField] float footOffset = .3f;                  //Offset between foots
    [SerializeField] float headClearence = .7f;               //Necessary free space above head for stand up or/and jump
    [SerializeField] float eyeHeight = .2f;			          //Height of eye for wall checks
    [SerializeField] float reachOffset = .7f;                 //X offset for wall grabbing
    [SerializeField] float grabDistance = .4f;                //The reach distance for wall grabs
    [SerializeField] float groundDistance = .2f;              //Distance to check on ground character or not
    [SerializeField] float evadingDuration = .35f;            //Duration of evade state and animation
    [SerializeField] float evadingDistance = 8f;              //Distance of evade
    [SerializeField] float evadingCooldown = 1f;              //Evade cooldown in sec
    [SerializeField] int direction { get; private set; } = 1; //Character direction
    [SerializeField] LayerMask groundLayer = 1 << 9;          //9 - Platforms layer

    [Header("Status Flags")]
    PlayerState currentState = PlayerState.Idle;
    [SerializeField]
    bool isOnGround,
    isJumping,
    isDoubleJump,
    isHeadBlocked,
    isCrouching,
    isAttacking,
    isEvading,
    isHanging,
    isClimbing,
    isHooked,
    isHurt,
    isDead,
    canFlip;
    /*public bool isJumping;
    public bool isDoubleJump;
    public bool isHeadBlocked;
    public bool isCrouching;
    public bool isAttacking;
    public bool isEvading;
    public bool isHanging;
    public bool isClimbing;
    bool isHooked;
    public bool isHurt;
    public bool isDead;
    public bool canFlip = true;*/                     //Check whether the character can flip or not
    public bool moveInputPriority = true;           //What the hell is this?

    [HideInInspector] 
    public float speedDivisor = 1f;                 //Used to decrease horizontal speed

    //REFERENCES!!!:japanise_goblin:
    PlayerInput input;
    Rigidbody2D rigidBody;
    BoxCollider2D bodyCollider;
    public SpriteRenderer sprite;                   //Maybe create property for public get?
    PlayerAttack attack;
    Transform playerTransform;
    WeaponAtributes weapon;
    Hook hook;
    Transform hookTransform;

    WeaponType weaponType;

    int extraJumpsCount;                          //Current count extra jumps

    float curCoyoteTime;                          //Current coyote time
    float playerHeight;                           //Based on vertical collider size
    float curJumpTime;                            //Current time in jump (Cant be higher max jumpHoldDuration)
    float curEvadingCooldown;                     //Current evade cooldown time
    float curEvadingDuration;                     //Current evade duration time

    Vector2 colliderStandSize;                    //Collider size for standing position
    Vector2 colliderStandOffset;                  //Collider offset for standing position
    Vector2 colliderCrouchSize;                   //Collider size for crouching position
    Vector2 colliderCrouchOffset;                 //Collider offset for crouching position
    Vector2 aimDirection;

    const float _smallAmount = .05f;              //A small amount used for hanging position

    void Start()
    {
        //Cache references
        input                   = GetComponent<PlayerInput>();
        rigidBody               = GetComponent<Rigidbody2D>();
        bodyCollider            = GetComponent<BoxCollider2D>();
        sprite                  = GetComponent<SpriteRenderer>();
        attack                  = GetComponent<PlayerAttack>();
        playerTransform         = GetComponent<Transform>();
        weapon                  = GetComponentInChildren<WeaponAtributes>();
        hook                    = GetComponentInChildren<Hook>();
        
        playerHeight            = bodyCollider.size.y;

        colliderStandSize       = bodyCollider.size;
        colliderStandOffset     = bodyCollider.offset;
        //Decrease collider size and offset for this strange values. Need to change they on variables with percent not numbers
        colliderCrouchSize      = new Vector2(bodyCollider.size.x, .85f);
        colliderCrouchOffset    = new Vector2(bodyCollider.offset.x, -.17f);

        weaponType = weapon.weaponType;
    }

    private void OnGUI()
    {
        GUI.TextField(new Rect(10, 10, 100, 200), rigidBody.velocity.ToString() + "\n" + playerTransform.position + "\nAim" + aimDirection + "\nAngle " + (Vector2.Angle(transform.right, Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)) * Mathf.Sign(Vector3.Dot(transform.forward, Vector3.Cross(transform.right, Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position))))));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            Time.timeScale = 0.1f;
        if (Input.GetKeyDown(KeyCode.V))
            Time.timeScale = 1f;
        if (Input.GetKeyDown(KeyCode.T))
            Time.timeScale = 0.01f;

        //If hook is not throwing or pulling
        if (input.hook && hook.throwCoroutine == null && hook.pullCoroutine == null)
        {
            //Value 10f in vector3 compensates for the Z position of the camera. Need to get this value from the camera (-Input.mousePosition.z)
            hook.throwCoroutine = hook.StartCoroutine(hook.Throw(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f))));
        }
        //Reset variables when character landed
        if (isOnGround)
        {
            curCoyoteTime = Time.time + coyoteDuration;
            extraJumpsCount = extraJumps;
        }

        //_aimDirection = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);

        //if (Mathf.Sign(_aimDirection.x) != direction)
        //    FlipCharacterDirection();
    }

    void FixedUpdate()
    {
        if(isHooked)
            Hooked();
        else
        {
            PhysicsCheck();
            GroundMovement();
            AirMovement();
        }

        if (isAttacking && !isOnGround && attack.attackState != AttackState.End)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
            rigidBody.gravityScale = isOnGround ? rigidBody.gravityScale : 0f;
        }
        else if (_rigidBody.gravityScale != 1f)
            rigidBody.gravityScale = 1f;

        switch(currentState)
        {
            case PlayerState.Idle:
            Idle();
            break;
            case PlayerState.Move:
            Move();
            break;
            case PlayerState.Jump:
            Jump();
            break;
            case PlayerState.Crouch:
            Crouch();
            break;
            case PlayerState.Climb:
            Climb();
            break;
            case PlayerState.Hang:
            Hang();
            break;
            case PlayerState.Attack:
            Attack();
            break;
            case PlayerState.Hooked:
            Hooked();
            break;
            case PlayerState.Hurt:
            Hurt();
            break;
            case PlayerState.Dead:
            Dead();
            break;
        }
    }

    void SwitchState(PlayerState newState)
    {
        isOnGround = isJumping = isDoubleJump = isHeadBlocked = isCrouching = isAttacking =
        isEvading = isHanging = isClimbing = isHooked = isHurt = isDead = false;

        currentState = newState;
    }

    void PhysicsCheck()
    {
        //Start by assuming the player isn't on the ground and the head isn't blocked
        isOnGround = false;
        isHeadBlocked = false;

        //Cast rays for the left and right foot
        RaycastHit2D leftCheck = Raycast(new Vector2(-footOffset, -playerHeight / 2f), Vector2.down, groundDistance);
        RaycastHit2D rightCheck = Raycast(new Vector2(footOffset, -playerHeight / 2f), Vector2.down, groundDistance);
        //Ground check, but not start jump
        if ((leftCheck || rightCheck) && !isJumping)
            isOnGround = true;

        //Check free space above the character (for crouch)
        RaycastHit2D headCheck = Raycast(Vector2.zero, Vector2.up, headClearence);

        if (headCheck)
            isHeadBlocked = true;

        //Determine the direction of the wall grab attempt
        Vector2 grabDir = new Vector2(direction, 0f);

        //Cast three rays to look for a wall grab
        RaycastHit2D blockedCheck   = Raycast(new Vector2(footOffset * direction, playerHeight / 2), grabDir, grabDistance);
        RaycastHit2D ledgeCheck     = Raycast(new Vector2(reachOffset * direction, playerHeight / 2), Vector2.down, grabDistance);
        RaycastHit2D wallCheck      = Raycast(new Vector2(footOffset * direction, eyeHeight), grabDir, grabDistance);
        //Cast ray to check for a character climbed
        RaycastHit2D climbCheck     = Raycast(new Vector2(footOffset * direction, -playerHeight / 2), grabDir, grabDistance);

        //If the player is off the ground AND is not hanging AND is falling AND
        //found a ledge AND found a wall AND the grab is NOT blocked...
        if (!isOnGround && !isHanging && !isEvading && !isAttacking && rigidBody.velocity.y <= 0f &&
            ledgeCheck && wallCheck && !blockedCheck && !isClimbing)
        {
            //...we have a ledge grab. Record the current position...
            Vector3 pos = playerTransform.position;
            //...move the distance to the wall (minus a small amount)...
            pos.x += wallCheck.distance * direction;
            //...move the player down to grab onto the ledge...
            pos.y -= ledgeCheck.distance;
            //...apply this position to the platform...
            playerTransform.position = pos;
            //...set the rigidbody to static...
            rigidBody.bodyType = RigidbodyType2D.Static;
            //...finally, set isHanging to true
            isHanging = true;
        }

        //Evading
        if (/*input.evade*/input.lastInputs.Contains(InputsEnums.Evade) && attack.attackState != AttackState.Damage && !isEvading && curEvadingCooldown <= Time.time)
        {
            //If use evade in hanging state
            if (rigidBody.bodyType == RigidbodyType2D.Static)
                rigidBody.bodyType = RigidbodyType2D.Dynamic;

            if (direction != Mathf.Sign(input.horizontal) && input.horizontal != 0)
                FlipCharacterDirection();

            input.lastInputs.Clear();       //Clear all last inputs
            isAttacking             = false;
            isEvading               = true;
            isClimbing              = false;
            isHanging               = false;
            input.horizontalAccess  = false;
            canFlip                 = false;
            curEvadingDuration      = Time.time + evadingDuration;
            rigidBody.velocity      = Vector2.zero;
            Crouch();

            //Choose evade direction according to the key is pressed or not
            if (input.horizontal != 0)
                rigidBody.AddForce(new Vector2(evadingDistance /** 1.5f*/ * Mathf.Sign(input.horizontal), 0f), ForceMode2D.Impulse);
            else
                rigidBody.AddForce(new Vector2(evadingDistance * direction, 0f), ForceMode2D.Impulse);

        }
        //If already evading
        else if (isEvading && (curEvadingDuration <= Time.time || (input.jumpPressed && extraJumpsCount > 0)))
        {
            isEvading              = false;
            input.horizontalAccess = true;
            canFlip                = true;
            curEvadingCooldown     = Time.time + evadingCooldown;

            //Skip stand up animation if pressed crouch key
            if (input.crouchHeld && isOnGround)
                Crouch();
        }
        //Clear up all evade inputs from the inputs list if CD has not yet passed
        if (curEvadingCooldown > Time.time)
            input.lastInputs.RemoveAll(x => x == InputsEnums.Evade);

        //Climbing
        if (!isEvading)
        {
            if (!isClimbing && !isAttacking && climbCheck && !wallCheck && input.horizontal != 0)
            {
                Climb();
            }
            if (isClimbing)
            {
                rigidBody.AddForce(Vector2.up * climbingSpeed, ForceMode2D.Force);

                if (!climbCheck)
                {
                    isClimbing = false;
                    rigidBody.velocity = Vector2.zero;
                    rigidBody.AddForce(new Vector2(5 * direction, 0), ForceMode2D.Impulse);
                    Invoke("HorizontalAcces", 0.05f);
                }
            }
        }
    }
}