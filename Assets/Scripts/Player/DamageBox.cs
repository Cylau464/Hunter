using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class DamageBox : MonoBehaviour
{
    public int damage;
    public DamageTypes damageType;
    public Element element;
    public Vector2 position;
    public Vector2 colliderSize;
    public float lifeTime;
    BoxCollider2D myCollider;

    // Start is called before the first frame update
    void Start()
    {
        myCollider = transform.GetComponent<BoxCollider2D>();
        myCollider.size = colliderSize;
        transform.position = position;
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeTime <= Time.time)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.layer == 12)
        {
            damage = RandomDamage(damage);
            collision.gameObject.TryGetComponent(out Enemy _enemy);

            if (_enemy)
                _enemy.TakeDamage(damage, damageType, element);
        }
    }

    int RandomDamage(int damage)
    {
        int percent = Mathf.CeilToInt(damage / 100f * 10f); //Get 10% of damage
        int newDamage = Random.Range(damage - percent, damage + percent);
        return newDamage;
    }
}
