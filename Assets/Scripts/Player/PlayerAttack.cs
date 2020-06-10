using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] int lightComboCount = 4;         //count of combo
    [SerializeField] int strongComboCount = 3;        //count of combo
    [SerializeField] int jointComboCount = 2;         //count of combo
    public int lightCombo;                            //current number of light combo
    public int strongCombo;                           //current number of strong combo
    public int jointCombo;                            //current number of joint combo
    //[SerializeField] int airLightComboCount = 3;      //count of combo
    //[SerializeField] int airStrongComboCount = 2;     //count of combo
    //[SerializeField] int airJointComboCount = 1;      //count of combo
    public int airLightCombo;                         //current number of air light combo
    public int airStrongCombo;                        //current number of air strong combo
    public int airJointCombo;                         //current number of air joint combo
    [SerializeField] float delayResetCombo = 1.5f;
    [SerializeField] float forceDuration = .2f;
    public bool canAttack;
    public bool switchAttack;

    public AttackState attackState;
    public AttackTypes attackType;

    public LayerMask[] whatIsEnemies;

    SpriteRenderer sprite;
    PlayerMovement movement;
    PlayerInput input;
    public Transform weapon;
    PlayerAttributes attributes;
    Animator anim;
    Rigidbody2D rigidBody;
    WeaponAttributes weaponAttributes;
    GameObject shellPrefab;
    GameObject shellObj;
    public GameObject damageBox = null;
    Shell shell;

    float attackRangeX;
    float attackRangeY;
    float attackDuration;
    float attackForceDistance;
    float shellSpeed;
    float shellFlyTime;
    float timeBtwAttacks;
    float curDelayResetCombo;
    float curForceDurtaion;
    float posX;
    float rotZ;
    [HideInInspector] public float weaponMass;

    int damage;
    int staminaDamage;
    int staminaCosts;

    public WeaponAttackType weaponAttackType;
    DamageTypes weaponDamageType;
    Element weaponElement;

    AudioClip weaponSwingClip;
    AudioClip weaponAudioClip;
    AudioClip weaponImpactClip;

    private void OnGUI()
    {
        //GUI.TextField(new Rect(10, 10, 120, 200), "vel" + rigidBody.velocity + "damage: " + damage + "\nattack speed: " + timeBtwAttacks + "\nattack dur: " + attackDuration + "\nLA" + input.lightAttack + "\nSA" + input.strongAttack + "\nLIST: " + string.Join("\n", input.lastInputs.ConvertAll(i => i.ToString()).ToArray()) + "\n\n\n\n\n" + (input.lastInputs.Count == 0));
    }

    void Start()
    {
        movement            = GetComponent<PlayerMovement>();
        weapon              = GameObject.FindWithTag("Main Weapon").transform;
        GetWeapon();
        attributes           = GetComponent<PlayerAttributes>();
        rigidBody           = GetComponent<Rigidbody2D>();
        sprite              = GetComponent<SpriteRenderer>();
        anim                = GetComponent<Animator>();
        input               = GetComponent<PlayerInput>();
    }
    
    void Update()
    {
        if (!movement.isDead && !movement.isHurt)
        {
            if (!movement.isEvading && !movement.isClimbing && !movement.isHanging && !movement.isHealing && canAttack &&
                input.lastInputs.Count > 0 && (input.lastInputs[0] == InputsEnum.StrongAttack || input.lastInputs[0] == InputsEnum.LightAttack ||
                input.lastInputs[0] == InputsEnum.JointAttack || input.lastInputs[0] == InputsEnum.TopDownAttack) &&
                timeBtwAttacks + Time.deltaTime <= Time.time)
            {
                if (!movement.isOnGround && lightCombo + strongCombo + jointCombo > 0)
                    switchAttack = true;
                
                attackState = AttackState.Start;

                switch(input.lastInputs[0])
                {
                    case InputsEnum.LightAttack:
                        attackType = AttackTypes.Light;
                        break;
                    case InputsEnum.StrongAttack:
                        attackType = AttackTypes.Strong;
                        break;
                    case InputsEnum.JointAttack:
                        attackType = AttackTypes.Joint;
                        break;
                    case InputsEnum.TopDownAttack:
                        attackType = AttackTypes.TopDown;
                        break;
                }
                
                input.lastInputs.RemoveAt(0);
                MeleeAttack(attackType);
            }

            EndOfAttack();

            //Flip character if shooting
            Vector2 aimDirection = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            if (movement.isAttacking && weaponAttackType != WeaponAttackType.Melee && Mathf.Sign(aimDirection.x) != movement.direction)
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

    public void GetWeapon()
    {
        posX = weapon.localPosition.x;
        weaponAttributes = weapon.GetComponent<WeaponAttributes>();
        attackRangeX = weaponAttributes.attackRangeX;
        attackRangeY = weaponAttributes.attackRangeY;
        weaponAttackType = weaponAttributes.weaponAttackType;
        weaponMass = weaponAttributes.mass;

        switch (weaponAttributes.weaponUltimate.type)
        {
            case WeaponUltimate.Passive:
                movement.bonusAttributes = weaponAttributes.weaponUltimate.attributes;
                break;
            case WeaponUltimate.Buff:
                break;
            case WeaponUltimate.Charging:
                break;
            case WeaponUltimate.Spell:
                break;
        }
    }

    void MeleeAttack(AttackTypes attackType)
    {
        //Only one air combo in a jump
        if (!movement.isOnGround && !canAttack)
            return;

        if (movement.isCrouching)
            movement.StandUp();

        //movement.isAttacking = true;
        input.lightAttack = false;
        input.strongAttack = false;
        input.jointAttack = false;
        input.topDownAttack = false;

        GetWeaponAttributes(attackType);
        SetCombo(attackType);
        //Stop character before attack
        if(weaponAttackType != WeaponAttackType.Range)
            rigidBody.velocity = Vector2.zero;
    }

    void RangeAttack(AttackTypes attackType)
    {

    }

    void RotateRangeWeapon()
    {
        if (weaponAttackType != WeaponAttackType.Melee)
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
        curForceDurtaion = Time.time + forceDuration;
        attributes.Stamina -= staminaCosts;

        if (weaponAttackType != WeaponAttackType.Melee)
        {
            shellObj                    = Instantiate(shellPrefab, transform.position, Quaternion.Euler(0f, 0f, rotZ));
            shell                       = shellObj.GetComponent<Shell>();
            shell.SetParameters(damage, staminaDamage, weaponDamageType, weaponElement, shellSpeed, shellFlyTime, weaponImpactClip);
        }
        else
        {
            if (attackType == AttackTypes.TopDown)
                rigidBody.AddForce(Vector2.up * -attackForceDistance, ForceMode2D.Impulse);
            else
                rigidBody.AddForce(Vector2.right * movement.direction * attackForceDistance, ForceMode2D.Impulse);

            GameObject _inst            = Instantiate(damageBox, transform);
            DamageBox _damageBox        = _inst.GetComponent<DamageBox>();
            int targetLayer             = 12;
            _damageBox.GetParameters(damage, staminaDamage, weaponDamageType, weaponElement, weapon.position, new Vector2(attackRangeX, attackRangeY), attackDuration - Time.time, weaponImpactClip, targetLayer, attributes);
        }

        AudioManager.PlayAttackAudio(weaponAudioClip);
    }

    void EndOfAttack()
    {
        if (!movement.isAttacking)
            return;
        
        if(curForceDurtaion <= Time.time && attackType != AttackTypes.TopDown)
        {
            rigidBody.velocity = Vector2.zero;
        }

        //End of attack and pause anim
        if (timeBtwAttacks <= Time.time || (attackType == AttackTypes.TopDown && movement.isOnGround))
        {
            movement.canFlip = true;
            attackState = AttackState.End;
            //rigidBody.velocity = Vector2.zero;
            //anim.speed = 1f;

            //Switch animation from air attack to grounded
            if (airLightCombo + airStrongCombo + airJointCombo > 0 && movement.isOnGround)
            {
                movement.isAttacking = false;
                attackType = default;
            }
        }

        //Pause attack state for give little time for next attack
        if (attackDuration <= Time.time || (attackType == AttackTypes.TopDown && movement.isOnGround))
        {
            movement.isAttacking = false;
            attackType = default;
            attributes.SetAnimationSpeed(attributes.AnimSpeed);
            //attributes.speedDivisor = 1f;
        }
    }

    void SetCombo(AttackTypes type)
    {
        switch(type)
        {
            case AttackTypes.Light:
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
                    airLightCombo++;
                    canAttack = false;
                    //if (airStrongCombo == 0)
                    //{
                    //    if (++airLightCombo >= airLightComboCount)
                    //        canAttack = false;
                    //}
                    //else
                    //{
                    //    if (++airLightCombo + airStrongCombo >= airStrongComboCount)
                    //        canAttack = false;
                    //}
                    break;
                }
            case AttackTypes.Strong:
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
                    airStrongCombo++;
                    canAttack = false;
                    //if (++airStrongCombo + airLightCombo >= airStrongComboCount)
                    //    canAttack = false;
                    break;
                }
            case AttackTypes.Joint:
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
                    airJointCombo++;
                    canAttack = false;
                    //if (++airJointCombo >= airJointComboCount)
                    //    canAttack = false;
                    break;
                }
            case AttackTypes.TopDown:
                lightCombo = strongCombo = airLightCombo = airStrongCombo = airJointCombo = jointCombo = 0; //nullify all other combo

                break;
        }
    }

    void GetWeaponAttributes(AttackTypes type)
    {
        staminaDamage = weaponAttributes.staminaDamage;
        weaponAttackType = weaponAttributes.weaponAttackType;
        weaponImpactClip = weaponAttributes.impactClip;
        shellPrefab = weaponAttributes.shellPrefab;
        shellSpeed = weaponAttributes.shellSpeed;
        shellFlyTime = weaponAttributes.shellFlyTime;
        float _delayMultiplier = movement.isOnGround ? 1f : 1.5f;

        switch (type)
        {
            case AttackTypes.Light:
                damage                      = weaponAttributes.lightAttackDamage;
                staminaCosts                = weaponAttributes.lightAttackStaminaCosts;
                weaponElement               = weaponAttributes.elements[type];
                weaponDamageType            = weaponAttributes.damageTypesOfAttacks[type]; //fix it
                attackForceDistance         = weaponAttributes.lightAttackForce;
                //If the stamina enough to attack - multiply = 1 else = 2
                //anim.speed              = attributes.Stamina >= staminaCosts ? anim.speed : anim.speed / 2f;
                timeBtwAttacks              = weaponAttributes.lightAttackSpeed / attributes.AnimSpeed + Time.time;
                attackDuration              = weaponAttributes.lightAttackSpeed / attributes.AnimSpeed * _delayMultiplier + Time.time;
                weaponSwingClip             = weaponAttributes.lightSwingClips.Length > 0 ? weaponAttributes.lightSwingClips[Random.Range(0, weaponAttributes.lightSwingClips.Length)] : null;
                weaponAudioClip             = weaponAttributes.lightAttackClips[Random.Range(0, weaponAttributes.lightAttackClips.Length)];
                break;
            case AttackTypes.Strong:
                damage                      = weaponAttributes.strongAttackDamage;
                staminaCosts                = weaponAttributes.strongAttackStaminaCosts;
                weaponElement               = weaponAttributes.elements[type];
                weaponDamageType            = weaponAttributes.damageTypesOfAttacks[type]; //fix it
                attackForceDistance         = weaponAttributes.strongAttackForce;
                //If the stamina enough to attack - multiply = 1 else = 2
                //anim.speed              = attributes.Stamina >= staminaCosts ? anim.speed : anim.speed / 2f;
                timeBtwAttacks              = (movement.isOnGround ? weaponAttributes.strongAttackSpeed : weaponAttributes.lightAttackSpeed) / attributes.AnimSpeed + Time.time;
                attackDuration              = (movement.isOnGround ? weaponAttributes.strongAttackSpeed : weaponAttributes.lightAttackSpeed) / attributes.AnimSpeed * _delayMultiplier + Time.time;
                weaponSwingClip             = weaponAttributes.strongSwingClips.Length > 0 ? weaponAttributes.strongSwingClips[Random.Range(0, weaponAttributes.strongSwingClips.Length)] : null;
                weaponAudioClip             = weaponAttributes.strongAttackClips[Random.Range(0, weaponAttributes.strongAttackClips.Length)];
                break;
            case AttackTypes.Joint:
                damage                      = weaponAttributes.jointAttackDamage;
                staminaCosts                = weaponAttributes.jointAttackStaminaCosts;
                weaponElement               = weaponAttributes.elements[type];
                weaponDamageType            = weaponAttributes.damageTypesOfAttacks[type]; //fix it
                attackForceDistance         = weaponAttributes.jointAttackForce;
                //If the stamina enough to attack - multiply = 1 else = 2
                //anim.speed              = attributes.Stamina >= staminaCosts ? anim.speed : anim.speed / 2f;
                timeBtwAttacks              = (movement.isOnGround ? weaponAttributes.jointAttackSpeed : weaponAttributes.lightAttackSpeed) / attributes.AnimSpeed + Time.time;
                attackDuration              = (movement.isOnGround ? weaponAttributes.jointAttackSpeed : weaponAttributes.lightAttackSpeed) / attributes.AnimSpeed * _delayMultiplier + Time.time;
                weaponSwingClip             = weaponAttributes.jointSwingClips.Length > 0 ? weaponAttributes.jointSwingClips[Random.Range(0, weaponAttributes.jointSwingClips.Length)] : null;
                weaponAudioClip             = weaponAttributes.jointAttackClips[Random.Range(0, weaponAttributes.jointAttackClips.Length)];
                break;
            case AttackTypes.TopDown:
                damage                      = weaponAttributes.topDownAttackDamage;//attributes.Stamina >= staminaCosts ? weaponAttributes.topDownAttackDamage : weaponAttributes.topDownAttackDamage / 10;
                weaponElement               = weaponAttributes.elements[type];
                weaponDamageType            = weaponAttributes.damageTypesOfAttacks[type]; //fix it
                attackForceDistance         = weaponAttributes.topDownAttackForce;
                timeBtwAttacks              = weaponAttributes.topDownAttackSpeed / attributes.AnimSpeed + Time.time;
                attackDuration              = weaponAttributes.topDownAttackSpeed / attributes.AnimSpeed * _delayMultiplier + Time.time;
                break;
        }

        attributes.SetAnimationSpeed(attributes.defAnimSpeed / (timeBtwAttacks - Time.time));

        if (weaponSwingClip != null)
            AudioManager.PlaySwingAudio(weaponSwingClip);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (weapon != null)
            Gizmos.DrawWireCube(weapon.position, new Vector2(attackRangeX, attackRangeY));
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

    int RandomDamage(int damage)
    {
        int percent = Mathf.CeilToInt(damage / 100f * 10f); //Get 10% of damage
        int newDamage = Random.Range(damage - percent, damage + percent);
        return newDamage;
    }
}
