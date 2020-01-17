using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAtributes : MonoBehaviour 
{
    [SerializeField] int maxHealth = 10;
    int health;

    PlayerMovement movement;

    void Start()
    {
        health = maxHealth;
        movement = GetComponent<PlayerMovement>();
    }
    void Update()
    {

    }

    public void TakeDamage(int damage, HurtTypesEnum hurtType)
    {
        health -= damage;
        
        if(health <= 0)
            movement.isDead = true;
    }

    //Catched
    public void TakeDamage(int damage, HurtTypesEnum hurtType, Transform anchorPoint)
    {
        health -= damage;

        if (health <= 0)
            movement.isDead = true;
        else
            movement.GetCaught(hurtType, anchorPoint);
    }

    public void TakeDamage(int damage, HurtTypesEnum hurtType, Vector2 repulseDistantion, float dazedTime)
    {
        health -= damage;

        movement.Repulse(repulseDistantion, dazedTime);

        if (health <= 0)
            movement.isDead = true;
    }
}