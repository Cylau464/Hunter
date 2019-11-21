using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool drawDebugRaycast = true;

    [Header("Movement Properties")]
    public float speed = 8f;
    public float crouchSpeedDivisor = 3f;
    public float coyoteDuration = .05f;
    public float climbingSpeed = 70f;        //70 = 20 because 50 it's gravity force

    [Header("Jump Properties")]
    public float jumpForce = 6.3f;
    public float jumpHoldForce = 2.2f;
    public float jumpHoldDuration = .1f;
    public float hangingJumpForce = 15f;	//Force of wall hanging jump
    public float maxFallSpeed = -25f;
    public int extraJumps = 1;

    [Header("Enviropment Check Properties")]
    public float footOffset = .3f;
    public float headClearence = .7f;
    public float eyeHeight = .2f;			//Height of wall checks
    public float reachOffset = .7f;         //X offset for wall grabbing
    public float grabDistance = .4f;        //The reach distance for wall grabs
    public float groundDistance = .2f;
    public float evadingDuration = .35f;
    public float evadingDistance = 8f;
    public float evadingCooldown = 1f;
    public int direction { get; private set; } = 1;
    public LayerMask groundLayer = 1 << 9;  //9 - Platforms layer

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
    public bool isHurt;
    public bool isDead;
    public bool canFlip = true;
    public bool moveInputPriority = true;

    [HideInInspector] public float speedDivisor = 1f;

    PlayerInput input;
    Rigidbody2D rigidBody;
    BoxCollider2D bodyCollider;
    public SpriteRenderer sprite;
    PlayerAttack attack;
    Transform playerTransform;
    WeaponAtributes weapon;

    WeaponType _weaponType;

    int _extraJumpsCount;

    float _coyoteTime;
    float _playerHeight;
    float _jumpTime;
    float _evadingCooldown;
    float _evadingDuration;

    Vector2 _colliderStandSize;
    Vector2 _colliderStandOffset;
    Vector2 _colliderCrouchSize;
    Vector2 _colliderCrouchOffset;
    Vector2 _aimDirection;

    const float smallAmount = .05f;         //A small amount used for hanging position

    void Start()
    {
        input = GetComponent<PlayerInput>();
        rigidBody = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        attack = GetComponent<PlayerAttack>();
        playerTransform = GetComponent<Transform>();
        weapon = GetComponentInChildren<WeaponAtributes>();

        _playerHeight = bodyCollider.size.y;

        _colliderStandSize = bodyCollider.size;
        _colliderStandOffset = bodyCollider.offset;

        _colliderCrouchSize = new Vector2(bodyCollider.size.x, .85f);
        _colliderCrouchOffset = new Vector2(bodyCollider.offset.x, -.17f);
        //Dividing jump height by weapon mass
        //jumpForce /= weapon.weaponMass;
        //jumpHoldForce /= weapon.weaponMass;
        _weaponType = weapon.weaponType;
    }

    private void OnGUI()
    {
        GUI.TextField(new Rect(10, 10, 100, 200), rigidBody.velocity.ToString() + "\n" + playerTransform.position + "\nAim" + _aimDirection + "\nAngle " + (Vector2.Angle(transform.right, Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)) * Mathf.Sign(Vector3.Dot(transform.forward, Vector3.Cross(transform.right, Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position))))));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            Time.timeScale = 0.1f;
        if (Input.GetKeyDown(KeyCode.V))
            Time.timeScale = 1f;
        if (Input.GetKeyDown(KeyCode.T))
            Time.timeScale = 0.01f;

        if (isOnGround)
        {
            _coyoteTime = Time.time + coyoteDuration;
            _extraJumpsCount = extraJumps;
        }

        //_aimDirection = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);

        //if (Mathf.Sign(_aimDirection.x) != direction)
        //    FlipCharacterDirection();
    }

    void FixedUpdate()
    {
        PhysicsCheck();
        GroundMovement();
        AirMovement();

        if (isAttacking && !isOnGround && attack.attackState != AttackState.End)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
            if (!isOnGround)
                rigidBody.gravityScale = 0;
        }
        else if (rigidBody.gravityScale != 1)
            rigidBody.gravityScale = 1;
    }

    void PhysicsCheck()
    {
        //Start by assuming the player isn't on the ground and the head isn't blocked
        isOnGround = false;
        isHeadBlocked = false;

        //Cast rays for the left and right foot
        RaycastHit2D leftCheck = Raycast(new Vector2(-footOffset, -_playerHeight / 2f), Vector2.down, groundDistance);
        RaycastHit2D rightCheck = Raycast(new Vector2(footOffset, -_playerHeight / 2f), Vector2.down, groundDistance);

        if ((leftCheck || rightCheck) && !isJumping)
            isOnGround = true;

        //Check free space above the character (for crouch)
        RaycastHit2D headCheck = Raycast(Vector2.zero, Vector2.up, headClearence);

        if (headCheck)
            isHeadBlocked = true;

        //Determine the direction of the wall grab attempt
        Vector2 grabDir = new Vector2(direction, 0f);

        //Cast three rays to look for a wall grab
        RaycastHit2D blockedCheck = Raycast(new Vector2(footOffset * direction, _playerHeight / 2), grabDir, grabDistance);
        RaycastHit2D ledgeCheck = Raycast(new Vector2(reachOffset * direction, _playerHeight / 2), Vector2.down, grabDistance);
        RaycastHit2D wallCheck = Raycast(new Vector2(footOffset * direction, eyeHeight), grabDir, grabDistance);
        RaycastHit2D climbCheck = Raycast(new Vector2(footOffset * direction, -_playerHeight / 2), grabDir, grabDistance);

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
        if (/*input.evade*/input.lastInputs.Contains(InputsEnums.Evade) && attack.attackState != AttackState.Damage && !isEvading && _evadingCooldown <= Time.time)
        {
            //If use evade in hanging state
            if (rigidBody.bodyType == RigidbodyType2D.Static)
                rigidBody.bodyType = RigidbodyType2D.Dynamic;

            if (direction != Mathf.Sign(input.horizontal) && input.horizontal != 0)
                FlipCharacterDirection();

            input.lastInputs.Clear();       //Clear all last inputs
            isAttacking = false;
            isEvading = true;
            isClimbing = false;
            isHanging = false;
            input.horizontalAccess = false;
            canFlip = false;
            _evadingDuration = Time.time + evadingDuration;
            Crouch();
            rigidBody.velocity = Vector2.zero;

            if (input.horizontal != 0)
                rigidBody.AddForce(new Vector2(evadingDistance * 1.5f * Mathf.Sign(input.horizontal), 0f), ForceMode2D.Impulse); //input horizontal instead direction for evade after attack
            else
                rigidBody.AddForce(new Vector2(evadingDistance * direction, 0f), ForceMode2D.Impulse);

        }
        else if (isEvading && (_evadingDuration <= Time.time || (input.jumpPressed && _extraJumpsCount > 0)))
        {
            isEvading = false;
            input.horizontalAccess = true;
            canFlip = true;
            _evadingCooldown = Time.time + evadingCooldown;

            //Skip stand up animation
            if (input.crouchHeld && isOnGround)
                Crouch();
        }
        //Clear up all evade inputs from the inputs list if CD has not yet passed
        if (_evadingCooldown > Time.time)
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

    void GroundMovement()
    {
        //If currently hanging, the player can't move to exit
        if (isHanging)
            return;

        if (input.crouchHeld && !isCrouching && isOnGround && attack.attackState != AttackState.Damage)
            Crouch();
        else if (!input.crouchHeld && isCrouching && !isEvading)
            StandUp();
        else if (!isOnGround && isCrouching && !isEvading)
            StandUp();

        float xVelocity;

        if (input.horizontalAccess && !isAttacking || attack.weaponType == WeaponType.Range)
        {
            xVelocity = speed * speedDivisor/*/ weapon.weaponMass*/ * input.horizontal;
            rigidBody.velocity = new Vector2(xVelocity, rigidBody.velocity.y);
            //Флипать чара, если он смотрит в протиповоложную от движения сторону
            if (xVelocity * direction < 0f)
                FlipCharacterDirection();
        }
        else
        {
            //xVelocity = 0f;
            if (direction != Mathf.Sign(input.horizontal) && input.horizontal != 0)
                FlipCharacterDirection();
        }

        ////Decrease move speed on crouch
        //if (isCrouching)
        //    xVelocity /= crouchSpeedDivisor;
    }

    void AirMovement()
    {
        //If the player is currently hanging...
        if (isHanging)
        {
            //If crouch is pressed...
            if (input.crouchPressed || input.evade)
            {
                //...set the rigidbody to dynamic...
                rigidBody.bodyType = RigidbodyType2D.Dynamic;
                //...let go and exit
                isHanging = false;
                return;
            }

            //If jump is pressed...
            if (input.jumpPressed)
            {
                //...set the rigidbody to dynamic and apply a jump force...
                rigidBody.bodyType = RigidbodyType2D.Dynamic;
                //...let go...
                isHanging = false;
                if (Mathf.Sign(input.horizontal) == direction || input.horizontal == 0)
                    Climb();
                //rigidBody.AddForce(new Vector2(0f, hangingJumpForce), ForceMode2D.Impulse);
                //...and exit
                return;
            }
        }

        //Jump
        if (input.jumpPressed && attack.attackState != AttackState.Damage && (isOnGround || _coyoteTime > Time.time))
        {
            if (isCrouching && !isHeadBlocked)
                StandUp();

            isAttacking = false;
            isOnGround = false;
            isJumping = true;
            _jumpTime = Time.time + jumpHoldDuration;

            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
            rigidBody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
        //Higher jump
        else if (isJumping)
        {
            if (input.jumpHeld)
                rigidBody.AddForce(new Vector2(0f, jumpHoldForce), ForceMode2D.Impulse);

            if (_jumpTime <= Time.time)
            {
                isJumping = false;
                isDoubleJump = false;
            }
        }
        //Double jumps
        else if (!isOnGround && attack.attackState != AttackState.Damage && input.jumpPressed && _extraJumpsCount > 0 && !isHanging && !isClimbing /*&& !input.crouchHeld*/)
        {
            isAttacking = false;
            isJumping = true;
            isDoubleJump = true;
            _jumpTime = Time.time + jumpHoldDuration;

            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
            rigidBody.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

            _extraJumpsCount--;
        }

        //Фиксирование максимальной скорости падения
        if (rigidBody.velocity.y < maxFallSpeed)
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, maxFallSpeed);
    }

    public void FlipCharacterDirection()
    {
        if (!canFlip)
            return;

        direction *= -1;
        sprite.flipX = !sprite.flipX;
    }

    public void StandUp()
    {
        if (isHeadBlocked)
            return;

        isCrouching = false;
        input.horizontalAccess = true;
        bodyCollider.size = _colliderStandSize;
        bodyCollider.offset = _colliderStandOffset;
    }

    public void Crouch()
    {
        isCrouching = true;
        isAttacking = false;
        input.horizontalAccess = false;

        bodyCollider.size = _colliderCrouchSize;
        bodyCollider.offset = _colliderCrouchOffset;
        rigidBody.velocity = Vector2.zero;
    }

    void Climb()
    {
        int numColliders = 10;
        Collider2D[] colliders = new Collider2D[numColliders];
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(1 << 9); //9 - platform layer
        int colliderCount = bodyCollider.OverlapCollider(contactFilter, colliders);

        if (colliderCount > 0)
        {
            isJumping = false;
            isDoubleJump = false;
            isClimbing = true;
            input.horizontalAccess = false;
            rigidBody.velocity = Vector2.zero;
        }
    }
    
    void HorizontalAcces()
    {
        input.horizontalAccess = true;
    }

    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length)
    {
        //Если луч без указания слоя
        return Raycast(offset, rayDirection, length, groundLayer);
    }

    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
    {
        Vector2 pos = playerTransform.position;

        //Рассчёт луча от чара + отступ до ближайшего объекта на указанном слое (если не указан см. выше)
        RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length, mask);

        if (drawDebugRaycast)
        {
            Color color = hit ? Color.red : Color.cyan;

            Debug.DrawRay(pos + offset, rayDirection * length, color);
        }

        //Вернуть луч
        return hit;
    }
}
