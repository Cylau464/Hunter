using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Structures;
using Enums;

class Spell : MonoBehaviour
{
    public PlayerSpell spell;
    public SpellsTitle title;

    private void Start()
    {
        Debug.Log("CREARED");
    }
}
