using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Structures;
using Enums;

public class DebuffEffect : MonoBehaviour
{
    [SerializeField] Image image = null;
    [SerializeField] Text text = null;

    int stackCount;
    int cellIndex;
    float periodEffectDelay;
    Effect effect;
    List<float> curStackDuration;
    RectTransform rectTransform;
    PlayerAtributes playerAtributes;
    PlayerEffectsController controller;

    // Start is called before the first frame update
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        ReduceStacks();
        ApplyEffect();
    }

    public void GetEffect(Vector3 cellPos, PlayerEffectsController controller, int cellIndex, int stackCount, Effect effect, Sprite icon, PlayerAtributes playerAtributes, float periodEffectDelay = 0f)
    {
        curStackDuration = new List<float>(effect.maxStacksCount);
        this.controller = controller;
        this.effect = effect;
        this.playerAtributes = playerAtributes;
        this.periodEffectDelay = periodEffectDelay;
        image.sprite = icon;
        text.text = stackCount.ToString();

        SetPosition(cellPos, cellIndex);
        SetStackDuration(stackCount);
    }

    public void GetEffect(int stackCount)
    {
        text.text = stackCount.ToString();
        SetStackDuration(stackCount);
    }

    void SetStackDuration(int stackCount)
    {
        for (int i = 0; i < stackCount; i++)
        {
            if (this.stackCount >= effect.maxStacksCount)
            {
                effect.curStackDuration.RemoveAt(0);
                effect.curStackDuration.Insert(effect.maxStacksCount - 1, Time.time + effect.stackDuration);
            }
            else
            {
                effect.curStackDuration.Add(Time.time + effect.stackDuration);
                this.stackCount++;
            }
        }
    }

    void ReduceStacks()
    {
        foreach (var _stackDuration in effect.curStackDuration.ToArray())
        {
            if (_stackDuration <= Time.time)
            {
                effect.curStackDuration.Remove(_stackDuration);
                stackCount--;
            }
        }

        if(stackCount <= 0)
        {
            controller.RemoveCell(cellIndex, effect.effect);
            Destroy(gameObject);
        }
        else
            text.text = stackCount.ToString();
    }

    public void SetPosition(Vector3 newPos, int newIndex)
    {
        rectTransform.position = newPos;
        cellIndex = newIndex;
    }

    void ApplyEffect()
    {
        switch (effect.effect)
        {
            case Effects.Freeze:
                playerAtributes.speedDivisor = playerAtributes.defSpeedDivisor - effect.value * stackCount;
                playerAtributes.SetAnimationSpeed(1f - effect.value * stackCount);
                break;
            case Effects.Bleeding:
            case Effects.Poison:
            case Effects.Burning:
                if (periodEffectDelay <= Time.time)
                {
                    playerAtributes.TakeDamage((int)effect.value, HurtType.None, effect);
                    periodEffectDelay = Time.time + effect.effectPeriod;
                }
                break;
        }
    }
}
