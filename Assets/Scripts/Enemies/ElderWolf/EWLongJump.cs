using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EWLongJump : MonoBehaviour
{
    bool isPlayerCaught;      //Was the player caught?
    [HideInInspector] public Vector2 damageVector;
    [HideInInspector] public int damage;
    float startLocalPosY = 1.5f;
    Collider2D objectToDamage;
    BoxCollider2D myCollider;
    BoxCollider2D playerCollider;
    Transform myTransform;
    LayerMask playerLayer = 1 << 10;       //10 - player layer
    LayerMask platformLayer = 1 << 9;

    private void Awake()
    {
        myTransform = transform;
        myCollider = myTransform.GetComponent<BoxCollider2D>();
        playerCollider = GameObject.Find("Player").GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        myTransform.localPosition = new Vector2(myTransform.localPosition.x, startLocalPosY);
        isPlayerCaught = false;
        objectToDamage = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (myTransform.localPosition.y > playerCollider.size.y / 2f)
            myTransform.localPosition = new Vector2(myTransform.localPosition.x, myTransform.localPosition.y - 1f * Time.deltaTime);
            
        if (objectToDamage == null)
            objectToDamage = Physics2D.OverlapBox(myTransform.position, myCollider.size, 0f, playerLayer);
        //If player collided with front legs
        else if (!isPlayerCaught)
        {
            objectToDamage.GetComponent<PlayerAtributes>().TakeDamage(damage, HurtType.Catch, myTransform);
            isPlayerCaught = true;
        }
    }
}
