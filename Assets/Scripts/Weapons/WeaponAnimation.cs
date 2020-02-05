using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class WeaponAnimation : MonoBehaviour
{
    public Quaternion defaultRot;

    bool flipCond;

    int direction;

    PlayerMovement cMovement;
    PlayerAttack cAttack;
    Transform character;
    SpriteRenderer sprite;

    void Start()
    {
        character   = transform.parent;
        cMovement   = character.GetComponent<PlayerMovement>();
        cAttack     = character.GetComponent<PlayerAttack>();
        sprite      = GetComponent<SpriteRenderer>();
        direction   = cMovement.direction;
    }

    void Update()
    {
        //Flip weapon following the character
        if (cAttack.attackState != AttackState.Damage && cMovement.direction != direction)
            FlipWeapon();
    }

    void FlipWeapon()
    {
        transform.localPosition = new Vector2(-transform.localPosition.x, transform.localPosition.y);
        transform.rotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, -transform.localRotation.eulerAngles.z);
        direction = cMovement.direction;
        sprite.flipX = !sprite.flipX;
    }
}
