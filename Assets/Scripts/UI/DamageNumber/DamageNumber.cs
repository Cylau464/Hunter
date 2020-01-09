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

    // Start is called before the first frame update
    void Start()
    {
        curLifeTime = Time.time + lifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (curLifeTime <= Time.time)
            Destroy(gameObject);

        displayNumber.text = damage.ToString();
        transform.position = new Vector3(transform.position.x + moveSpeed * Time.deltaTime, transform.position.y + moveSpeed * Time.deltaTime, transform.position.z);
    }
}
