using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aim : MonoBehaviour
{
    [SerializeField] Texture2D cursorTexture = null;
    [SerializeField] CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] Vector2 centerOffset = Vector2.zero;

    private void Start()
    {
        if (gameObject.tag == "Main Weapon")
            SetCursorTexture();
    }

    public void SetCursorTexture()
    {
        Cursor.SetCursor(cursorTexture, centerOffset, cursorMode);
    }

    public void SetCursorDefault()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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
