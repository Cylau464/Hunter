using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAtributes : MonoBehaviour 
{
    [SerializeField] int maxHealth = 10;
    [SerializeField] int health;

    public float timeOfLastTakenDamage;

    PlayerMovement movement;

    void Start()
    {
        health = maxHealth;
        movement = GetComponent<PlayerMovement>();
    }
    void Update()
    {

    }
    /// <summary>
    /// Just damage
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType)
    {
        health -= damage;
        timeOfLastTakenDamage = Time.time;
        
        if(health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;        //Delete collider material for turn on friction
        }

        DamageText(damage);
    }

    /// <summary>
    /// Damage with catching
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, Transform anchorPoint)
    {
        health -= damage;
        timeOfLastTakenDamage = Time.time;

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
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
        health -= damage;
        timeOfLastTakenDamage = Time.time;

        movement.Repulse(repulseDistantion, dazedTime);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
        }

        DamageText(damage);
    }

    /// <summary>
    /// Damage with stun
    /// </summary>
    public void TakeDamage(int damage, HurtType hurtType, float dazedTime)
    {
        health -= damage;
        timeOfLastTakenDamage = Time.time;

        movement.Stunned(dazedTime);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
        }

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