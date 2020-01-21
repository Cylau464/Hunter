using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAtributes : MonoBehaviour 
{
    [SerializeField] int maxHealth = 10;
    [SerializeField] int health;

    PlayerMovement movement;

    void Start()
    {
        health = maxHealth;
        movement = GetComponent<PlayerMovement>();
    }
    void Update()
    {

    }

    public void TakeDamage(int damage, HurtType hurtType)
    {
        health -= damage;
        
        if(health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;        //Delete collider material for turn on friction
        }
    }

    //Catched
    public void TakeDamage(int damage, HurtType hurtType, Transform anchorPoint)
    {
        health -= damage;

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
        }
        else
            movement.GetCaught(hurtType, anchorPoint);
    }

    public void TakeDamage(int damage, HurtType hurtType, Vector2 repulseDistantion, float dazedTime)
    {
        health -= damage;

        movement.Repulse(repulseDistantion, dazedTime);

        if (health <= 0)
        {
            movement.isDead = true;
            movement.bodyCollider.sharedMaterial = null;
        }
    }
}