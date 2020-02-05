using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class IceAura : MonoBehaviour
{
    [SerializeField] int minDamage = 1;
    [SerializeField] int maxDamage = 5;
    Element element = new Element("Ice", Elements.Ice, 0);
    [SerializeField] float delay = 1.5f;
    float curDelay;
    [SerializeField] float damagePeriodicity = .5f;
    [SerializeField] float dazedTime = .3f;
    PlayerAtributes playerAtributes;

    void Start()
    {
        element.value = minDamage;
    }

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
                if (element.value < maxDamage)
                {
                    playerAtributes.TakeDamage(0, HurtType.None, element);
                    element.value = Mathf.RoundToInt(element.value + .4f);
                }
                else
                    playerAtributes.TakeDamage(0, HurtType.Repulsion, dazedTime, element);

                curDelay = damagePeriodicity + Time.time;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.TryGetComponent(out PlayerAtributes _atributes))
            playerAtributes = null;
    }
}