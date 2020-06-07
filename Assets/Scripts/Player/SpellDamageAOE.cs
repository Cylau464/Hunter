using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class SpellDamageAOE : MonoBehaviour
{
    CircleCollider2D myCollider;
    PlayerSpell spell;
    SpellTitles title;
    int attackNumber;
    List<int> takenColliders = new List<int>();
    AudioSource audioSource;
    AnimatorClipInfo[] info;

    void Awake()
    {
        myCollider = GetComponent<CircleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        info = GetComponent<Animator>().GetCurrentAnimatorClipInfo(0);
    }

    private void Update()
    {
        if (myCollider.radius < spell.damageRange[attackNumber].x)
            myCollider.radius += spell.damageRange[attackNumber].x * Time.deltaTime / spell.timeBtwAttack[attackNumber];
        else
            Destroy(gameObject);
    }

    public void SetParameters(PlayerSpell spell, SpellTitles title, int attackNumber)
    {
        this.spell = spell;
        this.title = title;
        this.attackNumber = attackNumber;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 12)
        {
            int _goID = takenColliders.Find(x => x == collision.gameObject.GetInstanceID());

            if (_goID != 0) return;

            spell.damage[attackNumber] = RandomDamage(spell.damage[attackNumber]);
            spell.element.value = RandomDamage(spell.element.value);

            collision.gameObject.TryGetComponent(out Enemy _enemy);

            if (_enemy)
            {
                takenColliders.Add(collision.gameObject.GetInstanceID());
                float damageDone = _enemy.TakeDamage(spell.damage[attackNumber], spell.damageType[attackNumber], spell.element);

                if (damageDone > 0)
                    audioSource.PlayOneShot(spell.audioImpact, .25f);
            }
        }
    }

    int RandomDamage(int damage)
    {
        int percent = Mathf.CeilToInt(damage / 100f * 10f); //Get 10% of damage
        int newDamage = Random.Range(damage - percent, damage + percent);
        return newDamage;
    }
}
