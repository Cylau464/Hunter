using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Structures;
using Enums;

public class SpellUI : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public GameObject spellSelector = null;
    [HideInInspector] public SpellsList spellsList = null;
    public PlayerSpell spell;
    public SpellTitles title;
    public Dictionary<SpellTitles, float> curCooldown = new Dictionary<SpellTitles, float>(2);
    Dictionary<SpellTitles, float> castTime = new Dictionary<SpellTitles, float>(2);
    Dictionary<SpellTitles, SpellState> spellState = new Dictionary<SpellTitles, SpellState>(2);
    public int cellIndex;

    Image icon;
    Image cooldownIcon;
    Image castIcon;
    Image activeIcon;
    Text keyText;

    bool spellOnCD;

    void Awake()
    {
        Image[] _images = GetComponentsInChildren<Image>();
        icon = _images[0];
        cooldownIcon = _images[1];
        castIcon = _images[2];
        activeIcon = _images[3];
        keyText = GetComponentInChildren<Text>();

        SetParameters();
    }

    void Update()
    {
        if (curCooldown.ContainsKey(title) && curCooldown[title] > Time.time)
            DecreaseFillOnCooldownImage();
        else if (cooldownIcon.fillAmount > 0f)
            cooldownIcon.fillAmount = 0f;

        if (castTime.ContainsKey(title) && castTime[title] > Time.time)
            DecreaseFillOnCastImage();
        else if (castIcon.fillAmount > 0f)
            castIcon.fillAmount = 0f;
    }

    void DecreaseFillOnCooldownImage()
    {
        cooldownIcon.fillAmount = (curCooldown[title] - Time.time) / spell.cooldown;
    }

    void DecreaseFillOnCastImage()
    {
        castIcon.fillAmount = (spell.castTime - (castTime[title] - Time.time)) / spell.castTime;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SpellSelectorActivate();
    }

    void SpellSelectorActivate()
    {
        if (curCooldown[title] > Time.time) return;

        spellSelector.SetActive(true);
        spellSelector.GetComponent<SpellSelector>().editableSpell = this;
    }

    public void ChangeSpell(SpellTitles title, PlayerSpell spell)
    {
        this.title = title;
        this.spell = spell;
        SetParameters();
        //Create window with spell list
    }

    void SetParameters()
    {
        icon.sprite = spell.icon;
        keyText.text = (cellIndex + 1).ToString();

        if (!curCooldown.ContainsKey(title))
            curCooldown.Add(title, 0f);

        if (!castTime.ContainsKey(title))
            castTime.Add(title, 0f);

        if (!spellState.ContainsKey(title))
            spellState.Add(title, SpellState.None);

        ChangeState(spellState[title]);
    }

    public void SetCooldown(SpellTitles title)
    {
        curCooldown[title] = spell.cooldown + Time.time;
        cooldownIcon.fillAmount = 1f;
    }

    void SetCastTime(SpellTitles title)
    {
        castTime[title] = spell.castTime + Time.time;
    }

    public void ChangeState(SpellState state)
    {
        spellState[title] = state;
        activeIcon.enabled = false;

        switch (state)
        {
            case SpellState.None:
                icon.sprite = spell.icon;
                castTime.Remove(title);
                break;
            case SpellState.Cast:
                SetCastTime(title);
                break;
            case SpellState.Active:
                activeIcon.enabled = true;
                break;
            case SpellState.Waiting:
                icon.sprite = spell.waitingIcon;
                break;
        }
    }
}
