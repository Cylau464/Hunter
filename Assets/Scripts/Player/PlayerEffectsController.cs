using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Structures;
using Enums;

public class PlayerEffectsController : MonoBehaviour
{
    [SerializeField] List<Image> debuffIcons = new List<Image>(5);
    [SerializeField] List<Text> debuffText = new List<Text>(5);
    Dictionary<Effects, int> debuffCellIndex = new Dictionary<Effects, int>(5)
    {
        { Effects.Freeze, 100 },
        { Effects.Burning, 100 },
        { Effects.Bleeding, 100 },
        { Effects.Poison, 100 },
        { Effects.Root, 100 },
    };
    [SerializeField] EffectsIconsDictionary effectIcons = null;

    [SerializeField] private EffectsDictionary effects = new EffectsDictionary()
    {
        { Effects.Freeze, new Effect(Effects.Freeze, 5, 1f, .1f) },
        { Effects.Burning, new Effect(Effects.Burning, 5, 2f, 2f, .5f) },
        { Effects.Bleeding, new Effect(Effects.Bleeding, 5, 3f, 2.5f, .5f) },
        { Effects.Poison, new Effect(Effects.Poison, 5, 3f, 2.5f, 1f) },
    };
    Dictionary<Effects, int> stackCount = new Dictionary<Effects, int>()
    {
        { Effects.Freeze, 0 },
        { Effects.Burning, 0 },
        { Effects.Bleeding, 0 },
        { Effects.Poison, 0 },
    };
    Dictionary<Effects, float> periodEffectDelay = new Dictionary<Effects, float>()
    {
        { Effects.Burning, 0f },
        { Effects.Bleeding, 0f },
        { Effects.Poison, 0 },
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

            ReduceStacks(kvp.Key);
            ApplyEffect(kvp.Key);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Effect _effect = new Effect();
            int _rand = Random.Range(0, 4);

            switch(_rand)
            {
                case 0:
                    _effect = new Effect(Effects.Bleeding, 1);
                    break;
                case 1:
                    _effect = new Effect(Effects.Burning, 1);
                    break;
                case 2:
                    _effect = new Effect(Effects.Freeze, 1);
                    break;
                case 3:
                    _effect = new Effect(Effects.Poison, 1);
                    break;
            }

            GetEffect(_effect);
        }
    }

    public void GetEffect(Effect effect)
    {
        for (int i = 1; i <= effect.stacksCount; i++)
        {
            if (stackCount[effect.effect] >= effects[effect.effect].maxStacksCount)
            {
                effects[effect.effect].curStackDuration.RemoveAt(0);
                effects[effect.effect].curStackDuration.Insert(effects[effect.effect].maxStacksCount - 1, Time.time + effects[effect.effect].stackDuration);
            }
            else
            {
                effects[effect.effect].curStackDuration.Add(Time.time + effects[effect.effect].stackDuration);
                stackCount[effect.effect]++;
            }
        }

        ApplyEffect(effect.effect);
    }

    void ReduceStacks(Effects key)
    {
        foreach (var _stackDuration in effects[key].curStackDuration.ToArray())
        {
            if (_stackDuration <= Time.time)
            {
                effects[key].curStackDuration.Remove(_stackDuration);
                stackCount[key]--;

                DisabledIconUI(key);
            }
        }
    }

    void DisabledIconUI(Effects key)
    {
        if (stackCount[key] == 0)
        {
            //Cell is not last...
            if (debuffCellIndex[key] < debuffCellIndex.Count - 1)
            {
                //...and next cell is active
                if (debuffText[debuffCellIndex[key] + 1].enabled)
                {
                    for (int i = debuffCellIndex[key]; i < debuffCellIndex.Count; i++)
                    {
                        if (i < debuffCellIndex.Count - 1)
                        {
                            debuffIcons[i].sprite = debuffIcons[i + 1].sprite;
                            debuffText[i].text = debuffText[i + 1].text;
                        }
                        else
                        {
                            debuffIcons[i].enabled = false;
                            debuffText[i].enabled = false;
                        }
                    }
                }
                //...and next cell is not active
                else
                {
                    debuffIcons[debuffCellIndex[key]].enabled = false;
                    debuffText[debuffCellIndex[key]].enabled = false;
                }
            }
            //Cell is last
            else
            {
                debuffIcons[debuffCellIndex[key]].enabled = false;
                debuffText[debuffCellIndex[key]].enabled = false;
            }

            debuffCellIndex[key] = 100; //100 random value, beacuse cant set null to int var
        }

        if (stackCount[key] != 0)
            ApplyEffect(key);
    }

    void ApplyToIconsUI(Effects key)
    {
        if (stackCount[key] <= 0)
            return;

        if (debuffCellIndex[key] == 100)
        {
            for (int i = 0; i < debuffText.Count; i++)
            {
                if (debuffText[i].enabled == false)
                {
                    debuffIcons[i].sprite = effectIcons[key];
                    debuffIcons[i].enabled = true;
                    debuffText[i].text = stackCount[key].ToString();
                    debuffText[i].enabled = true;

                    debuffCellIndex[key] = i;
                    break;
                }
            }
        }
        else
        {
            debuffText[debuffCellIndex[key]].text = stackCount[key].ToString();
        }
    }

    void ApplyEffect(Effects key)
    {
        switch(key)
        {
            case Effects.Freeze:
                playerAtributes.speedDivisor = playerAtributes.defSpeedDivisor - effects[key].value * stackCount[key];
                playerAtributes.SetAnimationSpeed(1f - effects[key].value * stackCount[key]);
                ApplyToIconsUI(key);
                break;
            case Effects.Bleeding:
            case Effects.Poison:
            case Effects.Burning:
                if(periodEffectDelay[key] <= Time.time)
                {
                    playerAtributes.TakeDamage((int)effects[key].value, HurtType.None, effects[key]);
                    periodEffectDelay[key] = Time.time + effects[key].effectPeriod;
                    ApplyToIconsUI(key);
                }
                break;
        }
    }
}