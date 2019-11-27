using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DragType { None, Draggable, NotDraggable };

public class Hook : MonoBehaviour
{
    public float maxLength = 5f;
    public float throwSpeed = 2f;
    public float pullSpeed = 3f;
    public float dragRadius = .25f;
    [SerializeField] float dazedTime = 1f;

    PlayerMovement playerMovement;
    Vector2 startPos;
    Vector2 localStartPos;
    Transform transform;
    Transform tipTransform;
    Transform tailTransform;
    Transform parent;
    Enemy hookedEnemy;
    Transform hookedTarget;
    [SerializeField] LayerMask layers = 1 << 9 | 1 << 12;

    public Coroutine throwCoroutine;
    public Coroutine pullCoroutine;

    void Start()
    {
        transform = transform;
        parent = transform.parent;
        playerMovement = transform.parent.GetComponent<PlayerMovement>();

        foreach(Transform child in transform)
        {
            tipTransform = child.name == "Tip" ? child : tipTransform;
            tailTransform = child.name == "Tail" ? child : tailTransform;
        }
        localStartPos = transform.localPosition;//tailTransform.localPosition;
    }

    public IEnumerator Throw(Vector2 target)
    {
        Collider2D[] dragTargets = Physics2D.OverlapCircleAll(tipTransform.position, dragRadius, layers);
        startPos = tailTransform.position;

        //While tip of hook didn't reach target position
        while(Vector2.MoveTowards(tipTransform.position, target, throwSpeed * Time.deltaTime) != (Vector2) tipTransform.position)//while (Mathf.Abs(target.x - transform.position.x) > 0 || Mathf.Abs(target.y - transform.position.y) > 0 && dragTargets.Length == 0)
        {
            yield return new WaitForEndOfFrame();
            //Draw a line from tip pos to target pos to check for objects on the way
            RaycastHit2D hit = Physics2D.Linecast(tipTransform.position, target, layers);
            //If have the object
            if (hit.transform)
            {
                //Looking for child object Hook Target
                foreach (Transform curTarget in hit.transform)
                {
                    if (curTarget.tag == "Hook Target")
                        target = curTarget.position;
                }
            }
            else
            {
                //Get cathets 
                float kat1 = Mathf.Abs(target.x - startPos.x);
                float kat2 = Mathf.Abs(target.y - startPos.y);

                //Check throw length. If it longer than max length...
                if (Mathf.Sqrt(Mathf.Pow(kat1, 2) + Mathf.Pow(kat2, 2)) > maxLength)
                {
                    //...find a new coordinates for throw, equals to the max length
                    float angle = Vector2.Angle(startPos, target);                       //Get alpha angle
                    kat1 = maxLength * Mathf.Cos(angle);                                  //Get first cathet (x)
                    kat2 = maxLength * Mathf.Sin(angle);//Mathf.Sqrt(Mathf.Pow(maxLength, 2) - Mathf.Pow(kat1, 2));      //Get second cathet (y)
                    target = new Vector2(startPos.x + kat1, startPos.y + kat2);
                }
            }
            //Move tip of hook
            tipTransform.position = Vector2.MoveTowards(tipTransform.position, target, throwSpeed * Time.deltaTime);

            //Looking for the closest target to grab it
            dragTargets = Physics2D.OverlapCircleAll(tipTransform.position, dragRadius, layers);
        }
        //If have a target into radius...
        if(dragTargets != null && dragTargets.Length != 0)
        {
            //...Check his layer name
            if(dragTargets[0].gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                tipTransform.position = target;                                //Move tip of hook to target
                hookedEnemy = dragTargets[0].GetComponent<Enemy>();            //Cache component
                hookedEnemy.HookOn(tipTransform);                             //Set target state to HookOn
                pullCoroutine = StartCoroutine(Pull(hookedEnemy.dragType));    //Pull up tip of hook
            }
            else
            {
                hookedTarget = dragTargets[0].transform;                       //Cache target transform component A NAHUYA?
                pullCoroutine = StartCoroutine(Pull(DragType.NotDraggable));    //Pull up tail of hook to tip
            }
        }
        //Havent a target
        else
        {
            //Just pull up tip of hook back
            pullCoroutine = StartCoroutine(Pull(DragType.None));
        }

        throwCoroutine = null;                                                  //Unset coroutine var 
        yield return null;
    }

    IEnumerator Pull(DragType type)
    {
        //Vector2.zero used because child objects Tip and Tail ever must be in zero local coordinates parent object Hook
        Debug.Log("PULL");
        //If target object is not draggable - pull up character
        if (type == DragType.NotDraggable)
        {
            //Player follow for tail of hook
            playerMovement.HookOn(tailTransform);
            //While tail of hook didn't reach tip position
            while (Vector2.MoveTowards(tailTransform.localPosition, tipTransform.localPosition, pullSpeed * Time.deltaTime) != (Vector2) tailTransform.localPosition)
            {
                yield return new WaitForEndOfFrame();

                tailTransform.localPosition = Vector2.MoveTowards(tailTransform.localPosition, tipTransform.localPosition, pullSpeed * Time.deltaTime);
            }
        }
        //If target object draggable or if haven't a target 
        else
        {
            //While target didn't reach tail of hook position
            while (Vector2.MoveTowards(parent.InverseTransformPoint(tipTransform.position), Vector2.zero, pullSpeed * Time.deltaTime) != (Vector2) tipTransform.localPosition)
            {
                yield return new WaitForEndOfFrame();
                
                tipTransform.localPosition = Vector2.MoveTowards(parent.InverseTransformPoint(tipTransform.position), Vector2.zero, pullSpeed * Time.deltaTime);
            }
        }

        Release();
        pullCoroutine = null;
        yield return null;
    }

    void Release()
    {
        if(hookedEnemy)
            hookedEnemy.HookOff(dazedTime);
        else
            playerMovement.HookOff();

        //Return all objects to they start positions
        transform.localPosition = localStartPos;
        tailTransform.localPosition = Vector2.zero;
        tipTransform.localPosition = Vector2.zero;
        //Unset targets
        hookedEnemy = null;
        hookedTarget = null;
    }
}