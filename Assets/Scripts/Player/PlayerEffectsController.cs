using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class PlayerEffectsController : MonoBehaviour
{
    [SerializeField] private EffectsDictionary effects = new EffectsDictionary()
    {
        { Effects.Freeze, new Effect(5, 1f) },
        { Effects.Burning, new Effect(5, 2f) },
        { Effects.Bleeding, new Effect(5, 3f) },
    };
    Dictionary<Effects, int> stackCount = new Dictionary<Effects, int>()
    {
        { Effects.Freeze, 0 },
        { Effects.Burning, 0 },
        { Effects.Bleeding, 0}
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
        foreach (KeyValuePair<Effects, Effect> kvp in effects)
        {
            if (stackCount[kvp.Key] <= 0)
                continue;

            ReduceStacks(kvp);
            ApplyEffect(kvp);
        }
    }

    public void GetEffect(Effects effect, int numberOfStacks, float value)
    {
        for (int i = 1; i <= numberOfStacks; i++)
        {
            if (stackCount[effect] >= effects[effect].maxStackCount)
            {
                effects[effect].curStackDuration.RemoveAt(0);
                effects[effect].curStackDuration.Insert(0, Time.time + effects[effect].stackDuration);
            }
            else
            {
                effects[effect].curStackDuration.Add(Time.time + effects[effect].stackDuration);
                stackCount[effect]++;
            }
        }
    }

    void ReduceStacks(KeyValuePair<Effects, Effect> kvp)
    {
        kvp.Value.curStackDuration.ForEach(x =>
        {
            if (x <= Time.time)
            {
                kvp.Value.curStackDuration.Remove(x);
                stackCount[kvp.Key]--;
            }
        });
    }

    void ApplyEffect(KeyValuePair<Effects, Effect> kvp)
    {
        switch(kvp.Key)
        {
            case Effects.Freeze:
                playerAtributes.speedDivisor = playerAtributes.defSpeedDivisor - kvp.Value.value * stackCount[kvp.Key];
                playerAtributes.SetAnimationSpeed(1f - kvp.Value.value * stackCount[kvp.Key]);
                break;
            case Effects.Burning:
                if(periodEffectDelay[kvp.Key] <= Time.time)
                {
                    playerAtributes.TakeDamage(kvp.Value.value, HurtType.None, )
                    periodEffectDelay[kvp.Key] = Time.time + kvp.Value.effectPeriod;
                }
                break;
        }
    }
}