using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Structures;
using Enums;

public class DamageNumber : MonoBehaviour
{
    public int damage;
    public Element element;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Text physicDamage = null;
    [SerializeField] Text elementDamage = null;
    [SerializeField] Image elementIcon = null;
    [SerializeField] float lifeTime = 2f;
    [SerializeField] ElementsIconsDictionary elementIcons = null;
    float curLifeTime;
    public Transform target;


    // Start is called before the first frame update
    void Start()
    {
        curLifeTime = Time.time + lifeTime;

        if (damage != 0)
        {
            physicDamage.color = target.tag == "Player" ? Color.white : Color.red;
            physicDamage.text = damage.ToString();
        }
        else
        {
            physicDamage.text = "";
        }
        
        if (element.value != 0)
        {
            elementDamage.color = element.color;
            elementDamage.text = element.value.ToString();
            elementIcon.sprite = elementIcons[element.element];
        }
        else
        {
            elementDamage.text = "";
            elementIcon.enabled = false;
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
