using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aim : MonoBehaviour
{
    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 centerOffset = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.SetCursor(cursorTexture, centerOffset, cursorMode);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //void OnMouseEnter()
    //{
    //    Cursor.SetCursor(cursorTexture, centerOffset, cursorMode);
    //}

    //void OnMouseExit()
    //{
    //    Cursor.SetCursor(null, Vector2.zero, cursorMode);
    //}
}
