using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EWLandingEffect : MonoBehaviour
{
    void AnimationEnd()
    {
        Destroy(gameObject);
    }
}
