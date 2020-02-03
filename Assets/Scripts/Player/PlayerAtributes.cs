using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

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
    public void TakeDamage(int damage, HurtType hurtType, Element element)
    {
        if (movement.isEvading) return;

        health -= damage + element.value;
        timeOfLastTakenDamage = Time.time;
        healthBar.HealthChange(health);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;        //Delete collider material for turn on friction
            return;
        }

        DamageText(damage, element);
    }

    /// <summary>
    /// Damage with catching
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Transform anchorPoint, Element element)
    {
        if (movement.isEvading) return;

        health -= damage + element.value;
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
        healthBar.HealthChange(health);
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

        health -= damage + element.value;
        timeOfLastTakenDamage = Time.time;
        healthBar.HealthChange(health);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
            return;
        }

        movement.Stunned(dazedTime);
        DamageText(damage, element);
    }

    void DamageText(int damage, Element element)
    {
        GameObject damageText = Resources.Load<GameObject>("DamageNumber");
        damageText = Instantiate(damageText, transform);
        damageText.GetComponent<DamageNumber>().damage = damage;
        damageText.GetComponent<DamageNumber>().element = element;
        damageText.GetComponent<DamageNumber>().target = movement.transform;
    }
}