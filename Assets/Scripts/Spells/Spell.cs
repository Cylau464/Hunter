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

    public virtual void NextAnimation(PlayerSpells pSpells)
    {
        Debug.Log("DEFAULT NEXT ANIMATION METHOD");
    }

    public virtual void ForceTheCharacter(PlayerSpells pSpells)
    {
        Debug.Log("DEFAULT FORCE METHOD");
    }

    public virtual void GiveSpellDamage(PlayerSpells pSpells, Vector3 attackPosition)
    {
        Debug.Log("DEFAULT DAMAGE METHOD");
    }

    public virtual void SetSpellToWaitingState(PlayerSpells pSpells)
    {
        Debug.Log("DEFAULT WAITING METHOD");
    }

    public virtual void SpellEnd()
    {
        Debug.Log("DEFAULT END METHOD");
    }

    //protected void DestroyOnEnd()
    //{
    //    Destroy(gameObject);
    //}
}
