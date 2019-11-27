using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimation : MonoBehaviour
{
    public Quaternion defaultRot;

    bool flipCond;

    int direction;

    PlayerMovement cMovement;
    Transform character;
    SpriteRenderer sprite;

    void Start()
    {
        character  = transform.parent;
        cMovement  = character.GetComponent<PlayerMovement>();
        sprite     = GetComponent<SpriteRenderer>();
        direction   = cMovement.direction;
        flipCond   = sprite.flipX;
        defaultRot  = transform.rotation;
    }

    void Update()
    {
        //Flip weapon following the character
        if (!cMovement.isAttacking && cMovement.sprite.flipX != sprite.flipX)
            FlipWeapon();

        if (cMovement.isAttacking)
            sprite.flipX = false;
    }

    void FlipWeapon()
    {
        transform.localPosition = new Vector2(-transform.localPosition.x, transform.localPosition.y);
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, -transform.localRotation.eulerAngles.z);
        direction = cMovement.direction;
        sprite.flipX = !sprite.flipX;
    }
}
