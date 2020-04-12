using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class IceBreath : MonoBehaviour
{
    public Vector2 colliderSize;
    public EnemySpell spell;
    [SerializeField] float yOffsetCollider = 1f;
    PolygonCollider2D myCollider;
    Transform player;
    Transform particleTransform;
    Animator anim;
    SpriteRenderer sprite;
    ParticleSystem particle;
    bool damageDone;
    int playerLayer = 10;
    int isCastParamID;
    [SerializeField] AudioSource audioSource = null;

    void Awake()
    {
        //anim = transform.GetComponent<Animator>();
        //isCastParamID = Animator.StringToHash("isCast");
        //sprite = transform.GetComponent<SpriteRenderer>();
        foreach (Transform child in transform)
        {
            particleTransform = child;
            particle = child.GetComponent<ParticleSystem>();
        }

        myCollider = transform.GetComponent<PolygonCollider2D>();
        myCollider.enabled = false;
        myCollider.pathCount = 1;
        myCollider.SetPath(0, new[] {
                    new Vector2(0f, yOffsetCollider), new Vector2(0, -yOffsetCollider),
                    new Vector2(colliderSize.x / 2f * Mathf.Sign(transform.localPosition.x), -colliderSize.y / 2f),
                    new Vector2(colliderSize.x / 2f * Mathf.Sign(transform.localPosition.x), colliderSize.y / 2f)
                });
        myCollider.enabled = true;

        gameObject.SetActive(false);
    }

    public void FlipObject()
    {
        myCollider = myCollider ?? transform.GetComponent<PolygonCollider2D>();
        //sprite = sprite ?? transform.GetComponent<SpriteRenderer>();
        transform.localPosition = new Vector2(-transform.localPosition.x, transform.localPosition.y);
        particleTransform.Rotate(0f, 0f, 180f);

        myCollider.enabled = false;
        myCollider.pathCount = 1;
        myCollider.SetPath(0, new[] {
                    new Vector2(0f, yOffsetCollider), new Vector2(0, -yOffsetCollider),
                    new Vector2(colliderSize.x / 2f *  Mathf.Sign(transform.localPosition.x), -colliderSize.y / 2f),
                    new Vector2(colliderSize.x / 2f *  Mathf.Sign(transform.localPosition.x), colliderSize.y / 2f)
                });
        myCollider.enabled = true;

        //sprite.flipX = !sprite.flipX;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == playerLayer)
            player = col.transform;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == playerLayer)
            player = null;
    }

    //Started from ElderWolf script
    public IEnumerator IceBreathCast()
    {
        //anim.SetBool(isCastParamID, true);
        particle.Play();
        audioSource.PlayOneShot(spell.spellClip);
        float _castTime = Time.time + spell.castTime;
        float _curDelay = 0f;
        int _hitCount = 1;
        PlayerAttributes _playerAttributes = null;
        //Coroutine _speedDivisorCoroutine = null;
        myCollider.SetPath(0, new[] {
                    new Vector2(0f, yOffsetCollider), new Vector2(0, -yOffsetCollider),
                    new Vector2(colliderSize.x / 2f * Mathf.Sign(transform.localPosition.x), -colliderSize.y / 2f),
                    new Vector2(colliderSize.x / 2f * Mathf.Sign(transform.localPosition.x), colliderSize.y / 2f)
                });

        while (_castTime > Time.time)
        {
            if (_curDelay <= Time.time)
                damageDone = false;

            //Increase collider
            Vector2[] _colPath = myCollider.GetPath(0);

            if (Mathf.Abs(_colPath[3].x) < colliderSize.x)
            {
                myCollider.SetPath(0, new[]
                {
                    _colPath[0], _colPath[1],
                    new Vector2(_colPath[2].x + colliderSize.x / 2f * Time.deltaTime * Mathf.Sign(transform.localPosition.x), _colPath[2].y),
                    new Vector2(_colPath[3].x + colliderSize.x / 2f * Time.deltaTime * Mathf.Sign(transform.localPosition.x), _colPath[3].y)
                });
            }

            if (player != null && !damageDone)
            {
                Vector2 _direction = new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), Mathf.Sign(player.transform.position.y - transform.parent.position.y));
                _playerAttributes = player.GetComponent<PlayerAttributes>();

                if (_hitCount < Mathf.Floor(spell.castTime / spell.periodicityDamage))
                {
                    //_playerAttributes.TakeDamage(spell.firstDamage, HurtType.None, spell.dazedTime, spell.elementDamage);
                    _playerAttributes.TakeDamage(0, HurtType.None, /*new Vector2(spell.repulseVector.x * _direction.x, 0f), 0f,*/ spell.elementDamage);
                    _playerAttributes.TakeEffect(spell.effect);

                    //if (_speedDivisorCoroutine != null)
                    //{
                    //    StopCoroutine(_speedDivisorCoroutine);
                    //    _playerAttributes.defSpeedDivisor = 1f;
                    //    Debug.Log(Time.time + " TIME of cour stop");
                    //}

                    /*_speedDivisorCoroutine = */
                    _playerAttributes.StartCoroutine(_playerAttributes.DecreaseSpeedDivisor(.7f, spell.dazedTime));
                }
                else
                {
                    spell.elementDamage.value = Mathf.CeilToInt(spell.elementDamage.value * 1.5f);
                    _playerAttributes.TakeDamage(0, HurtType.Repulsion, new Vector2(spell.repulseVector.x * _direction.x, spell.repulseVector.y * _direction.y), spell.dazedTime, spell.elementDamage);
                    _playerAttributes.TakeEffect(spell.effect);
                }

                _hitCount++;
                damageDone = true;
                _curDelay = spell.periodicityDamage + Time.time;
            }

            yield return new WaitForEndOfFrame();
        }

        particle.Stop();
        audioSource.Stop();
        //anim.SetBool(isCastParamID, false);
        gameObject.SetActive(false);
    }
}