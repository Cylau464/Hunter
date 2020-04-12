using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Structures;
using Enums;

public class SpellBar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject[] spellCells = new GameObject[3];
    [SerializeField] GameObject spellSelector = null;

    [HideInInspector] public SpellUI[] spellUI = new SpellUI[3];

    void Awake()
    {
        for(int i = 0; i < spellCells.Length; i++)
        {
            spellUI[i] = spellCells[i].GetComponent<SpellUI>();
            spellUI[i].cellIndex = i;
            spellUI[i].spellSelector = spellSelector;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameManager.UIOverlapsMouse = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.UIOverlapsMouse = false;
    }

    public void SetSpells(Spell[] spells)
    {
        for (int i = 0; i < spells.Length; i++)
        {
            spellUI[i].ChangeSpell(spells[i].title, spells[i].spell);
        }
    }
}
