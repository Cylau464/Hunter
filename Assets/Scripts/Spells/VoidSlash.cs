using System.Collections.Generic;
using UnityEngine;
using Enums;
using Structures;

public class VoidSlash : Spell
{
    GameObject spellPrefab;
    SpellBar spellBar;
    int cellIndex;
    [SerializeField] float[] shellsSizeMultiplier = new float[3];

    public override void GiveSpellDamage(PlayerSpells pSpells, Vector3 attackPosition)
    {
        int _targetLayer = 12;
        int _attackNumber = pSpells.attackNumber[pSpells.curSpellTitle] - 1;

        float _attackCountMultiplier = 1f / spell.attackCount;
        float _damageMultiplier = Mathf.Clamp((pSpells.chargingDamageMultiplier - _attackNumber * _attackCountMultiplier) / _attackCountMultiplier, 0f, 1f);
        pSpells.attackCount = Mathf.Clamp(Mathf.CeilToInt(pSpells.attackCount * pSpells.chargingDamageMultiplier), 1, pSpells.attackCount);
        int _damage = Mathf.FloorToInt(spell.damage[_attackNumber] * _damageMultiplier);
        int _elementDamage = Mathf.FloorToInt(spell.element.value * _damageMultiplier);
        Vector2 _shellColliderSize = spell.damageRange[_attackNumber];
        Element _element = spell.element;
        _element.value = _elementDamage;

        spellBar = pSpells.spellBar;
        cellIndex = pSpells.cellIndex;
        spellPrefab = Instantiate(spell.spellPrefab, attackPosition, spell.spellPrefab.transform.rotation);
        Vector3 _prefabScale = spellPrefab.transform.localScale;
        _prefabScale.x *= pSpells.movement.direction;
        _prefabScale *= shellsSizeMultiplier[_attackNumber];
        spellPrefab.transform.localScale = _prefabScale;
        _shellColliderSize *= shellsSizeMultiplier[_attackNumber];

        AudioManager.PlayClipAtPosition(spell.audioAttacks[_attackNumber], pSpells.myTransform.position, 1f, 5f);
        spellPrefab.GetComponent<DamageBox>().GetParameters(_damage, spell.staminaDamage[_attackNumber], spell.damageType[_attackNumber], _element, attackPosition, _shellColliderSize, spell.shellLifeTime[_attackNumber], spell.audioImpact, _targetLayer, pSpells.attributes, true, spell.shellClip[_attackNumber]);
        spellPrefab.GetComponent<Rigidbody2D>().AddForce(new Vector2(spell.forceDirection[_attackNumber].x * pSpells.movement.direction, spell.forceDirection[_attackNumber].y), ForceMode2D.Impulse);
    }

    public override void SpellEnd()
    {
        spellBar.spellUI[cellIndex].ChangeState(SpellState.None);
        //Destroy(spellPrefab);
    }
}
