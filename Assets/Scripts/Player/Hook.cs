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

        while(Vector2.MoveTowards(_tipTransform.position, target, throwSpeed * Time.deltaTime) != (Vector2) _tipTransform.position)//while (Mathf.Abs(target.x - _transform.position.x) > 0 || Mathf.Abs(target.y - _transform.position.y) > 0 && dragTargets.Length == 0)
        {
            yield return new WaitForEndOfFrame();

            RaycastHit2D hit = Physics2D.Linecast(_startPos, target, _layers);

            if (hit.transform)
            {
                foreach (Transform curTarget in hit.transform)
                {
                    if (curTarget.tag == "Hook Target")
                        target = curTarget.position;
                }
            }
            else
            {
                float kat1 = Mathf.Abs(target.x - _startPos.x);
                float kat2 = Mathf.Abs(target.y - _startPos.y);

                //Check throw length. If it longer than max length...
                if (Mathf.Sqrt(Mathf.Pow(kat1, 2) + Mathf.Pow(kat2, 2)) > maxLength)
                {
                    //...find a new coordinates for throw, equals to the max length
                    float angle = Vector2.Angle(_transform.position, target);             //Get alpha angle
                    kat1 = maxLength * Mathf.Cos(angle);                                  //Get first cathet (x)
                    kat2 = Mathf.Sqrt(Mathf.Pow(maxLength, 2) - Mathf.Pow(kat1, 2));      //Get second cathet (y)
                    target = new Vector2(_startPos.x + kat1, _startPos.y + kat2);
                }
            }
            //Move tip of hook
            _tipTransform.position = Vector2.MoveTowards(_tipTransform.position, target, throwSpeed * Time.deltaTime);

            //Looking for the closest target to grab it
            dragTargets = Physics2D.OverlapCircleAll(_tipTransform.position, dragRadius, _layers);
        }

        if(dragTargets != null && dragTargets.Length != 0)
        {
            if(dragTargets[0].gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                _tipTransform.position = target;
                _hookedEnemy = dragTargets[0].GetComponent<Enemy>();
                _hookedEnemy.HookOn(_tipTransform);
                pullCoroutine = StartCoroutine(Pull(_hookedEnemy.dragType));
            }
            else
            {
                _hookedTarget = dragTargets[0].transform;
                Debug.Log(_hookedTarget + "ht");
                pullCoroutine = StartCoroutine(Pull(DragType.NotDraggable));
            }
        }

        throwCoroutine = null;
        yield return null;
    }

    IEnumerator Pull(DragType type)//void Pull(DragType type)
    {
        Debug.Log("PULL");
        if(type == DragType.NotDraggable)
        {
            _playerMovement.HookOn(_tailTransform);
            /*while (Mathf.Abs(_tipTransform.position.x - _tailTransform.position.x) > 0 
                || Mathf.Abs(_tipTransform.position.y - _tailTransform.position.y) > 0)
            {
                yield return new WaitForEndOfFrame();

            }*/
            while (Vector2.MoveTowards(_tailTransform.localPosition, _tipTransform.localPosition, pullSpeed * Time.deltaTime) != (Vector2) _tailTransform.localPosition)//while (Mathf.Abs(_transform.position.x - _transform.TransformPoint(_localStartPos).x) > 0 || Mathf.Abs(_transform.position.y - _transform.TransformPoint(_localStartPos).y) > 0)
            {
                yield return new WaitForEndOfFrame();

                _tailTransform.localPosition = Vector2.MoveTowards(_tailTransform.localPosition, _tipTransform.localPosition, pullSpeed * Time.deltaTime);
                //_transform.position = Vector2.MoveTowards(_tailTransform.localPosition, _tipTransform.localPosition, pullSpeed * Time.deltaTime);
            }
        }
        else
        {
            Debug.Log(_transform.position + "nfds " + _parent.InverseTransformPoint(_transform.position) + "sdf " + _transform.localPosition.x);
            while (Vector2.MoveTowards(_parent.InverseTransformPoint(_tipTransform.position), Vector2.zero, pullSpeed * Time.deltaTime) != (Vector2) _tipTransform.localPosition)//while (Mathf.Abs(_transform.TransformPoint(_localStartPos).x - _transform.localPosition.x) > 0 || Mathf.Abs(_transform.TransformPoint(_localStartPos).y - _transform.localPosition.y) > 0)
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

        _transform.localPosition = _localStartPos;
        _tailTransform.localPosition = Vector2.zero;
        _tipTransform.localPosition = Vector2.zero;
        _hookedEnemy = null;
        _hookedTarget = null;
    }
}