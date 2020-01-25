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
    }
}