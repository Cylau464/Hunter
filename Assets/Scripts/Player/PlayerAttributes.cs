using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class PlayerAttributes : MonoBehaviour 
{
    [Header("Main Attributes")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] int health;
    public int Health
    {
        set { health = value > maxHealth ? maxHealth : value <= 0 ? 0 : value; }
        get { return health; }
    }
    [SerializeField] int healingPower = 50;
    [SerializeField] int healingPotionCount = 2;
    public int curHealingPotionCount;
    [SerializeField] int maxStamina = 100;
    float staminaFloat;
    [SerializeField] int stamina;
    public float Stamina
    { 
        set
        {
            //value > current stamina = stamina recovering
            if (value > stamina)
            {
                //curRestoreStaminaDelay = restoreStaminaDelay + Time.time;
                //staminaRestoreValue += staminaRestoreValue < maxStaminaRestoreValue ? .1f : 0f;
            }
            else
            {
                curRestoreStaminaDelay = restoreStaminaPause + Time.time;
                staminaFloat = value > maxStamina ? maxStamina : value <= 0 ? 0 : value;
                //staminaRestoreValue = 1f;
            }

            stamina = (int)(value > maxStamina ? maxStamina : value <= 0 ? 0 : value);
            statusBar.StaminaChange(stamina);
        } 
        get { return stamina; } 
    }
    [SerializeField] int maxEnergy = 100;
    int energy;
    float energyFloat;
    int energyPoints;

    public int EnergyPoints
    {
        set
        {
            if (value < energyPoints)
            {
                int _subtract = energyPoints - value;
                energyPoints = value;
                Energy -= Mathf.Floor((_subtract == 0 ? energyPoints : _subtract) * (maxEnergy / 3f));
            }    
            else
                energyPoints = value;
        }
        get { return energyPoints; }
    }
    public float Energy
    {
        set
        {
            if (value > maxEnergy)
            {
                energyFloat = energy = maxEnergy;
            }
            else if(value <= 0f)
            {
                energyFloat = energy = 0;
            }
            else if(value > energyFloat)
            {
                energyFloat += value - energy;
                energy = Mathf.FloorToInt(energyFloat);
            }
            else
            {
                energyFloat = value + (energyFloat % 1);
                energy = Mathf.FloorToInt(energyFloat);
            }

            energyPoints = Mathf.FloorToInt(energy / (maxEnergy / 3f)); // dont use property EnergyPoints here because it's bugging
            energyBar.GetParameters(energy, maxEnergy, EnergyPoints);
        }
        get { return energy; }
    }

    [SerializeField] float restoreStaminaPause = 2f;            //Pasue after last stamina reduction
    //[SerializeField] float restoreStaminaDelay = .1f;           //Delay between stamina recovery
    float curRestoreStaminaDelay;
    [SerializeField] float staminaRestoreValue = 25f;
    //float maxStaminaRestoreValue = 4f;

    [Header("Secondary Attributes")]
    public float defSpeedDivisor = 1f;
    public float speedDivisor;                             //Used to decrease horizontal speed
    public float defAttackSpeed = 1f;
    public float attackSpeed;
    [SerializeField] float healingDelay = .5f;
    [HideInInspector] public float curHealingDelay;
    public bool isInvulnerable;

    [Header("References")]
    [SerializeField] Transform statusBarTransform = null;
    StatusBar statusBar;
    [SerializeField] EnergyBar energyBar = null;
    [SerializeField] UsableItemUI healingPotion = null;
    PlayerMovement movement;
    Animator anim;
    PlayerEffectsController effectsController;
    [HideInInspector] public Rigidbody2D rigidBody;

    public float timeOfLastTakenDamage;

    [Header("Camera Properties")]
    [SerializeField] CameraShake cameraShake = null;

    void Start()
    {
        speedDivisor = defSpeedDivisor;
        attackSpeed = defAttackSpeed;
        health = maxHealth;
        curHealingPotionCount = healingPotionCount;
        staminaFloat = stamina = maxStamina;
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        rigidBody = GetComponent<Rigidbody2D>();
        effectsController = GetComponent<PlayerEffectsController>();
        statusBar = statusBarTransform.GetComponent<StatusBar>();
        statusBar.maxHealth = maxHealth;
        statusBar.HealthChange(health);
        statusBar.maxStamina = maxStamina;
        statusBar.StaminaChange(stamina);
        healingPotion.SetItemCount(curHealingPotionCount);
    }

    void Update()
    {
        if (curRestoreStaminaDelay <= Time.time && stamina < maxStamina)
        {
            staminaFloat += staminaRestoreValue * Time.deltaTime;
            Stamina = staminaFloat;
        }
    }

    /// <summary>
    /// Just damage
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Element element)
    {
        if (isInvulnerable) return;

        if (damage > 0 || element.value > 0)
        {
            health -= damage + element.value;
            timeOfLastTakenDamage = Time.time;
            statusBar.HealthChange(health);
            //Decrease energy
            float _percentageConversionToEnergy = .05f;
            Energy -= damage * _percentageConversionToEnergy;

            DamageText(damage, element);
        }

        if (health <= 0)
        {
            movement.isDead = true;
            isInvulnerable = true;
            movement.bodyCollider.sharedMaterial = null;        //Delete collider material for turn on friction
            AudioManager.PlayDeathAudio();
            return;
        }
        else
            AudioManager.PlayHurtAudio();

        //Camera shake
        float _maxAmplitude = 8f;
        float _shakeAmplitude = (damage + element.value) / (float)maxHealth * _maxAmplitude;
        cameraShake.Shake(_shakeAmplitude, 1f, .25f);
    }

    /// <summary>
    /// Effect damage
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Effect effect)
    {
        if (movement.isDead) return;

        if (damage > 0)
        {
            health -= damage;
            statusBar.HealthChange(health);
            //Decrease energy
            float _percentageConversionToEnergy = .05f;
            Energy -= damage * _percentageConversionToEnergy;

            DamageText(damage, effect);
        }

        if (health <= 0)
        {
            movement.isDead = true;
            isInvulnerable = true;
            movement.bodyCollider.sharedMaterial = null;        //Delete collider material for turn on friction
            AudioManager.PlayDeathAudio();
            return;
        }
        else
            AudioManager.PlayHurtAudio();

        //Camera shake
        float _maxAmplitude = 8f;
        float _shakeAmplitude = damage / (float)maxHealth * _maxAmplitude;
        cameraShake.Shake(_shakeAmplitude, 1f, .25f);
    }

    /// <summary>
    /// Damage with catching
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Transform anchorPoint, Rigidbody2D rigidBody, Element element)
    {
        if (isInvulnerable) return;

        if (damage > 0 || element.value > 0)
        {
            health -= damage + element.value;
            timeOfLastTakenDamage = Time.time;
            statusBar.HealthChange(health);
            //Decrease energy
            float _percentageConversionToEnergy = .05f;
            Energy -= (damage + element.value) * _percentageConversionToEnergy;

            DamageText(damage, element);
        }

        if (health <= 0)
        {
            movement.isDead = true;
            isInvulnerable = true;
            movement.bodyCollider.sharedMaterial = null;
            AudioManager.PlayDeathAudio();
            return;
        }
        else
        {
            movement.transform.position = anchorPoint.position;
            movement.GetCaught(hurtType, rigidBody);
            AudioManager.PlayHurtAudio();
        }   

        //Camera shake
        float _maxAmplitude = 8f;
        float _shakeAmplitude = (damage + element.value) / (float)maxHealth * _maxAmplitude;
        cameraShake.Shake(_shakeAmplitude, 1f, .25f);
    }

    /// <summary>
    /// Damage with repulse
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Vector2 repulseDistantion, float dazedTime, Element element)
    {
        if (isInvulnerable) return;

        if (damage > 0 || element.value > 0)
        {
            health -= damage + element.value;
            timeOfLastTakenDamage = Time.time;
            statusBar.HealthChange(health);
            repulseDistantion.y = movement.isOnGround && repulseDistantion.y < 0f ? 0f : repulseDistantion.y;
            movement.Repulse(repulseDistantion, dazedTime);
            //Decrease energy
            float _percentageConversionToEnergy = .05f;
            Energy -= (damage + element.value) * _percentageConversionToEnergy;

            DamageText(damage, element);
        }

        if (health <= 0)
        {
            movement.isDead = true;
            isInvulnerable = true;
            movement.bodyCollider.sharedMaterial = null;
            AudioManager.PlayDeathAudio();
            return;
        }
        else
            AudioManager.PlayHurtAudio();

        //Camera shake
        float _maxAmplitude = 8f;
        float _shakeAmplitude = (damage + element.value) / (float)maxHealth * _maxAmplitude;
        cameraShake.Shake(_shakeAmplitude, 1f, .25f);
    }

    /// <summary>
    /// Damage with stun
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, float dazedTime, Element element)
    {
        if (isInvulnerable) return;

        if (damage > 0 || element.value > 0)
        {
            health -= damage + element.value;
            timeOfLastTakenDamage = Time.time;
            statusBar.HealthChange(health);
            DamageText(damage, element);
            //Decrease energy
            float _percentageConversionToEnergy = .05f;
            Energy -= (damage + element.value) * _percentageConversionToEnergy;
        }

        if (health <= 0)
        {
            movement.isDead = true;
            isInvulnerable = true;
            movement.bodyCollider.sharedMaterial = null;
            AudioManager.PlayDeathAudio();
            return;
        }
        else
            AudioManager.PlayHurtAudio();

        movement.hurtType = hurtType;
        movement.Stunned(dazedTime);

        //Camera shake
        float _maxAmplitude = 8f;
        float _shakeAmplitude = (damage + element.value) / (float)maxHealth * _maxAmplitude;
        cameraShake.Shake(_shakeAmplitude, 1f, .25f);
    }

    public void TakeEffect(Effect effect)
    {
        if (movement.isEvading) return;

        effectsController.GetEffect(effect);
    }

    void DamageText(int damage, Element element)
    {
        GameObject _damageText = Resources.Load<GameObject>("UI/DamageNumber");
        _damageText = Instantiate(_damageText, transform);
        _damageText.GetComponent<DamageNumber>().GetParameters(damage, movement.transform, element);
    }

    /// <summary>
    /// Damage from effect text
    /// </summary>
    void DamageText(int damage, Effect effect)
    {
        GameObject damageText = Resources.Load<GameObject>("UI/EffectDamage");
        damageText = Instantiate(damageText, transform);
        damageText.GetComponent<EffectDamage>().effect = effect;
    }

    public void SetAnimationSpeed(float speed)
    {
        anim.speed = speed;
    }

    public void AnimationHealingEnd(bool value)
    {
        anim.SetBool("healingEnd", value);
    }

    void HealUp()
    {
        curHealingPotionCount--;
        healingPotion.SetItemCount(curHealingPotionCount);
        Health += healingPower;
        statusBar.HealthChange(health);
        AnimationHealingEnd(true);
        curHealingDelay = Time.time + healingDelay;
        AudioManager.PlayHealUpAudio();
    }

    public IEnumerator DecreaseSpeedDivisor(float value, float duration)
    {
        if (movement.isEvading) yield break;

        defSpeedDivisor = value;

        yield return new WaitForSeconds(duration);

        defSpeedDivisor = 1f;
    }
}