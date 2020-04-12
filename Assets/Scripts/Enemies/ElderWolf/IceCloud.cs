using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class IceCloud : MonoBehaviour
{
    [SerializeField] float lifeTime = default;
    float curLifeTime;
    [SerializeField] float delayApplyEffect = default;
    [SerializeField] float periodicityApplyEffect  = default;
    float curDelay;
    [SerializeField] Effect effect = new Effect(Effects.Freeze, 1);
    PlayerAttributes playerAttributes;

    void Start()
    {
        curLifeTime = Time.time + lifeTime;
        curDelay = Time.time + delayApplyEffect;
    }

    void Update()
    {
        if (curLifeTime <= Time.time)
            Destroy(gameObject);

        if(playerAttributes != null && curDelay <= Time.time)
        {
            playerAttributes.TakeEffect(effect);
            curDelay = Time.time + periodicityApplyEffect;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            playerAttributes = collision.GetComponent<PlayerAttributes>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            playerAttributes = null;
        }
    }
}
