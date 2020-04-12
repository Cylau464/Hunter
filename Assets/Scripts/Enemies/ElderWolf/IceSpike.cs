using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class IceSpike : MonoBehaviour
{
    [HideInInspector] public EnemySpell spell;
    [SerializeField] float lifeTime = 3f;
    [SerializeField] float prepareTime = 1f;
    [SerializeField] int health = 1;
    float maxColSizeY;
    float maxColOffsetY;
    float curLifeTime;
    float curPrepareTime;
    
    int spellCastParamID;
    int spellEndParamID;
    int spellDestroyedParamID;

    bool damageDone;
    Animator anim;
    BoxCollider2D myCollider;
    PolygonCollider2D tipCollider;
    PlayerAttributes playerAttributes;
    SpriteRenderer sprite;
    EnemySpellStates state = EnemySpellStates.Prepare;
    [SerializeField] AudioClip spikePrepareClip = null;
    [SerializeField] AudioClip spikeCastClip = null;
    [SerializeField] AudioClip spikeSmashClip = null;
    [SerializeField] AudioSource audioSource = null;
    [SerializeField] GameObject iceCloudPrefab = null;

    // Start is called before the first frame update
    void Awake()
    {
        anim = transform.GetComponent<Animator>();
        myCollider = GetComponent<BoxCollider2D>();
        tipCollider = GetComponent<PolygonCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        maxColSizeY = myCollider.size.y;
        maxColOffsetY = myCollider.offset.y;
        myCollider.size = new Vector2(myCollider.size.x, .3f);
        myCollider.offset = new Vector2(myCollider.offset.x, .1f);
        spellCastParamID = Animator.StringToHash("spellCast");
        spellEndParamID = Animator.StringToHash("spellEnd");
        spellDestroyedParamID = Animator.StringToHash("destroyed");
        curLifeTime = lifeTime + prepareTime + Time.time;
        curPrepareTime = prepareTime + Time.time;

        audioSource.PlayOneShot(spikePrepareClip);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch(state)
        {
            case EnemySpellStates.Prepare:
                Prepare();
                break;
            case EnemySpellStates.Cast:
                Cast();
                break;
            case EnemySpellStates.End:
                End();
                break;
            case EnemySpellStates.Destroy:
                Destroying();
                break;
        }
    }

    void Prepare()
    {
        if (curPrepareTime <= Time.time)
        {
            anim.SetTrigger(spellCastParamID);
            audioSource.PlayOneShot(spikeCastClip);
            state = EnemySpellStates.Cast;
        }
    }

    void Cast()
    {
        if (myCollider.size.y < maxColSizeY)
            myCollider.size = new Vector2(myCollider.size.x, myCollider.size.y + maxColSizeY / .25f * Time.deltaTime); //.25f - cast animation duration

        if (myCollider.offset.y < maxColOffsetY)
            myCollider.offset = new Vector2(myCollider.offset.x, myCollider.offset.y + maxColOffsetY / .25f * Time.deltaTime);

        if (playerAttributes != null && !damageDone)
        {
            int _direction = playerAttributesDirection();
            float _repulseForceY = (1f - myCollider.size.y / maxColSizeY) * spell.repulseVector.y;
            playerAttributes.TakeDamage(spell.firstDamage, HurtType.Repulsion, new Vector2(spell.repulseVector.x * _direction, _repulseForceY), spell.dazedTime, spell.elementDamage);
            playerAttributes.TakeEffect(spell.effect);
            damageDone = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            playerAttributes = collision.GetComponent<PlayerAttributes>();
        }
        if (state == EnemySpellStates.End && collision.gameObject.layer == 16) //16 layer = PlayerDamageObjects
        {
            health--;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            playerAttributes = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.transform.tag == "Player")
        //{
        //    //playerAttributes = collision.transform.GetComponent<PlayerAttributes>();
        //}
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            //playerAttributes = null;
        }
    }

    void Casted()
    {
        state = EnemySpellStates.End;
        anim.SetTrigger(spellEndParamID);
    }

    void End()
    {
        myCollider.isTrigger = false;
        tipCollider.enabled = true;

        if (playerAttributes != null && !damageDone)
        {
            if (playerAttributes.rigidBody.velocity.y < 0)
            {
                int _direction = playerAttributesDirection();
                float _repulseForceY = spell.repulseVector.y * .3f;
                playerAttributes.TakeDamage(spell.firstDamage, HurtType.Repulsion, new Vector2(spell.repulseVector.x * _direction, _repulseForceY), spell.dazedTime, spell.elementDamage);
                playerAttributes.TakeEffect(spell.effect);
                damageDone = true;
            }
        }

        if (curLifeTime <= Time.time || health <= 0)
        {
            anim.SetTrigger(spellDestroyedParamID);
            tipCollider.enabled = false;
            myCollider.enabled = false;
            Instantiate(iceCloudPrefab, transform.position, transform.rotation);
            audioSource.PlayOneShot(spikeSmashClip);
            state = EnemySpellStates.Destroy;
        }
    }

    void Destroying()
    {

    }

    int playerAttributesDirection()
    {
        return (int) Mathf.Sign(playerAttributes.transform.position.x - transform.position.x);
    }

    void DestroySpike()
    {
        sprite.enabled = false;

        if(audioSource.isPlaying)
        {
            Invoke("DestroySpike", .01f);
            return;
        }

        Destroy(gameObject);
    }
}
