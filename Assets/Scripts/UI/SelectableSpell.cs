using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Enums;
using Structures;

public class SelectableSpell : MonoBehaviour, IPointerClickHandler
{
    SpellTitles title;
    PlayerSpell spell;
    Image icon;
    public SpellSelector spellSelector;

    private void Awake()
    {
        icon = GetComponentInChildren<Image>();
    }

    public void GetSpell(SpellTitles title, PlayerSpell spell, SpellSelector spellSelector)
    {
        this.spellSelector = spellSelector;
        this.title = title;
        this.spell = spell;
        icon.sprite = spell.icon;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        spellSelector.SetSpellToSpellUI(title, spell);
    }
}
