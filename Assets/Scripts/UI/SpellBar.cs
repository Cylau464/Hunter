using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Structures;
using Enums;

class SpellBar : MonoBehaviour
{
    GameObject[] spellCells = new GameObject[3];

    Image[] spellIcons = new Image[3];
    Text[] spellKey = new Text[3];
    public SpellsTitle[] spellTitle = new SpellsTitle[3];

    void Start()
    {
        for(int i = 0; i < spellCells.Length; i++)
        {
            Spell _spell = spellCells[i].GetComponent<Spell>();
            spellTitle[i] = _spell.title;
            spellIcons[i] = spellCells[i].GetComponentInChildren<Image>();
            spellIcons[i].sprite = _spell.icon;
            spellKey[i] = spellCells[i].GetComponentInChildren<Text>();
        }
    }
}
