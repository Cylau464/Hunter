using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool UIOverlapsMouse;

    // Start is called before the first frame update
    void Awake()
    {
        //Persis this object between scene reloads
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
