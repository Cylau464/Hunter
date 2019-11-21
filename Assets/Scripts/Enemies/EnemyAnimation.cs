using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    Enemy _enemy;
    Rigidbody2D _rigidBody;      //Reference to the Rigidbody2D component
    Animator _anim;              //Reference to the Animator component

    int _patrolParamID;
    int _chaseParamID;
    int _attackingParamID;
    int _hurtParamID;
    int _deadParamID;
    int _speedParamID;
    int _fallParamID;

    // Start is called before the first frame update
    void Start()
    {
        //Get the integer hashes of the parameters. This is much more efficient
        //than passing strings into the animator
        //_patrolParamID          = Animator.StringToHash("isPatroling");
        //_chaseParamID           = Animator.StringToHash("isChasing");
        _attackingParamID       = Animator.StringToHash("isAttacking");
        _hurtParamID            = Animator.StringToHash("isHurt");
        _deadParamID            = Animator.StringToHash("isDead");
        _speedParamID           = Animator.StringToHash("speed");
        _fallParamID            = Animator.StringToHash("verticalVelocity");

        //Get references to the needed components
        _enemy                  = GetComponent<Enemy>();
        _rigidBody              = GetComponent<Rigidbody2D>();
        _anim                   = GetComponent<Animator>();

        //If any of the needed components don't exist...
        if (_enemy == null || _rigidBody == null || _anim == null)
        {
            //...log an error and then remove this component
            Debug.LogError("A needed component is missing from the enemy: " + gameObject.GetInstanceID());
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Update the Animator with the appropriate values
        //_anim.SetBool(_patrolParamID, _enemy.isPatrol);
        //_anim.SetBool(_chaseParamID, _enemy.isChase);
        _anim.SetBool(_hurtParamID, _enemy.isHurt);
        _anim.SetBool(_attackingParamID, _enemy.isAttack);
        _anim.SetBool(_deadParamID, _enemy.isDead);

        _anim.SetFloat(_fallParamID, _rigidBody.velocity.y);

        //Use the absolute value of speed so that we only pass in positive numbers
        _anim.SetFloat(_speedParamID, _rigidBody.velocity.x);
    }
}
