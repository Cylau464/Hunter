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

    Vector2 _startPos;
    Transform _transform;
    Enemy _hookedEnemy;
    Transform _hookedTarget;
    [SerializeField] LayerMask _layers = 1 << 9 || 1 << 11;

    void Start()
    {
        _transform = transform;
        _localStartPos = _transform.localPosition;
    }
    public void Throw(Vector2 target)
    {
        _startPos = _transform.position;
        RaycastHit2D hit = Physics2D.Raycast(_startPos, target, maxLength, _layers);

        if(hit != null)
            target = hit.transform.position; //!!Create a child object for enemies to which needed attaching a hook
        else
        {
            float kat1 = Mathf.Abs(target.x - _startPos.x);
            float kat2 = Mathf.Abs(target.y - _startPos.y);

            //Check throw length. If it longer than max length...
            if(Mathf.Sqrt(Mathf.Pow(kat1, 2) + Mathf.Pow(kat2, 2)) > maxLength)
            {
                //...find a new coordinates for throw, equals to the max length
                float angle = Vector2.Angle(_startPos, target);
                kat1 = maxLength * Mathf.Cos(angle);                                  //Get first cathet (x)
                kat2 = Mathf.Sqrt(Mathf.Pow(maxLength, 2) - Mathf.Pow(kat1, 2));      //Get second cathet (y)
                target = new Vector2(_startPos.x + kat1, _startPos.y + kat2);
            }
        }
        //---------Maybe need change on IF or start coroutine
        while(Mathf.Abs(target - _transform.position) > 0)
            _transform.position = Vector2.MoveTowards(_startPos, target, throwSpeed * Time.deltaTime);

        Collider2D[] dragTargets = Physics2D.OverlapCircle(_transform.position, dragRadius, _layers);
        
        if(dragTargets != null)
        {
            if(dragTargets.layer == "Enemy")
            {
                _hookedEnemy = dragTargets[0].GetComponent<Enemy>();
                _hookedEnemy.HookOn(_transform);
                Pull(_hookedEnemy.dragType);
            }
            else
            {
                _hookedTarget = dragTargets[0].transform;
                Pull(DragType.NotDraggable);
            }
        }
    }

    void Pull(DragType type)
    {
        if(type == DragType.NotDraggable)
        {
            //_playerMovement.HookOn(); <- Move player there
            while(Mathf.Abs(_transform.position - TransformPoint(_localStartPos)) > 0)
                _transform.position = Vector2.MoveTowards(TransformPoint(_localStartPos), _transform.position, pullSpeed * Time.deltaTime);
        }
        else
        {
            while(Mathf.Abs(TransformPoint(_localStartPos) - _transform.position) > 0)
                _transform.position = Vector2.MoveTowards(_transform.position, TransformPoint(_localStartPos), pullSpeed * Time.deltaTime);
        }

        Release();
    }

    void Release()
    {
        if(_hookedEnemy)
            _hookedEnemy.HookOff(_dazedTime);

        _hookedEnemy = null;
        _hookedTarget = null;
    }
}