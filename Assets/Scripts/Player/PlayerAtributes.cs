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
        if (movement.isHurt) return;

        health -= damage;
        
        if(health <= 0)
            movement.isDead = true;
    }
}