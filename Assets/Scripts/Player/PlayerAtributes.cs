using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class PlayerAtributes : MonoBehaviour 
{
    [SerializeField] int maxHealth = 10;
    [SerializeField] int health;
    [SerializeField] int maxStamina = 100;
    [SerializeField] int stamina;
    public int Stamina
    { 
        set
        {
            //value > current stamina = stamina recovering
            if (value > stamina)
            {
                curRestoreStaminaDelay = restoreStaminaDelay + Time.time;
                staminaRestoreValue += staminaRestoreValue < maxStaminaRestoreValue ? .1f : 0f;
            }
            else
            {
                curRestoreStaminaDelay = restoreStaminaPause + Time.time;
                staminaRestoreValue = 1f;
            }
            
            stamina = value > maxStamina ? maxStamina : value <= 0 ? 0 : value;
            statusBar.StaminaChange(stamina);
        } 
        get { return stamina; } 
    }

    [SerializeField] float restoreStaminaPause = 2f;            //Pasue after last stamina reduction
    [SerializeField] float restoreStaminaDelay = .1f;           //Delay between stamina recovery
    float curRestoreStaminaDelay;
    float staminaRestoreValue = 1f;
    float maxStaminaRestoreValue = 4f;
    public float defSpeedDivisor = 1f;
    public float speedDivisor;                             //Used to decrease horizontal speed
    public float attackSpeed;

    [SerializeField] Transform statusBarTransform = null;
    StatusBar statusBar;
    PlayerMovement movement;
    Animator anim;
    PlayerEffectsController effectsController;
    public Rigidbody2D rigidBody;

    public float timeOfLastTakenDamage;

    void Start()
    {
        speedDivisor = defSpeedDivisor;
        health = maxHealth;
        stamina = maxStamina;
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        rigidBody = GetComponent<Rigidbody2D>();
        effectsController = GetComponent<PlayerEffectsController>();
        statusBar = statusBarTransform.GetComponent<StatusBar>();
        statusBar.maxHealth = maxHealth;
        statusBar.HealthChange(health);
        statusBar.maxStamina = maxStamina;
        statusBar.StaminaChange(stamina);
    }

    void Update()
    {
        if (curRestoreStaminaDelay <= Time.time && stamina < maxStamina)
            Stamina += Mathf.RoundToInt(staminaRestoreValue);
    }

    /// <summary>
    /// Just damage
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Element element)
    {
        if (movement.isEvading) return;

        health -= damage + element.value;
        timeOfLastTakenDamage = Time.time;
        statusBar.HealthChange(health);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;        //Delete collider material for turn on friction
            return;
        }

        DamageText(damage, element);
    }

    /// <summary>
    /// Effect damage
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Effect effect)
    {
        health -= damage;
        statusBar.HealthChange(health);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;        //Delete collider material for turn on friction
            return;
        }

        DamageText(damage, effect);
    }

    /// <summary>
    /// Damage with catching
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Transform anchorPoint, Element element)
    {
        if (movement.isEvading) return;

        health -= damage + element.value;
        timeOfLastTakenDamage = Time.time;
        statusBar.HealthChange(health);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
            return;
        }
        else
            movement.GetCaught(hurtType, anchorPoint);

        DamageText(damage, element);
    }

    /// <summary>
    /// Damage with repulse
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Vector2 repulseDistantion, float dazedTime, Element element)
    {
        if (movement.isEvading) return;

        health -= damage + element.value;
        timeOfLastTakenDamage = Time.time;
        statusBar.HealthChange(health);
        movement.Repulse(repulseDistantion, dazedTime);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
            return;
        }

        DamageText(damage, element);
    }

    /// <summary>
    /// Damage with stun
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, float dazedTime, Element element)
    {
        if (movement.isEvading) return;

        if (damage > 0)
        {
            health -= damage + element.value;
            timeOfLastTakenDamage = Time.time;
            statusBar.HealthChange(health);
        }

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
            return;
        }

        movement.Stunned(dazedTime);
        DamageText(damage, element);
    }

    public void TakeEffect(Effect effect)
    {
        effectsController.GetEffect(effect);
    }

    void DamageText(int damage, Element element)
    {
        GameObject damageText = Resources.Load<GameObject>("UI/DamageNumber");
        damageText = Instantiate(damageText, transform);
        damageText.GetComponent<DamageNumber>().damage = damage;
        damageText.GetComponent<DamageNumber>().element = element;
        damageText.GetComponent<DamageNumber>().target = movement.transform;
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
}