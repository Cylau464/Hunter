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
    [SerializeField] float _dazedTime = 1f;

    PlayerMovement _playerMovement;
    Vector2 _startPos;
    Vector2 _localStartPos;
    Transform _transform;
    Transform _tipTransform;
    Transform _tailTransform;
    Transform _parent;
    Enemy _hookedEnemy;
    Transform _hookedTarget;
    [SerializeField] LayerMask _layers = 1 << 9 | 1 << 12;

    public Coroutine throwCoroutine;
    public Coroutine pullCoroutine;

    void Start()
    {
        _transform = transform;
        _parent = _transform.parent;
        _playerMovement = _transform.parent.GetComponent<PlayerMovement>();

        foreach(Transform child in _transform)
        {
            _tipTransform = child.name == "Tip" ? child : _tipTransform;
            _tailTransform = child.name == "Tail" ? child : _tailTransform;
        }
        _localStartPos = _transform.localPosition;//_tailTransform.localPosition;
    }

    public IEnumerator Throw(Vector2 target)
    {
        Collider2D[] dragTargets = Physics2D.OverlapCircleAll(_tipTransform.position, dragRadius, _layers);
        _startPos = _tailTransform.position;

        //While tip of hook didn't reach target position
        while(Vector2.MoveTowards(_tipTransform.position, target, throwSpeed * Time.deltaTime) != (Vector2) _tipTransform.position)//while (Mathf.Abs(target.x - _transform.position.x) > 0 || Mathf.Abs(target.y - _transform.position.y) > 0 && dragTargets.Length == 0)
        {
            yield return new WaitForEndOfFrame();
            //Draw a line from tip pos to target pos to check for objects on the way
            RaycastHit2D hit = Physics2D.Linecast(_tipTransform.position, target, _layers);
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
                float kat1 = Mathf.Abs(target.x - _startPos.x);
                float kat2 = Mathf.Abs(target.y - _startPos.y);

                //Check throw length. If it longer than max length...
                if (Mathf.Sqrt(Mathf.Pow(kat1, 2) + Mathf.Pow(kat2, 2)) > maxLength)
                {
                    //...find a new coordinates for throw, equals to the max length
                    float angle = Vector2.Angle(_startPos, target);                       //Get alpha angle
                    kat1 = maxLength * Mathf.Cos(angle);                                  //Get first cathet (x)
                    kat2 = maxLength * Mathf.Sin(angle);//Mathf.Sqrt(Mathf.Pow(maxLength, 2) - Mathf.Pow(kat1, 2));      //Get second cathet (y)
                    target = new Vector2(_startPos.x + kat1, _startPos.y + kat2);
                }
            }
            //Move tip of hook
            _tipTransform.position = Vector2.MoveTowards(_tipTransform.position, target, throwSpeed * Time.deltaTime);

            //Looking for the closest target to grab it
            dragTargets = Physics2D.OverlapCircleAll(_tipTransform.position, dragRadius, _layers);
        }
        //If have a target into radius...
        if(dragTargets != null && dragTargets.Length != 0)
        {
            //...Check his layer name
            if(dragTargets[0].gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                _tipTransform.position = target;                                //Move tip of hook to target
                _hookedEnemy = dragTargets[0].GetComponent<Enemy>();            //Cache component
                _hookedEnemy.HookOn(_tipTransform);                             //Set target state to HookOn
                pullCoroutine = StartCoroutine(Pull(_hookedEnemy.dragType));    //Pull up tip of hook
            }
            else
            {
                _hookedTarget = dragTargets[0].transform;                       //Cache target transform component A NAHUYA?
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
            _playerMovement.HookOn(_tailTransform);
            //While tail of hook didn't reach tip position
            while (Vector2.MoveTowards(_tailTransform.localPosition, _tipTransform.localPosition, pullSpeed * Time.deltaTime) != (Vector2) _tailTransform.localPosition)
            {
                yield return new WaitForEndOfFrame();

                _tailTransform.localPosition = Vector2.MoveTowards(_tailTransform.localPosition, _tipTransform.localPosition, pullSpeed * Time.deltaTime);
            }
        }
        //If target object draggable or if haven't a target 
        else
        {
            //While target didn't reach tail of hook position
            while (Vector2.MoveTowards(_parent.InverseTransformPoint(_tipTransform.position), Vector2.zero, pullSpeed * Time.deltaTime) != (Vector2) _tipTransform.localPosition)
            {
                yield return new WaitForEndOfFrame();
                
                _tipTransform.localPosition = Vector2.MoveTowards(_parent.InverseTransformPoint(_tipTransform.position), Vector2.zero, pullSpeed * Time.deltaTime);
            }
        }

        Release();
        pullCoroutine = null;
        yield return null;
    }

    void Release()
    {
        if(_hookedEnemy)
            _hookedEnemy.HookOff(_dazedTime);
        else
            _playerMovement.HookOff();

        //Return all objects to they start positions
        _transform.localPosition = _localStartPos;
        _tailTransform.localPosition = Vector2.zero;
        _tipTransform.localPosition = Vector2.zero;
        //Unset targets
        _hookedEnemy = null;
        _hookedTarget = null;
    }
}