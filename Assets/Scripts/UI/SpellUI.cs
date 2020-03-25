using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Structures;
using Enums;

class SpellUI : MonoBehaviour, IPointerClickHandler
{
    public Sprite icon;
    public SpellsTitle title;
    [SerializeField] int cellIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        ChangeSpell();
    }

    void ChangeSpell()
    {
        //Create window with spell list
    }
}
