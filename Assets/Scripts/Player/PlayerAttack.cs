using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum AttackTypes { NotAttacking, Light, Strong, Joint }
public enum AttackState { Free, Start, Damage, End }

public class PlayerAttack : MonoBehaviour
{
    public int lightComboCount = 4;         //count of combo
    public int strongComboCount = 3;        //count of combo
    public int jointComboCount = 2;         //count of combo
    public int lightCombo;                  //current number of light combo
    public int strongCombo;                 //current number of strong combo
    public int jointCombo;                  //current number of joint combo
    public int airLightComboCount = 3;      //count of combo
    public int airStrongComboCount = 2;     //count of combo
    public int airJointComboCount = 1;      //count of combo
    public int airLightCombo;               //current number of air light combo
    public int airStrongCombo;              //current number of air strong combo
    public int airJointCombo;               //current number of air joint combo
    public float delayResetCombo = 1.5f;
    public bool canAttack;
    public bool switchAttack;

    //[HideInInspector]
    public AttackState attackState;
    public AttackTypes attackType;

    public LayerMask[] whatIsEnemies;

    SpriteRenderer _sprite;
    SpriteRenderer _weaponSprite;
    PlayerMovement _movement;
    PlayerInput _input;
    Transform _weapon;
    Rigidbody2D _rigidBody;
    WeaponAtributes _weaponAtributes;
    WeaponAnimation _weaponAnimation;
    GameObject shellPrefab;
    GameObject _shellObj;
    Shell _shell;

    float _attackRangeX;
    float _attackRangeY;
    float _attackDuration;
    float _attackForceDistance;
    float _shellSpeed;
    float _timeBtwAttacks;
    float _delayResetCombo;
    float _posX;
    float _rotZ;

    int damage;

    public WeaponType weaponType;

    private void OnGUI()
    {
        //GUI.TextField(new Rect(10, 10, 120, 200), "vel" + _rigidBody.velocity + "damage: " + damage + "\nattack speed: " + _timeBtwAttacks + "\nattack dur: " + _attackDuration + "\nLA" + _input.lightAttack + "\nSA" + _input.strongAttack + "\nLIST: " + string.Join("\n", _input.lastInputs.ConvertAll(i => i.ToString()).ToArray()) + "\n\n\n\n\n" + (_input.lastInputs.Count == 0));
    }

    void Start()
    {
        _weapon = GameObject.FindWithTag("Main Weapon").transform;
        _posX = _weapon.localPosition.x;
        _weaponSprite = _weapon.GetComponent<SpriteRenderer>();
        _weaponAtributes = _weapon.GetComponent<WeaponAtributes>();
        _weaponAnimation = _weapon.GetComponent<WeaponAnimation>();
        //_anim = _parent.GetComponent<Animator>();
        _movement = GetComponent<PlayerMovement>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _input = GetComponent<PlayerInput>();

        _attackRangeX = _weaponAtributes.attackRangeX;
        _attackRangeY = _weaponAtributes.attackRangeY;

        weaponType = _weaponAtributes.weaponType;
    }
    
    void Update()
    {
        if (!_movement.isDead)
        {
            if (!_movement.isEvading && !_movement.isClimbing && !_movement.isHanging && canAttack &&
                _input.lastInputs.Count != 0 && (_input.lastInputs[0] == InputsEnums.StrongAttack || _input.lastInputs[0] == InputsEnums.LightAttack)/*_input.lastInputs.Exists(x => x == InputsEnums.StrongAttack || x == InputsEnums.LightAttack)*/ && _timeBtwAttacks + Time.deltaTime <= Time.time)
            {
                if (!_movement.isOnGround && lightCombo + strongCombo + jointCombo > 0)
                    switchAttack = true;
                
                attackState = AttackState.Start;
                //if list have both attack input...
                if (_input.lastInputs.Contains(InputsEnums.LightAttack) && _input.lastInputs.Contains(InputsEnums.StrongAttack))
                {
                    attackType = AttackTypes.Joint;
                    _input.lastInputs.Clear();          //...remove all list element
                }
                //if only one of they...
                else
                {
                    attackType = _input.lastInputs[0] == InputsEnums.LightAttack ? AttackTypes.Light : AttackTypes.Strong;
                    _input.lastInputs.RemoveAt(0);      //...remove first list element
                }

                MeleeAttack(attackType);
                Debug.Log(attackType);
            }

            EndOfAttack();

            //Flip character if shooting
            Vector2 _aimDirection = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            if (_movement.isAttacking && weaponType != WeaponType.Melee && Mathf.Sign(_aimDirection.x) != _movement.direction)
                _movement.FlipCharacterDirection();
        }

        ResetCombo();
    }

