using System;
using System.Collections.Generic;
using UnityEngine;

public class VoidShift : Spell
{
    int directionOfPrevForce;
    GameObject spellPrefab;

    public override void NextAnimation(PlayerSpells pSpells)
    {
        if (pSpells.attackNumber[pSpells.curSpellTitle] == 2)
        {
            if ((pSpells.movement.direction == directionOfPrevForce && (pSpells.myTransform.position.x - pSpells.startMovePosition[pSpells.curSpellTitle]) * directionOfPrevForce > 0f) ||
                (pSpells.movement.direction != directionOfPrevForce && (pSpells.myTransform.position.x - pSpells.startMovePosition[pSpells.curSpellTitle]) * directionOfPrevForce < 0f))
            {
                pSpells.movement.canFlip = true;
                pSpells.movement.FlipCharacterDirection();
                pSpells.movement.canFlip = false;
            }
        }
    }

    public override void ForceTheCharacter(PlayerSpells pSpells)
    {
        //Create audio source and destroy it after play
        AudioManager.PlayClipAtPosition(spell.audioAttacks[pSpells.attackNumber[pSpells.curSpellTitle] - 1], pSpells.myTransform.position, 1f, 5f, 0f, pSpells.myTransform);
        //GameObject _audioSourceGO = new GameObject("Spell Audio Source");
        //_audioSourceGO.transform.parent = myTransform;
        //_audioSourceGO.AddComponent<AudioSource>().clip = curSpell.spell.audioAttacks[attackNumber - 1];
        //_audioSourceGO.GetComponent<AudioSource>().Play();
        //Destroy(_audioSourceGO, curSpell.spell.audioAttacks[attackNumber - 1].length);

        GameObject _inst = Instantiate(spell.particle, pSpells.myTransform.position, spell.particle.transform.rotation, pSpells.myTransform);
        _inst.transform.Rotate(0f, 0f, pSpells.movement.direction < 0 ? 0f : 180f);

        if (pSpells.attackNumber[pSpells.curSpellTitle] == 1)
        {
            pSpells.startMovePosition.Add(pSpells.curSpellTitle, pSpells.myTransform.position.x);
            directionOfPrevForce = pSpells.movement.direction;
            spellPrefab = Instantiate(spell.spellPrefab, new Vector2(pSpells.myTransform.position.x, pSpells.myTransform.position.y - pSpells.movement.bodyCollider.size.y / 2f), spell.spellPrefab.transform.rotation);
        }
        else
            pSpells.movePosition = pSpells.startMovePosition[pSpells.curSpellTitle];
    }

    public override void SpellEnd()
    {
        Destroy(spellPrefab);
    }
}
