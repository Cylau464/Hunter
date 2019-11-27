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

    SpriteRenderer sprite;
    SpriteRenderer weaponSprite;
    PlayerMovement movement;
    PlayerInput input;
    Transform weapon;
    Rigidbody2D rigidBody;
    WeaponAtributes weaponAtributes;
    WeaponAnimation weaponAnimation;
    GameObject shellPrefab;
    GameObject shellObj;
    Shell shell;

    float attackRangeX;
    float attackRangeY;
    float attackDuration;
    float attackForceDistance;
    float shellSpeed;
    float timeBtwAttacks;
    float curDelayResetCombo;
    float posX;
    float rotZ;

    int damage;

    public WeaponType weaponType;

    private void OnGUI()
    {
        //GUI.TextField(new Rect(10, 10, 120, 200), "vel" + rigidBody.velocity + "damage: " + damage + "\nattack speed: " + timeBtwAttacks + "\nattack dur: " + attackDuration + "\nLA" + input.lightAttack + "\nSA" + input.strongAttack + "\nLIST: " + string.Join("\n", input.lastInputs.ConvertAll(i => i.ToString()).ToArray()) + "\n\n\n\n\n" + (input.lastInputs.Count == 0));
    }

    void Start()
    {
        weapon              = GameObject.FindWithTag("Main Weapon").transform;
        posX                = weapon.localPosition.x;
        weaponSprite        = weapon.GetComponent<SpriteRenderer>();
        weaponAtributes     = weapon.GetComponent<WeaponAtributes>();
        weaponAnimation     = weapon.GetComponent<WeaponAnimation>();
        attackRangeX        = weaponAtributes.attackRangeX;
        attackRangeY        = weaponAtributes.attackRangeY;
        weaponType          = weaponAtributes.weaponType;
        //anim = parent.GetComponent<Animator>();
        movement            = GetComponent<PlayerMovement>();
        rigidBody           = GetComponent<Rigidbody2D>();
        sprite              = GetComponent<SpriteRenderer>();
        input               = GetComponent<PlayerInput>();
    }
    
    void Update()
    {
        if (!movement.isDead)
        {
            if (!movement.isEvading && !movement.isClimbing && !movement.isHanging && canAttack &&
                input.lastInputs.Count != 0 && (input.lastInputs[0] == InputsEnums.StrongAttack || input.lastInputs[0] == InputsEnums.LightAttack)/*input.lastInputs.Exists(x => x == InputsEnums.StrongAttack || x == InputsEnums.LightAttack)*/ && timeBtwAttacks + Time.deltaTime <= Time.time)
            {
                if (!movement.isOnGround && lightCombo + strongCombo + jointCombo > 0)
                    switchAttack = true;
                
                attackState = AttackState.Start;
                //if list have both attack input...
                if (input.lastInputs.Contains(InputsEnums.LightAttack) && input.lastInputs.Contains(InputsEnums.StrongAttack))
                {
                    attackType = AttackTypes.Joint;
                    input.lastInputs.Clear();          //...remove all list element
                }
                //if only one of they...
                else
                {
                    attackType = input.lastInputs[0] == InputsEnums.LightAttack ? AttackTypes.Light : AttackTypes.Strong;
                    input.lastInputs.RemoveAt(0);      //...remove first list element
                }

                MeleeAttack(attackType);
                Debug.Log(attackType);
            }

            EndOfAttack();

            //Flip character if shooting
            Vector2 aimDirection = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            if (movement.isAttacking && weaponType != WeaponType.Melee && Mathf.Sign(aimDirection.x) != movement.direction)
                movement.FlipCharacterDirection();
        }

        ResetCombo();
    }

    void FixedUpdate()
    {
        if (!movement.isDead)
        {
            if(attackState == AttackState.Start)
                RotateRangeWeapon();
        }
    }

    void MeleeAttack(AttackTypes attackType)
    {
        //Only one air combo in a jump
        if (!movement.isOnGround && !canAttack)
            return;

        if (movement.isCrouching)
            movement.StandUp();

        movement.isAttacking = true;
        input.lightAttack = false;
        input.strongAttack = false;

        GetWeaponAtributes(attackType);
        SetCombo(attackType);
        //Stop character before attack
        if(weaponType != WeaponType.Range)
            rigidBody.velocity = Vector2.zero;
    }

    void RangeAttack(AttackTypes attackType)
    {

    }

    void RotateRangeWeapon()
    {
        if (weaponType != WeaponType.Melee)
        {
            Vector2 angle = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            rotZ = Mathf.Atan2(angle.y, angle.x) * Mathf.Rad2Deg;

            weapon.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
        }
    }

    void GiveDamage()
    {
        if (!movement.isAttacking)
            return;

        attackState = AttackState.Damage;
        movement.canFlip = false;
        curDelayResetCombo = Time.time + delayResetCombo;

        if (weaponType != WeaponType.Melee)
        {
            shellObj = Instantiate(shellPrefab, transform.position, Quaternion.Euler(0f, 0f, rotZ));
            shell = shellObj.GetComponent<Shell>();
            shell.damage = damage;
            shell.speed = shellSpeed;
        }
        else
        {
            rigidBody.AddForce(Vector2.right * movement.direction * attackForceDistance, ForceMode2D.Impulse);

            Collider2D[] enemiesToDamage = Physics2D.OverlapBoxAll(weapon.position, new Vector2(attackRangeX, attackRangeY), 0, whatIsEnemies[0]);
            LayerMask mask = 1 << 9 | 1 << 12;      //9 - platforms, 12 - enemy
            RaycastHit2D hit;

            //Raycast to player from enemy
            hit = Physics2D.Raycast(movement.transform.position, Vector2.right * movement.direction, 1.2f, mask);
            Debug.Log(hit + "\n" + hit.transform);
            Debug.DrawRay(movement.transform.position, Vector2.right * movement.direction * 1.2f, Color.cyan, 2f);

            for (int i = 0; i < enemiesToDamage.Length; i++)
            {
                //camShake.Shake(.2f, 1f, 2f);
                enemiesToDamage[i].GetComponent<Enemy>().TakeDamage(damage);
            }
        }
    }

    void EndOfAttack()
    {
        if (!movement.isAttacking)
            return;

        //End of attack and pause anim
        if (timeBtwAttacks <= Time.time)
        {
            movement.canFlip = true;
            attackState = AttackState.End;
            rigidBody.velocity = Vector2.zero;

            //Switch animation from air attack to grounded
            if (airLightCombo + airStrongCombo + airJointCombo > 0 && movement.isOnGround)
            {
                movement.isAttacking = false;
                attackType = default;
            }
        }

        //Pause attack state for give little time for next attack
        if (attackDuration <= Time.time)
        {
            //input.lightAttack = false;
            //input.strongAttack = false;
            movement.isAttacking = false;
            attackType = default;
            weapon.transform.rotation = weaponAnimation.defaultRot;
            movement.speedDivisor = 1f;
        }
    }

    void SetCombo(AttackTypes type)
    {
        switch(type)
        {
            case AttackTypes.Light:
            {
                    if (movement.isOnGround)
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
                if (movement.isOnGround)
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

                if (movement.isOnGround)
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
        weaponType = weaponAtributes.weaponType;
        shellPrefab = weaponAtributes.shellPrefab;
        shellSpeed = weaponAtributes.shellSpeed;
        movement.speedDivisor = type == AttackTypes.Light ? weaponAtributes.speedDivisorL : type == AttackTypes.Strong ? weaponAtributes.speedDivisorS : weaponAtributes.speedDivisorJ;

        switch (type)
        {
            case AttackTypes.Light:
            {
                damage = weaponAtributes.lightAttackDamage;
                attackForceDistance = weaponAtributes.lightAttackForce;
                timeBtwAttacks = Time.time + weaponAtributes.lightAttackSpeed;
                attackDuration = Time.time + weaponAtributes.lightAttackSpeed * 1.5f;
                break;
            }
            case AttackTypes.Strong:
            {
                damage = weaponAtributes.strongAttackDamage;

                attackForceDistance = weaponAtributes.strongAttackForce;
                timeBtwAttacks = Time.time + weaponAtributes.strongAttackSpeed;
                attackDuration = Time.time + weaponAtributes.strongAttackSpeed * 1.5f;
                break;
            }
            case AttackTypes.Joint:
            {
                damage = weaponAtributes.jointAttackDamage;
                attackForceDistance = weaponAtributes.jointAttackForce;
                timeBtwAttacks = Time.time + weaponAtributes.jointAttackSpeed;
                attackDuration = Time.time + weaponAtributes.jointAttackSpeed * 1.5f;
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if(weapon != null)
            Gizmos.DrawWireCube(weapon.position, new Vector3(attackRangeX, attackRangeY, 0f));
        Vector3 ray = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        //Gizmos.DrawLine(transform.position, ray);
    }

    void ResetCombo()
    {
        if (movement.isOnGround && !canAttack)
        {
            airLightCombo = airStrongCombo = airJointCombo = 0;
            canAttack = true;
        }

        if (timeBtwAttacks <= Time.time && curDelayResetCombo <= Time.time)
        {
            lightCombo = strongCombo = jointCombo = airLightCombo = airStrongCombo = airJointCombo = 0;
            attackState = default;
        }
    }
}
