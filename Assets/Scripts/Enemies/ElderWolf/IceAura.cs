using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class IceAura : MonoBehaviour
{
    [SerializeField] Effect effect = new Effect(Effects.Freeze, 1);
    [SerializeField] float delay = 1.5f;
    float curDelay;
    [SerializeField] float effectPeriodicity = .5f;
    PlayerAttributes playerAttributes;

    void OnTriggerStay2D(Collider2D col)
    {
        if (playerAttributes == null)
        {
            if (col.TryGetComponent(out PlayerAttributes _attributes))
            {
                playerAttributes = _attributes;
                curDelay = delay + Time.time;
            }
        }
        else
        {
            if(curDelay <= Time.time)
            {
                playerAttributes.TakeEffect(effect);
                curDelay = effectPeriodicity + Time.time;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.TryGetComponent(out PlayerAttributes _attributes))
        {
            playerAttributes = null;
        }
    }
}