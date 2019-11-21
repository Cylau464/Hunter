using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    PlayerAttack _attack;
    PlayerMovement _movement;    //Reference to the PlayerMovement script component
    Rigidbody2D _rigidBody;      //Reference to the Rigidbody2D component
    PlayerInput _input;          //Reference to the PlayerInput script component
    Animator _anim;              //Reference to the Animator component

    int _groundParamID;          //ID of the isOnGround parameter
    int _crouchParamID;          //ID of the isCrouching parameter
    int _hangingParamID;
    int _climbingParamID;        //ID of the isClimbing parameter
    int _attackingParamID;
    int _hurtParamID;
    int _doubleJumpParamID;      //ID of the isDoubleJump parameter
    int _deathParamID;
    int _speedParamID;           //ID of the speed parameter
    int _fallParamID;            //ID of the verticalVelocity parameter
    int _lightComboParamID;
    int _strongComboParamID;
    int _jointComboParamID;
    int _airLightComboParamID;
    int _airStrongComboParamID;
    int _airJointComboParamID;
    int _evadingParamID;
    int _lightAttackParamID;
    int _strongAttackParamID;
    int _jointAttackParamID;
    int _switchAttackParamID;

    // Start is called before the first frame update
    void Start()
    {
        //Get the integer hashes of the parameters. This is much more efficient
        //than passing strings into the animator
        _groundParamID          = Animator.StringToHash("isOnGround");
        _crouchParamID          = Animator.StringToHash("isCrouching");
        _hangingParamID         = Animator.StringToHash("isHanging");
        _climbingParamID        = Animator.StringToHash("isClimbing");
        _attackingParamID       = Animator.StringToHash("isAttacking");
        _doubleJumpParamID      = Animator.StringToHash("isDoubleJump");
        _hurtParamID            = Animator.StringToHash("isHurt");
        _evadingParamID         = Animator.StringToHash("isEvading");
        _deathParamID           = Animator.StringToHash("isDead");
        _speedParamID           = Animator.StringToHash("speed");
        _fallParamID            = Animator.StringToHash("verticalVelocity");
        _lightComboParamID      = Animator.StringToHash("lightCombo");
        _strongComboParamID     = Animator.StringToHash("strongCombo");
        _jointComboParamID      = Animator.StringToHash("jointCombo");
        _airLightComboParamID   = Animator.StringToHash("airLightCombo");
        _airStrongComboParamID  = Animator.StringToHash("airStrongCombo");
        _airJointComboParamID   = Animator.StringToHash("airJointCombo");
        _lightAttackParamID     = Animator.StringToHash("lightAttack");
        _strongAttackParamID    = Animator.StringToHash("strongAttack");
        _jointAttackParamID     = Animator.StringToHash("jointAttack");
        _switchAttackParamID    = Animator.StringToHash("switchAttack");

        //Get references to the needed components
        _attack                 = GetComponent<PlayerAttack>();
        _movement               = GetComponent<PlayerMovement>();
        _rigidBody              = GetComponent<Rigidbody2D>();
        _input                  = GetComponent<PlayerInput>();
        _anim                   = GetComponent<Animator>();

        //If any of the needed components don't exist...
        if (_movement == null || _rigidBody == null || _input == null || _anim == null)
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
        _anim.SetBool(_groundParamID, _movement.isOnGround);
        _anim.SetBool(_crouchParamID, _movement.isCrouching);
        _anim.SetBool(_hangingParamID, _movement.isHanging);
        _anim.SetBool(_climbingParamID, _movement.isClimbing);
        _anim.SetBool(_doubleJumpParamID, _movement.isDoubleJump);
        _anim.SetBool(_hurtParamID, _movement.isHurt);
        _anim.SetBool(_attackingParamID, _movement.isAttacking);
        _anim.SetBool(_evadingParamID, _movement.isEvading);
        _anim.SetBool(_deathParamID, _movement.isDead);
        //Attack types
        _anim.SetBool(_lightAttackParamID, _attack.attackType == AttackTypes.Light ? true : false);
        _anim.SetBool(_strongAttackParamID, _attack.attackType == AttackTypes.Strong ? true : false);
        _anim.SetBool(_jointAttackParamID, _attack.attackType == AttackTypes.Joint ? true : false);

        _anim.SetInteger(_lightComboParamID, _attack.lightCombo);
        _anim.SetInteger(_strongComboParamID, _attack.strongCombo);
        _anim.SetInteger(_jointComboParamID, _attack.jointCombo);
        _anim.SetInteger(_airLightComboParamID, _attack.airLightCombo);
        _anim.SetInteger(_airStrongComboParamID, _attack.airStrongCombo);
        _anim.SetInteger(_airJointComboParamID, _attack.airJointCombo);

        _anim.SetFloat(_fallParamID, _rigidBody.velocity.y);

        //Use the absolute value of speed so that we only pass in positive numbers
        _anim.SetFloat(_speedParamID, Mathf.Abs(_input.horizontal));

        if (_attack.switchAttack)
        {
            _anim.SetTrigger(_switchAttackParamID);
            _attack.switchAttack = false;
        }
    }

    //This method is called from events in the animation itself. This keeps the footstep
    //sounds in sync with the visuals
    public void StepAudio()
    {
        //Tell the Audio Manager to play a footstep sound
        //AudioManager.PlayFootstepAudio();
    }

    //This method is called from events in the animation itself. This keeps the footstep
    //sounds in sync with the visuals
    public void CrouchStepAudio()
    {
        //Tell the Audio Manager to play a crouching footstep sound
        //AudioManager.PlayCrouchFootstepAudio();
    }
}
