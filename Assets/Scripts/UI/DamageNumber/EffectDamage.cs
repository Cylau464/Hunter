using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Structures;
using Enums;

public class EffectDamage : MonoBehaviour
{
    public Effect effect;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Text damageText = null;
    [SerializeField] Image effectIcon = null;
    [SerializeField] float lifeTime = 2f;
    [SerializeField] EffectsIconsDictionary effectIcons = null;
    float curLifeTime;

    // Start is called before the first frame update
    void Start()
    {
        curLifeTime = Time.time + lifeTime;

        if (!effect.Equals(default(Effect)))
        {
            damageText.color = effect.color;
            damageText.text = effect.value.ToString();
            effectIcon.sprite = effectIcons[effect.effect];
        }
        else
        {
            damageText.text = "Где урон, урод?";
            effectIcon.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (curLifeTime <= Time.time)
            Destroy(gameObject);

        transform.position = new Vector3(transform.position.x + moveSpeed * Time.deltaTime, transform.position.y + moveSpeed * Time.deltaTime, transform.position.z);
    }
}
