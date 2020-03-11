using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class PlayerEffectsController : MonoBehaviour
{
    [SerializeField] private EffectsDictionary effects = new EffectsDictionary()
    {
        { Effects.Freeze, new Effect(Effects.Freeze, 5, 1f, .1f) },
        { Effects.Burning, new Effect(Effects.Burning, 5, 2f, 2f) },
        { Effects.Bleeding, new Effect(Effects.Bleeding, 5, 3f, 2.5f) },
    };
    Dictionary<Effects, int> stackCount = new Dictionary<Effects, int>()
    {
        { Effects.Freeze, 0 },
        { Effects.Burning, 0 },
        { Effects.Bleeding, 0 }
    };
    Dictionary<Effects, float> periodEffectDelay = new Dictionary<Effects, float>()
    {
        { Effects.Burning, 0f },
        { Effects.Bleeding, 0f }
    };
    PlayerAtributes playerAtributes;

    void Start()
    {
        playerAtributes = GetComponent<PlayerAtributes>();
    }

    void Update()
    {
        foreach(KeyValuePair<Effects, Effect> kvp in effects)
        {
            if (stackCount[kvp.Key] <= 0)
                continue;

            ReduceStacks(kvp.Key);
            ApplyEffect(kvp.Key);
        }
    }

    public void GetEffect(Effect effect)
    {
        for (int i = 1; i <= effect.stacksCount; i++)
        {
            if (stackCount[effect.effect] >= effects[effect.effect].maxStacksCount)
            {
                effects[effect.effect].curStackDuration.RemoveAt(0);
                effects[effect.effect].curStackDuration.Insert(0, Time.time + effects[effect.effect].stackDuration);
            }
            else
            {
                effects[effect.effect].curStackDuration.Add(Time.time + effects[effect.effect].stackDuration);
                stackCount[effect.effect]++;
            }
        }
    }

    void ReduceStacks(Effects key)
    {
        foreach(var _list in effects[key].curStackDuration.ToArray())
        {
            if (_list <= Time.time)
            {
                effects[key].curStackDuration.Remove(_list);
                stackCount[key]--;
            }
        }
    }

    void ApplyEffect(Effects key)
    {
        switch(key)
        {
            case Effects.Freeze:
                playerAtributes.speedDivisor = playerAtributes.defSpeedDivisor - effects[key].value * stackCount[key];
                playerAtributes.SetAnimationSpeed(1f - effects[key].value * stackCount[key]);
                break;
            case Effects.Burning:
                if(periodEffectDelay[key] <= Time.time)
                {
                    playerAtributes.TakeDamage((int)effects[key].value, HurtType.None, effects[key]);
                    periodEffectDelay[key] = Time.time + effects[key].effectPeriod;
                }
                break;
        }
    }
}