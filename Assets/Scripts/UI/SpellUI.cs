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
    public int cellIndex;

    Image icon;
    Image cooldownIcon;
    Text keyText;

    bool spellOnCD;

    void Awake()
    {
        Image[] _images = GetComponentsInChildren<Image>();
        icon = _images[0];//GetComponentInChildren<Image>();
        cooldownIcon = _images[1];//GetComponentInChildren<Image>();
        keyText = GetComponentInChildren<Text>();

        SetParameters();
    }

    void Update()
    {
        if (curCooldown.ContainsKey(title) && curCooldown[title] > Time.time)
            DecreaseFillOnCooldownImage();
        else if (cooldownIcon.fillAmount > 0f)
            cooldownIcon.fillAmount = 0f;
    }

    void DecreaseFillOnCooldownImage()
    {
        cooldownIcon.fillAmount = (curCooldown[title] - Time.time) / spell.cooldown;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //SpellSelectorActivate();
    }

    //void SpellSelectorActivate()
    //{
    //    spellSelector.SetActive(true);
    //    spellSelector.GetComponent<SpellSelector>().editableSpell = this;
    //}

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
    }

    public void SetCooldown(SpellTitles title)
    {
        curCooldown[title] = spell.cooldown + Time.time;
        cooldownIcon.fillAmount = 1f;
    }
}
