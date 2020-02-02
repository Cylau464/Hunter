using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAtributes : MonoBehaviour 
{
    [SerializeField] int maxHealth = 10;
    [SerializeField] int health;
    [SerializeField] Transform hpBarTransform = null;
    HealthBar healthBar;

    public float timeOfLastTakenDamage;

    PlayerMovement movement;

    void Start()
    {
        health = maxHealth;
        movement = GetComponent<PlayerMovement>();
        healthBar = hpBarTransform.GetComponent<HealthBar>();
        healthBar.maxHealth = maxHealth;
        healthBar.HealthChange(health);
    }
    void Update()
    {

    }
    /// <summary>
    /// Just damage
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType)
    {
        if (movement.isEvading) return;

        health -= damage;
        timeOfLastTakenDamage = Time.time;
        healthBar.HealthChange(health);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;        //Delete collider material for turn on friction
            return;
        }

        DamageText(damage);
    }

    /// <summary>
    /// Damage with catching
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Transform anchorPoint)
    {
        if (movement.isEvading) return;

        health -= damage;
        timeOfLastTakenDamage = Time.time;
        healthBar.HealthChange(health);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
            return;
        }
        else
            movement.GetCaught(hurtType, anchorPoint);

        DamageText(damage);
    }

    /// <summary>
    /// Damage with repulse
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Vector2 repulseDistantion, float dazedTime)
    {
        if (movement.isEvading) return;

        health -= damage;
        timeOfLastTakenDamage = Time.time;
        healthBar.HealthChange(health);
        movement.Repulse(repulseDistantion, dazedTime);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
            return;
        }

        DamageText(damage);
    }

    /// <summary>
    /// Damage with stun
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, float dazedTime)
    {
        if (movement.isEvading) return;

        health -= damage;
        timeOfLastTakenDamage = Time.time;
        healthBar.HealthChange(health);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
            return;
        }

        movement.Stunned(dazedTime);
        DamageText(damage);
    }

    void DamageText(int damage)
    {
        GameObject damageText = Resources.Load<GameObject>("DamageNumber");
        damageText = Instantiate(damageText, transform);
        damageText.GetComponent<DamageNumber>().damage = damage;
        damageText.GetComponent<DamageNumber>().target = movement.transform;
    }
}