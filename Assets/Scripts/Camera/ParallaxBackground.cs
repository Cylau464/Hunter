using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    float startPos;
    Transform cameraTransform, myTransform;
    [SerializeField] float parallaxEffect = 0f;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;
        myTransform = transform;
        startPos = myTransform.position.x;
    }

    void Update()
    {
        float dist = cameraTransform.position.x * (1 - parallaxEffect);
        //myTransform.position = Vector2.Lerp(myTransform.position, new Vector2(startPos + dist, myTransform.position.y), 1f);
        myTransform.position = new Vector2(startPos + dist, myTransform.position.y);
    }
}