    void FixedUpdate()
    {
        if (!_movement.isDead)
        {
            if(attackState == AttackState.Start)
                RotateRangeWeapon();
        }
    }

    void MeleeAttack(AttackTypes attackType)
    {
        //Only one air combo in a jump
        if (!_movement.isOnGround && !canAttack)
            return;

        if (_movement.isCrouching)
            _movement.StandUp();

        _movement.isAttacking = true;
        _input.lightAttack = false;
        _input.strongAttack = false;

        GetWeaponAtributes(attackType);
        SetCombo(attackType);
        //Stop character before attack
        if(weaponType != WeaponType.Range)
            _rigidBody.velocity = Vector2.zero;
    }

    void RangeAttack(AttackTypes attackType)
    {

    }

    void RotateRangeWeapon()
    {
        if (weaponType != WeaponType.Melee)
        {
            Vector2 _angle = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            _rotZ = Mathf.Atan2(_angle.y, _angle.x) * Mathf.Rad2Deg;

            _weapon.transform.rotation = Quaternion.Euler(0f, 0f, _rotZ);
        }
    }

    void GiveDamage()
    {
        if (!_movement.isAttacking)
            return;

        attackState = AttackState.Damage;
        _movement.canFlip = false;
        _delayResetCombo = Time.time + delayResetCombo;

        if (weaponType != WeaponType.Melee)
        {
            _shellObj = Instantiate(shellPrefab, transform.position, Quaternion.Euler(0f, 0f, _rotZ));
            _shell = _shellObj.GetComponent<Shell>();
            _shell.damage = damage;
            _shell.speed = _shellSpeed;
        }
        else
        {
            _rigidBody.AddForce(Vector2.right * _movement.direction * _attackForceDistance, ForceMode2D.Impulse);

            Collider2D[] enemiesToDamage = Physics2D.OverlapBoxAll(_weapon.position, new Vector2(_attackRangeX, _attackRangeY), 0, whatIsEnemies[0]);
            LayerMask mask = 1 << 9 | 1 << 12;      //9 - platforms, 12 - enemy
            RaycastHit2D hit;

            //Raycast to player from enemy
            hit = Physics2D.Raycast(_movement.transform.position, Vector2.right * _movement.direction, 1.2f, mask);
            Debug.Log(hit + "\n" + hit.transform);
            Debug.DrawRay(_movement.transform.position, Vector2.right * _movement.direction * 1.2f, Color.cyan, 2f);

            for (int i = 0; i < enemiesToDamage.Length; i++)
            {
                //_camShake.Shake(.2f, 1f, 2f);
                enemiesToDamage[i].GetComponent<Enemy>().TakeDamage(damage);
            }
        }
    }

    void EndOfAttack()
    {
        if (!_movement.isAttacking)
            return;

        //End of attack and pause anim
        if (_timeBtwAttacks <= Time.time)
        {
            _movement.canFlip = true;
            attackState = AttackState.End;
            _rigidBody.velocity = Vector2.zero;

            //Switch animation from air attack to grounded
            if (airLightCombo + airStrongCombo + airJointCombo > 0 && _movement.isOnGround)
            {
                _movement.isAttacking = false;
                attackType = default;
            }
        }

        //Pause attack state for give little time for next attack
        if (_attackDuration <= Time.time)
        {
            //_input.lightAttack = false;
            //_input.strongAttack = false;
            _movement.isAttacking = false;
            attackType = default;
            _weapon.transform.rotation = _weaponAnimation.defaultRot;
            _movement.speedDivisor = 1f;
        }
    }

