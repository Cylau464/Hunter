using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class HowlCirclePulse : MonoBehaviour
{
    public EnemySpell spell;
    //int isCastParamID;
    int playerLayer = 10;
    CircleCollider2D circleCol;
    //Animator anim;
    ParticleSystem particle;
    Transform player;
    PlayerAttributes playerAttributes;
    [SerializeField] AudioSource audioSource = null;

    void Awake()
    {
        //sprite = transform.GetComponent<SpriteRenderer>();
        foreach (Transform child in transform)
        {
            particle = child.GetComponent<ParticleSystem>();
        }
        //anim = transform.GetComponent<Animator>();
        circleCol = transform.GetComponent<CircleCollider2D>();
        //isCastParamID = Animator.StringToHash("isCast");

        gameObject.SetActive(false);
    }

    //Started from ElderWolf script
    public IEnumerator CirclePulse()
    {
        PlayHowlAudio();
        particle.Play();
        //anim.SetBool(isCastParamID, true);
        circleCol.radius = circleCol.radius = .2f;
        float _castTime = Time.time + spell.castTime;
        float _сircleMaxRadius = spell.castRange;
        //float _curPeriod = 0f;

        while (_castTime > Time.time)
        {
            if (player != null)
            {
                playerAttributes.TakeEffect(spell.effect);
                playerAttributes.StartCoroutine(playerAttributes.DecreaseSpeedDivisor(.4f, spell.dazedTime));
                //playerAttributes.TakeDamage(0, HurtType.Stun, _castTime - Time.time, spell.elementDamage);
                ////Not last pulse
                //if (_castTime - Time.time > spell.periodicityDamage)
                //{
                //    if (_curPeriod < Time.time)
                //    {
                //        playerAttributes.TakeDamage(0, HurtType.Stun, spell.dazedTime * .95f, spell.elementDamage);
                //        _curPeriod = spell.periodicityDamage + Time.time;
                //    }
                //}
                //else
                //{
                //    Vector2 _direction = new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), Mathf.Sign(player.transform.position.y - transform.position.y));
                //    playerAttributes.TakeDamage(spell.firstDamage, HurtType.Repulsion, new Vector2(spell.repulseVector.x * _direction.x, spell.repulseVector.y * _direction.y), spell.dazedTime, spell.elementDamage);
                //    break;
                //}
            }

            if (circleCol.radius < _сircleMaxRadius)
                circleCol.radius += spell.castRange * 1f * Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        particle.Stop();
        //anim.SetBool(isCastParamID, false);
        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == playerLayer)
        {
            player = collision.transform;
            playerAttributes = player.GetComponent<PlayerAttributes>();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == playerLayer)
        {
            player = null;
            playerAttributes = null;
        }
    }

    void PlayHowlAudio()
    {
        if (audioSource == null)
            return;

        audioSource.PlayOneShot(spell.spellClip, .5f);
    }
}