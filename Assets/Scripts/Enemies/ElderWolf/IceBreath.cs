using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class IceBreath : MonoBehaviour
{
    public Vector2 colliderSize;
    [SerializeField] float yOffsetCollider = 1f;
    PolygonCollider2D myCollider;
    Enemy enemy;
    public Collider2D playerCol;
    int direction;

    void Start()
    {
        enemy = transform.parent.GetComponent<Enemy>();
        direction = enemy.direction;

        myCollider = transform.GetComponent<PolygonCollider2D>();
        myCollider.enabled = false;
        myCollider.pathCount = 4;
        myCollider.SetPath(0, new[] {
                    new Vector2(myCollider.transform.position.x, myCollider.transform.position.y - yOffsetCollider), new Vector2(myCollider.transform.position.x, myCollider.transform.position.y + yOffsetCollider),
                    new Vector2((myCollider.transform.position.x + colliderSize.x) * direction, myCollider.transform.position.y + colliderSize.y / 2f),
                    new Vector2((myCollider.transform.position.x + colliderSize.x) * direction, myCollider.transform.position.y - colliderSize.y / 2f)
                });
        myCollider.enabled = true;
    }

    void Update()
    {
        if (direction != enemy.direction)
            FlipObject();
    }

    void FlipObject()
    {
        transform.localPosition = new Vector2(-transform.localPosition.x, transform.localPosition.y);
        direction = enemy.direction;

        myCollider.enabled = false;
        myCollider.pathCount = 4;
        myCollider.SetPath(0, new[] {
                    new Vector2(myCollider.transform.position.x, myCollider.transform.position.y - yOffsetCollider), new Vector2(myCollider.transform.position.x, myCollider.transform.position.y + yOffsetCollider),
                    new Vector2((myCollider.transform.position.x + colliderSize.x) * direction, myCollider.transform.position.y + colliderSize.y / 2f),
                    new Vector2((myCollider.transform.position.x + colliderSize.x) * direction, myCollider.transform.position.y - colliderSize.y / 2f)
                });
        myCollider.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 10)
            playerCol = col;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == 10)
            playerCol = null;
    }
}