    void SetCombo(AttackTypes type)
    {
        switch(type)
        {
            case AttackTypes.Light:
            {
                    if (_movement.isOnGround)
                    {
                        airLightCombo = airStrongCombo = 0; //nullify air combo

                        if (strongCombo == 0)
                        {
                            if (lightCombo < lightComboCount)
                                lightCombo++;
                            else
                                lightCombo = 1;
                        }
                        else
                        {
                            if (lightCombo + strongCombo < strongComboCount)
                                lightCombo++;
                            else
                            {
                                lightCombo = 1;
                                strongCombo = 0;
                            }
                        }
                        break;
                    }
                    else
                    {
                        lightCombo = strongCombo = 0; //nullify ground combo

                        if (airStrongCombo == 0)
                        {
                            if (++airLightCombo >= airLightComboCount)
                                canAttack = false;
                        }
                        else
                        {
                            if (++airLightCombo + airStrongCombo >= airStrongComboCount)
                                canAttack = false;
                        }
                        break;
                    }
                }
            case AttackTypes.Strong:
            {
                if (_movement.isOnGround)
                {
                    airLightCombo = airStrongCombo = 0;

                    if (strongCombo + lightCombo < strongComboCount)
                        strongCombo++;
                    else
                    {
                        strongCombo = 1;
                        lightCombo = 0;
                    }
                    break;
                }
                else
                {
                    lightCombo = strongCombo = 0;
                    if (++airStrongCombo + airLightCombo >= airStrongComboCount)
                        canAttack = false;
                    break;
                }
            }
            case AttackTypes.Joint:
            {
                lightCombo = strongCombo = airLightCombo = airStrongCombo = 0; //nullify all other combo

                if (_movement.isOnGround)
                {
                    airJointCombo = 0;

                    if(jointCombo < jointComboCount)
                        jointCombo++;
                    else
                        jointCombo = 1;
                    break;
                }
                else
                {
                    jointCombo = 0;
                    if (++airJointCombo >= airJointComboCount)
                        canAttack = false;
                    break;
                }
            }
        }
    }

    void GetWeaponAtributes(AttackTypes type)
    {
        weaponType = _weaponAtributes.weaponType;
        shellPrefab = _weaponAtributes.shellPrefab;
        _shellSpeed = _weaponAtributes.shellSpeed;
        _movement.speedDivisor = type == AttackTypes.Light ? _weaponAtributes.speedDivisorL : type == AttackTypes.Strong ? _weaponAtributes.speedDivisorS : _weaponAtributes.speedDivisorJ;

        switch (type)
        {
            case AttackTypes.Light:
            {
                damage = _weaponAtributes.lightAttackDamage;
                _attackForceDistance = _weaponAtributes.lightAttackForce;
                _timeBtwAttacks = Time.time + _weaponAtributes.lightAttackSpeed;
                _attackDuration = Time.time + _weaponAtributes.lightAttackSpeed * 1.5f;
                break;
            }
            case AttackTypes.Strong:
            {
                damage = _weaponAtributes.strongAttackDamage;

                _attackForceDistance = _weaponAtributes.strongAttackForce;
                _timeBtwAttacks = Time.time + _weaponAtributes.strongAttackSpeed;
                _attackDuration = Time.time + _weaponAtributes.strongAttackSpeed * 1.5f;
                break;
            }
            case AttackTypes.Joint:
            {
                damage = _weaponAtributes.jointAttackDamage;
                _attackForceDistance = _weaponAtributes.jointAttackForce;
                _timeBtwAttacks = Time.time + _weaponAtributes.jointAttackSpeed;
                _attackDuration = Time.time + _weaponAtributes.jointAttackSpeed * 1.5f;
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if(_weapon != null)
            Gizmos.DrawWireCube(_weapon.position, new Vector3(_attackRangeX, _attackRangeY, 0f));
        Vector3 ray = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        //Gizmos.DrawLine(transform.position, ray);
    }

    void ResetCombo()
    {
        if (_movement.isOnGround && !canAttack)
        {
            airLightCombo = airStrongCombo = airJointCombo = 0;
            canAttack = true;
        }

        if (_timeBtwAttacks <= Time.time && _delayResetCombo <= Time.time)
        {
            lightCombo = strongCombo = jointCombo = airLightCombo = airStrongCombo = airJointCombo = 0;
            attackState = default;
        }
    }
}
