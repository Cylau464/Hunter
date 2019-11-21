using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType { Grounding, Flying };
public enum DefenseTypes { Slash, Chop, Thrust, Blunt };
enum EnemyState { Patroling, Chasing, Attacking, Hurt, Dead };

public class EnemyCUCOLD : MonoBehaviour
{
    [Header("Movement Properties")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    
    [Header("Atributes")]
    public int maxHealth = 5;
    public int health;
    public int patrolDistance = 20;
    public MovementType movementType;
    /*
    public ValueOfDefense[] defense = {
        new ValueOfDefense("Slash", DefenseTypes.Slash, 0),
        new ValueOfDefense("Chop", DefenseTypes.Chop, 0),
        new ValueOfDefense("Thrust", DefenseTypes.Thrust, 0),
        new ValueOfDefense("Blunt", DefenseTypes.Blunt, 0)
    };
    public Element[] elementDefense = {
        new Element("Fire", Elements.Fire, 0),
        new Element("Wind", Elements.Wind, 0),
        new Element("Earth", Elements.Earth, 0),
        new Element("Water", Elements.Water, 0)
    };*/

    [Header("Attack atributes")]
    public int damage;
    /*
    public Element[] elementDamage = {
        new Element("Fire", Elements.Fire, 0),
        new Element("Wind", Elements.Wind, 0),
        new Element("Earth", Elements.Earth, 0),
        new Element("Water", Elements.Water, 0)
    };*/

    [Header("Timings")]
    public float attackDelay = .2f;
    public float attackDuration = 3f;
    public float timeBtwAttack = 2f;

    [Header("Status Flags")]
    public bool isPatroling = true;
    public bool isChasing;
    public bool isAttacking;
    public bool isHurt;
    public bool isDead;

    Rigidbody2D _rigidBody;
    PolygonCollider2D _bodyCollider;
    BoxCollider2D _playerCollider;
    SpriteRenderer _sprite;
    Transform _player;
    Transform _transform;
    Animator _anim;

    int direction;

    float _patrolDistance;

    Vector2 _startPos;

    void Start()
    {
        health = maxHealth;
        _startPos = transform.position;
        direction = (int) Mathf.Sign(Random.Range(-1, 1));
        _patrolDistance = Random.Range(direction * patrolDistance / 2, direction * patrolDistance);

        _transform = transform;
        _rigidBody = GetComponent<Rigidbody2D>();
        _bodyCollider = GetComponent<PolygonCollider2D>();
        _sprite = GetComponent<SpriteRenderer>();

        _player = GameObject.FindWithTag("Player").transform;
        _playerCollider = _player.GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (health <= 0 && !isDead)
        {
            SwitchState(EnemyState.Dead);
        }
        else
        {
            RaycastHit2D hit;

            hit = Raycast(Vector2.zero, Vector2.right * direction, 5f);

            if (!isPatroling && hit.transform == null && !isChasing)
            {
                SwitchState(EnemyState.Patroling);
            }
            else if(!isChasing && hit.transform.tag == "Player")
            {
                SwitchState(EnemyState.Chasing);
            }
        }
    }

    void FixedUpdate()
    {
        Patroling();
    }

    void SwitchState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Patroling:
                isPatroling = true;
                isChasing = false;
                isAttacking = false;
                break;
            case EnemyState.Chasing:
                isChasing = true;
                isPatroling = false;
                isAttacking = false;
                break;
            case EnemyState.Attacking:
                break;
            case EnemyState.Hurt:
                break;
            case EnemyState.Dead:
                isChasing = false;
                isPatroling = false;
                isAttacking = false;
                isHurt = false;
                isDead = true;
                break;
        }
    }

    void Patroling()
    {
        if (!isPatroling)
            return;

        _rigidBody.velocity = new Vector2(patrolSpeed * direction, _rigidBody.velocity.y);
        //if (_transform.position.x < _startPos.x + _patrolDistance)

            
    }

    void Chasing()
    {
        if (!isChasing)
            return;

        _rigidBody.velocity = new Vector2(chaseSpeed * direction, _rigidBody.velocity.y);
    }

    void FlipCharacterDirection()
    {
        direction = -direction;
        _sprite.flipX = !_sprite.flipX;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        //_dazedTime = startDazedTime;
        health -= damage;
    }

    void Death()
    {
        if (isDead)
            return;

        isDead = true;
    }

    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length)
    {
        //Если луч без указания слоя
        return Raycast(offset, rayDirection, length, 1 << 10);
    }

    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
    {
        Vector2 pos = _transform.position;

        //Рассчёт луча от чара + отступ до ближайшего объекта на указанном слое (если не указан см. выше)
        RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length, mask);
        Color color = hit ? Color.red : Color.cyan;
        Debug.DrawRay(pos + offset, rayDirection * length, color);

        return hit;
    }
}

[System.Serializable]
public struct ValueOfDefense
{
    public string title;
    public DefenseTypes defenseType;
    public int value;

    public ValueOfDefense(string title, DefenseTypes defenseType, int value)
    {
        this.title = title;
        this.defenseType = defenseType;
        this.value = value;
    }
}
