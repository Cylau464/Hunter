using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class IceSpike : MonoBehaviour
{
    [HideInInspector] public EnemySpell spell;
    [SerializeField] float lifeTime = 3f;
    [SerializeField] float prepareTime = 1f;
    float maxColSizeY;
    float maxColOffsetY;
    float curLifeTime;
    float curPrepareTime;
    
    int spellCastParamID;
    int spellEndParamID;

    bool damageDone;
    Animator anim;
    BoxCollider2D myCollider;
    Transform player;
    SpellStates state = SpellStates.Prepare;

    // Start is called before the first frame update
    void Start()
    {
        anim = transform.GetComponent<Animator>();
        myCollider = transform.GetComponent<BoxCollider2D>();
        maxColSizeY = myCollider.size.y;
        maxColOffsetY = myCollider.offset.y;
        myCollider.size = new Vector2(myCollider.size.x, .3f);
        myCollider.offset = new Vector2(myCollider.offset.x, .1f);
        spellCastParamID = Animator.StringToHash("spellCast");
        spellEndParamID = Animator.StringToHash("spellEnd");
        curLifeTime = lifeTime + prepareTime + Time.time;
        curPrepareTime = prepareTime + Time.time;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch(state)
        {
            case SpellStates.Prepare:
                Prepare();
                break;
            case SpellStates.Cast:
                Cast();
                break;
            case SpellStates.End:
                End();
                break;
        }
    }

    void Prepare()
    {
        if (curPrepareTime <= Time.time)
        {
            anim.SetTrigger(spellCastParamID);
            state = SpellStates.Cast;
        }
    }

    void Cast()
    {
        if (myCollider.size.y < maxColSizeY)
            myCollider.size = new Vector2(myCollider.size.x, myCollider.size.y + .3f);

        if (myCollider.offset.y < maxColOffsetY)
            myCollider.offset = new Vector2(myCollider.offset.x, myCollider.offset.y + .3f);

        if (player != null && !damageDone)
        {
            int _direction = PlayerDirection();
            player.GetComponent<PlayerAtributes>().TakeDamage(spell.lastDamage, HurtType.Repulsion, new Vector2(spell.repulseVector.x * _direction, spell.repulseVector.y), spell.dazedTime);
            damageDone = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            player = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            player = null;
        }
    }

    void Casted()
    {
        state = SpellStates.End;
    }

    void End()
    {
        myCollider.isTrigger = false;

        if (curLifeTime <= Time.time)
            Destroy(gameObject);
    }

    int PlayerDirection()
    {
        return (int) Mathf.Sign(player.position.x - transform.position.x);
    }
}
