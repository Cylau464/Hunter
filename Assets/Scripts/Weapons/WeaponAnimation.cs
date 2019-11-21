using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimation : MonoBehaviour
{
    public Quaternion defaultRot;

    bool _flipCond;

    int direction;

    PlayerMovement _cMovement;
    Transform _character;
    SpriteRenderer _sprite;

    void Start()
    {
        _character  = transform.parent;
        _cMovement  = _character.GetComponent<PlayerMovement>();
        _sprite     = GetComponent<SpriteRenderer>();
        direction   = _cMovement.direction;
        _flipCond   = _sprite.flipX;
        defaultRot  = transform.rotation;
    }

    void Update()
    {
        //Flip weapon following the character
        if (!_cMovement.isAttacking && _cMovement.sprite.flipX != _sprite.flipX)
            FlipWeapon();

        if (_cMovement.isAttacking)
            _sprite.flipX = false;
    }

    void FlipWeapon()
    {
        transform.localPosition = new Vector2(-transform.localPosition.x, transform.localPosition.y);
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, -transform.localRotation.eulerAngles.z);
        direction = _cMovement.direction;
        _sprite.flipX = !_sprite.flipX;
    }
}
