using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Enums;
using Structures;

public class SpellSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] SpellsList spellsList = null;
    [SerializeField] GameObject spellCellPrefab = null;
    [HideInInspector] public SpellUI editableSpell;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 cellPos = new Vector2(-65f, 72f);
        
        foreach (KeyValuePair<SpellTitles, GameObject> kvp in spellsList.spells)
        {
            Spell _spell = kvp.Value.GetComponent<Spell>();
            SelectableSpell _selectableSpell = Instantiate(spellCellPrefab, rectTransform.position + cellPos, transform.rotation, transform).GetComponent<SelectableSpell>();
            cellPos.x += 65;
            _selectableSpell.GetSpell(_spell.title, _spell.spell, this);
        }
    }

    public void SetSpellToSpellUI(SpellTitles title, PlayerSpell spell)
    {
        editableSpell.ChangeSpell(title, spell);
        GameManager.UIOverlapsMouse = false;
        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameManager.UIOverlapsMouse = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.UIOverlapsMouse = false;
    }
}
