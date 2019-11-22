using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DragType { None, Draggable, NotDraggable };

public class Hook : MonoBehaviour
{
    public float maxLength = 5f;
    public float throwSpeed = 2f;
    public float pullSpeed = 3f;
    public float dragRadius = .5f;
    [SerializeField] float _dazedTime = 1f;

    PlayerMovement _playerMovement;
    Vector2 _startPos;
    Vector2 _localStartPos;
    Transform _transform;
    Transform _parent;
    Enemy _hookedEnemy;
    Transform _hookedTarget;
    [SerializeField] LayerMask _layers = 1 << 9 | 1 << 12;

    public Coroutine throwCoroutine;
    public Coroutine pullCoroutine;

    void Start()
    {
        _transform = transform;
        _localStartPos = _transform.localPosition;
        _parent = _parent.transform;
        _playerMovement = _transform.parent.GetComponent<PlayerMovement>();
    }

    public IEnumerator Throw(Vector2 target)//void Throw(Vector2 target)
    {
        _startPos = _transform.position;
        RaycastHit2D hit = Physics2D.Raycast(_startPos, target, maxLength, _layers);

        if(hit)
            target = hit.transform.position; //!!Create a child object for enemies to which needed attaching a hook
        else
        {
            float kat1 = Mathf.Abs(target.x - _startPos.x);
            float kat2 = Mathf.Abs(target.y - _startPos.y);

            //Check throw length. If it longer than max length...
            if(Mathf.Sqrt(Mathf.Pow(kat1, 2) + Mathf.Pow(kat2, 2)) > maxLength)
            {
                //...find a new coordinates for throw, equals to the max length
                float angle = Vector2.Angle(_transform.position, target);             //Get alpha angle
                kat1 = maxLength * Mathf.Cos(angle);                                  //Get first cathet (x)
                kat2 = Mathf.Sqrt(Mathf.Pow(maxLength, 2) - Mathf.Pow(kat1, 2));      //Get second cathet (y)
                target = new Vector2(_startPos.x + kat1, _startPos.y + kat2);
            }
        }

        while (Mathf.Abs(target.x - _transform.position.x) > 0 || Mathf.Abs(target.y - _transform.position.y) > 0)
        {
            yield return new WaitForEndOfFrame();

            _transform.position = Vector2.MoveTowards(_transform.position/*_startPos*/, target, throwSpeed * Time.deltaTime);
        }

        Collider2D[] dragTargets = Physics2D.OverlapCircleAll(_transform.position, dragRadius, _layers);

        if(dragTargets.Length != 0)
        {
            if(dragTargets[0].gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                _hookedEnemy = dragTargets[0].GetComponent<Enemy>();
                _hookedEnemy.HookOn(_transform);
                pullCoroutine = StartCoroutine(Pull(_hookedEnemy.dragType));
            }
            else
            {
                _hookedTarget = dragTargets[0].transform;
                pullCoroutine = StartCoroutine(Pull(DragType.NotDraggable));
            }
        }
        throwCoroutine = null;
        yield return null;
    }

    IEnumerator Pull(DragType type)//void Pull(DragType type)
    {
        if(type == DragType.NotDraggable)
        {
            _playerMovement.HookOn(_transform);
            while (Mathf.Abs(_transform.position.x - _parent.position.x) > 0 
                || Mathf.Abs(_transform.position.y - _parent.position.y) > 0)
            {
                yield return new WaitForEndOfFrame();

            }
            /*while (Mathf.Abs(_transform.position.x - _transform.TransformPoint(_localStartPos).x) > 0
                || Mathf.Abs(_transform.position.y - _transform.TransformPoint(_localStartPos).y) > 0)
            {
                yield return new WaitForEndOfFrame();
                _transform.position = Vector2.MoveTowards(_transform.TransformPoint(_localStartPos), _transform.position, pullSpeed * Time.deltaTime);
            }*/
        }
        else
        {
            while (Mathf.Abs(_transform.TransformPoint(_localStartPos).x - _transform.position.x) > 0
                || Mathf.Abs(_transform.TransformPoint(_localStartPos).y - _transform.position.y) > 0)
            {
                yield return new WaitForEndOfFrame();
                _transform.position = Vector2.MoveTowards(_transform.position, _transform.TransformPoint(_localStartPos), pullSpeed * Time.deltaTime);
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

        _hookedEnemy = null;
        _hookedTarget = null;
    }
}