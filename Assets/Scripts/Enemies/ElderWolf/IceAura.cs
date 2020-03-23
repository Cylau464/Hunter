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
    [SerializeField] float damagePeriodicity = .5f;
    PlayerAtributes playerAtributes;

    void OnTriggerStay2D(Collider2D col)
    {
        if (playerAtributes == null)
        {
            if (col.TryGetComponent(out PlayerAtributes _atributes))
            {
                playerAtributes = _atributes;
                curDelay = delay + Time.time;
            }
        }
        else
        {
            if(curDelay <= Time.time)
            {
                playerAtributes.TakeEffect(effect);
                curDelay = damagePeriodicity + Time.time;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.TryGetComponent(out PlayerAtributes _atributes))
        {
            playerAtributes = null;
        }
    }
}