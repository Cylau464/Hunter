using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class DamageBox : MonoBehaviour
{
    int damage;
    int staminaDamage;
    DamageTypes damageType;
    Element element;
    Vector2 position;
    Vector2 colliderSize;
    float lifeTime;
    float dazedTime;            //For enemy's damage box
    Vector2 repulseVector;      //For enemy's damage box
    int targetLayer;
    BoxCollider2D myCollider;
    AudioClip impactClip;
    AudioClip spellClip;
    [SerializeField] AudioSource audioSource = null;
    Enemy enemyParent;
    PlayerAttributes playerAttributes;
    List<int> takenColliders = new List<int>();
    bool isSpell;

    // Start is called before the first frame update
    void Start()
    {
        myCollider = transform.GetComponent<BoxCollider2D>();
        myCollider.size = colliderSize;
        transform.position = position;

        if(spellClip != null)
            audioSource.PlayOneShot(spellClip);
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeTime <= Time.time)
        {
            if(myCollider.enabled)
                myCollider.enabled = false;

            if(!audioSource.isPlaying)
                Destroy(gameObject);
        } 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (targetLayer == 12)
        {
            //Player's damage box
            if (collision.gameObject.layer == targetLayer)
            {
                int _goID = takenColliders.Find(x => x == collision.gameObject.GetInstanceID());

                if (_goID != 0) return;

                damage = RandomDamage(damage);
                collision.gameObject.TryGetComponent(out Enemy _enemy);

                if (_enemy)
                {
                    takenColliders.Add(collision.gameObject.GetInstanceID());
                    int _damageDone = _enemy.TakeDamage(damage, damageType, element, staminaDamage);

                    if (_damageDone > 0)
                        AudioManager.PlayClipAtPosition(impactClip, transform.position, .25f, 10f);
                        //audioSource.PlayOneShot(impactClip, .25f);

                    //float _percentageConversionToEnergy = .05f;

                    //if (!isSpell)
                    //    playerAttributes.Energy += _damageDone * _percentageConversionToEnergy;
                }
            }
        }
        else if(targetLayer == 10)
        {
            //Enemy's damage box
            if (collision.gameObject.layer == targetLayer)
            {
                int _goID = takenColliders.Find(x => x == collision.gameObject.GetInstanceID());

                if (_goID != 0) return;

                damage = RandomDamage(damage);
                collision.gameObject.TryGetComponent(out PlayerAttributes _player);

                if (_player)
                {
                    takenColliders.Add(collision.gameObject.GetInstanceID());
                    int _damageDone = _player.TakeDamage(damage, HurtType.Repulsion, repulseVector, dazedTime, element);

                    if(_damageDone > 0)
                        AudioManager.PlayClipAtPosition(impactClip, transform.position, .25f, 10f);
                    //audioSource.PlayOneShot(impactClip, .25f);

                    enemyParent.isHitPlayer = true;
                }
            }
        }
    }

    int RandomDamage(int damage)
    {
        int percent = Mathf.CeilToInt(damage / 100f * 10f); //Get 10% of damage
        int newDamage = Random.Range(damage - percent, damage + percent);
        return newDamage;
    }

    /// <summary>
    /// Player's damage box
    /// </summary>
    public void GetParameters(int damage, int staminaDamage, DamageTypes damageType, Element element, Vector2 position, Vector2 colliderSize, float lifeTime, AudioClip impactClip, int targetLayer, PlayerAttributes playerAttributes, bool isSpell = false, AudioClip spellClip = null)
    {
        this.damage = damage;
        this.staminaDamage = staminaDamage;
        this.damageType = damageType;
        this.element = element;
        this.position = position;
        this.colliderSize = colliderSize;
        this.lifeTime = lifeTime + Time.time;
        this.impactClip = impactClip;
        this.targetLayer = targetLayer;
        this.playerAttributes = playerAttributes;
        this.isSpell = isSpell;
        this.spellClip = spellClip;

        dazedTime = 0f;
        repulseVector = Vector2.zero;
    }

    /// <summary>
    /// Enemy's damage box
    /// </summary>
    public void GetParameters(int damage, Element element, Vector2 position, Vector2 colliderSize, float lifeTime, AudioClip impactClip, int targetLayer, float dazedTime, Vector2 repulseVector, Enemy enemyParent)
    {
        this.damage = damage;
        this.element = element;
        this.position = position;
        this.colliderSize = colliderSize;
        this.lifeTime = lifeTime + Time.time;
        this.impactClip = impactClip;
        this.targetLayer = targetLayer;
        this.dazedTime = dazedTime;
        this.repulseVector = repulseVector;
        this.enemyParent = enemyParent;

        damageType = DamageTypes.Slash;
    }
}
