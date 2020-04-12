using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class DamageBox : MonoBehaviour
{
    int damage;
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
    [SerializeField] AudioSource audioSource = null;
    Enemy enemyParent;
    PlayerAttributes playerAttributes;
    List<int> takenColliders = new List<int>();
    bool isSpell;
    int hitCount; //TEST

    // Start is called before the first frame update
    void Start()
    {
        myCollider = transform.GetComponent<BoxCollider2D>();
        myCollider.size = colliderSize;
        transform.position = position;
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeTime <= Time.time && !audioSource.isPlaying)
            Destroy(gameObject);
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
                    float damageDone = _enemy.TakeDamage(damage, damageType, element);
                    audioSource.PlayOneShot(impactClip, .25f);
                    float _percentageConversionToEnergy = .05f;

                    if (!isSpell)
                        playerAttributes.Energy += damageDone * _percentageConversionToEnergy * 10f;

                    hitCount++;
                    Debug.Log("HIT COINT: " + hitCount);
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
                    _player.TakeDamage(damage, HurtType.Repulsion, repulseVector, dazedTime, element);
                    audioSource.PlayOneShot(impactClip, .25f);
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
    public void GetParameters(int damage, DamageTypes damageType, Element element, Vector2 position, Vector2 colliderSize, float lifeTime, AudioClip impactClip, int targetLayer, PlayerAttributes playerAttributes, bool isSpell = false)
    {
        this.damage = damage;
        this.damageType = damageType;
        this.element = element;
        this.position = position;
        this.colliderSize = colliderSize;
        this.lifeTime = lifeTime + Time.time;
        this.impactClip = impactClip;
        this.targetLayer = targetLayer;
        this.playerAttributes = playerAttributes;
        this.isSpell = isSpell;

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
