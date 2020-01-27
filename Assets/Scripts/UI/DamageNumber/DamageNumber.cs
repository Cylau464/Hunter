using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
    public int damage;
    [SerializeField]
    float moveSpeed = 5f;
    [SerializeField]
    Text displayNumber = null;
    [SerializeField]
    float lifeTime = 2f;
    float curLifeTime;
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        curLifeTime = Time.time + lifeTime;
        displayNumber.text = damage.ToString();
        displayNumber.color = target.tag == "Player" ? Color.white : Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        if (curLifeTime <= Time.time)
            Destroy(gameObject);

        transform.position = new Vector3(transform.position.x + moveSpeed * Time.deltaTime, transform.position.y + moveSpeed * Time.deltaTime, transform.position.z);
    }
}
