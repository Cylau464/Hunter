﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] bool drawDebugRaycast = true;

    [Header("Movement Properties")]
    [SerializeField] float speed                    = 8f;           //Horizontal speed
    //[SerializeField] float crouchSpeedDivisor       = 3f;           //Speed divisor for crouching (Not used now)
    [SerializeField] float coyoteDuration           = .05f;         //Little delay for jump after character start falling down
    [SerializeField] float climbingSpeed            = 70f;          //70 = 20 because 50 it's gravity force

    [Header("Jump Properties")]
    [SerializeField] float jumpForce                = 6.3f;         //Used once after pressing
    [SerializeField] float jumpHoldForce            = 2.2f;         //Used all time if key hold
    [SerializeField] float jumpHoldDuration         = .1f;          //Max duration of holding jump
    //public float hangingJumpForce                 = 15f;	        //Force of wall hanging jump
    [SerializeField] float maxFallSpeed             = -25f;         //Max falling speed 
    [SerializeField] int extraJumps                 = 1;            //Count of extra jumps

    [Header("Enviropment Check Properties")]
    [SerializeField] float footOffset               = .3f;          //Offset between foots
    [SerializeField] float headClearence            = .7f;          //Necessary free space above head for stand up or/and jump
    [SerializeField] float eyeHeight                = .2f;			//Height of eye for wall checks
    [SerializeField] float reachOffset              = .7f;          //X offset for wall grabbing
    [SerializeField] float grabDistance             = .4f;          //The reach distance for wall grabs
    [SerializeField] float groundDistance           = .2f;          //Distance to check on ground character or not
    [SerializeField] float evadingDuration          = .35f;         //Duration of evade state
    [SerializeField] float evadingDistance          = 8f;           //Distance of evade
    [SerializeField] float evadingCooldown          = 1f;           //Evade cooldown in sec
    [SerializeField] int evadingStaminaCosts        = 25;           //Spent stamina for using evade
    public int direction { get; private set; }      = 1;            //Character direction
    [SerializeField] LayerMask groundLayer          = 1 << 9;       //9 - Platforms layer

    [Header("Attributes Bonus")]
    public AttributesDictionary bonusAttributes = new AttributesDictionary();

    [Header("Hurt Properties")]
    float curDazedTime = 0f;
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
    bool isHooked;
    public bool isCast;
    public bool isHealing;
    public bool isHurt;
    public bool isDead;
    public bool canFlip = true;
    public bool moveInputPriority = true;          //What the hell is this?

    PlayerInput input;
    Rigidbody2D rigidBody;
    [HideInInspector] public BoxCollider2D bodyCollider;
    SpriteRenderer sprite;                         //Maybe create property for public get?
    PlayerAttack attack;
    PlayerAttributes attributes;
    Transform playerTransform;
    Hook hook;
    Transform hookTransform;
    Rigidbody2D targetRigidBody;
    PhysicsMaterial2D physicMaterial;

    int extraJumpsCount;                          //Current count extra jumps

    float curCoyoteTime;                          //Current coyote time
    float playerHeight;                           //Based on vertical collider size
    float curJumpTime;                            //Current time in jump (Cant be higher max jumpHoldDuration)
    float curEvadingCooldown;                     //Current evade cooldown time left
    float curEvadingDuration;                     //Current evade duration time

    Vector2 colliderStandSize;                    //Collider size for standing position
    Vector2 colliderStandOffset;                  //Collider offset for standing position
    Vector2 colliderCrouchSize;                   //Collider size for crouching position
    Vector2 colliderCrouchOffset;                 //Collider offset for crouching position
    //Vector2 aimDirection;

    const float smallAmount = .05f;               //A small amount used for hanging position and something else

    public AnimationClip castAnim = null;         //Set here default cast animation

    void Start()
    {
        //Get all references
        input                   = GetComponent<PlayerInput>();
        rigidBody               = GetComponent<Rigidbody2D>();
        bodyCollider            = GetComponent<BoxCollider2D>();
        sprite                  = GetComponent<SpriteRenderer>();
        attack                  = GetComponent<PlayerAttack>();
        attributes               = GetComponent<PlayerAttributes>();
        playerTransform         = GetComponent<Transform>();
        hook                    = GetComponentInChildren<Hook>();
        physicMaterial          = bodyCollider.sharedMaterial;

        playerHeight            = bodyCollider.size.y;

        colliderStandSize       = bodyCollider.size;
        colliderStandOffset     = bodyCollider.offset;

        //Decrease collider size and offset for this strange values. Need to change they on variables with percent, not numbers
        colliderCrouchSize      = new Vector2(bodyCollider.size.x, .85f);
        colliderCrouchOffset    = new Vector2(bodyCollider.offset.x, -.17f);
    }

    private void OnGUI()
    {
       // GUI.TextField(new Rect(10, 10, 150, 200), rigidBody.velocity.ToString() + "\n" + playerTransform.position + "\nAngle " + (Vector2.Angle(transform.right, Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)) * Mathf.Sign(Vector3.Dot(transform.forward, Vector3.Cross(transform.right, Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position))))) + "\ninputs " + System.String.Join("", new List<InputsEnum>(input.lastInputs).ConvertAll(i => i.ToString()).ToArray()));
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
            curCoyoteTime = Time.time + coyoteDuration;
            extraJumpsCount = extraJumps + (bonusAttributes.ContainsKey(BonusAttributes.JumpCount) ? (int) bonusAttributes[BonusAttributes.JumpCount] : 0);
        }

        //aimDirection = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);

        //if (Mathf.Sign(aimDirection.x) != direction)
        //    FlipCharacterDirection();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isHooked)
            Hooked();
        else if (isHurt)
            Hurt();
        else
        {
            PhysicsCheck();
            Healing();
            GroundMovement();
            AirMovement();
        }

        if (isAttacking && !isOnGround && attack.attackState != AttackState.End && attack.attackType != AttackTypes.TopDown)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);

            if (!isOnGround)
                rigidBody.gravityScale = 0;
        }
        else if (rigidBody.gravityScale != 1 && !isHurt)
            rigidBody.gravityScale = 1;
    }

    void PhysicsCheck()
    {
        //Start by assuming the player isn't on the ground and the head isn't blocked
        isOnGround = false;
        isHeadBlocked = false;

        //Cast rays for the left and right foot
        RaycastHit2D leftCheck      = Raycast(new Vector2(-footOffset, -playerHeight / 2f), Vector2.down, groundDistance);
        RaycastHit2D rightCheck     = Raycast(new Vector2(footOffset, -playerHeight / 2f), Vector2.down, groundDistance);

        if ((leftCheck || rightCheck) && !isJumping)
            isOnGround = true;

        //Check free space above the character (for crouch)
        RaycastHit2D headCheck = Raycast(Vector2.zero, Vector2.up, headClearence);

        if (headCheck)
            isHeadBlocked = true;

        //Determine the direction of the wall grab attempt
        Vector2 grabDir = new Vector2(direction, 0f);

        //Cast three rays to look for a wall grab
        RaycastHit2D blockedCheck   = Raycast(new Vector2(footOffset * direction, playerHeight / 2f), grabDir, grabDistance);
        RaycastHit2D ledgeCheck     = Raycast(new Vector2(reachOffset * direction, playerHeight / 2f), Vector2.down, grabDistance);
        RaycastHit2D wallCheck      = Raycast(new Vector2(footOffset * direction, eyeHeight), grabDir, grabDistance);
        RaycastHit2D climbCheck     = Raycast(new Vector2(footOffset * direction, -playerHeight / 2f), grabDir, grabDistance);
        RaycastHit2D climbCheck2    = Raycast(new Vector2(footOffset * direction, -playerHeight / 4f), grabDir, grabDistance);

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
        if (input.lastInputs.Contains(InputsEnum.Evade) && attack.attackState != AttackState.Damage && !isEvading && !isCast && !isHealing && curEvadingCooldown <= Time.time)
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
            CancelInvoke("HorizontalAccess");
            input.horizontalAccess = false;
            canFlip = false;
            attributes.isInvulnerable = true;
            curEvadingDuration = Time.time + evadingDuration;
            Crouch();
            rigidBody.velocity = Vector2.zero;
            attributes.Stamina -= evadingStaminaCosts - (int)bonusAttributes[BonusAttributes.EvadeCosts];

            if (input.horizontal != 0)
                rigidBody.AddForce(new Vector2((evadingDistance * 1.5f + bonusAttributes[BonusAttributes.EvadeDistance]) * attributes.speedDivisor * Mathf.Sign(input.horizontal), 0f), ForceMode2D.Impulse); //input horizontal instead direction for evade after attack
            else
                rigidBody.AddForce(new Vector2((evadingDistance + bonusAttributes[BonusAttributes.EvadeDistance]) * attributes.speedDivisor * direction, 0f), ForceMode2D.Impulse);

            AudioManager.PlayEvadeAudio();

        }
        else if (isEvading && (curEvadingDuration <= Time.time || (input.jumpPressed && extraJumpsCount > 0)))
        {
            isEvading = false;
            input.horizontalAccess = true;
            canFlip = true;
            attributes.isInvulnerable = false;
            curEvadingCooldown = Time.time + evadingCooldown;

            //Skip stand up animation
            if (input.crouchHeld && isOnGround)
                Crouch();
        }
        //Clear up all evade inputs from the inputs list if CD has not yet passed or not enough stamina
        if (curEvadingCooldown > Time.time || attributes.Stamina < evadingStaminaCosts)
            input.lastInputs.RemoveAll(x => x == InputsEnum.Evade);

        //Climbing
        if (!isEvading)
        {
            if (!isClimbing && !isAttacking && (climbCheck || climbCheck2) && !wallCheck && input.horizontal != 0)
            {
                Climb();
            }
            if (isClimbing)
            {
                rigidBody.AddForce(Vector2.up * climbingSpeed, ForceMode2D.Force);

                if (!climbCheck && !climbCheck2)
                {
                    isClimbing = false;
                    rigidBody.velocity = Vector2.zero;
                    rigidBody.AddForce(new Vector2(5f * direction, 0f), ForceMode2D.Impulse);
                    Invoke("HorizontalAccess", 0.05f);
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

        if (input.horizontalAccess && (!isAttacking || attack.weaponAttackType == WeaponAttackType.Range))
        {
            xVelocity = ((speed * attributes.speedDivisor + (bonusAttributes.ContainsKey(BonusAttributes.Speed) ? bonusAttributes[BonusAttributes.Speed] : 0)) * input.horizontal) / attack.weaponMass;
            rigidBody.velocity = new Vector2(xVelocity, rigidBody.velocity.y);
            //Flip caharcter if his direction != input horizontal
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
        if (input.jumpPressed && attack.attackState != AttackState.Damage && !isHealing && (isOnGround || curCoyoteTime > Time.time))
        {
            if (isCrouching && !isHeadBlocked)
                StandUp();

            isAttacking = false;
            isOnGround = false;
            isJumping = true;
            curJumpTime = Time.time + jumpHoldDuration;

            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
            rigidBody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

            AudioManager.PlayJumpAudio();
        }
        //Higher jump
        else if (isJumping)
        {
            if (input.jumpHeld)
                rigidBody.AddForce(new Vector2(0f, jumpHoldForce), ForceMode2D.Impulse);

            if (curJumpTime <= Time.time)
            {
                isJumping = false;
                isDoubleJump = false;
            }
        }
        //Double jumps
        else if (!isOnGround && attack.attackState != AttackState.Damage && input.jumpPressed && extraJumpsCount > 0 && !isHanging && !isClimbing /*&& !input.crouchHeld*/)
        {
            isAttacking = false;
            isJumping = true;
            isDoubleJump = true;
            curJumpTime = Time.time + jumpHoldDuration;

            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
            rigidBody.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

            extraJumpsCount--;

            AudioManager.PlayDoubleJumpAudio();
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
        bodyCollider.size = colliderStandSize;
        bodyCollider.offset = colliderStandOffset;
    }

    public void Crouch()
    {
        isCrouching = true;
        isAttacking = false;
        input.horizontalAccess = false;

        bodyCollider.size = colliderCrouchSize;
        bodyCollider.offset = colliderCrouchOffset;
        rigidBody.velocity = Vector2.zero;
    }

    public void CastSpell(AnimationClip castAnim)
    {
        if (isCrouching)
            StandUp();

        this.castAnim = castAnim;
        
        isAttacking = false;
        isCast = true;
        input.horizontalAccess = false;
        canFlip = false;
        rigidBody.velocity = Vector2.zero;
    }

    public void NextSpellAttack(AnimationClip castAnim)
    {
        this.castAnim = castAnim;
    }

    public void SpellCastEnd()
    {
        isCast = false;
        canFlip = true;
        HorizontalAccess();
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
    
    public void HorizontalAccess()
    {
        input.horizontalAccess = true;
    }

    public void HookOn(Transform hook)
    {
        isHooked = true;
        hookTransform = hook;
        rigidBody.bodyType = RigidbodyType2D.Static;
    }

    void Hooked()
    {
        Vector2 temp = hookTransform.position;
        transform.position = Vector2.MoveTowards(transform.position, hookTransform.position, 20f * Time.deltaTime);
        hookTransform.position = temp;
    }

    public void HookOff()
    {
        isHooked = false;
        hookTransform = null;
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
    }

    void Hurt()
    {
        if (curDazedTime != 0f && curDazedTime <= Time.time)
        {
            canFlip = true;
            curDazedTime = 0f;
            isHurt = false;
            hurtType = HurtType.None;
            targetRigidBody = null;
            bodyCollider.sharedMaterial = physicMaterial;

            if(rigidBody.gravityScale == 0f)
                rigidBody.gravityScale = 1f;
        }
        else
        {
            SpellCastEnd();
            bodyCollider.sharedMaterial = null;
            canFlip = false;

            if (isHealing) HealingEnd();

            //Follow for catch source
            if (hurtType == HurtType.Catch)
            {
                rigidBody.velocity = targetRigidBody.velocity;
            }
            else if(hurtType == HurtType.Stun)
            {
                rigidBody.velocity = Vector2.zero;
                rigidBody.gravityScale = 0f;
            }
        }
    }

    public void GetCaught(HurtType hurtType, Rigidbody2D targetRigidBody)
    {
        isJumping = isDoubleJump = isHeadBlocked = isCrouching = isAttacking = isHanging = isClimbing = isHooked = false;
        this.hurtType = hurtType;
        this.targetRigidBody = targetRigidBody;
        isHurt = true;
    }

    public void Repulse(Vector2 repulseDistantion, float dazedTime)
    {
        isJumping = isDoubleJump = isHeadBlocked = isCrouching = isAttacking = isHanging = isClimbing = isHooked = false;
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        rigidBody.velocity = Vector2.zero;
        isHurt = true;
        hurtType = HurtType.Repulsion;
        rigidBody.AddForce(repulseDistantion, ForceMode2D.Impulse);
        curDazedTime = dazedTime + Time.time;
    }

    public void Stunned(float dazedTime)
    {
        isJumping = isDoubleJump = isHeadBlocked = isCrouching = isAttacking = isHanging = isClimbing = isHooked = false;
        isHurt = true;
        curDazedTime = dazedTime + Time.time;
    }

    void Healing()
    {
        if (!input.healingPotionHeld || isCast || !isOnGround || isHanging || isClimbing || isAttacking || isEvading || attributes.curHealingPotionCount <= 0 || attributes.curHealingDelay > Time.time)
        {
            if ((!input.healingPotionHeld && isHealing) || (attributes.curHealingPotionCount <= 0 && isHealing))
                attributes.AnimationHealingEnd(true);

            return;
        }

        if (!isHealing)
        {
            if (isCrouching) StandUp();

            rigidBody.velocity = Vector2.zero;
            input.horizontalAccess = false;
            isHealing = true;
            // Healing applies from animation from attributes script
        }

        if (!AudioManager.current.playerSource.isPlaying)
            AudioManager.PlayHealingAudio();
    }

    void HealingEnd()
    {
        isHealing = false;
        HorizontalAccess();
        attributes.AnimationHealingEnd(false);
    }

    //private void OnTriggerEnter2D(Collider2D col)
    //{
    //    if (col.gameObject.TryGetComponent(out CollisionRepulse _colRepulse))
    //    {
    //        colRepulse = _colRepulse;
    //        halfColSizeX = _colRepulse.maxRepulseDistance;
    //        repulseQuantity = repulseQuantity <= -1 ? 3 : repulseQuantity;
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D col)
    //{
    //    colRepulse = col.gameObject.TryGetComponent(out CollisionRepulse _colRepulse) ? _colRepulse : colRepulse;
    //}

    //float DistanceToEnemy()
    //{
    //    return playerTransform.position.x - colRepulse.transform.position.x;
    //}

    //void OnTriggerStay2D(Collider2D col)
    //{
    //    //If interacting with the enemy (14 - enemy body layer)
    //    if (col.gameObject.layer == 14)
    //    {
    //        if (!isHurt && !isDead && !isEvading && !isAttacking && isOnGround && input.horizontalAccess)
    //        {
    //            //Not moving - push player from the enemy
    //            if (input.horizontal == 0)
    //            {
    //                float _forceDir = Mathf.Sign(playerTransform.position.x - col.transform.position.x);

    //                rigidBody.AddForce(new Vector2(speed / halfColSizeX * _forceDir, 0f), ForceMode2D.Impulse);
    //                //rigidBody.velocity = new Vector2(rigidBody.velocity.x + _forceDir * speed / 2f, rigidBody.velocity.y);
    //            }
    //            //Moving - push player in the opposite direction to the movement
    //            else
    //            {
    //                if (Mathf.Abs(DistanceToEnemy()) >= halfColSizeX && Mathf.Sign(DistanceToEnemy()) != Mathf.Sign(input.horizontal) && repulseQuantity > 0)
    //                {
    //                    rigidBody.AddForce(new Vector2(speed * 1.3f * -Mathf.Sign(input.horizontal), 0f), ForceMode2D.Impulse);
    //                    //rigidBody.velocity = new Vector2(rigidBody.velocity.x + speed / DistanceToEnemy(), rigidBody.velocity.y);
    //                    repulseQuantity--;
    //                }
    //                else
    //                {
    //                    rigidBody.velocity = new Vector2(rigidBody.velocity.x + Mathf.Lerp(0f, speed / 2f, DistanceToEnemy() / halfColSizeX), rigidBody.velocity.y);
    //                    repulseQuantity--;
    //                }
    //            }

    //            input.horizontalAccess = false;
    //            Invoke("HorizontalAccess", .1f);
    //        }
    //    }
    //    else
    //        halfColSizeX = 1.2f;
    //}

    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length)
    {
        //If ray used without layer
        return Raycast(offset, rayDirection, length, groundLayer);
    }

    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
    {
        Vector2 pos = playerTransform.position;
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