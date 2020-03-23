using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Structures;
using Enums;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] Vector2 moveSpeed = new Vector2(5f, 5f);
    [SerializeField] Text physicDamage = null;
    [SerializeField] Text elementDamage = null;
    [SerializeField] Image elementIcon = null;
    [SerializeField] float lifeTime = 2f;
    [SerializeField] ElementsIconsDictionary elementIcons = null;
    int damage;
    Element element;
    float curLifeTime;
    Transform target;
    Rigidbody2D rigidBody;


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

        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.AddForce(moveSpeed, ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        if (curLifeTime <= Time.time)
            Destroy(gameObject);

        //transform.position = new Vector3(transform.position.x + moveSpeed * Time.deltaTime, transform.position.y + moveSpeed * Time.deltaTime, transform.position.z);
    }

    public void GetParameters(int damage, Transform target, Element element = new Element())
    {
        this.damage = damage;
        this.target = target;
        this.element = element;
    }
}
