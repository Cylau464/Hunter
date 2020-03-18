using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class HowlCirclePulse : MonoBehaviour
{
    public EnemySpell spell;
    int isCastParamID;
    int playerLayer = 10;
    CircleCollider2D circleCol;
    Animator anim;
    Transform player;
    PlayerAtributes playerAtributes;

    void Awake()
    {
        anim = transform.GetComponent<Animator>();
        circleCol = transform.GetComponent<CircleCollider2D>();
        circleCol.radius = .2f;
        isCastParamID = Animator.StringToHash("isCast");
    }

    //Started from ElderWolf script
    public IEnumerator CirclePulse()
    {
        anim.SetBool(isCastParamID, true);
        circleCol.radius = .2f;
        float _castTime = Time.time + spell.castTime;
        float _сircleMaxRadius = spell.castRange;
        float _curPeriod = 0f;

        while (_castTime > Time.time)
        {
            if (player != null)
            {
                //Not last pulse
                if (_castTime - Time.time > spell.periodicityDamage)
                {
                    if (_curPeriod < Time.time)
                    {
                        playerAtributes.TakeDamage(0, HurtType.Repulsion, spell.dazedTime, spell.elementDamage);
                        _curPeriod = spell.periodicityDamage + Time.time;
                    }
                }
                else
                {
                    Vector2 _direction = new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), Mathf.Sign(player.transform.position.y - transform.position.y));
                    playerAtributes.TakeDamage(spell.firstDamage, HurtType.Repulsion, new Vector2(spell.repulseVector.x * _direction.x, spell.repulseVector.y * _direction.y), spell.dazedTime, spell.elementDamage);
                }
            }

            if(circleCol.radius < _сircleMaxRadius)
                circleCol.radius += spell.castRange * 3f * Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        anim.SetBool(isCastParamID, false);
        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == playerLayer)
        {
            player = collision.transform;
            playerAtributes = player.GetComponent<PlayerAtributes>();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == playerLayer)
        {
            player = null;
            playerAtributes = null;
        }
    }
}