using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class HowlCirclePulse : MonoBehaviour
{
    public EnemySpell spell;
    bool damageDone;
    int isCastParamID;
    CircleCollider2D circleCol;
    Animator anim;
    SpriteRenderer sprite;
    Transform player;

    void Start()
    {
        sprite = transform.GetComponent<SpriteRenderer>();
        anim = transform.GetComponent<Animator>();
        circleCol = transform.GetComponent<CircleCollider2D>();
        circleCol.radius = .2f;
        sprite.enabled = false;
        isCastParamID = Animator.StringToHash("isCast");
    }

    //Started from ElderWolf script
    public IEnumerator CirclePulse()
    {
        sprite.enabled = true;
        anim.SetBool(isCastParamID, true);
        circleCol.radius = .2f;
        float _castTime = Time.time + spell.castTime;
        float _howlCircleMaxRadius = spell.castRange;

        while (_castTime > Time.time)
        {
            //Circle reached max radius -> reset radius to min
            if (circleCol.radius >= _howlCircleMaxRadius)
            {
                circleCol.radius = .2f;
                damageDone = false;
            }

            if(player != null && !damageDone)
            {
                player.GetComponent<PlayerAtributes>().TakeDamage(spell.firstDamage, HurtType.Repulsion, spell.dazedTime);
                damageDone = true;
            }

            circleCol.radius += 1f;
            yield return new WaitForEndOfFrame();
        }

        anim.SetBool(isCastParamID, false);
        sprite.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
            player = collision.transform;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
            player = null;
    }
}