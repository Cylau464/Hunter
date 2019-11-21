using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAtributes : MonoBehaviour 
{
    [SerializeField] int maxHealth = 10;
    int _health;

    PlayerMovement _movement;

    void Start()
    {
        _health = maxHealth;
        _movement = GetComponent<PlayerMovement>();
    }
    void Update()
    {

    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        
        if(_health <= 0)
            _movement.isDead = true;
    }
}