using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    Enemy enemy;
    Rigidbody2D rigidBody;      //Reference to the Rigidbody2D component
    Animator anim;              //Reference to the Animator component

    int patrolParamID;
    int chaseParamID;
    int attackingParamID;
    int hurtParamID;
    int deadParamID;
    int speedParamID;
    int fallParamID;

    // Start is called before the first frame update
    void Start()
    {
        //Get the integer hashes of the parameters. This is much more efficient
        //than passing strings into the animator
        //patrolParamID          = Animator.StringToHash("isPatroling");
        //chaseParamID           = Animator.StringToHash("isChasing");
        attackingParamID       = Animator.StringToHash("isAttacking");
        hurtParamID            = Animator.StringToHash("isHurt");
        deadParamID            = Animator.StringToHash("isDead");
        speedParamID           = Animator.StringToHash("speed");
        fallParamID            = Animator.StringToHash("verticalVelocity");

        //Get references to the needed components
        enemy                  = GetComponent<Enemy>();
        rigidBody              = GetComponent<Rigidbody2D>();
        anim                   = GetComponent<Animator>();

        //If any of the needed components don't exist...
        if (enemy == null || rigidBody == null || anim == null)
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
        //anim.SetBool(patrolParamID, enemy.isPatrol);
        //anim.SetBool(chaseParamID, enemy.isChase);
        anim.SetBool(hurtParamID, enemy.isHurt);
        anim.SetBool(attackingParamID, enemy.isAttack);
        anim.SetBool(deadParamID, enemy.isDead);

        anim.SetFloat(fallParamID, rigidBody.velocity.y);

        //Use the absolute value of speed so that we only pass in positive numbers
        anim.SetFloat(speedParamID, rigidBody.velocity.x);
    }
}
