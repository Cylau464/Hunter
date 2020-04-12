using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Structures;
using Enums;

public class Spell : MonoBehaviour
{
    public PlayerSpell spell;
    public SpellTitles title;

    private void Start()
    {
        Debug.Log("CREARED");
    }

    protected void DestroyOnEnd()
    {
        Destroy(gameObject);
    }
}
