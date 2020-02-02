using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class IceBreath : MonoBehaviour
{
    public Vector2 colliderSize;
    public EnemySpell spell;
    [SerializeField] float yOffsetCollider = 1f;
    PolygonCollider2D myCollider;
    Transform player;
    Animator anim;
    SpriteRenderer sprite;
    bool damageDone;
    int playerLayer = 10;
    int isCastParamID;

    void Awake()
    {
        anim = transform.GetComponent<Animator>();
        isCastParamID = Animator.StringToHash("isCast");
        sprite = transform.GetComponent<SpriteRenderer>();

        myCollider = transform.GetComponent<PolygonCollider2D>();
        myCollider.enabled = false;
        myCollider.pathCount = 1;
        myCollider.SetPath(0, new[] {
                    new Vector2(0f, yOffsetCollider), new Vector2(0, -yOffsetCollider),
                    new Vector2((colliderSize.x) * Mathf.Sign(transform.localPosition.x), -colliderSize.y / 2f),
                    new Vector2((colliderSize.x) * Mathf.Sign(transform.localPosition.x), colliderSize.y / 2f)
                });
        myCollider.enabled = true;
    }

    public void FlipObject()
    {
        myCollider = myCollider ?? transform.GetComponent<PolygonCollider2D>();
        sprite = sprite ?? transform.GetComponent<SpriteRenderer>();
        transform.localPosition = new Vector2(-transform.localPosition.x, transform.localPosition.y);

        myCollider.enabled = false;
        myCollider.pathCount = 1;
        myCollider.SetPath(0, new[] {
                    new Vector2(0f, yOffsetCollider), new Vector2(0, -yOffsetCollider),
                    new Vector2((colliderSize.x) *  Mathf.Sign(transform.localPosition.x), -colliderSize.y / 2f),
                    new Vector2((colliderSize.x) *  Mathf.Sign(transform.localPosition.x), colliderSize.y / 2f)
                });
        myCollider.enabled = true;

        sprite.flipX = !sprite.flipX;
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
        anim.SetBool(isCastParamID, true);
        float _castTime = Time.time + spell.castTime;
        float _curDelay = 0f;
        int _hitCount = 1;

        while (_castTime > Time.time)
        {
            if (_curDelay <= Time.time)
                damageDone = false;

            if (player != null && !damageDone)
            {
                Vector2 _direction = new Vector2(Mathf.Sign(player.transform.position.x - transform.position.x), Mathf.Sign(player.transform.position.y - transform.position.y));

                if(_hitCount < Mathf.Floor(spell.castTime / spell.periodicityDamage))
                    player.GetComponent<PlayerAtributes>().TakeDamage(spell.firstDamage, HurtType.Repulsion, new Vector2(spell.repulseVector.x * _direction.x, spell.repulseVector.y * _direction.y), spell.dazedTime);
                else
                    player.GetComponent<PlayerAtributes>().TakeDamage(spell.lastDamage, HurtType.Repulsion, new Vector2(spell.repulseVector.x * 2f * _direction.x, spell.repulseVector.y * _direction.y), spell.dazedTime);

                _hitCount++;
                damageDone = true;
                _curDelay = spell.periodicityDamage + Time.time;
            }

            yield return new WaitForEndOfFrame();
        }

        anim.SetBool(isCastParamID, false);
        gameObject.SetActive(false);
    }
}