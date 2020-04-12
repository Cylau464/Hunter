using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class PlayerAnimation : MonoBehaviour
{
    PlayerAttack attack;
    PlayerSpells spell;
    PlayerMovement movement;    //Reference to the PlayerMovement script component
    Rigidbody2D rigidBody;      //Reference to the Rigidbody2D component
    PlayerInput input;          //Reference to the PlayerInput script component
    Animator anim;              //Reference to the Animator component
    AnimatorOverrideController animatorOverrideController;
    AnimationClip castClip, castDefClip;

    int groundParamID;          //ID of the isOnGround parameter
    int crouchParamID;          //ID of the isCrouching parameter
    int hangingParamID;
    int climbingParamID;        //ID of the isClimbing parameter
    int attackingParamID;
    int hurtParamID;
    int doubleJumpParamID;      //ID of the isDoubleJump parameter
    int deathParamID;
    int speedParamID;           //ID of the speed parameter
    int fallParamID;            //ID of the verticalVelocity parameter
    int lightComboParamID;
    int strongComboParamID;
    int jointComboParamID;
    int airLightComboParamID;
    int airStrongComboParamID;
    int airJointComboParamID;
    int evadingParamID;
    int lightAttackParamID;
    int strongAttackParamID;
    int jointAttackParamID;
    int topDownAttackParamID;
    int switchAttackParamID;
    int castParamID;
    int healingParamID;
    int spellAttackNumberParamID;

    // Start is called before the first frame update
    void Start()
    {
        //Get the integer hashes of the parameters. This is much more efficient
        //than passing strings into the animator
        groundParamID               = Animator.StringToHash("isOnGround");
        crouchParamID               = Animator.StringToHash("isCrouching");
        hangingParamID              = Animator.StringToHash("isHanging");
        climbingParamID             = Animator.StringToHash("isClimbing");
        attackingParamID            = Animator.StringToHash("isAttacking");
        doubleJumpParamID           = Animator.StringToHash("isDoubleJump");
        hurtParamID                 = Animator.StringToHash("isHurt");
        evadingParamID              = Animator.StringToHash("isEvading");
        deathParamID                = Animator.StringToHash("isDead");
        speedParamID                = Animator.StringToHash("speed");
        fallParamID                 = Animator.StringToHash("verticalVelocity");
        lightComboParamID           = Animator.StringToHash("lightCombo");
        strongComboParamID          = Animator.StringToHash("strongCombo");
        jointComboParamID           = Animator.StringToHash("jointCombo");
        airLightComboParamID        = Animator.StringToHash("airLightCombo");
        airStrongComboParamID       = Animator.StringToHash("airStrongCombo");
        airJointComboParamID        = Animator.StringToHash("airJointCombo");
        lightAttackParamID          = Animator.StringToHash("lightAttack");
        strongAttackParamID         = Animator.StringToHash("strongAttack");
        jointAttackParamID          = Animator.StringToHash("jointAttack");
        topDownAttackParamID        = Animator.StringToHash("topDownAttack");
        switchAttackParamID         = Animator.StringToHash("switchAttack");
        castParamID                 = Animator.StringToHash("isCast");
        healingParamID              = Animator.StringToHash("isHealing");
        spellAttackNumberParamID    = Animator.StringToHash("spellAttackNumber");

        //Get references to the needed components
        attack                      = GetComponent<PlayerAttack>();
        spell                       = GetComponent<PlayerSpells>();
        movement                    = GetComponent<PlayerMovement>();
        rigidBody                   = GetComponent<Rigidbody2D>();
        input                       = GetComponent<PlayerInput>();
        anim                        = GetComponent<Animator>();
        castDefClip = castClip      = movement.castAnim;
        animatorOverrideController  = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animatorOverrideController;

        //If any of the needed components don't exist...
        if (movement == null || rigidBody == null || input == null || anim == null)
        {
            //...log an error and then remove this component
            Debug.LogError("A needed component is missing from the player");
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Update the Animator with the appropriate values
        anim.SetBool(groundParamID, movement.isOnGround);
        anim.SetBool(crouchParamID, movement.isCrouching);
        anim.SetBool(hangingParamID, movement.isHanging);
        anim.SetBool(climbingParamID, movement.isClimbing);
        anim.SetBool(doubleJumpParamID, movement.isDoubleJump);
        anim.SetBool(hurtParamID, movement.isHurt);
        anim.SetBool(attackingParamID, movement.isAttacking);
        anim.SetBool(evadingParamID, movement.isEvading);
        anim.SetBool(deathParamID, movement.isDead);
        anim.SetBool(castParamID, movement.isCast);
        anim.SetBool(healingParamID, movement.isHealing);
        //Attack types
        anim.SetBool(lightAttackParamID, attack.attackType == AttackTypes.Light ? true : false);
        anim.SetBool(strongAttackParamID, attack.attackType == AttackTypes.Strong ? true : false);
        anim.SetBool(jointAttackParamID, attack.attackType == AttackTypes.Joint ? true : false);
        anim.SetBool(topDownAttackParamID, attack.attackType == AttackTypes.TopDown ? true : false);

        anim.SetInteger(lightComboParamID, attack.lightCombo);
        anim.SetInteger(strongComboParamID, attack.strongCombo);
        anim.SetInteger(jointComboParamID, attack.jointCombo);
        anim.SetInteger(airLightComboParamID, attack.airLightCombo);
        anim.SetInteger(airStrongComboParamID, attack.airStrongCombo);
        anim.SetInteger(airJointComboParamID, attack.airJointCombo);
        anim.SetInteger(spellAttackNumberParamID, spell.attackNumber);

        anim.SetFloat(fallParamID, rigidBody.velocity.y);

        //Use the absolute value of speed so that we only pass in positive numbers
        anim.SetFloat(speedParamID, Mathf.Abs(input.horizontal));

        if (attack.switchAttack)
        {
            anim.SetTrigger(switchAttackParamID);
            attack.switchAttack = false;
        }

        if (movement.castAnim != castClip)
        {
            castClip = movement.castAnim;
            
            animatorOverrideController[castDefClip] = castClip;
        }
    }

    //This method is called from events in the animation itself. This keeps the footstep
    //sounds in sync with the visuals
    public void StepAudio()
    {
        //Tell the Audio Manager to play a footstep sound
        AudioManager.PlayFootstepAudio();
    }
}
